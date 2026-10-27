using UnityEngine;

namespace OrbitLauncher
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class GravityAnchor : MonoBehaviour
    {
        public int hopIndex;
        public int tierIndex = 1;
        public float planetRadius = 1.0f;
        public float orbitRadius = 2.4f;
        public float captureRadius = 3.36f; // ~1.4x orbitRadius
        public int direction = -1;          // -1 = Clockwise, +1 = Counter-Clockwise

        [Header("Tier Mechanics")]
        public bool isDecaying;             // Tier 3: shrinks & collapses after 2 spins
        public float maxSpinsBeforeCollapse = 2.0f;
        public bool isPulsing;              // Tier 4: oscillating gravitational strength
        public float pulseFrequency = 2.6f;
        public bool isMoving;               // Tier 5: moving planets
        public Vector2 moveAxis = Vector2.up;
        public float moveAmplitude = 1.6f;
        public float moveFrequency = 1.35f;

        public bool IsVisited { get; set; }
        public bool IsCollapsed { get; private set; }
        public float CurrentPulseMultiplier { get; private set; } = 1.0f;
        public float AccumulatedOrbitRadians { get; private set; }

        private Vector2 _initialPosition;
        private float _phaseOffset;
        private CircleCollider2D _triggerCollider;

        private Transform _planetVisual;
        private SpriteRenderer _planetRenderer;
        private SpriteRenderer _glowRenderer;
        private SpriteRenderer _captureDiscRenderer;
        private Transform _orbitRingTransform;
        private SpriteRenderer _orbitRingRenderer;
        private TextMesh _labelText;
        private SpriteRenderer _labelBg;
        private Color _themeColor;

        private static readonly string[] PlanetSprites =
        {
            "planet_arcade", "planet_cyan", "planet_purple", "planet_amber", "planet_emerald", "planet_crimson"
        };

        public void Initialize(int index, int tier, Vector2 pos, float pRadius, float oRadius, int dir,
            bool decaying, bool pulsing, bool moving, Vector2 mvAxis, float mvAmp)
        {
            hopIndex = index;
            tierIndex = tier;
            _initialPosition = pos;
            transform.position = pos;
            planetRadius = pRadius;
            orbitRadius = oRadius;
            captureRadius = oRadius * 1.42f;
            direction = dir >= 0 ? 1 : -1;

            isDecaying = decaying;
            isPulsing = pulsing;
            isMoving = moving;
            moveAxis = mvAxis.normalized;
            moveAmplitude = mvAmp;
            _phaseOffset = index * 1.37f;

            _triggerCollider = GetComponent<CircleCollider2D>();
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = captureRadius;

            BuildVisuals();
        }

        private void BuildVisuals()
        {
            // Choose theme color based on mechanics & direction
            if (isDecaying)
                _themeColor = new Color(1f, 0.38f, 0.25f, 1f);
            else if (isPulsing)
                _themeColor = OrbitAssetFactory.PulsarPurple;
            else if (direction > 0)
                _themeColor = new Color(0.75f, 0.42f, 1f, 1f); // Counter-clockwise purple-cyan
            else
                _themeColor = OrbitAssetFactory.NeonCyan;      // Standard Neon Cyan #00f0ff

            // 1. Faint Capture Zone Disc (18% opacity per GDD Section 6)
            GameObject captureGo = new GameObject("CaptureZoneDisc");
            captureGo.transform.SetParent(transform, false);
            _captureDiscRenderer = captureGo.AddComponent<SpriteRenderer>();
            _captureDiscRenderer.sprite = OrbitAssetFactory.GetSprite("glow_circle");
            _captureDiscRenderer.color = new Color(_themeColor.r, _themeColor.g, _themeColor.b, 0.18f);
            _captureDiscRenderer.sortingOrder = -5;
            float capDiameter = captureRadius * 2f;
            captureGo.transform.localScale = new Vector3(capDiameter / 2.4f, capDiameter / 2.4f, 1f);

            // 2. Dashed Orbit Ring
            GameObject ringGo = new GameObject("DashedOrbitRing");
            ringGo.transform.SetParent(transform, false);
            _orbitRingTransform = ringGo.transform;
            _orbitRingRenderer = ringGo.AddComponent<SpriteRenderer>();
            _orbitRingRenderer.sprite = OrbitAssetFactory.GetSprite("orbit_ring");
            _orbitRingRenderer.color = new Color(_themeColor.r, _themeColor.g, _themeColor.b, 0.88f);
            _orbitRingRenderer.sortingOrder = -2;
            float ringScale = (orbitRadius * 2f) / 4.64f;
            ringGo.transform.localScale = new Vector3(ringScale, ringScale, 1f);

            // Add 4 directional arrow markers on the orbit ring so player can read CW vs CCW
            Sprite arrowSp = OrbitAssetFactory.GetSprite("hull_scout");
            for (int i = 0; i < 4; i++)
            {
                float ang = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                GameObject arrowGo = new GameObject("DirMarker_" + i);
                arrowGo.transform.SetParent(_orbitRingTransform, false);
                SpriteRenderer arSr = arrowGo.AddComponent<SpriteRenderer>();
                arSr.sprite = arrowSp;
                arSr.color = new Color(_themeColor.r, _themeColor.g, _themeColor.b, 0.75f);
                arSr.sortingOrder = -1;
                float localR = 2.32f;
                arrowGo.transform.localPosition = new Vector3(Mathf.Cos(ang) * localR, Mathf.Sin(ang) * localR, 0f);
                Vector2 tangent = direction * new Vector2(-Mathf.Sin(ang), Mathf.Cos(ang));
                float rotZ = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
                arrowGo.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
                arrowGo.transform.localScale = Vector3.one * 0.16f;
            }

            // 3. Planet Core & Atmospheric Halo
            GameObject planetGo = new GameObject("PlanetBody");
            planetGo.transform.SetParent(transform, false);
            _planetVisual = planetGo.transform;
            _planetRenderer = planetGo.AddComponent<SpriteRenderer>();
            string spriteName = PlanetSprites[Mathf.Abs(hopIndex + tierIndex - 1) % PlanetSprites.Length];
            if (isDecaying) spriteName = "planet_amber";
            else if (isPulsing) spriteName = "planet_purple";
            else if (direction > 0 && tierIndex >= 2) spriteName = "planet_purple";
            _planetRenderer.sprite = OrbitAssetFactory.GetSprite(spriteName);
            _planetRenderer.sortingOrder = 2;
            float bodyScale = (planetRadius * 2f) / 2.08f;
            planetGo.transform.localScale = new Vector3(bodyScale, bodyScale, 1f);

            GameObject haloGo = new GameObject("AtmosphereGlow");
            haloGo.transform.SetParent(planetGo.transform, false);
            _glowRenderer = haloGo.AddComponent<SpriteRenderer>();
            _glowRenderer.sprite = OrbitAssetFactory.GetSprite("glow_circle");
            _glowRenderer.color = new Color(_themeColor.r, _themeColor.g, _themeColor.b, 0.38f);
            _glowRenderer.sortingOrder = 1;
            haloGo.transform.localScale = Vector3.one * 1.45f;

            // 4. Pill Badge Label below orbit ring (matching GDD screenshots)
            string badgeText = GetBadgeString();
            if (!string.IsNullOrEmpty(badgeText))
            {
                GameObject labelGo = new GameObject("AnchorBadge");
                labelGo.transform.SetParent(transform, false);
                labelGo.transform.localPosition = new Vector3(0f, -orbitRadius - 0.55f, -0.5f);
                labelGo.transform.localScale = Vector3.one * 0.065f;

                GameObject bgGo = new GameObject("BadgeBg");
                bgGo.transform.SetParent(labelGo.transform, false);
                _labelBg = bgGo.AddComponent<SpriteRenderer>();
                _labelBg.sprite = OrbitAssetFactory.GetSprite("solid_disc");
                _labelBg.color = new Color(0.02f, 0.05f, 0.12f, 0.82f);
                _labelBg.sortingOrder = 8;
                bgGo.transform.localScale = new Vector3(badgeText.Length * 0.48f + 1.4f, 1.35f, 1f);

                _labelText = labelGo.AddComponent<TextMesh>();
                _labelText.text = badgeText;
                _labelText.fontSize = 32;
                _labelText.characterSize = 0.25f;
                _labelText.anchor = TextAnchor.MiddleCenter;
                _labelText.alignment = TextAlignment.Center;
                _labelText.fontStyle = FontStyle.Bold;
                _labelText.color = _themeColor;
                MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
                if (mr != null) mr.sortingOrder = 9;
            }
        }

        private string GetBadgeString()
        {
            if (isDecaying) return "DECAYING ORBIT";
            if (isPulsing) return "PULSAR";
            if (isMoving) return "MOVING PLANET";
            if (tierIndex >= 2) return direction > 0 ? "COUNTER-CLOCKWISE" : "CLOCKWISE";
            return "";
        }

        public void AddOrbitProgress(float deltaRadians, ProbeController probe)
        {
            AccumulatedOrbitRadians += Mathf.Abs(deltaRadians);
            if (!isDecaying || IsCollapsed) return;

            float spins = AccumulatedOrbitRadians / (Mathf.PI * 2f);
            float remainingRatio = Mathf.Clamp01(1f - (spins / maxSpinsBeforeCollapse));

            // Shrink planet body visually as it decays
            if (_planetVisual != null)
            {
                float baseScale = (planetRadius * 2f) / 2.08f;
                float currentScale = baseScale * Mathf.Lerp(0.35f, 1f, remainingRatio);
                _planetVisual.localScale = new Vector3(currentScale, currentScale, 1f);
            }

            if (spins >= maxSpinsBeforeCollapse)
            {
                CollapsePlanet(probe);
            }
        }

        private void CollapsePlanet(ProbeController probe)
        {
            if (IsCollapsed) return;
            IsCollapsed = true;
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnDeathShatter(transform.position, OrbitAssetFactory.AlertCrimson, OrbitAssetFactory.StarGold);
            }
            if (probe != null && probe.CurrentAnchor == this)
            {
                probe.Die("Crushed by collapsing binary star");
            }
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (IsCollapsed) return;

            // Rotate dashed orbit ring in orbit direction
            if (_orbitRingTransform != null)
            {
                float ringSpeed = 18f * direction * CurrentPulseMultiplier;
                _orbitRingTransform.Rotate(0f, 0f, ringSpeed * Time.deltaTime);
            }

            if (_planetVisual != null)
            {
                _planetVisual.Rotate(0f, 0f, 8f * direction * Time.deltaTime);
            }

            // Tier 4: Oscillating gravitational pulse
            if (isPulsing)
            {
                float wave = Mathf.Sin(Time.time * pulseFrequency + _phaseOffset);
                CurrentPulseMultiplier = Mathf.Lerp(0.75f, 1.42f, (wave + 1f) * 0.5f);
                if (_captureDiscRenderer != null)
                {
                    float pulseScale = (captureRadius * 2f / 2.4f) * Mathf.Lerp(0.92f, 1.12f, (wave + 1f) * 0.5f);
                    _captureDiscRenderer.transform.localScale = new Vector3(pulseScale, pulseScale, 1f);
                }
            }
            else
            {
                CurrentPulseMultiplier = 1.0f;
            }

            // Tier 3: Flash warning when decaying
            if (isDecaying && AccumulatedOrbitRadians > 0.01f && _orbitRingRenderer != null)
            {
                float spins = AccumulatedOrbitRadians / (Mathf.PI * 2f);
                float urgency = Mathf.Clamp01(spins / maxSpinsBeforeCollapse);
                float flash = Mathf.Sin(Time.time * Mathf.Lerp(6f, 22f, urgency)) * 0.5f + 0.5f;
                _orbitRingRenderer.color = Color.Lerp(_themeColor, OrbitAssetFactory.AlertCrimson, flash * urgency);
            }

            // Tier 5: Moving planet oscillation
            if (isMoving)
            {
                float offset = Mathf.Sin(Time.time * moveFrequency + _phaseOffset) * moveAmplitude;
                transform.position = _initialPosition + moveAxis * offset;
            }
        }
    }
}

