using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace OrbitLauncher
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Canvas _canvas;
        private Font _uiFont;

        // Splash Screen References
        private GameObject _splashPanel;
        private CanvasGroup _splashCanvasGroup;
        private readonly List<RectTransform> _splashPlanets = new List<RectTransform>();
        private readonly List<RectTransform> _splashPlanetRings = new List<RectTransform>();
        private readonly List<Vector2> _splashPlanetBasePos = new List<Vector2>();
        private readonly List<Vector2> _splashPlanetDriftVel = new List<Vector2>();
        private RectTransform _splashOrbitingProbe;
        private readonly List<RectTransform> _titleLetterRects = new List<RectTransform>();
        private readonly List<Text> _titleLetterTexts = new List<Text>();
        private readonly List<Vector2> _titleLetterBasePos = new List<Vector2>();
        private RectTransform _splashBarFill;
        private RectTransform _splashBarSpark;
        private Text _splashStatusText;
        private Text _splashPercentText;
        private bool _splashActive = true;

        // HUD References
        private RectTransform _scoreCardRt;
        private Text _scoreValueText;
        private float _displayedScore;
        private float _scorePunchTimer;

        private RectTransform _comboCardRt;
        private Text _comboValueText;
        private Text _comboHeaderText;
        private Image _comboBorderImage;
        private Image[] _warpPipImages;
        private float _comboPunchTimer;

        private RectTransform _shardCardRt;
        private Text _shardsValueText;
        private float _shardPunchTimer;

        private RectTransform _bottomPillRt;
        private Text _bottomPromptText;
        private Text _muteBtnText;

        // Animated Buttons for continuous pulse
        private readonly List<RectTransform> _pulsingButtons = new List<RectTransform>();

        // Tier Banner References
        private GameObject _tierBannerPanel;
        private CanvasGroup _tierBannerGroup;
        private RectTransform _tierBannerRt;
        private Text _tierBannerHeader;
        private Text _tierBannerTitle;
        private Text _tierBannerSubtitle;
        private Coroutine _bannerCoroutine;

        // Screens / Modals
        private GameObject _gameOverPanel;
        private RectTransform _gameOverCardRt;
        private Text _deathReasonText;
        private Text _goScoreText;
        private Text _goBestText;
        private Text _goShardsText;
        private Text _goTierText;

        private GameObject _pausePanel;
        private RectTransform _pauseCardRt;

        private GameObject _homePanel;
        private RectTransform _homeCardRt;
        private Text _homeTierBtnText;
        private Text _homeSeedBtnText;

        private GameObject _hullsPanel;
        private RectTransform _hullsCardRt;
        private Text _hullsShardBalanceText;
        private Button[] _hullActionButtons;
        private Text[] _hullActionLabels;

        private GameObject _leaderboardPanel;
        private RectTransform _leaderboardCardRt;
        private Text _leaderboardRowsText;

        private float _gameOverUnlockTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_uiFont == null) _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_uiFont == null) _uiFont = Font.CreateDynamicFontFromOSFont("Segoe UI", 24);

            EnsureEventSystem();
            BuildCompleteUI();
        }

        private void Start()
        {
            StartCoroutine(RunSplashSequence());
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGo.AddComponent<InputSystemUIInputModule>();
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
        }

        private void BuildCompleteUI()
        {
            GameObject canvasGo = new GameObject("OrbitLauncherCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            BuildGameplayHUD(canvasGo.transform);
            BuildTierBanner(canvasGo.transform);
            BuildGameOverScreen(canvasGo.transform);
            BuildPauseModal(canvasGo.transform);
            BuildHomeModal(canvasGo.transform);
            BuildHullsModal(canvasGo.transform);
            BuildLeaderboardModal(canvasGo.transform);
            BuildSplashScreen(canvasGo.transform);
        }

        private void BuildSplashScreen(Transform root)
        {
            _splashPanel = CreateFullScreenOverlay(root, "SplashScreenOverlay", OrbitAssetFactory.DeepVoidBlue);
            _splashCanvasGroup = _splashPanel.AddComponent<CanvasGroup>();

            // 1. Moving Background Planets & Orbiting Rings on Splash Screen
            string[] planetSprites = { "planet_cyan", "planet_purple", "planet_amber", "planet_emerald", "planet_crimson" };
            Color[] ringColors =
            {
                OrbitAssetFactory.NeonCyan,
                OrbitAssetFactory.PulsarPurple,
                OrbitAssetFactory.StarGold,
                OrbitAssetFactory.LimeGreen,
                OrbitAssetFactory.AlertCrimson
            };
            Vector2[] startPositions =
            {
                new Vector2(-340f, 520f),
                new Vector2(360f, 440f),
                new Vector2(-380f, -380f),
                new Vector2(330f, -460f),
                new Vector2(0f, 210f)
            };
            float[] planetSizes = { 210f, 175f, 165f, 190f, 250f };
            Vector2[] velocities =
            {
                new Vector2(28f, -18f),
                new Vector2(-24f, -22f),
                new Vector2(32f, 20f),
                new Vector2(-26f, 25f),
                new Vector2(16f, 14f)
            };

            for (int i = 0; i < planetSprites.Length; i++)
            {
                GameObject pContainer = new GameObject("SplashPlanet_" + i);
                pContainer.transform.SetParent(_splashPanel.transform, false);
                RectTransform pRt = pContainer.AddComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0.5f);
                pRt.anchorMax = new Vector2(0.5f, 0.5f);
                pRt.pivot = new Vector2(0.5f, 0.5f);
                pRt.anchoredPosition = startPositions[i];
                pRt.sizeDelta = new Vector2(planetSizes[i], planetSizes[i]);

                // Atmospheric Glow
                GameObject glowGo = new GameObject("Glow");
                glowGo.transform.SetParent(pContainer.transform, false);
                RectTransform glowRt = glowGo.AddComponent<RectTransform>();
                glowRt.anchorMin = new Vector2(0.5f, 0.5f);
                glowRt.anchorMax = new Vector2(0.5f, 0.5f);
                glowRt.sizeDelta = new Vector2(planetSizes[i] * 2.1f, planetSizes[i] * 2.1f);
                Image glowImg = glowGo.AddComponent<Image>();
                glowImg.sprite = OrbitAssetFactory.GetSprite("glow_circle");
                glowImg.color = new Color(ringColors[i].r, ringColors[i].g, ringColors[i].b, 0.25f);

                // Dashed Orbit Ring
                GameObject ringGo = new GameObject("OrbitRing");
                ringGo.transform.SetParent(pContainer.transform, false);
                RectTransform ringRt = ringGo.AddComponent<RectTransform>();
                ringRt.anchorMin = new Vector2(0.5f, 0.5f);
                ringRt.anchorMax = new Vector2(0.5f, 0.5f);
                ringRt.sizeDelta = new Vector2(planetSizes[i] * 1.75f, planetSizes[i] * 1.75f);
                Image ringImg = ringGo.AddComponent<Image>();
                ringImg.sprite = OrbitAssetFactory.GetSprite("orbit_ring");
                ringImg.color = new Color(ringColors[i].r, ringColors[i].g, ringColors[i].b, 0.65f);

                // Planet Body
                GameObject bodyGo = new GameObject("Body");
                bodyGo.transform.SetParent(pContainer.transform, false);
                RectTransform bodyRt = bodyGo.AddComponent<RectTransform>();
                bodyRt.anchorMin = new Vector2(0.5f, 0.5f);
                bodyRt.anchorMax = new Vector2(0.5f, 0.5f);
                bodyRt.sizeDelta = new Vector2(planetSizes[i], planetSizes[i]);
                Image bodyImg = bodyGo.AddComponent<Image>();
                bodyImg.sprite = OrbitAssetFactory.GetSprite(planetSprites[i]);

                _splashPlanets.Add(pRt);
                _splashPlanetRings.Add(ringRt);
                _splashPlanetBasePos.Add(startPositions[i]);
                _splashPlanetDriftVel.Add(velocities[i]);

                // Attach an orbiting Scout Probe around the centerpiece planet (index 4)
                if (i == 4)
                {
                    GameObject probeGo = new GameObject("SplashOrbitingProbe");
                    probeGo.transform.SetParent(pContainer.transform, false);
                    _splashOrbitingProbe = probeGo.AddComponent<RectTransform>();
                    _splashOrbitingProbe.anchorMin = new Vector2(0.5f, 0.5f);
                    _splashOrbitingProbe.anchorMax = new Vector2(0.5f, 0.5f);
                    _splashOrbitingProbe.sizeDelta = new Vector2(54f, 54f);
                    Image prImg = probeGo.AddComponent<Image>();
                    prImg.sprite = OrbitAssetFactory.GetSprite("hull_scout");
                }
            }

            // 2. Center Translucent Backing Badge for Title & Loading Bar
            GameObject centerPanel = CreateStyledCard(_splashPanel.transform, "SplashCenterCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(920f, 520f),
                new Color(0.02f, 0.045f, 0.11f, 0.84f), new Color(0f, 0.94f, 1f, 0.45f));

            CreateText(centerPanel.transform, "StudioBadge", "COSMIC ARCADE EDITION • V1.1", 20, FontStyle.Bold,
                OrbitAssetFactory.LimeGreen, TextAnchor.UpperCenter,
                new Vector2(0f, -34f), new Vector2(780f, 32f), true);

            // 3. Per-Letter Animated Game Title: "ORBIT LAUNCHER"
            string titleStr = "ORBIT LAUNCHER";
            float letterSpacing = 56f;
            float startX = -(titleStr.Length - 1) * letterSpacing * 0.5f;

            GameObject titleRow = new GameObject("AnimatedTitleLetters");
            titleRow.transform.SetParent(centerPanel.transform, false);
            RectTransform titleRowRt = titleRow.AddComponent<RectTransform>();
            titleRowRt.anchorMin = new Vector2(0.5f, 1f);
            titleRowRt.anchorMax = new Vector2(0.5f, 1f);
            titleRowRt.anchoredPosition = new Vector2(0f, -130f);
            titleRowRt.sizeDelta = new Vector2(880f, 100f);

            for (int i = 0; i < titleStr.Length; i++)
            {
                char ch = titleStr[i];
                GameObject letterGo = new GameObject("Letter_" + i + "_" + ch);
                letterGo.transform.SetParent(titleRow.transform, false);
                RectTransform lRt = letterGo.AddComponent<RectTransform>();
                lRt.anchorMin = new Vector2(0.5f, 0.5f);
                lRt.anchorMax = new Vector2(0.5f, 0.5f);
                lRt.pivot = new Vector2(0.5f, 0.5f);
                Vector2 bPos = new Vector2(startX + i * letterSpacing, 0f);
                lRt.anchoredPosition = bPos;
                lRt.sizeDelta = new Vector2(68f, 96f);
                lRt.localScale = Vector3.zero; // Starts scaled 0 for staggered pop-in!

                Text lTxt = letterGo.AddComponent<Text>();
                lTxt.font = _uiFont;
                lTxt.text = ch.ToString();
                lTxt.fontSize = 68;
                lTxt.fontStyle = FontStyle.Bold;
                lTxt.alignment = TextAnchor.MiddleCenter;
                lTxt.color = i < 5 ? OrbitAssetFactory.NeonCyan : Color.white;
                lTxt.raycastTarget = false;

                Outline ol = letterGo.AddComponent<Outline>();
                ol.effectColor = new Color(0f, 0.94f, 1f, 0.55f);
                ol.effectDistance = new Vector2(2f, -2f);

                _titleLetterRects.Add(lRt);
                _titleLetterTexts.Add(lTxt);
                _titleLetterBasePos.Add(bPos);
            }

            CreateText(centerPanel.transform, "SplashSubtitle", "TOP-DOWN ORBITAL HOPPER", 26, FontStyle.Bold,
                new Color(0.75f, 0.90f, 1f, 0.92f), TextAnchor.UpperCenter,
                new Vector2(0f, -210f), new Vector2(780f, 40f), true);

            // 4. High-Tech Animated Loading Bar
            GameObject barBg = CreateStyledCard(centerPanel.transform, "LoadingBarTrack",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 130f), new Vector2(720f, 32f),
                new Color(0.01f, 0.02f, 0.06f, 0.95f), new Color(0f, 0.94f, 1f, 0.55f));

            GameObject fillGo = new GameObject("LoadingBarFill");
            fillGo.transform.SetParent(barBg.transform, false);
            _splashBarFill = fillGo.AddComponent<RectTransform>();
            _splashBarFill.anchorMin = new Vector2(0f, 0f);
            _splashBarFill.anchorMax = new Vector2(0f, 1f);
            _splashBarFill.pivot = new Vector2(0f, 0.5f);
            _splashBarFill.anchoredPosition = new Vector2(4f, 0f);
            _splashBarFill.sizeDelta = new Vector2(0f, -8f);
            Image fillImg = fillGo.AddComponent<Image>();
            fillImg.color = OrbitAssetFactory.LimeGreen;

            // Glowing spark at tip of loading bar
            GameObject sparkGo = new GameObject("LoadingTipSpark");
            sparkGo.transform.SetParent(fillGo.transform, false);
            _splashBarSpark = sparkGo.AddComponent<RectTransform>();
            _splashBarSpark.anchorMin = new Vector2(1f, 0.5f);
            _splashBarSpark.anchorMax = new Vector2(1f, 0.5f);
            _splashBarSpark.pivot = new Vector2(0.5f, 0.5f);
            _splashBarSpark.anchoredPosition = Vector2.zero;
            _splashBarSpark.sizeDelta = new Vector2(68f, 68f);
            Image sparkImg = sparkGo.AddComponent<Image>();
            sparkImg.sprite = OrbitAssetFactory.GetSprite("glow_circle");
            sparkImg.color = OrbitAssetFactory.NeonCyan;

            _splashStatusText = CreateText(centerPanel.transform, "SplashStatus", "INITIALIZING GRAVITY WELLS...", 21, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.MiddleLeft,
                new Vector2(100f, -395f), new Vector2(560f, 36f));

            _splashPercentText = CreateText(centerPanel.transform, "SplashPercent", "0%", 24, FontStyle.Bold,
                OrbitAssetFactory.LimeGreen, TextAnchor.MiddleRight,
                new Vector2(620f, -395f), new Vector2(200f, 36f));
        }

        private IEnumerator RunSplashSequence()
        {
            _splashActive = true;
            _splashPanel.SetActive(true);
            _splashCanvasGroup.alpha = 1f;

            string[] stages =
            {
                "INITIALIZING GRAVITY WELLS...",
                "CALIBRATING ORBITAL TANGENTS...",
                "LOADING FIVE COSMIC SECTORS...",
                "SYNTHESIZING PENTATONIC RESONANCE...",
                "SYSTEMS NOMINAL • LAUNCHING PROBE!"
            };

            float duration = 3.2f;
            float elapsed = 0f;
            int lastStageIdx = -1;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth ease-in-out progress curve
                float prog = Mathf.SmoothStep(0f, 1f, t);

                if (_splashBarFill != null)
                {
                    _splashBarFill.sizeDelta = new Vector2(712f * prog, -8f);
                }
                if (_splashPercentText != null)
                {
                    _splashPercentText.text = Mathf.RoundToInt(prog * 100f) + "%";
                }

                int stageIdx = Mathf.Clamp(Mathf.FloorToInt(prog * stages.Length), 0, stages.Length - 1);
                if (stageIdx != lastStageIdx)
                {
                    lastStageIdx = stageIdx;
                    if (_splashStatusText != null) _splashStatusText.text = stages[stageIdx];
                    if (AudioManager.Instance != null && stageIdx > 0)
                    {
                        AudioManager.Instance.PlayCaptureChime(stageIdx - 1);
                    }
                }

                yield return null;
            }

            if (_splashPercentText != null) _splashPercentText.text = "100%";
            if (_splashStatusText != null) _splashStatusText.text = "SYSTEMS NOMINAL • LAUNCHING PROBE!";

            yield return new WaitForSecondsRealtime(0.35f);

            // Smooth fade-out & zoom transition into gameplay
            float fadeDur = 0.45f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeDur)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                float ft = Mathf.Clamp01(fadeElapsed / fadeDur);
                _splashCanvasGroup.alpha = 1f - ft;
                _splashPanel.transform.localScale = Vector3.one * (1f + ft * 0.08f);
                yield return null;
            }

            _splashActive = false;
            _splashPanel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSplashCompleted();
            }
        }

        private void UpdateSplashAnimations()
        {
            if (!_splashActive || _splashPanel == null) return;

            float time = Time.unscaledTime;

            // 1. Move & float background planets + rotate their dashed orbit rings
            for (int i = 0; i < _splashPlanets.Count; i++)
            {
                RectTransform pRt = _splashPlanets[i];
                if (pRt == null) continue;

                Vector2 baseP = _splashPlanetBasePos[i];
                Vector2 vel = _splashPlanetDriftVel[i];
                float offsetX = Mathf.Sin(time * 0.7f + i * 1.4f) * 48f + Mathf.Cos(time * 0.35f + i) * vel.x;
                float offsetY = Mathf.Cos(time * 0.6f + i * 1.1f) * 38f + Mathf.Sin(time * 0.4f + i) * vel.y;
                pRt.anchoredPosition = baseP + new Vector2(offsetX, offsetY);

                float scaleBreath = 1f + 0.04f * Mathf.Sin(time * 1.8f + i);
                pRt.localScale = new Vector3(scaleBreath, scaleBreath, 1f);

                if (i < _splashPlanetRings.Count && _splashPlanetRings[i] != null)
                {
                    float dir = (i % 2 == 0) ? -1f : 1f;
                    _splashPlanetRings[i].Rotate(0f, 0f, dir * 28f * Time.unscaledDeltaTime);
                }
            }

            // Orbit the scout probe around the center splash planet
            if (_splashOrbitingProbe != null)
            {
                float orbitAng = -time * 2.4f;
                float orbitR = 218f;
                _splashOrbitingProbe.anchoredPosition = new Vector2(Mathf.Cos(orbitAng) * orbitR, Mathf.Sin(orbitAng) * orbitR);
                Vector2 tangent = new Vector2(Mathf.Sin(orbitAng), -Mathf.Cos(orbitAng));
                float rotZ = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
                _splashOrbitingProbe.localRotation = Quaternion.Euler(0f, 0f, rotZ);
            }

            // 2. Staggered Per-Letter Pop-In, Wave Float & Neon Shimmer on "ORBIT LAUNCHER"
            for (int i = 0; i < _titleLetterRects.Count; i++)
            {
                RectTransform lRt = _titleLetterRects[i];
                if (lRt == null) continue;

                float delay = i * 0.055f;
                float letterTime = Mathf.Max(0f, time - delay);
                float popT = Mathf.Clamp01(letterTime / 0.38f);
                // Elastic overshoot curve
                float elasticScale = popT < 1f
                    ? Mathf.Sin(popT * Mathf.PI * 0.5f) + Mathf.Sin(popT * Mathf.PI) * 0.28f
                    : 1f + 0.06f * Mathf.Sin(time * 4.2f - i * 0.45f);

                float waveY = Mathf.Sin(time * 4.2f - i * 0.45f) * 9f * popT;
                lRt.anchoredPosition = _titleLetterBasePos[i] + new Vector2(0f, waveY);
                lRt.localScale = new Vector3(elasticScale, elasticScale, 1f);

                if (i < _titleLetterTexts.Count && _titleLetterTexts[i] != null)
                {
                    float shimmer = Mathf.Sin(time * 3.5f - i * 0.35f) * 0.5f + 0.5f;
                    Color baseCol = i < 5 ? OrbitAssetFactory.NeonCyan : Color.white;
                    _titleLetterTexts[i].color = Color.Lerp(baseCol, OrbitAssetFactory.LimeGreen, shimmer * 0.45f);
                }
            }

            // 3. Pulse loading tip spark
            if (_splashBarSpark != null)
            {
                float s = 0.85f + 0.3f * Mathf.Sin(time * 14f);
                _splashBarSpark.localScale = new Vector3(s, s, 1f);
            }
        }

        private void BuildGameplayHUD(Transform root)
        {
            // 1. Top-Left SCORE Box
            GameObject scoreCard = CreateStyledCard(root, "ScoreCard",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -32f), new Vector2(275f, 118f),
                new Color(0.03f, 0.06f, 0.14f, 0.86f), new Color(0f, 0.94f, 1f, 0.45f));
            _scoreCardRt = scoreCard.GetComponent<RectTransform>();

            CreateText(scoreCard.transform, "ScoreLabel", "SCORE", 20, FontStyle.Bold,
                new Color(0.55f, 0.78f, 0.95f, 0.9f), TextAnchor.UpperLeft,
                new Vector2(22f, -14f), new Vector2(220f, 30f));

            _scoreValueText = CreateText(scoreCard.transform, "ScoreValue", "0", 48, FontStyle.Bold,
                Color.white, TextAnchor.LowerLeft,
                new Vector2(22f, -40f), new Vector2(240f, 66f));

            // 2. Top-Center STARLIGHT COMBO Box
            GameObject comboCard = CreateStyledCard(root, "ComboCard",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(320f, 118f),
                new Color(0.03f, 0.06f, 0.14f, 0.86f), new Color(0f, 0.94f, 1f, 0.45f));
            _comboCardRt = comboCard.GetComponent<RectTransform>();
            _comboBorderImage = comboCard.GetComponent<Image>();

            _comboHeaderText = CreateText(comboCard.transform, "ComboLabel", "STARLIGHT COMBO", 18, FontStyle.Bold,
                OrbitAssetFactory.LimeGreen, TextAnchor.UpperCenter,
                new Vector2(0f, -12f), new Vector2(290f, 26f), true);

            _comboValueText = CreateText(comboCard.transform, "ComboValue", "×1", 44, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0f, -34f), new Vector2(260f, 54f), true);

            // 5 Warp Invulnerability Charge Pips
            _warpPipImages = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject pip = new GameObject("WarpPip_" + i);
                pip.transform.SetParent(comboCard.transform, false);
                RectTransform rt = pip.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2((i - 2) * 42f, 15f);
                rt.sizeDelta = new Vector2(34f, 7f);
                Image img = pip.AddComponent<Image>();
                img.color = new Color(0.15f, 0.25f, 0.38f, 0.8f);
                _warpPipImages[i] = img;
            }

            // 3. Top-Right STAR SHARDS Pill + Mute + Pause Button
            GameObject shardCard = CreateStyledCard(root, "ShardCard",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-190f, -32f), new Vector2(170f, 76f),
                new Color(0.03f, 0.06f, 0.14f, 0.86f), new Color(1f, 0.84f, 0.25f, 0.45f));
            _shardCardRt = shardCard.GetComponent<RectTransform>();

            GameObject shardIcon = new GameObject("ShardIcon");
            shardIcon.transform.SetParent(shardCard.transform, false);
            RectTransform sIconRt = shardIcon.AddComponent<RectTransform>();
            sIconRt.anchorMin = new Vector2(0f, 0.5f);
            sIconRt.anchorMax = new Vector2(0f, 0.5f);
            sIconRt.pivot = new Vector2(0.5f, 0.5f);
            sIconRt.anchoredPosition = new Vector2(34f, 0f);
            sIconRt.sizeDelta = new Vector2(40f, 40f);
            Image sIconImg = shardIcon.AddComponent<Image>();
            sIconImg.sprite = OrbitAssetFactory.GetSprite("star_shard");

            _shardsValueText = CreateText(shardCard.transform, "ShardsValue", "30", 34, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(62f, -12f), new Vector2(100f, 52f));

            // Pause Button
            CreateStyledButton(root, "PauseBtn", "||", 30,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -32f), new Vector2(76f, 76f),
                new Color(0.04f, 0.09f, 0.20f, 0.9f), OrbitAssetFactory.NeonCyan,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    if (GameManager.Instance != null) GameManager.Instance.TogglePause();
                });

            // Mute Button
            Button muteBtn = CreateStyledButton(root, "MuteBtn", "SFX", 18,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-102f, -32f), new Vector2(76f, 76f),
                new Color(0.04f, 0.09f, 0.20f, 0.9f), new Color(0.6f, 0.85f, 1f, 0.85f),
                () =>
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.ToggleMute();
                        UpdateMuteButtonLabel();
                    }
                });
            _muteBtnText = muteBtn.GetComponentInChildren<Text>();
            UpdateMuteButtonLabel();

            // Quick Hangar & Tiers buttons
            CreateStyledButton(root, "QuickHullsBtn", "HULLS", 18,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -162f), new Vector2(130f, 52f),
                new Color(0.04f, 0.09f, 0.20f, 0.85f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    OpenHullsModal();
                });

            CreateStyledButton(root, "QuickMenuBtn", "TIERS", 18,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, -162f), new Vector2(130f, 52f),
                new Color(0.04f, 0.09f, 0.20f, 0.85f), OrbitAssetFactory.NeonCyan,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    OpenHomeModal();
                });

            // 4. Bottom-Center "TAP ANYWHERE TO RELEASE" Pill
            GameObject bottomPill = CreateStyledCard(root, "BottomPromptPill",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 54f), new Vector2(480f, 64f),
                new Color(0.02f, 0.05f, 0.12f, 0.82f), new Color(0f, 0.94f, 1f, 0.45f));
            _bottomPillRt = bottomPill.GetComponent<RectTransform>();

            _bottomPromptText = CreateText(bottomPill.transform, "PromptText", "TAP ANYWHERE TO RELEASE", 22, FontStyle.Bold,
                new Color(0.85f, 0.96f, 1f, 0.95f), TextAnchor.MiddleCenter,
                new Vector2(0f, -8f), new Vector2(460f, 48f), true);
        }

        private void BuildTierBanner(Transform root)
        {
            _tierBannerPanel = CreateStyledCard(root, "TierBannerPanel",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -235f), new Vector2(680f, 132f),
                new Color(0.02f, 0.05f, 0.13f, 0.92f), OrbitAssetFactory.NeonCyan);
            _tierBannerRt = _tierBannerPanel.GetComponent<RectTransform>();
            _tierBannerGroup = _tierBannerPanel.AddComponent<CanvasGroup>();
            _tierBannerGroup.alpha = 0f;
            _tierBannerGroup.blocksRaycasts = false;

            _tierBannerHeader = CreateText(_tierBannerPanel.transform, "TierHeader", "TIER 1 • COSMIC SECTOR", 18, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.UpperCenter,
                new Vector2(0f, -12f), new Vector2(640f, 26f), true);

            _tierBannerTitle = CreateText(_tierBannerPanel.transform, "TierTitle", "NEBULA", 38, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0f, -38f), new Vector2(640f, 46f), true);

            _tierBannerSubtitle = CreateText(_tierBannerPanel.transform, "TierSub", "Static planets, open space. Learn the timing.", 20, FontStyle.Normal,
                new Color(0.75f, 0.88f, 1f, 0.92f), TextAnchor.LowerCenter,
                new Vector2(0f, -86f), new Vector2(640f, 34f), true);
        }

        private void BuildGameOverScreen(Transform root)
        {
            _gameOverPanel = CreateFullScreenOverlay(root, "GameOverScreen", new Color(0.015f, 0.025f, 0.065f, 0.88f));

            GameObject card = CreateStyledCard(_gameOverPanel.transform, "SignalLostCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 880f),
                new Color(0.03f, 0.055f, 0.13f, 0.96f), new Color(1f, 0.165f, 0.373f, 0.55f));
            _gameOverCardRt = card.GetComponent<RectTransform>();

            CreateText(card.transform, "SignalLostTitle", "SIGNAL LOST", 64, FontStyle.Bold,
                Color.white, TextAnchor.UpperCenter,
                new Vector2(0f, -48f), new Vector2(680f, 80f), true);

            _deathReasonText = CreateText(card.transform, "DeathReason", "Probe destroyed by asteroid", 26, FontStyle.Normal,
                OrbitAssetFactory.AlertCrimson, TextAnchor.UpperCenter,
                new Vector2(0f, -130f), new Vector2(680f, 40f), true);

            GameObject statsBox = CreateStyledCard(card.transform, "StatsBox",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(620f, 320f),
                new Color(0.018f, 0.035f, 0.09f, 0.95f), new Color(0f, 0.94f, 1f, 0.25f));

            _goScoreText = CreateStatRow(statsBox.transform, "SCORE", "0", -28f, Color.white);
            _goBestText = CreateStatRow(statsBox.transform, "BEST", "0", -104f, OrbitAssetFactory.StarGold);
            _goShardsText = CreateStatRow(statsBox.transform, "STAR SHARDS", "+0", -180f, OrbitAssetFactory.NeonCyan);
            _goTierText = CreateStatRow(statsBox.transform, "SECTOR", "Tier 1 • Nebula", -252f, OrbitAssetFactory.LimeGreen);

            Button retryBtn = CreateStyledButton(card.transform, "TapToRetryBtn", "TAP TO RETRY", 34,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 175f), new Vector2(540f, 96f),
                new Color(0.02f, 0.28f, 0.22f, 0.96f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    if (GameManager.Instance != null) GameManager.Instance.StartNewRun(true);
                });
            _pulsingButtons.Add(retryBtn.GetComponent<RectTransform>());

            CreateStyledButton(card.transform, "GoHomeBtn", "Home", 24,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 65f), new Vector2(165f, 64f),
                new Color(0.05f, 0.10f, 0.22f, 0.9f), OrbitAssetFactory.NeonCyan,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    OpenHomeModal();
                });

            CreateStyledButton(card.transform, "GoHullsBtn", "Hulls", 24,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(165f, 64f),
                new Color(0.05f, 0.10f, 0.22f, 0.9f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    OpenHullsModal();
                });

            CreateStyledButton(card.transform, "GoLeaderboardBtn", "Leaderboard", 24,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 65f), new Vector2(185f, 64f),
                new Color(0.05f, 0.10f, 0.22f, 0.9f), OrbitAssetFactory.StarGold,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    OpenLeaderboardModal();
                });

            _gameOverPanel.SetActive(false);
        }

        private Text CreateStatRow(Transform parent, string label, string initialValue, float yOffset, Color valueColor)
        {
            CreateText(parent, label + "_Lbl", label, 24, FontStyle.Bold,
                new Color(0.55f, 0.72f, 0.90f, 0.9f), TextAnchor.MiddleLeft,
                new Vector2(36f, yOffset), new Vector2(280f, 54f));

            return CreateText(parent, label + "_Val", initialValue, 34, FontStyle.Bold,
                valueColor, TextAnchor.MiddleRight,
                new Vector2(300f, yOffset), new Vector2(284f, 54f));
        }

        private void BuildPauseModal(Transform root)
        {
            _pausePanel = CreateFullScreenOverlay(root, "PauseModal", new Color(0.01f, 0.02f, 0.06f, 0.85f));
            GameObject card = CreateStyledCard(_pausePanel.transform, "PauseCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 540f),
                new Color(0.03f, 0.06f, 0.14f, 0.96f), OrbitAssetFactory.NeonCyan);
            _pauseCardRt = card.GetComponent<RectTransform>();

            CreateText(card.transform, "PauseTitle", "SYSTEM PAUSED", 46, FontStyle.Bold,
                Color.white, TextAnchor.UpperCenter,
                new Vector2(0f, -44f), new Vector2(560f, 64f), true);

            CreateStyledButton(card.transform, "ResumeBtn", "RESUME FLIGHT", 28,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(420f, 82f),
                new Color(0.02f, 0.26f, 0.22f, 0.95f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    if (GameManager.Instance != null) GameManager.Instance.TogglePause();
                });

            CreateStyledButton(card.transform, "RestartBtn", "RESTART RUN", 26,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(420f, 76f),
                new Color(0.05f, 0.11f, 0.24f, 0.95f), OrbitAssetFactory.NeonCyan,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _pausePanel.SetActive(false);
                    if (GameManager.Instance != null) GameManager.Instance.StartNewRun(true);
                });

            CreateStyledButton(card.transform, "PauseHomeBtn", "SECTOR & MODE SELECT", 24,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(420f, 72f),
                new Color(0.05f, 0.11f, 0.24f, 0.95f), OrbitAssetFactory.StarGold,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _pausePanel.SetActive(false);
                    OpenHomeModal();
                });

            _pausePanel.SetActive(false);
        }

        private void BuildHomeModal(Transform root)
        {
            _homePanel = CreateFullScreenOverlay(root, "HomeModal", new Color(0.01f, 0.02f, 0.06f, 0.90f));
            GameObject card = CreateStyledCard(_homePanel.transform, "HomeCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 940f),
                new Color(0.03f, 0.06f, 0.14f, 0.98f), OrbitAssetFactory.NeonCyan);
            _homeCardRt = card.GetComponent<RectTransform>();

            CreateText(card.transform, "GameLogo", "ORBIT LAUNCHER", 58, FontStyle.Bold,
                Color.white, TextAnchor.UpperCenter,
                new Vector2(0f, -45f), new Vector2(720f, 72f), true);

            CreateText(card.transform, "GameSub", "TOP-DOWN ORBITAL HOPPER • FIVE COSMIC TIERS", 22, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.UpperCenter,
                new Vector2(0f, -118f), new Vector2(720f, 36f), true);

            CreateText(card.transform, "HowToPlay",
                "1. Ride a planet's gravity orbit.\n2. Wait until the dotted line turns LIME and points at the next planet.\n3. Tap / Space / Click to slingshot across deep space!\n4. Chain 5 Perfect Hops for a 3.5s WARP INVULNERABILITY SHIELD!",
                22, FontStyle.Normal, new Color(0.80f, 0.90f, 1f, 0.92f), TextAnchor.MiddleCenter,
                new Vector2(0f, -185f), new Vector2(680f, 160f), true);

            Button tierBtn = CreateStyledButton(card.transform, "TierCycleBtn", "START TIER: TIER 1 • NEBULA", 26,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(580f, 80f),
                new Color(0.04f, 0.11f, 0.24f, 0.95f), OrbitAssetFactory.NeonCyan,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    if (GameManager.Instance != null)
                    {
                        int next = (GameManager.Instance.SelectedStartingTier % 5) + 1;
                        GameManager.Instance.SetStartingTier(next);
                        RefreshHomeModal();
                    }
                });
            _homeTierBtnText = tierBtn.GetComponentInChildren<Text>();

            Button seedBtn = CreateStyledButton(card.transform, "SeedModeBtn", "MODE: PROCEDURAL COSMIC VOID", 24,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -68f), new Vector2(580f, 76f),
                new Color(0.04f, 0.11f, 0.24f, 0.95f), OrbitAssetFactory.StarGold,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ToggleDailySeedMode();
                        RefreshHomeModal();
                    }
                });
            _homeSeedBtnText = seedBtn.GetComponentInChildren<Text>();

            Button launchBtn = CreateStyledButton(card.transform, "LaunchNowBtn", "LAUNCH MISSION", 34,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 195f), new Vector2(580f, 96f),
                new Color(0.02f, 0.28f, 0.22f, 0.96f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _homePanel.SetActive(false);
                    if (GameManager.Instance != null) GameManager.Instance.StartNewRun(true);
                });
            _pulsingButtons.Add(launchBtn.GetComponent<RectTransform>());

            CreateStyledButton(card.transform, "HomeHullsBtn", "PROBE HULLS", 24,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 80f), new Vector2(270f, 70f),
                new Color(0.05f, 0.10f, 0.22f, 0.95f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _homePanel.SetActive(false);
                    OpenHullsModal();
                });

            CreateStyledButton(card.transform, "HomeLbBtn", "LEADERBOARD", 24,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 80f), new Vector2(270f, 70f),
                new Color(0.05f, 0.10f, 0.22f, 0.95f), OrbitAssetFactory.StarGold,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _homePanel.SetActive(false);
                    OpenLeaderboardModal();
                });

            _homePanel.SetActive(false);
        }

        private void BuildHullsModal(Transform root)
        {
            _hullsPanel = CreateFullScreenOverlay(root, "HullsModal", new Color(0.01f, 0.02f, 0.06f, 0.92f));
            GameObject card = CreateStyledCard(_hullsPanel.transform, "HullsCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 1120f),
                new Color(0.03f, 0.06f, 0.14f, 0.98f), OrbitAssetFactory.LimeGreen);
            _hullsCardRt = card.GetComponent<RectTransform>();

            CreateText(card.transform, "HullsHeader", "PROBE HULL HANGAR", 46, FontStyle.Bold,
                Color.white, TextAnchor.UpperCenter,
                new Vector2(0f, -36f), new Vector2(740f, 60f), true);

            _hullsShardBalanceText = CreateText(card.transform, "ShardBalance", "AVAILABLE STAR SHARDS: ◆ 30", 26, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.UpperCenter,
                new Vector2(0f, -98f), new Vector2(740f, 40f), true);

            _hullActionButtons = new Button[OrbitAssetFactory.Hulls.Length];
            _hullActionLabels = new Text[OrbitAssetFactory.Hulls.Length];

            for (int i = 0; i < OrbitAssetFactory.Hulls.Length; i++)
            {
                int hullIdx = i;
                HullDefinition def = OrbitAssetFactory.Hulls[i];
                float yPos = 240f - i * 195f;

                GameObject row = CreateStyledCard(card.transform, "HullRow_" + i,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, yPos), new Vector2(740f, 172f),
                    new Color(0.02f, 0.04f, 0.10f, 0.95f), def.primaryColor);

                GameObject iconGo = new GameObject("HullSprite");
                iconGo.transform.SetParent(row.transform, false);
                RectTransform iconRt = iconGo.AddComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(82f, 0f);
                iconRt.sizeDelta = new Vector2(110f, 110f);
                Image iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = OrbitAssetFactory.GetSprite(def.spriteResource);

                CreateText(row.transform, "HullName", def.name, 28, FontStyle.Bold,
                    def.primaryColor, TextAnchor.UpperLeft,
                    new Vector2(158f, -24f), new Vector2(340f, 38f));

                CreateText(row.transform, "HullDesc", def.description, 20, FontStyle.Normal,
                    new Color(0.78f, 0.88f, 1f, 0.9f), TextAnchor.UpperLeft,
                    new Vector2(158f, -66f), new Vector2(360f, 80f));

                Button actionBtn = CreateStyledButton(row.transform, "HullBtn_" + i, "EQUIP", 22,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-115f, 0f), new Vector2(190f, 74f),
                    new Color(0.04f, 0.16f, 0.22f, 0.95f), def.primaryColor,
                    () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            if (GameManager.Instance.TryUnlockOrEquipHull(hullIdx))
                            {
                                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                            }
                            RefreshHullsModal();
                            UpdateHUD();
                        }
                    });
                _hullActionButtons[i] = actionBtn;
                _hullActionLabels[i] = actionBtn.GetComponentInChildren<Text>();
            }

            CreateStyledButton(card.transform, "CloseHullsBtn", "BACK TO FLIGHT", 28,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 68f), new Vector2(420f, 80f),
                new Color(0.02f, 0.28f, 0.22f, 0.96f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _hullsPanel.SetActive(false);
                    if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                    {
                        GameManager.Instance.StartNewRun(true);
                    }
                    else
                    {
                        Time.timeScale = 1f;
                    }
                });

            _hullsPanel.SetActive(false);
        }

        private void BuildLeaderboardModal(Transform root)
        {
            _leaderboardPanel = CreateFullScreenOverlay(root, "LeaderboardModal", new Color(0.01f, 0.02f, 0.06f, 0.92f));
            GameObject card = CreateStyledCard(_leaderboardPanel.transform, "LbCard",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 860f),
                new Color(0.03f, 0.06f, 0.14f, 0.98f), OrbitAssetFactory.StarGold);
            _leaderboardCardRt = card.GetComponent<RectTransform>();

            CreateText(card.transform, "LbHeader", "COSMIC VOID LEADERBOARD", 44, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.UpperCenter,
                new Vector2(0f, -42f), new Vector2(720f, 58f), true);

            CreateText(card.transform, "LbSub", "WEEKLY GLOBAL & SEED CHALLENGE STANDINGS", 20, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.UpperCenter,
                new Vector2(0f, -104f), new Vector2(720f, 34f), true);

            _leaderboardRowsText = CreateText(card.transform, "LbRows", "", 28, FontStyle.Bold,
                Color.white, TextAnchor.UpperLeft,
                new Vector2(56f, -175f), new Vector2(668f, 500f));

            CreateStyledButton(card.transform, "CloseLbBtn", "CLOSE", 28,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(380f, 80f),
                new Color(0.02f, 0.28f, 0.22f, 0.96f), OrbitAssetFactory.LimeGreen,
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                    _leaderboardPanel.SetActive(false);
                    if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                    {
                        _gameOverPanel.SetActive(true);
                    }
                });

            _leaderboardPanel.SetActive(false);
        }

        public void TriggerScorePunch()
        {
            _scorePunchTimer = 0.28f;
        }

        public void TriggerComboPunch()
        {
            _comboPunchTimer = 0.35f;
        }

        public void TriggerShardPunch()
        {
            _shardPunchTimer = 0.30f;
        }

        private void AnimateModalPopIn(GameObject panel, RectTransform cardRt)
        {
            if (panel == null) return;
            panel.SetActive(true);
            if (cardRt != null)
            {
                StartCoroutine(ModalSpringCoroutine(cardRt));
            }
        }

        private IEnumerator ModalSpringCoroutine(RectTransform cardRt)
        {
            float dur = 0.32f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                if (cardRt == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float scale = 0.80f + 0.20f * Mathf.Sin(t * Mathf.PI * 0.5f) + 0.06f * Mathf.Sin(t * Mathf.PI);
                cardRt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            if (cardRt != null) cardRt.localScale = Vector3.one;
        }

        public void OnRunStarted()
        {
            _displayedScore = 0f;
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_hullsPanel != null) _hullsPanel.SetActive(false);
            if (_leaderboardPanel != null) _leaderboardPanel.SetActive(false);
        }

        public void UpdateHUD()
        {
            if (GameManager.Instance == null) return;

            if (_shardsValueText != null)
                _shardsValueText.text = GameManager.Instance.TotalStarShards.ToString();

            int mult = GameManager.Instance.ComboMultiplier;
            if (_comboValueText != null)
            {
                _comboValueText.text = "×" + mult;
                _comboValueText.color = mult >= 2 ? OrbitAssetFactory.LimeGreen : Color.white;
            }
            if (_comboHeaderText != null)
            {
                bool warpActive = ProbeController.Instance != null && ProbeController.Instance.IsWarpInvulnerable;
                _comboHeaderText.text = warpActive ? "WARP INVULNERABILITY!" : "STARLIGHT COMBO";
                _comboHeaderText.color = warpActive ? OrbitAssetFactory.NeonCyan : OrbitAssetFactory.LimeGreen;
            }

            if (_warpPipImages != null)
            {
                int streakMod = GameManager.Instance.ConsecutivePerfectHops % 5;
                bool warpActive = ProbeController.Instance != null && ProbeController.Instance.IsWarpInvulnerable;
                for (int i = 0; i < _warpPipImages.Length; i++)
                {
                    if (_warpPipImages[i] == null) continue;
                    if (warpActive)
                        _warpPipImages[i].color = OrbitAssetFactory.NeonCyan;
                    else if (i < streakMod)
                        _warpPipImages[i].color = OrbitAssetFactory.LimeGreen;
                    else
                        _warpPipImages[i].color = new Color(0.15f, 0.25f, 0.38f, 0.8f);
                }
            }
        }

        public void ShowTierBanner(CosmicTierInfo tier)
        {
            if (_tierBannerPanel == null || _splashActive) return;
            if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = StartCoroutine(AnimateTierBanner(tier));
        }

        private IEnumerator AnimateTierBanner(CosmicTierInfo tier)
        {
            _tierBannerHeader.text = $"TIER {tier.tierNumber} • {tier.subtitle.ToUpper()}";
            _tierBannerHeader.color = tier.bannerColor;
            _tierBannerTitle.text = tier.title;
            _tierBannerSubtitle.text = tier.playerTip;

            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                float prog = Mathf.Clamp01(t / 0.35f);
                _tierBannerGroup.alpha = prog;
                if (_tierBannerRt != null)
                {
                    float y = Mathf.Lerp(-185f, -235f, Mathf.Sin(prog * Mathf.PI * 0.5f));
                    _tierBannerRt.anchoredPosition = new Vector2(0f, y);
                }
                yield return null;
            }
            _tierBannerGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(2.8f);
            t = 0f;
            while (t < 0.45f)
            {
                t += Time.unscaledDeltaTime;
                _tierBannerGroup.alpha = 1f - Mathf.Clamp01(t / 0.45f);
                yield return null;
            }
            _tierBannerGroup.alpha = 0f;
        }

        public void ShowGameOverScreen(string causeOfDeath)
        {
            StartCoroutine(ShowGameOverDelayed(causeOfDeath));
        }

        private IEnumerator ShowGameOverDelayed(string causeOfDeath)
        {
            _gameOverUnlockTime = Time.unscaledTime + 0.55f;
            yield return new WaitForSecondsRealtime(0.42f);

            if (GameManager.Instance == null || GameManager.Instance.IsPlaying) yield break;

            if (_deathReasonText != null) _deathReasonText.text = causeOfDeath;
            if (_goScoreText != null) _goScoreText.text = GameManager.Instance.CurrentScore.ToString("N0");
            if (_goBestText != null) _goBestText.text = GameManager.Instance.BestScore.ToString("N0");
            if (_goShardsText != null) _goShardsText.text = "+" + GameManager.Instance.RunStarShards;
            if (_goTierText != null)
            {
                CosmicTierInfo tInfo = WorldSpawner.GetTierInfo(GameManager.Instance.CurrentTier);
                _goTierText.text = $"Tier {tInfo.tierNumber} • {tInfo.title}";
            }

            AnimateModalPopIn(_gameOverPanel, _gameOverCardRt);
        }

        public void SetPauseMenuVisible(bool visible)
        {
            if (_pausePanel != null)
            {
                if (visible) AnimateModalPopIn(_pausePanel, _pauseCardRt);
                else _pausePanel.SetActive(false);
            }
        }

        public void OpenHomeModal()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                Time.timeScale = 0f;
            }
            RefreshHomeModal();
            AnimateModalPopIn(_homePanel, _homeCardRt);
        }

        private void RefreshHomeModal()
        {
            if (GameManager.Instance == null) return;
            CosmicTierInfo t = WorldSpawner.GetTierInfo(GameManager.Instance.SelectedStartingTier);
            if (_homeTierBtnText != null)
                _homeTierBtnText.text = $"START SECTOR: TIER {t.tierNumber} • {t.title}";
            if (_homeSeedBtnText != null)
                _homeSeedBtnText.text = GameManager.Instance.UseDailySeed
                    ? $"MODE: DAILY SEED ({System.DateTime.UtcNow:yyyy-MM-dd})"
                    : "MODE: ENDLESS PROCEDURAL VOID";
        }

        public void OpenHullsModal()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                Time.timeScale = 0f;
            }
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            RefreshHullsModal();
            AnimateModalPopIn(_hullsPanel, _hullsCardRt);
        }

        private void RefreshHullsModal()
        {
            if (GameManager.Instance == null) return;
            if (_hullsShardBalanceText != null)
                _hullsShardBalanceText.text = $"AVAILABLE STAR SHARDS: ◆ {GameManager.Instance.TotalStarShards}";

            for (int i = 0; i < OrbitAssetFactory.Hulls.Length; i++)
            {
                HullDefinition def = OrbitAssetFactory.Hulls[i];
                bool unlocked = GameManager.Instance.IsHullUnlocked(i);
                bool equipped = GameManager.Instance.EquippedHullId == i;
                if (_hullActionLabels[i] != null)
                {
                    if (equipped) _hullActionLabels[i].text = "EQUIPPED";
                    else if (unlocked) _hullActionLabels[i].text = "EQUIP";
                    else _hullActionLabels[i].text = $"UNLOCK ◆{def.cost}";
                }
            }
        }

        public void OpenLeaderboardModal()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (GameManager.Instance != null && _leaderboardRowsText != null)
            {
                List<LeaderboardEntry> list = GameManager.Instance.GetLeaderboard();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    string marker = e.pilotName == "KASHAN" ? "★ " : "   ";
                    sb.AppendLine($"#{i + 1}  {marker}{e.pilotName,-10}  {e.score,7:N0} pts   [Tier {e.tier} • {e.hullName}]");
                    sb.AppendLine();
                }
                _leaderboardRowsText.text = sb.ToString();
            }
            AnimateModalPopIn(_leaderboardPanel, _leaderboardCardRt);
        }

        private void UpdateMuteButtonLabel()
        {
            if (_muteBtnText != null && AudioManager.Instance != null)
            {
                _muteBtnText.text = AudioManager.Instance.IsMuted ? "MUTE" : "SFX";
            }
        }

        private void Update()
        {
            if (_splashActive)
            {
                UpdateSplashAnimations();
                return;
            }

            float dt = Time.unscaledDeltaTime;
            float time = Time.unscaledTime;

            // 1. Smoothly roll displayed score toward actual score
            if (GameManager.Instance != null && _scoreValueText != null)
            {
                float targetScore = GameManager.Instance.CurrentScore;
                _displayedScore = Mathf.MoveTowards(_displayedScore, targetScore, Mathf.Max(450f, Mathf.Abs(targetScore - _displayedScore) * 10f) * dt);
                _scoreValueText.text = Mathf.RoundToInt(_displayedScore).ToString("N0");
            }

            // 2. Elastic scale punch on Score, Combo, and Shard cards
            if (_scoreCardRt != null)
            {
                if (_scorePunchTimer > 0f)
                {
                    _scorePunchTimer -= dt;
                    float s = 1f + Mathf.Sin((_scorePunchTimer / 0.28f) * Mathf.PI) * 0.14f;
                    _scoreCardRt.localScale = new Vector3(s, s, 1f);
                }
                else _scoreCardRt.localScale = Vector3.one;
            }

            if (_comboCardRt != null)
            {
                if (_comboPunchTimer > 0f)
                {
                    _comboPunchTimer -= dt;
                    float s = 1f + Mathf.Sin((_comboPunchTimer / 0.35f) * Mathf.PI) * 0.18f;
                    _comboCardRt.localScale = new Vector3(s, s, 1f);
                }
                else if (GameManager.Instance != null && GameManager.Instance.ComboMultiplier >= 4)
                {
                    float pulse = 1f + 0.035f * Mathf.Sin(time * 7f);
                    _comboCardRt.localScale = new Vector3(pulse, pulse, 1f);
                }
                else _comboCardRt.localScale = Vector3.one;
            }

            if (_shardCardRt != null)
            {
                if (_shardPunchTimer > 0f)
                {
                    _shardPunchTimer -= dt;
                    float s = 1f + Mathf.Sin((_shardPunchTimer / 0.30f) * Mathf.PI) * 0.18f;
                    _shardCardRt.localScale = new Vector3(s, s, 1f);
                }
                else _shardCardRt.localScale = Vector3.one;
            }

            // 3. Breathing pulse on CTA Buttons & Bottom Prompt Pill
            float btnBreath = 1f + 0.03f * Mathf.Sin(time * 5.2f);
            for (int i = 0; i < _pulsingButtons.Count; i++)
            {
                if (_pulsingButtons[i] != null && _pulsingButtons[i].gameObject.activeInHierarchy)
                {
                    _pulsingButtons[i].localScale = new Vector3(btnBreath, btnBreath, 1f);
                }
            }

            if (ProbeController.Instance != null && _bottomPromptText != null)
            {
                bool inOrbit = ProbeController.Instance.CurrentState == ProbeState.GravitationalLock ||
                               ProbeController.Instance.CurrentState == ProbeState.StableOrbit;
                _bottomPromptText.text = inOrbit
                    ? "TAP ANYWHERE / SPACE TO RELEASE"
                    : "IN TRANSIT • ENTER GRAVITY WELL";

                if (_bottomPillRt != null)
                {
                    float pillScale = inOrbit ? (1f + 0.035f * Mathf.Sin(time * 6.5f)) : 1f;
                    _bottomPillRt.localScale = new Vector3(pillScale, pillScale, 1f);
                }
            }

            // Allow instant Space / Enter retry when on Game Over screen
            if (_gameOverPanel != null && _gameOverPanel.activeSelf && Time.unscaledTime > _gameOverUnlockTime)
            {
                bool retryKey = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                    retryKey = true;
#else
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    retryKey = true;
#endif
                if (retryKey && GameManager.Instance != null)
                {
                    GameManager.Instance.StartNewRun(true);
                }
            }
        }

        private GameObject CreateFullScreenOverlay(Transform parent, string name, Color bgColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        private GameObject CreateStyledCard(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size, Color bgColor, Color outlineColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            if (anchorMin.x == 0.5f && anchorMax.x == 0.5f)
                rt.pivot = new Vector2(0.5f, anchorMin.y);
            if (anchorMin == new Vector2(0.5f, 0.5f))
                rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);

            return go;
        }

        private Button CreateStyledButton(Transform parent, string name, string label, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size,
            Color bgColor, Color borderAndTextColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = CreateStyledCard(parent, name, anchorMin, anchorMax, anchoredPos, size, bgColor, borderAndTextColor);
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            btn.onClick.AddListener(onClick);

            CreateText(go.transform, "Label", label, fontSize, FontStyle.Bold,
                borderAndTextColor, TextAnchor.MiddleCenter, Vector2.zero, size, true);
            return btn;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, FontStyle style,
            Color color, TextAnchor alignment, Vector2 anchoredPos, Vector2 size, bool centerAnchor = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            if (centerAnchor)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                if (anchoredPos == Vector2.zero)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                }
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0f, 1f);
            }
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Text txt = go.AddComponent<Text>();
            txt.font = _uiFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
