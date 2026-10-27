using UnityEngine;

namespace OrbitLauncher
{
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        public static CameraRig Instance { get; private set; }

        [Header("Follow & Framing")]
        public Transform targetProbe;
        public float smoothSpeed = 6.5f;
        public float baseOrthoSize = 8.2f;
        public float releaseZoomBoost = 1.15f;

        private Camera _cam;
        private Vector3 _basePosition = new Vector3(0f, 0f, -10f);
        private float _currentZoomBoost;

        // Screenshake state
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeMagnitude;

        // Parallax starfield layers
        private Transform[] _starElements;
        private Vector2[] _starBaseOffsets;
        private float[] _starParallaxes;
        private Transform[] _nebulaClouds;
        private Vector2[] _nebulaOffsets;
        private const int StarCount = 110;
        private const int NebulaCount = 8;
        private const float WrapSpan = 28f;

        private void Awake()
        {
            Instance = this;
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = OrbitAssetFactory.DeepVoidBlue;
            _cam.orthographicSize = baseOrthoSize;
            _basePosition = transform.position;

            CreateParallaxStarfield();
        }

        private void CreateParallaxStarfield()
        {
            GameObject root = new GameObject("ParallaxStarfield");
            Sprite glowSp = OrbitAssetFactory.GetSprite("glow_circle");
            Sprite discSp = OrbitAssetFactory.GetSprite("solid_disc");

            _starElements = new Transform[StarCount];
            _starBaseOffsets = new Vector2[StarCount];
            _starParallaxes = new float[StarCount];

            System.Random rng = new System.Random(777);
            for (int i = 0; i < StarCount; i++)
            {
                GameObject sGo = new GameObject("Star_" + i);
                sGo.transform.SetParent(root.transform, false);
                SpriteRenderer sr = sGo.AddComponent<SpriteRenderer>();
                sr.sprite = (i % 5 == 0) ? glowSp : discSp;
                sr.sortingOrder = -50;

                float depth = (float)rng.NextDouble();
                _starParallaxes[i] = Mathf.Lerp(0.12f, 0.28f, depth); // GDD: ~0.2x camera speed
                _starBaseOffsets[i] = new Vector2(
                    ((float)rng.NextDouble() - 0.5f) * WrapSpan,
                    ((float)rng.NextDouble() - 0.5f) * WrapSpan
                );

                float scale = (i % 5 == 0) ? Mathf.Lerp(0.08f, 0.22f, depth) : Mathf.Lerp(0.025f, 0.06f, depth);
                sGo.transform.localScale = Vector3.one * scale;

                Color starCol = (i % 7 == 0)
                    ? new Color(0.45f, 0.92f, 1f, Mathf.Lerp(0.25f, 0.75f, depth))
                    : new Color(0.85f, 0.92f, 1f, Mathf.Lerp(0.20f, 0.65f, depth));
                sr.color = starCol;
                _starElements[i] = sGo.transform;
            }

            _nebulaClouds = new Transform[NebulaCount];
            _nebulaOffsets = new Vector2[NebulaCount];
            Color[] nebulaColors =
            {
                new Color(0.0f, 0.55f, 0.85f, 0.06f),
                new Color(0.45f, 0.15f, 0.75f, 0.055f),
                new Color(0.0f, 0.75f, 0.55f, 0.05f),
                new Color(0.75f, 0.15f, 0.45f, 0.045f)
            };

            for (int i = 0; i < NebulaCount; i++)
            {
                GameObject nGo = new GameObject("Nebula_" + i);
                nGo.transform.SetParent(root.transform, false);
                SpriteRenderer sr = nGo.AddComponent<SpriteRenderer>();
                sr.sprite = glowSp;
                sr.sortingOrder = -60;
                sr.color = nebulaColors[i % nebulaColors.Length];
                float sc = Mathf.Lerp(5.5f, 9.5f, (float)rng.NextDouble());
                nGo.transform.localScale = Vector3.one * sc;
                _nebulaOffsets[i] = new Vector2(
                    ((float)rng.NextDouble() - 0.5f) * WrapSpan,
                    ((float)rng.NextDouble() - 0.5f) * WrapSpan
                );
                _nebulaClouds[i] = nGo.transform;
            }
        }

        public void TriggerReleaseZoom()
        {
            _currentZoomBoost = releaseZoomBoost;
        }

        public void TriggerShake(float duration = 0.2f, float magnitude = 0.4f)
        {
            _shakeDuration = Mathf.Max(0.01f, duration);
            _shakeTimer = _shakeDuration;
            _shakeMagnitude = magnitude;
        }

        public void SnapToTarget(Vector2 pos)
        {
            _basePosition = new Vector3(pos.x, pos.y, -10f);
            transform.position = _basePosition;
            UpdateStarfield();
        }

        private void LateUpdate()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsInMainMenu || GameManager.Instance.IsSplashActive))
            {
                Vector3 menuCenter = new Vector3(0f, 0f, -10f);
                _basePosition = Vector3.Lerp(_basePosition, menuCenter, smoothSpeed * Time.unscaledDeltaTime);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, 8.2f, 5f * Time.unscaledDeltaTime);
                transform.position = _basePosition;
                UpdateStarfield();
                return;
            }

            if (targetProbe != null)
            {
                Vector2 focusPos = targetProbe.position;
                float desiredOrtho = baseOrthoSize;

                // Frame both current/probe and the upcoming planet for clean readability
                if (WorldSpawner.Instance != null)
                {
                    GravityAnchor nextPlanet = WorldSpawner.Instance.GetNextTargetAnchor(focusPos);
                    if (nextPlanet != null)
                    {
                        Vector2 nextPos = nextPlanet.transform.position;
                        focusPos = Vector2.Lerp(focusPos, nextPos, 0.36f);
                        float dist = Vector2.Distance(targetProbe.position, nextPos);
                        desiredOrtho = Mathf.Max(baseOrthoSize, Mathf.Clamp(dist * 0.65f + 3.2f, baseOrthoSize, 11.5f));
                    }
                }

                // Adjust for portrait or narrow aspect ratios so horizontal margins stay visible
                float aspect = _cam.aspect;
                if (aspect < 0.75f)
                {
                    desiredOrtho *= Mathf.Clamp(0.75f / Mathf.Max(0.4f, aspect), 1f, 1.35f);
                }

                Vector3 desiredPos = new Vector3(focusPos.x, focusPos.y, -10f);
                _basePosition = Vector3.Lerp(_basePosition, desiredPos, smoothSpeed * Time.deltaTime);

                _currentZoomBoost = Mathf.MoveTowards(_currentZoomBoost, 0f, 2.8f * Time.deltaTime);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, desiredOrtho + _currentZoomBoost, 5f * Time.deltaTime);
            }

            Vector3 shakeOffset = Vector3.zero;
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.unscaledDeltaTime;
                float damper = Mathf.Clamp01(_shakeTimer / _shakeDuration);
                Vector2 rand = Random.insideUnitCircle * (_shakeMagnitude * damper);
                shakeOffset = new Vector3(rand.x, rand.y, 0f);
            }

            transform.position = _basePosition + shakeOffset;
            UpdateStarfield();
        }

        private void UpdateStarfield()
        {
            if (_starElements == null) return;
            Vector2 camPos = _basePosition;
            float halfSpan = WrapSpan * 0.5f;

            for (int i = 0; i < StarCount; i++)
            {
                if (_starElements[i] == null) continue;
                float p = _starParallaxes[i];
                float rx = RepeatSigned((_starBaseOffsets[i].x - camPos.x * (1f - p)), halfSpan);
                float ry = RepeatSigned((_starBaseOffsets[i].y - camPos.y * (1f - p)), halfSpan);
                _starElements[i].position = new Vector3(camPos.x + rx, camPos.y + ry, 5f);
            }

            for (int i = 0; i < NebulaCount; i++)
            {
                if (_nebulaClouds[i] == null) continue;
                float rx = RepeatSigned((_nebulaOffsets[i].x - camPos.x * 0.88f), halfSpan);
                float ry = RepeatSigned((_nebulaOffsets[i].y - camPos.y * 0.88f), halfSpan);
                _nebulaClouds[i].position = new Vector3(camPos.x + rx, camPos.y + ry, 8f);
            }
        }

        private static float RepeatSigned(float val, float halfSpan)
        {
            float span = halfSpan * 2f;
            float m = Mathf.Repeat(val + halfSpan, span);
            return m - halfSpan;
        }
    }
}

