using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OrbitLauncher
{
    public enum ProbeState
    {
        FreeLaunch,
        GravitationalLock,
        StableOrbit,
        SlingshotThrust,
        DeathAndReset
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ProbeController : MonoBehaviour
    {
        public static ProbeController Instance { get; private set; }

        [Header("GDD Tuned Physics Parameters")]
        public float launchSpeed = 10.5f;
        public float momentumDamping = 0.6f;     // 40% damping -> keeps 60%
        public float minEntrySpeed = 6.4f;       // Ensures crisp orbital speed
        public float lockSpeed = 9.5f;           // Smooth interpolation onto orbitRadius
        public float speedMultiplier = 1.5f;     // Slingshot release multiplier
        public float gravityOffCooldown = 0.15f; // 0.15s cooldown on release
        public float cosmicDrag = 0.015f;        // Slight cosmic drag in free flight
        public float deepSpaceTimeout = 4.0f;    // 4s timeout in empty space

        public ProbeState CurrentState { get; private set; } = ProbeState.FreeLaunch;
        public GravityAnchor CurrentAnchor { get; private set; }
        public GravityAnchor LastReleasedAnchor { get; private set; }

        public bool IsWarpInvulnerable => _warpTimer > 0f;
        public float WarpTimeRemaining => Mathf.Max(0f, _warpTimer);

        private Rigidbody2D _rb;
        private CircleCollider2D _collider;

        // Orbital coordinates
        private int _dir = -1;
        private float _angle;
        private float _radius;
        private float _omega;
        private float _currentPlanetRadians;
        private bool _ignoreGravity;
        private float _freeFlightTimer;
        private bool _lastReleaseWasAimedPerfect;
        private float _warpTimer;

        // Visual components
        private Transform _hullVisual;
        private SpriteRenderer _hullRenderer;
        private SpriteRenderer _hullGlowRenderer;
        private TrailRenderer _mainTrail;
        private TrailRenderer _twinTrailLeft;
        private TrailRenderer _twinTrailRight;
        private LineRenderer _aimLine;
        private Transform _warpShieldVisual;
        private SpriteRenderer _warpShieldRenderer;
        private HullDefinition _currentHull;

        private void Awake()
        {
            Instance = this;
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.linearDamping = cosmicDrag;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
            _collider.radius = 0.24f;

            BuildProbeVisuals();
        }

        private void BuildProbeVisuals()
        {
            // Hull visual child
            GameObject hullGo = new GameObject("HullVisual");
            hullGo.transform.SetParent(transform, false);
            _hullVisual = hullGo.transform;
            _hullVisual.localScale = Vector3.one * 0.58f;

            _hullRenderer = hullGo.AddComponent<SpriteRenderer>();
            _hullRenderer.sortingOrder = 12;

            GameObject glowGo = new GameObject("HullGlow");
            glowGo.transform.SetParent(hullGo.transform, false);
            _hullGlowRenderer = glowGo.AddComponent<SpriteRenderer>();
            _hullGlowRenderer.sprite = OrbitAssetFactory.GetSprite("glow_circle");
            _hullGlowRenderer.sortingOrder = 10;
            glowGo.transform.localScale = Vector3.one * 2.1f;

            // Main TrailRenderer (0.25s per GDD Section 6)
            _mainTrail = gameObject.AddComponent<TrailRenderer>();
            _mainTrail.time = 0.28f;
            _mainTrail.minVertexDistance = 0.05f;
            _mainTrail.sharedMaterial = OrbitAssetFactory.GetDefaultSpriteMaterial();
            _mainTrail.sortingOrder = 9;
            AnimationCurve widthCurve = new AnimationCurve(
                new Keyframe(0f, 0.34f),
                new Keyframe(0.5f, 0.16f),
                new Keyframe(1f, 0.0f)
            );
            _mainTrail.widthCurve = widthCurve;

            // Twin trails for Neon Glider hull
            _twinTrailLeft = CreateSubTrail("TwinTrailL", new Vector3(-0.18f, -0.15f, 0f));
            _twinTrailRight = CreateSubTrail("TwinTrailR", new Vector3(0.18f, -0.15f, 0f));

            // Dotted Predicted Path LineRenderer (GDD Section 3)
            GameObject aimGo = new GameObject("PredictedPathLine");
            aimGo.transform.SetParent(transform, false);
            _aimLine = aimGo.AddComponent<LineRenderer>();
            _aimLine.useWorldSpace = true;
            _aimLine.positionCount = 2;
            _aimLine.startWidth = 0.11f;
            _aimLine.endWidth = 0.08f;
            _aimLine.textureMode = LineTextureMode.Tile;
            Material aimMat = new Material(OrbitAssetFactory.GetDefaultSpriteMaterial());
            aimMat.mainTexture = OrbitAssetFactory.GetDottedLineSprite().texture;
            _aimLine.sharedMaterial = aimMat;
            _aimLine.sortingOrder = 8;
            _aimLine.enabled = false;

            // Warp Invulnerability Shield Visual
            GameObject shieldGo = new GameObject("WarpShield");
            shieldGo.transform.SetParent(transform, false);
            _warpShieldVisual = shieldGo.transform;
            _warpShieldRenderer = shieldGo.AddComponent<SpriteRenderer>();
            _warpShieldRenderer.sprite = OrbitAssetFactory.GetSprite("orbit_ring");
            _warpShieldRenderer.color = new Color(0f, 1f, 0.75f, 0.85f);
            _warpShieldRenderer.sortingOrder = 14;
            _warpShieldVisual.localScale = Vector3.one * 0.26f;
            shieldGo.SetActive(false);

            ApplyHull(0);
        }

        private TrailRenderer CreateSubTrail(string name, Vector3 localPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_hullVisual, false);
            go.transform.localPosition = localPos;
            TrailRenderer tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.26f;
            tr.minVertexDistance = 0.05f;
            tr.sharedMaterial = OrbitAssetFactory.GetDefaultSpriteMaterial();
            tr.sortingOrder = 8;
            tr.widthCurve = new AnimationCurve(new Keyframe(0f, 0.14f), new Keyframe(1f, 0f));
            tr.emitting = false;
            return tr;
        }

        public void ApplyHull(int hullId)
        {
            int idx = Mathf.Clamp(hullId, 0, OrbitAssetFactory.Hulls.Length - 1);
            _currentHull = OrbitAssetFactory.Hulls[idx];

            if (_hullRenderer != null)
            {
                _hullRenderer.sprite = OrbitAssetFactory.GetSprite(_currentHull.spriteResource);
            }
            if (_hullGlowRenderer != null)
            {
                _hullGlowRenderer.color = new Color(_currentHull.primaryColor.r, _currentHull.primaryColor.g, _currentHull.primaryColor.b, 0.42f);
            }

            UpdateTrailGradient(1);
        }

        public void UpdateTrailGradient(int comboMultiplier)
        {
            if (_mainTrail == null) return;

            Color cStart = _currentHull.primaryColor;
            Color cEnd = _currentHull.secondaryColor;
            if (comboMultiplier >= 4)
            {
                cStart = Color.Lerp(cStart, OrbitAssetFactory.LimeGreen, 0.5f);
                _mainTrail.time = 0.36f;
            }
            else
            {
                _mainTrail.time = 0.26f;
            }

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(cStart, 0f), new GradientColorKey(cEnd, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.0f, 1f) }
            );
            _mainTrail.colorGradient = grad;

            bool isTwin = _currentHull.id == 1;
            if (_twinTrailLeft != null && _twinTrailRight != null)
            {
                _twinTrailLeft.colorGradient = grad;
                _twinTrailRight.colorGradient = grad;
                _twinTrailLeft.emitting = isTwin;
                _twinTrailRight.emitting = isTwin;
            }
        }

        public void ActivateWarpInvulnerability(float duration = 3.0f)
        {
            _warpTimer = Mathf.Max(_warpTimer, duration);
            if (_warpShieldVisual != null) _warpShieldVisual.gameObject.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWarpShield();
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnFloatingPopup(transform.position, "WARP SHIELD ACTIVE!", OrbitAssetFactory.NeonCyan, 1.15f);
            }
        }

        public void ResetAndLaunch(Vector2 startPosition, Vector2 initialDirection)
        {
            StopAllCoroutines();
            CurrentAnchor = null;
            LastReleasedAnchor = null;
            _ignoreGravity = false;
            _freeFlightTimer = 0f;
            _warpTimer = 0f;
            _lastReleaseWasAimedPerfect = true;

            if (_warpShieldVisual != null) _warpShieldVisual.gameObject.SetActive(false);
            if (_hullVisual != null) _hullVisual.gameObject.SetActive(true);
            if (_aimLine != null) _aimLine.enabled = false;

            transform.position = startPosition;
            _rb.position = startPosition;
            Vector2 dir = initialDirection.sqrMagnitude > 0.001f ? initialDirection.normalized : Vector2.right;
            _rb.linearVelocity = dir * launchSpeed;
            OrientHullAlong(dir);

            if (_mainTrail != null) _mainTrail.Clear();
            if (_twinTrailLeft != null) _twinTrailLeft.Clear();
            if (_twinTrailRight != null) _twinTrailRight.Clear();

            CurrentState = ProbeState.FreeLaunch;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            if (CurrentState == ProbeState.DeathAndReset)
                return;

            // Update Warp Invulnerability timer
            if (_warpTimer > 0f)
            {
                _warpTimer -= Time.deltaTime;
                if (_warpShieldVisual != null)
                {
                    _warpShieldVisual.Rotate(0f, 0f, 220f * Time.deltaTime);
                    float pulse = 0.25f + 0.03f * Mathf.Sin(Time.time * 16f);
                    _warpShieldVisual.localScale = Vector3.one * pulse;
                    if (_warpTimer <= 0f)
                    {
                        _warpShieldVisual.gameObject.SetActive(false);
                    }
                }
            }

            // Check 1-Tap Release Input
            if ((CurrentState == ProbeState.GravitationalLock || CurrentState == ProbeState.StableOrbit) &&
                CheckReleaseInputThisFrame())
            {
                Release();
            }

            // Update Free-Flight Deep-Space Timeout & Orientation
            if (CurrentState == ProbeState.FreeLaunch)
            {
                if (_aimLine != null) _aimLine.enabled = false;
                Vector2 vel = _rb.linearVelocity;
                if (vel.sqrMagnitude > 0.05f)
                {
                    OrientHullAlong(vel.normalized);
                }

                _freeFlightTimer += Time.deltaTime;
                if (_freeFlightTimer >= deepSpaceTimeout)
                {
                    Die("Probe drifted into deep space");
                }
            }
            else if (CurrentState == ProbeState.GravitationalLock || CurrentState == ProbeState.StableOrbit)
            {
                UpdatePredictedTangentPath();
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            if (CurrentState == ProbeState.GravitationalLock || CurrentState == ProbeState.StableOrbit)
            {
                if (CurrentAnchor == null || CurrentAnchor.IsCollapsed)
                {
                    CurrentState = ProbeState.FreeLaunch;
                    return;
                }

                // 1. State 2: Gravitational Lock - smoothly ease onto orbitRadius
                if (CurrentState == ProbeState.GravitationalLock)
                {
                    _radius = Mathf.MoveTowards(_radius, CurrentAnchor.orbitRadius, lockSpeed * Time.fixedDeltaTime);
                    if (Mathf.Abs(_radius - CurrentAnchor.orbitRadius) <= 0.02f)
                    {
                        _radius = CurrentAnchor.orbitRadius;
                        CurrentState = ProbeState.StableOrbit;
                    }
                }

                // 2. State 2 & 3: Revolve around the anchor center via polar coordinates
                float pulseMult = CurrentAnchor.CurrentPulseMultiplier;
                float deltaAngle = _dir * _omega * pulseMult * Time.fixedDeltaTime;
                _angle += deltaAngle;
                _currentPlanetRadians += Mathf.Abs(deltaAngle);
                CurrentAnchor.AddOrbitProgress(deltaAngle, this);

                Vector2 anchorPos = CurrentAnchor.transform.position;
                Vector2 offset = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle)) * _radius;
                Vector2 targetPos = anchorPos + offset;
                _rb.MovePosition(targetPos);

                // Orient hull along tangent direction
                Vector2 tangent = _dir * new Vector2(-offset.y, offset.x).normalized;
                OrientHullAlong(tangent);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (CurrentState != ProbeState.FreeLaunch || _ignoreGravity)
                return;

            GravityAnchor anchor = other.GetComponent<GravityAnchor>();
            if (anchor != null && !anchor.IsCollapsed && anchor != LastReleasedAnchor)
            {
                OnAnchorEnter(anchor);
            }
        }

        public void OnAnchorEnter(GravityAnchor a)
        {
            CurrentAnchor = a;
            _dir = a.direction; // +1 = counter-clockwise, -1 = clockwise
            Vector2 r = _rb.position - (Vector2)a.transform.position;
            _angle = Mathf.Atan2(r.y, r.x);
            _radius = r.magnitude;

            // GDD Section 2 & 7: 40% momentum damping on capture -> omega = v_entry / R_orbit
            float rawSpeed = _rb.linearVelocity.magnitude;
            float vEntry = Mathf.Max(minEntrySpeed, rawSpeed * momentumDamping);
            _omega = vEntry / Mathf.Max(0.5f, a.orbitRadius);

            _rb.linearVelocity = Vector2.zero;
            _freeFlightTimer = 0f;
            _currentPlanetRadians = 0f;
            CurrentState = ProbeState.GravitationalLock;

            if (!a.IsVisited)
            {
                a.IsVisited = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnPlanetCaptured(a, _lastReleaseWasAimedPerfect);
                }
            }
            else
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCaptureChime(0);
                }
            }
        }

        public void Release()
        {
            if (CurrentAnchor == null) return;

            CurrentState = ProbeState.SlingshotThrust;

            Vector2 anchorPos = CurrentAnchor.transform.position;
            Vector2 r = _rb.position - anchorPos;
            Vector2 tangent = _dir * new Vector2(-r.y, r.x).normalized; // perpendicular to radius

            float pulseMult = CurrentAnchor.CurrentPulseMultiplier;
            float releaseSpeed = (_omega * _radius * pulseMult) * speedMultiplier;
            releaseSpeed = Mathf.Clamp(releaseSpeed, 8.5f, 16.5f);

            // Check if this release aims cleanly into the next planet's gravity capture zone
            bool aimedAtNext = IsTangentAimedAtNextAnchor(_rb.position, tangent, out GravityAnchor nextTarget, out Vector2 gentleAssistDir);
            bool loopedTooMuch = (_currentPlanetRadians > Mathf.PI * 4.1f); // More than 2 full spins breaks clean rhythm
            _lastReleaseWasAimedPerfect = aimedAtNext && !loopedTooMuch;

            // Gentle subtle aim-assist inside the sweet spot so clean shots feel super rewarding
            Vector2 finalDir = aimedAtNext ? Vector2.Lerp(tangent, gentleAssistDir, 0.25f).normalized : tangent;

            _rb.linearVelocity = finalDir * releaseSpeed;
            OrientHullAlong(finalDir);

            LastReleasedAnchor = CurrentAnchor;
            CurrentAnchor = null;
            _freeFlightTimer = 0f;
            if (_aimLine != null) _aimLine.enabled = false;

            CurrentState = ProbeState.FreeLaunch;
            StartCoroutine(IgnoreGravityFor(gravityOffCooldown));

            if (CameraRig.Instance != null)
            {
                CameraRig.Instance.TriggerReleaseZoom();
            }
            if (AudioManager.Instance != null && GameManager.Instance != null)
            {
                AudioManager.Instance.PlayRelease(GameManager.Instance.ComboMultiplier);
            }
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnReleaseBurst(_rb.position, finalDir, _currentHull.primaryColor, _currentHull.secondaryColor);
            }
        }

        private IEnumerator IgnoreGravityFor(float duration)
        {
            _ignoreGravity = true;
            yield return new WaitForSeconds(duration);
            _ignoreGravity = false;
        }

        private void UpdatePredictedTangentPath()
        {
            if (_aimLine == null || CurrentAnchor == null) return;

            _aimLine.enabled = true;
            Vector2 pos = transform.position;
            Vector2 r = pos - (Vector2)CurrentAnchor.transform.position;
            Vector2 tangent = _dir * new Vector2(-r.y, r.x).normalized;

            bool onTarget = IsTangentAimedAtNextAnchor(pos, tangent, out _, out _);
            float lineLength = 6.2f;
            _aimLine.SetPosition(0, new Vector3(pos.x, pos.y, -0.2f));
            _aimLine.SetPosition(1, new Vector3(pos.x + tangent.x * lineLength, pos.y + tangent.y * lineLength, -0.2f));

            Color lineCol = onTarget
                ? new Color(0f, 1f, 0.53f, 0.92f)   // Bright Lime when aimed at next planet!
                : new Color(0.75f, 0.92f, 1f, 0.42f);
            _aimLine.startColor = lineCol;
            _aimLine.endColor = new Color(lineCol.r, lineCol.g, lineCol.b, 0.05f);
        }

        private bool IsTangentAimedAtNextAnchor(Vector2 origin, Vector2 tangent, out GravityAnchor nextAnchor, out Vector2 idealDir)
        {
            nextAnchor = null;
            idealDir = tangent;
            if (WorldSpawner.Instance == null) return false;

            int currentIndex = CurrentAnchor != null ? CurrentAnchor.hopIndex : -1;
            nextAnchor = WorldSpawner.Instance.GetAnchorByHopIndex(currentIndex + 1);
            if (nextAnchor == null) return false;

            Vector2 toNext = (Vector2)nextAnchor.transform.position - origin;
            float dist = toNext.magnitude;
            if (dist < 0.1f) return false;

            float dot = Vector2.Dot(tangent, toNext.normalized);
            if (dot <= 0.5f) return false;

            // Closest distance from tangent ray to next planet center
            float perpDist = Vector2.Perpendicular(tangent).normalized != Vector2.zero
                ? Mathf.Abs(Vector2.Dot(toNext, new Vector2(-tangent.y, tangent.x)))
                : 99f;

            idealDir = toNext.normalized;
            return perpDist <= (nextAnchor.captureRadius * 0.92f);
        }

        public void ApplyExternalPull(Vector2 deltaVelocity)
        {
            if (CurrentState == ProbeState.FreeLaunch)
            {
                _rb.linearVelocity += deltaVelocity;
            }
        }

        public void Die(string causeOfDeath)
        {
            if (CurrentState == ProbeState.DeathAndReset) return;
            CurrentState = ProbeState.DeathAndReset;

            _rb.linearVelocity = Vector2.zero;
            if (_aimLine != null) _aimLine.enabled = false;
            if (_hullVisual != null) _hullVisual.gameObject.SetActive(false);
            if (_warpShieldVisual != null) _warpShieldVisual.gameObject.SetActive(false);

            if (CameraRig.Instance != null)
            {
                // GDD Section 4 & 7: screenshake for 0.2s at magnitude 0.4
                CameraRig.Instance.TriggerShake(0.2f, 0.4f);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDeath();
            }
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnDeathShatter(transform.position, _currentHull.primaryColor, _currentHull.secondaryColor);
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProbeDied(causeOfDeath);
            }
        }

        private void OrientHullAlong(Vector2 dir)
        {
            if (_hullVisual == null || dir.sqrMagnitude < 0.0001f) return;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            _hullVisual.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private bool CheckReleaseInputThisFrame()
        {
            // Ignore clicks/taps that land on UI buttons (like Pause)
            bool pointerOverButton = false;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    pointerOverButton = true;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                    return true;
            }
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                if (!IsScreenPointInTopRightPauseZone(Touchscreen.current.primaryTouch.position.ReadValue()) && !pointerOverButton)
                    return true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (!IsScreenPointInTopRightPauseZone(Mouse.current.position.ReadValue()) && !pointerOverButton)
                    return true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                return true;
            if (Input.GetMouseButtonDown(0) && !IsScreenPointInTopRightPauseZone(Input.mousePosition) && !pointerOverButton)
                return true;
#endif
            return false;
        }

        private static bool IsScreenPointInTopRightPauseZone(Vector2 screenPos)
        {
            return screenPos.x > Screen.width - 110f && screenPos.y > Screen.height - 110f;
        }
    }
}

