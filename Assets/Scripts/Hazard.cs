using UnityEngine;

namespace OrbitLauncher
{
    public enum HazardType
    {
        Asteroid,
        LaserBarrier,
        SpinningDebris,
        MagneticMine,
        BlackHole
    }

    public class Hazard : MonoBehaviour
    {
        public HazardType hazardType = HazardType.Asteroid;
        public Vector2 driftAxis = Vector2.up;
        public float driftAmplitude = 1.2f;
        public float driftFrequency = 1.1f;
        public float rotationSpeed = 45f;
        public float pullRadius = 3.2f;
        public float pullStrength = 6.5f;

        private Vector2 _startPos;
        private float _phase;
        private Transform _visualRoot;
        private SpriteRenderer _glowRenderer;
        private LineRenderer _laserBeam;
        private BoxCollider2D _laserCollider;
        private bool _laserActive = true;

        public void Initialize(HazardType type, Vector2 position, Vector2 axis, float amplitude, float freq, float phase = 0f)
        {
            hazardType = type;
            _startPos = position;
            transform.position = position;
            driftAxis = axis.sqrMagnitude > 0.01f ? axis.normalized : Vector2.up;
            driftAmplitude = amplitude;
            driftFrequency = freq;
            _phase = phase;

            BuildHazardVisuals();
        }

        private void BuildHazardVisuals()
        {
            _visualRoot = new GameObject("HazardVisual").transform;
            _visualRoot.SetParent(transform, false);

            if (hazardType == HazardType.Asteroid || hazardType == HazardType.MagneticMine)
            {
                CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = hazardType == HazardType.MagneticMine ? 0.42f : 0.38f;

                SpriteRenderer sr = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = OrbitAssetFactory.GetSprite("hazard_asteroid");
                sr.sortingOrder = 5;
                float sc = hazardType == HazardType.MagneticMine ? 0.85f : 0.78f;
                _visualRoot.localScale = Vector3.one * sc;

                GameObject glowGo = new GameObject("CrimsonPulse");
                glowGo.transform.SetParent(_visualRoot, false);
                _glowRenderer = glowGo.AddComponent<SpriteRenderer>();
                _glowRenderer.sprite = OrbitAssetFactory.GetSprite("glow_circle");
                _glowRenderer.color = new Color(1f, 0.165f, 0.373f, 0.45f);
                _glowRenderer.sortingOrder = 4;
                glowGo.transform.localScale = Vector3.one * (hazardType == HazardType.MagneticMine ? 2.1f : 1.5f);

                if (hazardType == HazardType.MagneticMine)
                {
                    // Outer magnetic warning ring
                    GameObject magRing = new GameObject("MagneticFieldRing");
                    magRing.transform.SetParent(transform, false);
                    SpriteRenderer ringSr = magRing.AddComponent<SpriteRenderer>();
                    ringSr.sprite = OrbitAssetFactory.GetSprite("orbit_ring");
                    ringSr.color = new Color(1f, 0.165f, 0.373f, 0.42f);
                    ringSr.sortingOrder = 3;
                    magRing.transform.localScale = Vector3.one * 0.72f;
                    pullRadius = 2.4f;
                    pullStrength = 4.2f;
                }
            }
            else if (hazardType == HazardType.SpinningDebris)
            {
                rotationSpeed = 125f;
                // Ring of 3 orbiting crimson debris rocks with a gap
                GameObject ringGuide = new GameObject("DebrisTrack");
                ringGuide.transform.SetParent(_visualRoot, false);
                SpriteRenderer trackSr = ringGuide.AddComponent<SpriteRenderer>();
                trackSr.sprite = OrbitAssetFactory.GetSprite("orbit_ring");
                trackSr.color = new Color(1f, 0.165f, 0.373f, 0.45f);
                trackSr.sortingOrder = 3;
                ringGuide.transform.localScale = Vector3.one * 0.55f;

                float r = 1.25f;
                for (int i = 0; i < 2; i++)
                {
                    float ang = i * Mathf.PI;
                    GameObject rock = new GameObject("DebrisRock_" + i);
                    rock.transform.SetParent(_visualRoot, false);
                    rock.transform.localPosition = new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f);
                    rock.transform.localScale = Vector3.one * 0.62f;

                    SpriteRenderer sr = rock.AddComponent<SpriteRenderer>();
                    sr.sprite = OrbitAssetFactory.GetSprite("hazard_asteroid");
                    sr.sortingOrder = 5;

                    CircleCollider2D c = rock.AddComponent<CircleCollider2D>();
                    c.isTrigger = true;
                    c.radius = 0.42f;
                    HazardChildRelay relay = rock.AddComponent<HazardChildRelay>();
                    relay.parentHazard = this;
                }
            }
            else if (hazardType == HazardType.LaserBarrier)
            {
                float halfLen = 1.45f;
                _laserCollider = gameObject.AddComponent<BoxCollider2D>();
                _laserCollider.isTrigger = true;
                _laserCollider.size = new Vector2(0.24f, halfLen * 2f);

                // Beam LineRenderer
                _laserBeam = _visualRoot.gameObject.AddComponent<LineRenderer>();
                _laserBeam.useWorldSpace = false;
                _laserBeam.positionCount = 2;
                _laserBeam.SetPosition(0, new Vector3(0f, -halfLen, 0f));
                _laserBeam.SetPosition(1, new Vector3(0f, halfLen, 0f));
                _laserBeam.startWidth = 0.16f;
                _laserBeam.endWidth = 0.16f;
                _laserBeam.sharedMaterial = OrbitAssetFactory.GetDefaultSpriteMaterial();
                _laserBeam.startColor = OrbitAssetFactory.AlertCrimson;
                _laserBeam.endColor = OrbitAssetFactory.AlertCrimson;
                _laserBeam.sortingOrder = 5;

                // Two laser emitter pylons
                for (int i = -1; i <= 1; i += 2)
                {
                    GameObject pylon = new GameObject("LaserPylon");
                    pylon.transform.SetParent(_visualRoot, false);
                    pylon.transform.localPosition = new Vector3(0f, i * halfLen, 0f);
                    pylon.transform.localScale = Vector3.one * 0.48f;
                    SpriteRenderer psr = pylon.AddComponent<SpriteRenderer>();
                    psr.sprite = OrbitAssetFactory.GetSprite("hazard_asteroid");
                    psr.sortingOrder = 6;
                }
            }
            else if (hazardType == HazardType.BlackHole)
            {
                CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.72f;

                SpriteRenderer sr = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = OrbitAssetFactory.GetSprite("black_hole");
                sr.sortingOrder = 5;
                _visualRoot.localScale = Vector3.one * 1.45f;

                // Danger pull boundary ring
                GameObject pullRing = new GameObject("BlackHoleHorizonRing");
                pullRing.transform.SetParent(transform, false);
                _glowRenderer = pullRing.AddComponent<SpriteRenderer>();
                _glowRenderer.sprite = OrbitAssetFactory.GetSprite("orbit_ring");
                _glowRenderer.color = new Color(1f, 0.165f, 0.373f, 0.65f);
                _glowRenderer.sortingOrder = 3;
                pullRing.transform.localScale = Vector3.one * 1.35f;

                pullRadius = 3.6f;
                pullStrength = 12.5f;
                rotationSpeed = -65f;
            }
        }

        private void Update()
        {
            // Drift / wander motion
            if (driftAmplitude > 0.01f)
            {
                float offset = Mathf.Sin(Time.time * driftFrequency + _phase) * driftAmplitude;
                transform.position = _startPos + driftAxis * offset;
            }

            if (_visualRoot != null && hazardType != HazardType.LaserBarrier)
            {
                _visualRoot.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }

            // Pulse crimson glow
            if (_glowRenderer != null)
            {
                float pulse = 0.35f + 0.25f * (Mathf.Sin(Time.time * 6.5f + _phase) * 0.5f + 0.5f);
                Color c = _glowRenderer.color;
                c.a = pulse;
                _glowRenderer.color = c;
            }

            // Laser barrier rhythmic pulse
            if (hazardType == HazardType.LaserBarrier && _laserBeam != null)
            {
                float cycle = Mathf.Sin(Time.time * 2.2f + _phase);
                _laserActive = cycle > -0.35f;
                if (_laserCollider != null) _laserCollider.enabled = _laserActive;

                if (_laserActive)
                {
                    float w = 0.14f + 0.05f * Mathf.Sin(Time.time * 18f);
                    _laserBeam.startWidth = w;
                    _laserBeam.endWidth = w;
                    _laserBeam.startColor = OrbitAssetFactory.AlertCrimson;
                    _laserBeam.endColor = OrbitAssetFactory.AlertCrimson;
                }
                else
                {
                    _laserBeam.startWidth = 0.03f;
                    _laserBeam.endWidth = 0.03f;
                    Color warn = new Color(1f, 0.165f, 0.373f, 0.22f);
                    _laserBeam.startColor = warn;
                    _laserBeam.endColor = warn;
                }
            }

            // Gravitational / Magnetic pull on free-flying probe (Magnetic Mine & Black Hole)
            if ((hazardType == HazardType.BlackHole || hazardType == HazardType.MagneticMine) &&
                ProbeController.Instance != null &&
                ProbeController.Instance.CurrentState == ProbeState.FreeLaunch &&
                !ProbeController.Instance.IsWarpInvulnerable)
            {
                Vector2 probePos = ProbeController.Instance.transform.position;
                Vector2 toHazard = (Vector2)transform.position - probePos;
                float dist = toHazard.magnitude;
                if (dist < pullRadius && dist > 0.1f)
                {
                    float falloff = 1f - (dist / pullRadius);
                    ProbeController.Instance.ApplyExternalPull(toHazard.normalized * (pullStrength * falloff * Time.deltaTime));
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }

        public void HandleCollision(Collider2D other)
        {
            ProbeController probe = other.GetComponent<ProbeController>();
            if (probe == null) return;

            if (probe.IsWarpInvulnerable && hazardType != HazardType.BlackHole)
            {
                // Warp Invulnerability shatters hazards on contact!
                if (VFXManager.Instance != null)
                {
                    VFXManager.Instance.SpawnRadialSparks(transform.position, 28, OrbitAssetFactory.AlertCrimson, OrbitAssetFactory.NeonCyan, 5.5f, 0.45f);
                    VFXManager.Instance.SpawnFloatingPopup(transform.position, "WARP SMASH! +250", OrbitAssetFactory.NeonCyan, 0.95f);
                }
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(250);
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayShardPickup();
                }
                gameObject.SetActive(false);
                return;
            }

            string reason = "Probe destroyed by asteroid";
            switch (hazardType)
            {
                case HazardType.Asteroid: reason = "Probe destroyed by asteroid"; break;
                case HazardType.LaserBarrier: reason = "Vaporized by laser barrier"; break;
                case HazardType.SpinningDebris: reason = "Shattered by spinning orbital debris"; break;
                case HazardType.MagneticMine: reason = "Detonated by magnetic mine"; break;
                case HazardType.BlackHole: reason = "Swallowed by wandering black hole"; break;
            }
            probe.Die(reason);
        }
    }

    public class HazardChildRelay : MonoBehaviour
    {
        public Hazard parentHazard;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (parentHazard != null) parentHazard.HandleCollision(other);
        }
    }
}

