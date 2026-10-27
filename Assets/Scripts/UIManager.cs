using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

        [Header("Canvas & Root Panels (Editable in Scene Hierarchy)")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Font _uiFont;
        [SerializeField] private GameObject _splashPanel;
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _gameplayHudRoot;
        [SerializeField] private GameObject _tierBannerPanel;
        [SerializeField] private GameObject _upgradesPanel;
        [SerializeField] private GameObject _hullsPanel;
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _missionsPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _gameOverPanel;

        // ==================== 1. SPLASH SCREEN ====================
        [Header("1. Splash Screen References")]
        [SerializeField] private CanvasGroup _splashCanvasGroup;
        [SerializeField] private List<RectTransform> _splashPlanets = new List<RectTransform>();
        [SerializeField] private List<RectTransform> _splashPlanetRings = new List<RectTransform>();
        [SerializeField] private List<Vector2> _splashPlanetDriftVel = new List<Vector2>();
        [SerializeField] private RectTransform _splashOrbitingProbe;
        [SerializeField] private List<RectTransform> _splashLetterRects = new List<RectTransform>();
        [SerializeField] private RectTransform _splashBarFill;
        [SerializeField] private RectTransform _splashBarSpark;
        [SerializeField] private Text _splashStatusText;
        [SerializeField] private Text _splashPercentText;
        private readonly List<Vector2> _splashPlanetBasePos = new List<Vector2>();
        private readonly List<Vector2> _splashLetterBasePos = new List<Vector2>();
        private bool _splashActive = true;

        // ==================== 2. OPTION 3 MAIN MENU ====================
        [Header("2. Main Menu References")]
        [SerializeField] private CanvasGroup _mainMenuGroup;
        [SerializeField] private RectTransform _mmTopBarRt;
        [SerializeField] private RectTransform _mmLogoGroupRt;
        [SerializeField] private RectTransform _mmBottomGroupRt;
        [SerializeField] private Text _mmBestScoreText;
        [SerializeField] private Text _mmShardsText;
        [SerializeField] private Button _mmPlusBtn;
        [SerializeField] private Button _mmSettingsBtn;
        [SerializeField] private Button _mmTapToPlayBtn;
        [SerializeField] private Button _mmDockUpgradesBtn;
        [SerializeField] private Button _mmDockSkinsBtn;
        [SerializeField] private Button _mmDockRanksBtn;
        [SerializeField] private Button _mmDockMissionsBtn;
        [SerializeField] private List<RectTransform> _mmLetterRects = new List<RectTransform>();
        private readonly List<Vector2> _mmLetterBasePos = new List<Vector2>();
        private Vector2 _mmTopBarBasePos;
        private Vector2 _mmLogoGroupBasePos = new Vector2(0f, -155f);
        private Vector2 _mmBottomGroupBasePos;

        // ==================== 3. GAMEPLAY HUD ====================
        [Header("3. Gameplay HUD References")]
        [SerializeField] private CanvasGroup _gameplayHudGroup;
        [SerializeField] private RectTransform _scoreCardRt;
        [SerializeField] private Text _scoreValueText;
        [SerializeField] private RectTransform _comboCardRt;
        [SerializeField] private Text _comboValueText;
        [SerializeField] private Text _comboHeaderText;
        [SerializeField] private Image[] _warpPipImages;
        [SerializeField] private RectTransform _shardCardRt;
        [SerializeField] private Text _shardsValueText;
        [SerializeField] private Button _hudPauseBtn;
        [SerializeField] private RectTransform _bottomPillRt;
        [SerializeField] private Text _bottomPromptText;
        private float _displayedScore;
        private float _scorePunchTimer;
        private float _comboPunchTimer;
        private float _shardPunchTimer;

        // Pulsing CTA Buttons
        [SerializeField] private List<RectTransform> _pulsingButtons = new List<RectTransform>();

        // Tier Banner
        [Header("Tier Banner References")]
        [SerializeField] private CanvasGroup _tierBannerGroup;
        [SerializeField] private RectTransform _tierBannerRt;
        [SerializeField] private Text _tierBannerHeader;
        [SerializeField] private Text _tierBannerTitle;
        [SerializeField] private Text _tierBannerSubtitle;
        private Coroutine _bannerCoroutine;

        // ==================== 4. MODALS ====================
        [Header("4. Modals & Popups References")]
        [SerializeField] private RectTransform _gameOverCardRt;
        [SerializeField] private Text _deathReasonText;
        [SerializeField] private Text _goScoreText;
        [SerializeField] private Text _goBestText;
        [SerializeField] private Text _goShardsText;
        [SerializeField] private Text _goTierText;
        [SerializeField] private Button _goRetryBtn;
        [SerializeField] private Button _goMenuBtn;
        [SerializeField] private Button _goUpgBtn;
        [SerializeField] private Button _goSkinsBtn;

        [SerializeField] private RectTransform _pauseCardRt;
        [SerializeField] private Button _pauseResumeBtn;
        [SerializeField] private Button _pauseRestartBtn;
        [SerializeField] private Button _pauseMenuBtn;

        [SerializeField] private RectTransform _upgradesCardRt;
        [SerializeField] private Text _upgShardBalanceText;
        [SerializeField] private Text[] _upgLevelTexts;
        [SerializeField] private Text[] _upgBtnLabels;
        [SerializeField] private Button[] _upgBuyBtns;
        [SerializeField] private Button _upgCloseBtn;
        [SerializeField] private Image[] _upgPipFlatList;

        [SerializeField] private RectTransform _hullsCardRt;
        [SerializeField] private Text _hullsShardBalanceText;
        [SerializeField] private Text[] _hullActionLabels;
        [SerializeField] private Button[] _hullActionBtns;
        [SerializeField] private Button _hullCloseBtn;

        [SerializeField] private RectTransform _leaderboardCardRt;
        [SerializeField] private Text _leaderboardRowsText;
        [SerializeField] private Button _lbCloseBtn;

        [SerializeField] private RectTransform _missionsCardRt;
        [SerializeField] private Text _missionTierBtnText;
        [SerializeField] private Text _missionSeedBtnText;
        [SerializeField] private Button _missionTierBtn;
        [SerializeField] private Button _missionSeedBtn;
        [SerializeField] private Button _missionCloseBtn;

        [SerializeField] private RectTransform _settingsCardRt;
        [SerializeField] private Text _settingsAudioBtnText;
        [SerializeField] private Text _settingsShakeBtnText;
        [SerializeField] private Button _settingsAudioBtn;
        [SerializeField] private Button _settingsShakeBtn;
        [SerializeField] private Button _settingsCloseBtn;

        private float _gameOverUnlockTime;
        [SerializeField] private int _uiSchemaVersion;
        public const int CurrentSchemaVersion = 2;

        public bool HasBuiltHierarchy => _canvas != null && _mainMenuPanel != null && _gameplayHudRoot != null && _uiSchemaVersion == CurrentSchemaVersion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureFontLoaded();
            EnsureEventSystem();

            // If the UI panels already exist in the Scene Hierarchy, DO NOT overwrite them!
            // Keep all user edits made in the Unity Editor and bind runtime button events.
            if (!HasBuiltHierarchy)
            {
                BuildCompleteUI();
            }

            BindRuntimeEventsAndPositions();
        }

        private void Start()
        {
            StartCoroutine(RunSplashSequence());
        }

        private void EnsureFontLoaded()
        {
            if (_uiFont != null) return;
            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_uiFont == null) _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_uiFont == null) _uiFont = Font.CreateDynamicFontFromOSFont("Segoe UI", 24);
        }

        public void EnsureEventSystem()
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

        [ContextMenu("Rebuild UI Panels in Hierarchy")]
        public void RebuildHierarchyInEditor()
        {
            EnsureFontLoaded();
            EnsureEventSystem();

            // Remove old canvas children if rebuilding from menu
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _splashPlanets.Clear();
            _splashPlanetRings.Clear();
            _splashPlanetDriftVel.Clear();
            _splashLetterRects.Clear();
            _mmLetterRects.Clear();
            _pulsingButtons.Clear();

            BuildCompleteUI();

            // In Edit Mode, keep Option3MainMenu visible and SplashScreenOverlay hidden
            // so the designer can see and edit the Main Menu & Planet directly in Scene/Game View!
            if (!Application.isPlaying)
            {
                ShowOnlyPanelInEditor(_mainMenuPanel);
            }
        }

        public void ShowOnlyPanelInEditor(GameObject activePanel)
        {
            if (_splashPanel != null) _splashPanel.SetActive(activePanel == _splashPanel);
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(activePanel == _mainMenuPanel);
            if (_gameplayHudRoot != null) _gameplayHudRoot.SetActive(activePanel == _gameplayHudRoot);
            if (_upgradesPanel != null) _upgradesPanel.SetActive(activePanel == _upgradesPanel);
            if (_hullsPanel != null) _hullsPanel.SetActive(activePanel == _hullsPanel);
            if (_leaderboardPanel != null) _leaderboardPanel.SetActive(activePanel == _leaderboardPanel);
            if (_missionsPanel != null) _missionsPanel.SetActive(activePanel == _missionsPanel);
            if (_settingsPanel != null) _settingsPanel.SetActive(activePanel == _settingsPanel);
            if (_pausePanel != null) _pausePanel.SetActive(activePanel == _pausePanel);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(activePanel == _gameOverPanel);
        }

        private void BindRuntimeEventsAndPositions()
        {
            // Capture current positions from the Hierarchy so any Editor adjustments by the user are preserved!
            if (_mmTopBarRt != null) _mmTopBarBasePos = _mmTopBarRt.anchoredPosition;
            if (_mmLogoGroupRt != null) _mmLogoGroupBasePos = _mmLogoGroupRt.anchoredPosition;
            if (_mmBottomGroupRt != null) _mmBottomGroupBasePos = _mmBottomGroupRt.anchoredPosition;

            _splashPlanetBasePos.Clear();
            for (int i = 0; i < _splashPlanets.Count; i++)
            {
                _splashPlanetBasePos.Add(_splashPlanets[i] != null ? _splashPlanets[i].anchoredPosition : Vector2.zero);
            }

            _splashLetterBasePos.Clear();
            for (int i = 0; i < _splashLetterRects.Count; i++)
            {
                _splashLetterBasePos.Add(_splashLetterRects[i] != null ? _splashLetterRects[i].anchoredPosition : Vector2.zero);
            }

            _mmLetterBasePos.Clear();
            for (int i = 0; i < _mmLetterRects.Count; i++)
            {
                _mmLetterBasePos.Add(_mmLetterRects[i] != null ? _mmLetterRects[i].anchoredPosition : Vector2.zero);
            }

            // Bind Main Menu Buttons
            BindButton(_mmPlusBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenUpgradesModal();
            });
            BindButton(_mmSettingsBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenSettingsModal();
            });
            BindButton(_mmTapToPlayBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                StartCoroutine(TransitionMainMenuToGameplay());
            });
            BindButton(_mmDockUpgradesBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenUpgradesModal();
            });
            BindButton(_mmDockSkinsBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenHullsModal();
            });
            BindButton(_mmDockRanksBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenLeaderboardModal();
            });
            BindButton(_mmDockMissionsBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenMissionsModal();
            });

            // Bind HUD Buttons
            BindButton(_hudPauseBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null) GameManager.Instance.TogglePause();
            });

            // Bind Upgrades Modal Buttons
            if (_upgBuyBtns != null)
            {
                for (int i = 0; i < _upgBuyBtns.Length; i++)
                {
                    int upgIdx = i;
                    BindButton(_upgBuyBtns[i], () =>
                    {
                        if (GameManager.Instance != null && GameManager.Instance.TryPurchaseUpgrade(upgIdx))
                        {
                            RefreshUpgradesModal();
                            RefreshMainMenuTopBar();
                            UpdateHUD();
                        }
                    });
                }
            }
            BindButton(_upgCloseBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_upgradesPanel != null) _upgradesPanel.SetActive(false);
                RefreshMainMenuTopBar();
            });

            // Bind Hulls / Skins Modal Buttons
            if (_hullActionBtns != null)
            {
                for (int i = 0; i < _hullActionBtns.Length; i++)
                {
                    int hullIdx = i;
                    BindButton(_hullActionBtns[i], () =>
                    {
                        if (GameManager.Instance != null && GameManager.Instance.TryUnlockOrEquipHull(hullIdx))
                        {
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                            RefreshHullsModal();
                            RefreshMainMenuTopBar();
                            UpdateHUD();
                        }
                    });
                }
            }
            BindButton(_hullCloseBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_hullsPanel != null) _hullsPanel.SetActive(false);
                RefreshMainMenuTopBar();
            });

            // Bind Leaderboard Modal
            BindButton(_lbCloseBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_leaderboardPanel != null) _leaderboardPanel.SetActive(false);
            });

            // Bind Missions Modal
            BindButton(_missionTierBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null)
                {
                    int next = (GameManager.Instance.SelectedStartingTier % 5) + 1;
                    GameManager.Instance.SetStartingTier(next);
                    RefreshMissionsModal();
                }
            });
            BindButton(_missionSeedBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ToggleDailySeedMode();
                    RefreshMissionsModal();
                }
            });
            BindButton(_missionCloseBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_missionsPanel != null) _missionsPanel.SetActive(false);
            });

            // Bind Settings Modal
            BindButton(_settingsAudioBtn, () =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.ToggleMute();
                    AudioManager.Instance.PlayClick();
                    RefreshSettingsModal();
                }
            });
            BindButton(_settingsShakeBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ToggleScreenShake();
                    RefreshSettingsModal();
                }
            });
            BindButton(_settingsCloseBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_settingsPanel != null) _settingsPanel.SetActive(false);
            });

            // Bind Game Over Modal
            BindButton(_goRetryBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null) GameManager.Instance.StartNewRun(true);
            });
            BindButton(_goMenuBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null) GameManager.Instance.EnterMainMenu();
            });
            BindButton(_goUpgBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenUpgradesModal();
            });
            BindButton(_goSkinsBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                OpenHullsModal();
            });

            // Bind Pause Modal
            BindButton(_pauseResumeBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (GameManager.Instance != null) GameManager.Instance.TogglePause();
            });
            BindButton(_pauseRestartBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_pausePanel != null) _pausePanel.SetActive(false);
                if (GameManager.Instance != null) GameManager.Instance.StartNewRun(true);
            });
            BindButton(_pauseMenuBtn, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (_pausePanel != null) _pausePanel.SetActive(false);
                if (GameManager.Instance != null) GameManager.Instance.EnterMainMenu();
            });
        }

        private static void BindButton(Button btn, UnityAction action)
        {
            if (btn == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }

        private void BuildCompleteUI()
        {
            _uiSchemaVersion = CurrentSchemaVersion;
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
            BuildOption3MainMenu(canvasGo.transform);
            BuildGameOverScreen(canvasGo.transform);
            BuildPauseModal(canvasGo.transform);
            BuildUpgradesModal(canvasGo.transform);
            BuildHullsModal(canvasGo.transform);
            BuildLeaderboardModal(canvasGo.transform);
            BuildMissionsModal(canvasGo.transform);
            BuildSettingsModal(canvasGo.transform);
            BuildSplashScreen(canvasGo.transform);
        }

        // =====================================================================
        // 1. VIBRANT COSMIC ARCADE SPLASH SCREEN
        // =====================================================================
        private void BuildSplashScreen(Transform root)
        {
            _splashPanel = CreateFullScreenOverlay(root, "SplashScreenOverlay", OrbitAssetFactory.DeepVoidBlue);
            _splashCanvasGroup = _splashPanel.AddComponent<CanvasGroup>();

            string[] planetSprites = { "planet_arcade", "planet_cyan", "planet_purple", "planet_amber", "planet_emerald" };
            Color[] ringColors =
            {
                OrbitAssetFactory.ArcadePink,
                OrbitAssetFactory.NeonCyan,
                OrbitAssetFactory.PulsarPurple,
                OrbitAssetFactory.StarGold,
                OrbitAssetFactory.LimeGreen
            };
            Vector2[] startPositions =
            {
                new Vector2(0f, 360f),
                new Vector2(-360f, 640f),
                new Vector2(370f, 580f),
                new Vector2(-360f, -590f),
                new Vector2(360f, -610f)
            };
            float[] planetSizes = { 260f, 175f, 160f, 155f, 180f };
            Vector2[] velocities =
            {
                new Vector2(14f, 12f),
                new Vector2(26f, -18f),
                new Vector2(-22f, -20f),
                new Vector2(30f, 18f),
                new Vector2(-24f, 22f)
            };

            for (int i = 0; i < planetSprites.Length; i++)
            {
                RectTransform pRt = CreateRect(_splashPanel.transform, "SplashPlanet_" + i,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    startPositions[i], new Vector2(planetSizes[i], planetSizes[i]));

                GameObject glowGo = CreateSpriteBox(pRt, "Glow", "glow_circle",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(planetSizes[i] * 2.1f, planetSizes[i] * 2.1f), false);
                glowGo.GetComponent<Image>().color = new Color(ringColors[i].r, ringColors[i].g, ringColors[i].b, 0.28f);

                GameObject ringGo = CreateSpriteBox(pRt, "OrbitRing", "orbit_ring",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(planetSizes[i] * 1.72f, planetSizes[i] * 1.72f), false);
                ringGo.GetComponent<Image>().color = new Color(ringColors[i].r, ringColors[i].g, ringColors[i].b, 0.78f);

                CreateSpriteBox(pRt, "Body", planetSprites[i],
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(planetSizes[i], planetSizes[i]), false);

                _splashPlanets.Add(pRt);
                _splashPlanetRings.Add(ringGo.GetComponent<RectTransform>());
                _splashPlanetBasePos.Add(startPositions[i]);
                _splashPlanetDriftVel.Add(velocities[i]);

                if (i == 0)
                {
                    GameObject probeGo = CreateSpriteBox(pRt, "SplashOrbitingRocket", "icon_rocket",
                        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                        new Vector2(82f, 82f), false);
                    _splashOrbitingProbe = probeGo.GetComponent<RectTransform>();
                }
            }

            // Splash Center Glossy Card (centered slightly below the main planet)
            GameObject centerPanel = CreateSpriteBox(_splashPanel.transform, "SplashCenterCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -210f), new Vector2(900f, 590f), true);
            Transform cardTr = centerPanel.transform;

            // Golden Ring Emblem behind 3D Title
            CreateSpriteBox(cardTr, "LogoRing", "ui_logo_ring",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -165f), new Vector2(250f, 250f), false);

            // 2-Line Per-Letter Animated Logo ("ORBIT" in Cyan, "LAUNCHER" in Pink/Magenta)
            BuildStackedAnimatedLogo(cardTr, new Vector2(0f, -45f), _splashLetterRects, _splashLetterBasePos);

            CreateLabel(cardTr, "SplashSubtitle", "Top-Down Orbital Hopper", 30, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -320f), new Vector2(780f, 44f), true);

            // Glossy Arcade Loading Bar (9-Sliced dedicated track & fill so ends never stretch!)
            GameObject barBg = CreateSpriteBox(cardTr, "LoadingTrack", "ui_bar_track",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -415f), new Vector2(720f, 56f), false);

            GameObject fillGo = CreateSpriteBox(barBg.transform, "LoadingBarFill", "ui_bar_fill",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(40f, 40f), false);
            _splashBarFill = fillGo.GetComponent<RectTransform>();

            GameObject sparkGo = CreateSpriteBox(fillGo.transform, "LoadingTipSpark", "glow_circle",
                new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(68f, 68f), false);
            sparkGo.GetComponent<Image>().color = OrbitAssetFactory.NeonCyan;
            _splashBarSpark = sparkGo.GetComponent<RectTransform>();

            _splashStatusText = CreateLabel(cardTr, "SplashStatus", "LOADING COSMIC SECTORS...", 22, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(85f, -495f), new Vector2(540f, 40f), false);

            _splashPercentText = CreateLabel(cardTr, "SplashPercent", "0%", 28, FontStyle.Bold,
                OrbitAssetFactory.LimeGreen, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-85f, -495f), new Vector2(180f, 40f), false);
        }

        private void BuildStackedAnimatedLogo(Transform parent, Vector2 topOffset, List<RectTransform> letterRects, List<Vector2> basePositions)
        {
            RectTransform cRt = CreateRect(parent, "StackedLogoContainer",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                topOffset, new Vector2(860f, 230f));

            // Row 1: "ORBIT" (Electric Cyan #00f0ff)
            string row1 = "ORBIT";
            float sp1 = 74f;
            float start1 = -(row1.Length - 1) * sp1 * 0.5f;
            for (int i = 0; i < row1.Length; i++)
            {
                CreateAnimatedLogoLetter(cRt, row1[i], new Vector2(start1 + i * sp1, -68f),
                    82, OrbitAssetFactory.NeonCyan, new Color(0.02f, 0.25f, 0.55f, 1f), letterRects, basePositions);
            }

            // Row 2: "LAUNCHER" (Vibrant Pink/Magenta #ff2ec7)
            string row2 = "LAUNCHER";
            float sp2 = 66f;
            float start2 = -(row2.Length - 1) * sp2 * 0.5f;
            for (int i = 0; i < row2.Length; i++)
            {
                CreateAnimatedLogoLetter(cRt, row2[i], new Vector2(start2 + i * sp2, -158f),
                    74, OrbitAssetFactory.ArcadePink, new Color(0.45f, 0.04f, 0.35f, 1f), letterRects, basePositions);
            }
        }

        private void CreateAnimatedLogoLetter(Transform parent, char ch, Vector2 pos, int fontSize,
            Color fillCol, Color outlineCol, List<RectTransform> rectList, List<Vector2> posList)
        {
            RectTransform rt = CreateRect(parent, "Letter_" + ch,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                pos, new Vector2(90f, 100f));

            Text txt = rt.gameObject.AddComponent<Text>();
            txt.font = _uiFont;
            txt.text = ch.ToString();
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = fillCol;
            txt.raycastTarget = false;

            Outline ol = rt.gameObject.AddComponent<Outline>();
            ol.effectColor = outlineCol;
            ol.effectDistance = new Vector2(4f, -4f);

            Shadow sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(fillCol.r, fillCol.g, fillCol.b, 0.65f);
            sh.effectDistance = new Vector2(0f, -6f);

            rectList.Add(rt);
            posList.Add(pos);
        }

        // =====================================================================
        // 2. OPTION 3 VIBRANT COSMIC ARCADE MAIN MENU
        // =====================================================================
        private void BuildOption3MainMenu(Transform root)
        {
            _mainMenuPanel = new GameObject("Option3MainMenu");
            _mainMenuPanel.transform.SetParent(root, false);
            RectTransform mmRt = _mainMenuPanel.AddComponent<RectTransform>();
            mmRt.anchorMin = Vector2.zero;
            mmRt.anchorMax = Vector2.one;
            mmRt.offsetMin = Vector2.zero;
            mmRt.offsetMax = Vector2.zero;
            _mainMenuGroup = _mainMenuPanel.AddComponent<CanvasGroup>();

            // --- 1. TOP BAR (Best Score Gold Pill, Star Shards Purple Pill, Settings Gear Button) ---
            _mmTopBarRt = CreateRect(_mainMenuPanel.transform, "MainMenuTopBar",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, new Vector2(0f, 150f));

            // 1A. Top-Left Best Score Gold Pill (X: 30 .. 386)
            GameObject bestPill = CreateSpriteBox(_mmTopBarRt, "BestScorePill", "ui_pill_gold",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -28f), new Vector2(356f, 102f), false);

            CreateSpriteBox(bestPill.transform, "TrophyIcon", "icon_trophy",
                new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(54f, 0f), new Vector2(62f, 62f), false);

            CreateLabel(bestPill.transform, "BestLbl", "BEST:", 20, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 19f), new Vector2(235f, 28f), true);

            _mmBestScoreText = CreateLabel(bestPill.transform, "BestVal", "19,450", 34, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, -13f), new Vector2(235f, 44f), true);

            // 1B. Top-Center-Right Star Shards Purple Pill (X: 487 .. 823)
            GameObject shardPill = CreateSpriteBox(_mmTopBarRt, "ShardVaultPill", "ui_pill_purple",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(115f, -28f), new Vector2(336f, 102f), false);

            CreateSpriteBox(shardPill.transform, "DiamondIcon", "icon_diamond_pink",
                new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(52f, 0f), new Vector2(58f, 58f), false);

            _mmShardsText = CreateLabel(shardPill.transform, "ShardsVal", "85", 36, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(92f, 0f), new Vector2(155f, 52f), true);

            // Circular Green '+' button on shard pill (uses circular sprite so it never stretches!)
            _mmPlusBtn = CreateArcadeButton(shardPill.transform, "PlusBtn", "ui_circle_green", "+", 30,
                new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-44f, 0f), new Vector2(56f, 56f), Color.white, null);

            // 1C. Top-Right Circular Settings Gear Button (X: 942 .. 1050)
            GameObject settingsBtnGo = CreateSpriteBox(_mmTopBarRt, "SettingsBtn", "ui_circle_settings",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -25f), new Vector2(108f, 108f), true);
            _mmSettingsBtn = settingsBtnGo.AddComponent<Button>();
            _mmSettingsBtn.targetGraphic = settingsBtnGo.GetComponent<Image>();
            CreateSpriteBox(settingsBtnGo.transform, "GearIcon", "icon_gear",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), false);

            // --- 2. TOP-CENTER FLOATING LOGO EMBLEM (Cleanly below Top Bar, above Center Planet) ---
            _mmLogoGroupRt = CreateRect(_mainMenuPanel.transform, "MainMenuLogoGroup",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -155f), new Vector2(900f, 320f));

            CreateSpriteBox(_mmLogoGroupRt, "GoldenRingEmblem", "ui_logo_ring",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(250f, 250f), false);

            BuildStackedAnimatedLogo(_mmLogoGroupRt, new Vector2(0f, -15f), _mmLetterRects, _mmLetterBasePos);

            CreateLabel(_mmLogoGroupRt, "Tagline", "Top-Down Orbital Hopper", 30, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -282f), new Vector2(820f, 44f), true);

            // --- 3. BOTTOM GROUP (3D Green "TAP TO PLAY >" Button + 4-Icon Arcade Dock) ---
            _mmBottomGroupRt = CreateRect(_mainMenuPanel.transform, "MainMenuBottomGroup",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                Vector2.zero, new Vector2(1080f, 500f));

            // Juicy 3D Green "TAP TO PLAY >" Button (Y: 325 .. 451, cleanly above the Dock!)
            _mmTapToPlayBtn = CreateArcadeButton(_mmBottomGroupRt, "TapToPlayBtn", "ui_btn_play_green",
                "TAP TO PLAY   >", 40,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 388f), new Vector2(560f, 126f), Color.white, null);
            _pulsingButtons.Add(_mmTapToPlayBtn.GetComponent<RectTransform>());

            // Rounded Purple Bottom Dock Tray (Y: 24 .. 292, fully inside screen with rounded frame)
            CreateSpriteBox(_mmBottomGroupRt, "BottomDockTray", "ui_card_arcade",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(980f, 268f), true);

            // 4 Colorful Squircle Buttons: UPGRADES, SKINS, RANKS, MISSIONS
            _mmDockUpgradesBtn = CreateDockItem(_mmBottomGroupRt, "Dock_Upgrades", "UPGRADES", "ui_squircle_orange", "icon_lightning",
                new Vector2(-330f, 182f));

            _mmDockSkinsBtn = CreateDockItem(_mmBottomGroupRt, "Dock_Skins", "SKINS", "ui_squircle_pink", "icon_rocket",
                new Vector2(-110f, 182f));

            _mmDockRanksBtn = CreateDockItem(_mmBottomGroupRt, "Dock_Ranks", "RANKS", "ui_squircle_gold", "icon_trophy",
                new Vector2(110f, 182f));

            _mmDockMissionsBtn = CreateDockItem(_mmBottomGroupRt, "Dock_Missions", "MISSIONS", "ui_squircle_blue", "icon_star_badge",
                new Vector2(330f, 182f));

            _mainMenuPanel.SetActive(false);
        }

        private Button CreateDockItem(Transform parent, string name, string label, string squircleSprite, string iconSprite,
            Vector2 centerPos)
        {
            GameObject sqGo = CreateSpriteBox(parent, name, squircleSprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), centerPos, new Vector2(164f, 164f), true);

            Button btn = sqGo.AddComponent<Button>();
            btn.targetGraphic = sqGo.GetComponent<Image>();

            CreateSpriteBox(sqGo.transform, "Icon", iconSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(92f, 92f), false);

            CreateLabel(sqGo.transform, "DockLabel", label, 23, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, -26f), new Vector2(210f, 34f), true);

            return btn;
        }

        public void ShowMainMenu()
        {
            if (_gameplayHudRoot != null) _gameplayHudRoot.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_pausePanel != null) _pausePanel.SetActive(false);

            RefreshMainMenuTopBar();
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);
            if (_mainMenuGroup != null) _mainMenuGroup.alpha = 1f;
            if (_mmTopBarRt != null) _mmTopBarRt.anchoredPosition = _mmTopBarBasePos;
            if (_mmLogoGroupRt != null) _mmLogoGroupRt.anchoredPosition = _mmLogoGroupBasePos;
            if (_mmBottomGroupRt != null) _mmBottomGroupRt.anchoredPosition = _mmBottomGroupBasePos;
        }

        private void RefreshMainMenuTopBar()
        {
            if (GameManager.Instance == null) return;
            if (_mmBestScoreText != null) _mmBestScoreText.text = GameManager.Instance.BestScore.ToString("N0");
            if (_mmShardsText != null) _mmShardsText.text = GameManager.Instance.TotalStarShards.ToString("N0");
        }

        private IEnumerator TransitionMainMenuToGameplay()
        {
            float dur = 0.38f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float ease = t * t;
                if (_mainMenuGroup != null) _mainMenuGroup.alpha = 1f - t;
                if (_mmTopBarRt != null) _mmTopBarRt.anchoredPosition = _mmTopBarBasePos + new Vector2(0f, ease * 220f);
                if (_mmLogoGroupRt != null) _mmLogoGroupRt.anchoredPosition = _mmLogoGroupBasePos + new Vector2(0f, ease * 280f);
                if (_mmBottomGroupRt != null) _mmBottomGroupRt.anchoredPosition = _mmBottomGroupBasePos + new Vector2(0f, -ease * 480f);
                yield return null;
            }
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRun(false);
            }
        }

        // =====================================================================
        // 3. GAMEPLAY HUD (Vibrant Cosmic Arcade Styled)
        // =====================================================================
        private void BuildGameplayHUD(Transform root)
        {
            _gameplayHudRoot = new GameObject("GameplayHUD");
            _gameplayHudRoot.transform.SetParent(root, false);
            RectTransform hudRt = _gameplayHudRoot.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;
            _gameplayHudGroup = _gameplayHudRoot.AddComponent<CanvasGroup>();

            // 1. Top-Left SCORE Gold Pill (X: 28 .. 338)
            GameObject scoreCard = CreateSpriteBox(_gameplayHudRoot.transform, "ScoreCard", "ui_pill_gold",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(310f, 104f), false);
            _scoreCardRt = scoreCard.GetComponent<RectTransform>();

            CreateSpriteBox(scoreCard.transform, "TrophyIcon", "icon_trophy",
                new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(52f, 0f), new Vector2(58f, 58f), false);

            CreateLabel(scoreCard.transform, "ScoreLabel", "SCORE", 18, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(92f, 20f), new Vector2(195f, 26f), true);

            _scoreValueText = CreateLabel(scoreCard.transform, "ScoreValue", "0", 36, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(92f, -12f), new Vector2(195f, 46f), true);

            // 2. Top-Center STARLIGHT COMBO Pill (X: 370 .. 690)
            GameObject comboCard = CreateSpriteBox(_gameplayHudRoot.transform, "ComboCard", "ui_pill_purple",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -28f), new Vector2(320f, 108f), false);
            _comboCardRt = comboCard.GetComponent<RectTransform>();

            _comboHeaderText = CreateLabel(comboCard.transform, "ComboLabel", "STARLIGHT COMBO", 16, FontStyle.Bold,
                OrbitAssetFactory.LimeGreen, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(290f, 24f), true);

            _comboValueText = CreateLabel(comboCard.transform, "ComboValue", "×1", 38, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(260f, 46f), true);

            _warpPipImages = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                RectTransform pipRt = CreateRect(comboCard.transform, "WarpPip_" + i,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                    new Vector2((i - 2) * 40f, 18f), new Vector2(32f, 7f));
                Image img = pipRt.gameObject.AddComponent<Image>();
                img.color = new Color(0.25f, 0.15f, 0.45f, 0.85f);
                img.raycastTarget = false;
                _warpPipImages[i] = img;
            }

            // 3. Top-Right STAR SHARDS Pill (X: 732 .. 942) + Pause Button (X: 956 .. 1052)
            GameObject shardCard = CreateSpriteBox(_gameplayHudRoot.transform, "ShardCard", "ui_pill_purple",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-138f, -32f), new Vector2(210f, 96f), false);
            _shardCardRt = shardCard.GetComponent<RectTransform>();

            CreateSpriteBox(shardCard.transform, "ShardIcon", "icon_diamond_pink",
                new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(48f, 0f), new Vector2(50f, 50f), false);

            _shardsValueText = CreateLabel(shardCard.transform, "ShardsValue", "85", 32, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 0f), new Vector2(112f, 50f), true);

            // Pause Button
            GameObject pauseGo = CreateSpriteBox(_gameplayHudRoot.transform, "PauseBtn", "ui_circle_settings",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(96f, 96f), true);
            _hudPauseBtn = pauseGo.AddComponent<Button>();
            _hudPauseBtn.targetGraphic = pauseGo.GetComponent<Image>();
            CreateLabel(pauseGo.transform, "PauseIcon", "||", 30, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(88f, 88f), true);

            // 4. Bottom-Center Prompt Pill (9-sliced bar track for crisp slim capsule)
            GameObject bottomPill = CreateSpriteBox(_gameplayHudRoot.transform, "BottomPromptPill", "ui_bar_track",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 82f), new Vector2(580f, 64f), false);
            _bottomPillRt = bottomPill.GetComponent<RectTransform>();

            _bottomPromptText = CreateLabel(bottomPill.transform, "PromptText", "TAP ANYWHERE / SPACE TO RELEASE", 22, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(540f, 54f), true);

            _gameplayHudRoot.SetActive(false);
        }

        private void BuildTierBanner(Transform root)
        {
            _tierBannerPanel = CreateSpriteBox(root, "TierBannerPanel", "ui_card_arcade",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(740f, 150f), false);
            _tierBannerRt = _tierBannerPanel.GetComponent<RectTransform>();
            _tierBannerGroup = _tierBannerPanel.AddComponent<CanvasGroup>();
            _tierBannerGroup.alpha = 0f;
            _tierBannerGroup.blocksRaycasts = false;

            _tierBannerHeader = CreateLabel(_tierBannerPanel.transform, "TierHeader", "TIER 1 • COSMIC SECTOR", 19, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -28f), new Vector2(680f, 28f), true);

            _tierBannerTitle = CreateLabel(_tierBannerPanel.transform, "TierTitle", "NEBULA", 40, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(680f, 48f), true);

            _tierBannerSubtitle = CreateLabel(_tierBannerPanel.transform, "TierSub", "Release when the dotted line turns LIME!", 20, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 28f), new Vector2(680f, 34f), true);
        }

        // =====================================================================
        // 4. MODALS (UPGRADES, SKINS, RANKS, MISSIONS, SETTINGS, GAME OVER, PAUSE)
        // =====================================================================
        private void BuildUpgradesModal(Transform root)
        {
            _upgradesPanel = CreateFullScreenOverlay(root, "UpgradesModal", new Color(0.03f, 0.01f, 0.09f, 0.90f));
            GameObject card = CreateSpriteBox(_upgradesPanel.transform, "UpgradesCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 1140f), true);
            _upgradesCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "UpgTitle", "SHIP UPGRADES LAB", 46, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(780f, 60f), true);

            _upgShardBalanceText = CreateLabel(card.transform, "UpgShards", "STAR SHARDS: ◆ 85", 28, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -126f), new Vector2(780f, 40f), true);

            int count = OrbitAssetFactory.Upgrades.Length;
            _upgLevelTexts = new Text[count];
            _upgBtnLabels = new Text[count];
            _upgBuyBtns = new Button[count];
            _upgPipFlatList = new Image[count * 5];

            for (int i = 0; i < count; i++)
            {
                UpgradeDefinition def = OrbitAssetFactory.Upgrades[i];
                float yPos = 220f - i * 232f;

                GameObject row = CreateSpriteBox(card.transform, "UpgRow_" + i, "ui_panel_inset",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, yPos), new Vector2(780f, 208f), false);

                CreateSpriteBox(row.transform, "UpgIcon", def.iconSprite,
                    new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(84f, 0f), new Vector2(104f, 104f), false);

                CreateLabel(row.transform, "UpgName", def.name, 26, FontStyle.Bold,
                    def.themeColor, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -20f), new Vector2(390f, 36f), true);

                _upgLevelTexts[i] = CreateLabel(row.transform, "UpgLvl", "LEVEL 1 / 5", 20, FontStyle.Bold,
                    OrbitAssetFactory.StarGold, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -58f), new Vector2(390f, 28f), false);

                CreateLabel(row.transform, "UpgDesc", def.description, 19, FontStyle.Normal,
                    new Color(0.88f, 0.92f, 1f, 0.95f), TextAnchor.UpperLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -90f), new Vector2(390f, 64f), false);

                // 5 Level Pips
                for (int p = 0; p < 5; p++)
                {
                    RectTransform pRt = CreateRect(row.transform, "Pip_" + p,
                        new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0.5f),
                        new Vector2(156f + p * 46f, 28f), new Vector2(36f, 10f));
                    Image pImg = pRt.gameObject.AddComponent<Image>();
                    pImg.color = OrbitAssetFactory.LimeGreen;
                    pImg.raycastTarget = false;
                    _upgPipFlatList[i * 5 + p] = pImg;
                }

                _upgBuyBtns[i] = CreateArcadeButton(row.transform, "BuyUpgBtn_" + i, "ui_btn_play_green", "◆ 15", 24,
                    new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-112f, 0f), new Vector2(180f, 84f), Color.white, null);
                _upgBtnLabels[i] = _upgBuyBtns[i].GetComponentInChildren<Text>();
            }

            _upgCloseBtn = CreateArcadeButton(card.transform, "CloseUpgBtn", "ui_btn_play_green", "DONE", 32,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 88f), new Vector2(380f, 92f), Color.white, null);

            _upgradesPanel.SetActive(false);
        }

        private void BuildHullsModal(Transform root)
        {
            _hullsPanel = CreateFullScreenOverlay(root, "HullsModal", new Color(0.03f, 0.01f, 0.09f, 0.90f));
            GameObject card = CreateSpriteBox(_hullsPanel.transform, "HullsCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 1220f), true);
            _hullsCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "HullsHeader", "ROCKET SKINS & HULLS", 46, FontStyle.Bold,
                OrbitAssetFactory.ArcadePink, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(780f, 60f), true);

            _hullsShardBalanceText = CreateLabel(card.transform, "ShardBalance", "STAR SHARDS: ◆ 85", 26, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -126f), new Vector2(780f, 40f), true);

            int count = OrbitAssetFactory.Hulls.Length;
            _hullActionLabels = new Text[count];
            _hullActionBtns = new Button[count];

            for (int i = 0; i < count; i++)
            {
                HullDefinition def = OrbitAssetFactory.Hulls[i];
                float yPos = 285f - i * 200f;

                GameObject row = CreateSpriteBox(card.transform, "HullRow_" + i, "ui_panel_inset",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, yPos), new Vector2(780f, 182f), false);

                CreateSpriteBox(row.transform, "HullSprite", def.spriteResource,
                    new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(84f, 0f), new Vector2(104f, 104f), false);

                CreateLabel(row.transform, "HullName", def.name, 27, FontStyle.Bold,
                    def.primaryColor, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -26f), new Vector2(390f, 38f), true);

                CreateLabel(row.transform, "HullDesc", def.description, 20, FontStyle.Normal,
                    new Color(0.85f, 0.92f, 1f, 0.92f), TextAnchor.UpperLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -68f), new Vector2(390f, 86f), false);

                _hullActionBtns[i] = CreateArcadeButton(row.transform, "HullBtn_" + i, "ui_btn_play_green", "EQUIP", 22,
                    new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-112f, 0f), new Vector2(184f, 82f), Color.white, null);
                _hullActionLabels[i] = _hullActionBtns[i].GetComponentInChildren<Text>();
            }

            _hullCloseBtn = CreateArcadeButton(card.transform, "CloseHullsBtn", "ui_btn_play_green", "DONE", 32,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 88f), new Vector2(380f, 92f), Color.white, null);

            _hullsPanel.SetActive(false);
        }

        private void BuildLeaderboardModal(Transform root)
        {
            _leaderboardPanel = CreateFullScreenOverlay(root, "LeaderboardModal", new Color(0.03f, 0.01f, 0.09f, 0.90f));
            GameObject card = CreateSpriteBox(_leaderboardPanel.transform, "LbCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 920f), true);
            _leaderboardCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "LbHeader", "COSMIC RANKS", 48, FontStyle.Bold,
                OrbitAssetFactory.StarGold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(740f, 60f), true);

            CreateLabel(card.transform, "LbSub", "GLOBAL & WEEKLY SEED LEADERBOARD", 20, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -124f), new Vector2(740f, 36f), true);

            GameObject rowsBox = CreateSpriteBox(card.transform, "LbRowsBox", "ui_panel_inset",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(720f, 520f), false);

            _leaderboardRowsText = CreateLabel(rowsBox.transform, "LbRows", "", 27, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(630f, 440f), true);

            _lbCloseBtn = CreateArcadeButton(card.transform, "CloseLbBtn", "ui_btn_play_green", "CLOSE", 30,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(360f, 88f), Color.white, null);

            _leaderboardPanel.SetActive(false);
        }

        private void BuildMissionsModal(Transform root)
        {
            _missionsPanel = CreateFullScreenOverlay(root, "MissionsModal", new Color(0.03f, 0.01f, 0.09f, 0.90f));
            GameObject card = CreateSpriteBox(_missionsPanel.transform, "MissionsCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 800f), true);
            _missionsCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "MissionsTitle", "COSMIC MISSIONS & TIERS", 44, FontStyle.Bold,
                OrbitAssetFactory.NeonCyan, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(740f, 58f), true);

            CreateLabel(card.transform, "MissionsDesc",
                "Select your starting Cosmic Sector or enable the Daily Seed Challenge!",
                24, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -148f), new Vector2(680f, 70f), false);

            _missionTierBtn = CreateArcadeButton(card.transform, "TierSelectBtn", "ui_pill_gold",
                "SECTOR: TIER 1 • NEBULA", 28,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 58f), new Vector2(640f, 108f), Color.white, null);
            _missionTierBtnText = _missionTierBtn.GetComponentInChildren<Text>();

            _missionSeedBtn = CreateArcadeButton(card.transform, "SeedSelectBtn", "ui_pill_purple",
                "MODE: ENDLESS ARCADE", 26,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(640f, 104f), OrbitAssetFactory.NeonCyan, null);
            _missionSeedBtnText = _missionSeedBtn.GetComponentInChildren<Text>();

            _missionCloseBtn = CreateArcadeButton(card.transform, "CloseMissionsBtn", "ui_btn_play_green", "SELECT & CLOSE", 30,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(420f, 90f), Color.white, null);

            _missionsPanel.SetActive(false);
        }

        private void BuildSettingsModal(Transform root)
        {
            _settingsPanel = CreateFullScreenOverlay(root, "SettingsModal", new Color(0.03f, 0.01f, 0.09f, 0.90f));
            GameObject card = CreateSpriteBox(_settingsPanel.transform, "SettingsCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 680f), true);
            _settingsCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "SettingsTitle", "GAME SETTINGS", 46, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(660f, 60f), true);

            _settingsAudioBtn = CreateArcadeButton(card.transform, "ToggleAudioBtn", "ui_pill_purple",
                "SOUND & MUSIC: ON", 28,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 65f), new Vector2(560f, 102f), OrbitAssetFactory.LimeGreen, null);
            _settingsAudioBtnText = _settingsAudioBtn.GetComponentInChildren<Text>();

            _settingsShakeBtn = CreateArcadeButton(card.transform, "ToggleShakeBtn", "ui_pill_purple",
                "SCREEN SHAKE: ON", 28,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -65f), new Vector2(560f, 102f), OrbitAssetFactory.NeonCyan, null);
            _settingsShakeBtnText = _settingsShakeBtn.GetComponentInChildren<Text>();

            _settingsCloseBtn = CreateArcadeButton(card.transform, "CloseSettingsBtn", "ui_btn_play_green", "SAVE & CLOSE", 30,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 88f), new Vector2(380f, 90f), Color.white, null);

            _settingsPanel.SetActive(false);
        }

        private void BuildGameOverScreen(Transform root)
        {
            _gameOverPanel = CreateFullScreenOverlay(root, "GameOverScreen", new Color(0.03f, 0.01f, 0.08f, 0.88f));

            GameObject card = CreateSpriteBox(_gameOverPanel.transform, "SignalLostCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 960f), true);
            _gameOverCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "SignalLostTitle", "SIGNAL LOST", 60, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -78f), new Vector2(720f, 74f), true);

            _deathReasonText = CreateLabel(card.transform, "DeathReason", "Probe destroyed by asteroid", 26, FontStyle.Bold,
                OrbitAssetFactory.ArcadePink, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -142f), new Vector2(720f, 40f), true);

            GameObject statsBox = CreateSpriteBox(card.transform, "StatsBox", "ui_panel_inset",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 65f), new Vector2(680f, 340f), false);

            _goScoreText = CreateStatRow(statsBox.transform, "SCORE", "0", -58f, Color.white);
            _goBestText = CreateStatRow(statsBox.transform, "BEST", "0", -132f, OrbitAssetFactory.StarGold);
            _goShardsText = CreateStatRow(statsBox.transform, "STAR SHARDS", "+0", -206f, OrbitAssetFactory.NeonCyan);
            _goTierText = CreateStatRow(statsBox.transform, "SECTOR", "Tier 1 • Nebula", -280f, OrbitAssetFactory.LimeGreen);

            _goRetryBtn = CreateArcadeButton(card.transform, "TapToRetryBtn", "ui_btn_play_green", "TAP TO RETRY   >", 36,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 215f), new Vector2(560f, 112f), Color.white, null);
            _pulsingButtons.Add(_goRetryBtn.GetComponent<RectTransform>());

            _goMenuBtn = CreateArcadeButton(card.transform, "GoMenuBtn", "ui_pill_gold", "MAIN MENU", 22,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-215f, 92f), new Vector2(195f, 76f), Color.white, null);

            _goUpgBtn = CreateArcadeButton(card.transform, "GoUpgBtn", "ui_pill_purple", "UPGRADES", 22,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 92f), new Vector2(195f, 76f), OrbitAssetFactory.LimeGreen, null);

            _goSkinsBtn = CreateArcadeButton(card.transform, "GoSkinsBtn", "ui_pill_purple", "SKINS", 22,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(215f, 92f), new Vector2(195f, 76f), OrbitAssetFactory.ArcadePink, null);

            _gameOverPanel.SetActive(false);
        }

        private Text CreateStatRow(Transform parent, string label, string initialValue, float yCenterFromTop, Color valueColor)
        {
            CreateLabel(parent, label + "_Lbl", label, 24, FontStyle.Bold,
                new Color(0.84f, 0.76f, 1f, 0.96f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(48f, yCenterFromTop), new Vector2(290f, 54f), false);

            return CreateLabel(parent, label + "_Val", initialValue, 34, FontStyle.Bold,
                valueColor, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-48f, yCenterFromTop), new Vector2(290f, 54f), true);
        }

        private void BuildPauseModal(Transform root)
        {
            _pausePanel = CreateFullScreenOverlay(root, "PauseModal", new Color(0.03f, 0.01f, 0.09f, 0.86f));
            GameObject card = CreateSpriteBox(_pausePanel.transform, "PauseCard", "ui_card_arcade",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 600f), true);
            _pauseCardRt = card.GetComponent<RectTransform>();

            CreateLabel(card.transform, "PauseTitle", "GAME PAUSED", 46, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -74f), new Vector2(580f, 60f), true);

            _pauseResumeBtn = CreateArcadeButton(card.transform, "ResumeBtn", "ui_btn_play_green", "RESUME PLAY   >", 30,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 65f), new Vector2(460f, 96f), Color.white, null);

            _pauseRestartBtn = CreateArcadeButton(card.transform, "RestartBtn", "ui_pill_purple", "RESTART MISSION", 26,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -52f), new Vector2(460f, 86f), OrbitAssetFactory.NeonCyan, null);

            _pauseMenuBtn = CreateArcadeButton(card.transform, "PauseMenuBtn", "ui_pill_gold", "MAIN MENU", 26,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -162f), new Vector2(460f, 86f), Color.white, null);

            _pausePanel.SetActive(false);
        }

        // =====================================================================
        // SEQUENCES & ANIMATIONS
        // =====================================================================
        private IEnumerator RunSplashSequence()
        {
            _splashActive = true;
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
            if (_gameplayHudRoot != null) _gameplayHudRoot.SetActive(false);
            if (_splashPanel != null)
            {
                _splashPanel.SetActive(true);
                _splashPanel.transform.localScale = Vector3.one;
            }
            if (_splashCanvasGroup != null) _splashCanvasGroup.alpha = 1f;

            string[] stages =
            {
                "IGNITING COSMIC NEBULA...",
                "CALIBRATING ORBITAL RINGS...",
                "PREPARING ROCKET SKINS & UPGRADES...",
                "TUNING PENTATONIC SYNTH CHIMES...",
                "WELCOME TO ORBIT LAUNCHER!"
            };

            float duration = 3.0f;
            float elapsed = 0f;
            int lastStageIdx = -1;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float prog = Mathf.SmoothStep(0f, 1f, t);

                if (_splashBarFill != null)
                {
                    _splashBarFill.sizeDelta = new Vector2(Mathf.Lerp(40f, 704f, prog), 40f);
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
            yield return new WaitForSecondsRealtime(0.28f);

            float fadeDur = 0.45f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeDur)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                float ft = Mathf.Clamp01(fadeElapsed / fadeDur);
                if (_splashCanvasGroup != null) _splashCanvasGroup.alpha = 1f - ft;
                if (_splashPanel != null) _splashPanel.transform.localScale = Vector3.one * (1f + ft * 0.08f);
                yield return null;
            }

            _splashActive = false;
            if (_splashPanel != null) _splashPanel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSplashCompleted();
            }
        }

        private void AnimateLettersWave(List<RectTransform> rects, List<Vector2> basePositions, float time, bool popIn)
        {
            for (int i = 0; i < rects.Count; i++)
            {
                RectTransform lRt = rects[i];
                if (lRt == null || i >= basePositions.Count) continue;

                float popT = 1f;
                if (popIn)
                {
                    float delay = i * 0.055f;
                    popT = Mathf.Clamp01(Mathf.Max(0f, time - delay) / 0.38f);
                }

                float scale = popT < 1f
                    ? Mathf.Sin(popT * Mathf.PI * 0.5f) + Mathf.Sin(popT * Mathf.PI) * 0.28f
                    : 1f + 0.045f * Mathf.Sin(time * 4.0f - i * 0.45f);

                float waveY = Mathf.Sin(time * 4.0f - i * 0.45f) * 6f * popT;
                lRt.anchoredPosition = basePositions[i] + new Vector2(0f, waveY);
                lRt.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void UpdateSplashAnimations()
        {
            if (!_splashActive || _splashPanel == null) return;
            float time = Time.unscaledTime;

            for (int i = 0; i < _splashPlanets.Count; i++)
            {
                RectTransform pRt = _splashPlanets[i];
                if (pRt == null || i >= _splashPlanetBasePos.Count || i >= _splashPlanetDriftVel.Count) continue;
                Vector2 baseP = _splashPlanetBasePos[i];
                Vector2 vel = _splashPlanetDriftVel[i];
                float offsetX = Mathf.Sin(time * 0.75f + i * 1.4f) * 40f + Mathf.Cos(time * 0.35f + i) * vel.x;
                float offsetY = Mathf.Cos(time * 0.65f + i * 1.1f) * 32f + Mathf.Sin(time * 0.4f + i) * vel.y;
                pRt.anchoredPosition = baseP + new Vector2(offsetX, offsetY);

                if (i < _splashPlanetRings.Count && _splashPlanetRings[i] != null)
                {
                    float dir = (i % 2 == 0) ? -1f : 1f;
                    _splashPlanetRings[i].Rotate(0f, 0f, dir * 28f * Time.unscaledDeltaTime);
                }
            }

            if (_splashOrbitingProbe != null)
            {
                float orbitAng = -time * 2.3f;
                float orbitR = 224f;
                _splashOrbitingProbe.anchoredPosition = new Vector2(Mathf.Cos(orbitAng) * orbitR, Mathf.Sin(orbitAng) * orbitR);
                Vector2 tangent = new Vector2(Mathf.Sin(orbitAng), -Mathf.Cos(orbitAng));
                float rotZ = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 45f;
                _splashOrbitingProbe.localRotation = Quaternion.Euler(0f, 0f, rotZ);
            }

            AnimateLettersWave(_splashLetterRects, _splashLetterBasePos, time, true);
        }

        public void TriggerScorePunch() => _scorePunchTimer = 0.28f;
        public void TriggerComboPunch() => _comboPunchTimer = 0.35f;
        public void TriggerShardPunch() => _shardPunchTimer = 0.30f;

        public void SpawnWorldPopup(Vector2 worldPos, string message, Color textColor, float scale = 1f)
        {
            if (_canvas == null || _splashActive) return;
            StartCoroutine(AnimateCanvasWorldPopup(worldPos, message, textColor, scale));
        }

        private IEnumerator AnimateCanvasWorldPopup(Vector2 startWorldPos, string message, Color textColor, float scale)
        {
            EnsureFontLoaded();
            float pillWidth = Mathf.Clamp(message.Length * 24f + 84f, 250f, 680f);
            float pillHeight = 68f;

            GameObject popGo = CreateSpriteBox(_canvas.transform, "FloatingPopupBadge", "ui_bar_track",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(pillWidth, pillHeight), false);

            // Keep popup inside GameplayHUD so it stays behind any modals
            if (_gameplayHudRoot != null)
            {
                popGo.transform.SetParent(_gameplayHudRoot.transform, false);
            }

            RectTransform popRt = popGo.GetComponent<RectTransform>();
            CanvasGroup cg = popGo.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            Text lbl = CreateLabel(popGo.transform, "PopupText", message, 30, FontStyle.Bold,
                textColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(pillWidth - 32f, pillHeight), true);

            Shadow sh = lbl.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.75f);
            sh.effectDistance = new Vector2(0f, -3f);

            RectTransform canvasRt = _canvas.GetComponent<RectTransform>();
            float duration = 0.95f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (popRt == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Convert world position + upward float to Canvas anchored position
                Vector2 curWorld = startWorldPos + new Vector2(0f, t * 1.4f);
                Camera cam = Camera.main;
                if (cam != null && canvasRt != null)
                {
                    Vector3 screenPt = cam.WorldToScreenPoint(new Vector3(curWorld.x, curWorld.y, 0f));
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPt, null, out Vector2 localPt))
                    {
                        popRt.anchoredPosition = localPt;
                    }
                }

                // Juicy elastic pop-in scale
                float popScale = t < 0.18f
                    ? Mathf.Lerp(0.45f, 1.16f, t / 0.18f)
                    : (t < 0.32f ? Mathf.Lerp(1.16f, 1.0f, (t - 0.18f) / 0.14f) : 1.0f);
                popRt.localScale = Vector3.one * (popScale * scale);

                // Smooth fade out in last 30%
                cg.alpha = t < 0.68f ? 1f : 1f - ((t - 0.68f) / 0.32f);
                yield return null;
            }

            if (popGo != null) Destroy(popGo);
        }

        private void AnimateModalPopIn(GameObject panel, RectTransform cardRt)
        {
            if (panel == null) return;
            panel.SetActive(true);
            if (cardRt != null) StartCoroutine(ModalSpringCoroutine(cardRt));
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
                float scale = 0.80f + 0.20f * Mathf.Sin(t * Mathf.PI * 0.5f) + 0.07f * Mathf.Sin(t * Mathf.PI);
                cardRt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            if (cardRt != null) cardRt.localScale = Vector3.one;
        }

        public void OnRunStarted()
        {
            _displayedScore = 0f;
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
            if (_gameplayHudRoot != null) _gameplayHudRoot.SetActive(true);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_upgradesPanel != null) _upgradesPanel.SetActive(false);
            if (_hullsPanel != null) _hullsPanel.SetActive(false);
            if (_leaderboardPanel != null) _leaderboardPanel.SetActive(false);
            if (_missionsPanel != null) _missionsPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
        }

        public void UpdateHUD()
        {
            if (GameManager.Instance == null) return;

            if (_shardsValueText != null)
                _shardsValueText.text = GameManager.Instance.TotalStarShards.ToString();

            RefreshMainMenuTopBar();

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
                        _warpPipImages[i].color = new Color(0.25f, 0.15f, 0.45f, 0.85f);
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
            if (_tierBannerHeader != null)
            {
                _tierBannerHeader.text = $"TIER {tier.tierNumber} • {tier.subtitle.ToUpper()}";
                _tierBannerHeader.color = tier.bannerColor;
            }
            if (_tierBannerTitle != null) _tierBannerTitle.text = tier.title;
            if (_tierBannerSubtitle != null) _tierBannerSubtitle.text = tier.playerTip;

            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                float prog = Mathf.Clamp01(t / 0.35f);
                if (_tierBannerGroup != null) _tierBannerGroup.alpha = prog;
                if (_tierBannerRt != null)
                {
                    float y = Mathf.Lerp(-125f, -165f, Mathf.Sin(prog * Mathf.PI * 0.5f));
                    _tierBannerRt.anchoredPosition = new Vector2(0f, y);
                }
                yield return null;
            }
            if (_tierBannerGroup != null) _tierBannerGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(2.6f);
            t = 0f;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                if (_tierBannerGroup != null) _tierBannerGroup.alpha = 1f - Mathf.Clamp01(t / 0.4f);
                yield return null;
            }
            if (_tierBannerGroup != null) _tierBannerGroup.alpha = 0f;
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

        public void OpenUpgradesModal()
        {
            RefreshUpgradesModal();
            AnimateModalPopIn(_upgradesPanel, _upgradesCardRt);
        }

        private void RefreshUpgradesModal()
        {
            if (GameManager.Instance == null) return;
            if (_upgShardBalanceText != null)
                _upgShardBalanceText.text = $"STAR SHARDS: ◆ {GameManager.Instance.TotalStarShards}";

            for (int i = 0; i < OrbitAssetFactory.Upgrades.Length; i++)
            {
                int lvl = GameManager.Instance.GetUpgradeLevel(i);
                int cost = GameManager.Instance.GetUpgradeCost(i);
                if (_upgLevelTexts != null && i < _upgLevelTexts.Length && _upgLevelTexts[i] != null)
                    _upgLevelTexts[i].text = $"LEVEL {lvl} / 5";
                if (_upgBtnLabels != null && i < _upgBtnLabels.Length && _upgBtnLabels[i] != null)
                    _upgBtnLabels[i].text = lvl >= 5 ? "MAXED" : $"◆ {cost}";

                if (_upgPipFlatList != null)
                {
                    for (int p = 0; p < 5; p++)
                    {
                        int idx = i * 5 + p;
                        if (idx < _upgPipFlatList.Length && _upgPipFlatList[idx] != null)
                        {
                            _upgPipFlatList[idx].color = p < lvl
                                ? OrbitAssetFactory.LimeGreen
                                : new Color(0.25f, 0.15f, 0.42f, 0.8f);
                        }
                    }
                }
            }
        }

        public void OpenHullsModal()
        {
            RefreshHullsModal();
            AnimateModalPopIn(_hullsPanel, _hullsCardRt);
        }

        private void RefreshHullsModal()
        {
            if (GameManager.Instance == null) return;
            if (_hullsShardBalanceText != null)
                _hullsShardBalanceText.text = $"STAR SHARDS: ◆ {GameManager.Instance.TotalStarShards}";

            for (int i = 0; i < OrbitAssetFactory.Hulls.Length; i++)
            {
                HullDefinition def = OrbitAssetFactory.Hulls[i];
                bool unlocked = GameManager.Instance.IsHullUnlocked(i);
                bool equipped = GameManager.Instance.EquippedHullId == i;
                if (_hullActionLabels != null && i < _hullActionLabels.Length && _hullActionLabels[i] != null)
                {
                    if (equipped) _hullActionLabels[i].text = "EQUIPPED";
                    else if (unlocked) _hullActionLabels[i].text = "EQUIP";
                    else _hullActionLabels[i].text = $"◆ {def.cost}";
                }
            }
        }

        public void OpenLeaderboardModal()
        {
            if (GameManager.Instance != null && _leaderboardRowsText != null)
            {
                List<LeaderboardEntry> list = GameManager.Instance.GetLeaderboard();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    string marker = e.pilotName == "KASHAN" ? "★ " : "   ";
                    sb.AppendLine($"#{i + 1}  {marker}{e.pilotName,-10}  {e.score,7:N0} pts   [Tier {e.tier}]");
                    if (i < list.Count - 1) sb.AppendLine();
                }
                _leaderboardRowsText.text = sb.ToString();
            }
            AnimateModalPopIn(_leaderboardPanel, _leaderboardCardRt);
        }

        public void OpenMissionsModal()
        {
            RefreshMissionsModal();
            AnimateModalPopIn(_missionsPanel, _missionsCardRt);
        }

        private void RefreshMissionsModal()
        {
            if (GameManager.Instance == null) return;
            CosmicTierInfo t = WorldSpawner.GetTierInfo(GameManager.Instance.SelectedStartingTier);
            if (_missionTierBtnText != null)
                _missionTierBtnText.text = $"SECTOR: TIER {t.tierNumber} • {t.title}";
            if (_missionSeedBtnText != null)
                _missionSeedBtnText.text = GameManager.Instance.UseDailySeed
                    ? $"MODE: DAILY SEED ({System.DateTime.UtcNow:yyyy-MM-dd})"
                    : "MODE: ENDLESS COSMIC ARCADE";
        }

        public void OpenSettingsModal()
        {
            RefreshSettingsModal();
            AnimateModalPopIn(_settingsPanel, _settingsCardRt);
        }

        private void RefreshSettingsModal()
        {
            if (_settingsAudioBtnText != null && AudioManager.Instance != null)
                _settingsAudioBtnText.text = AudioManager.Instance.IsMuted ? "SOUND & MUSIC: OFF" : "SOUND & MUSIC: ON";
            if (_settingsShakeBtnText != null && GameManager.Instance != null)
                _settingsShakeBtnText.text = GameManager.Instance.ScreenShakeEnabled ? "SCREEN SHAKE: ON" : "SCREEN SHAKE: OFF";
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

            if (_mainMenuPanel != null && _mainMenuPanel.activeSelf)
            {
                AnimateLettersWave(_mmLetterRects, _mmLetterBasePos, time, false);
            }

            if (GameManager.Instance != null && _scoreValueText != null)
            {
                float targetScore = GameManager.Instance.CurrentScore;
                _displayedScore = Mathf.MoveTowards(_displayedScore, targetScore, Mathf.Max(450f, Mathf.Abs(targetScore - _displayedScore) * 10f) * dt);
                _scoreValueText.text = Mathf.RoundToInt(_displayedScore).ToString("N0");
            }

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
                    float pulse = 1f + 0.04f * Mathf.Sin(time * 7f);
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

            float btnBreath = 1f + 0.035f * Mathf.Sin(time * 5.2f);
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

        // =====================================================================
        // EXPLICIT, PRECISION UI BUILDER HELPERS
        // =====================================================================
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

        private RectTransform CreateRect(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        private GameObject CreateSpriteBox(Transform parent, string name, string spriteName,
            Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, bool raycastTarget)
        {
            RectTransform rt = CreateRect(parent, name, anchor, anchor, pivot, anchoredPos, size);
            Image img = rt.gameObject.AddComponent<Image>();
            Sprite sp = OrbitAssetFactory.GetSprite(spriteName);
            img.sprite = sp;
            img.color = Color.white;
            img.raycastTarget = raycastTarget;

            Vector4 border = OrbitAssetFactory.GetSpriteBorder(spriteName);
            if (border != Vector4.zero)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
            }
            else
            {
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
            }
            return rt.gameObject;
        }

        private Button CreateArcadeButton(Transform parent, string name, string spriteName, string label, int fontSize,
            Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color textColor, UnityAction onClick)
        {
            GameObject go = CreateSpriteBox(parent, name, spriteName, anchor, pivot, anchoredPos, size, true);
            Image img = go.GetComponent<Image>();

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            CreateLabel(go.transform, "Label", label, fontSize, FontStyle.Bold,
                textColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size, true);
            return btn;
        }

        private Text CreateLabel(Transform parent, string name, string content, int fontSize, FontStyle style,
            Color color, TextAnchor alignment, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, bool addOutline)
        {
            RectTransform rt = CreateRect(parent, name, anchor, anchor, pivot, anchoredPos, size);

            Text txt = rt.gameObject.AddComponent<Text>();
            txt.font = _uiFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            if (addOutline)
            {
                Outline ol = rt.gameObject.AddComponent<Outline>();
                ol.effectColor = new Color(0.04f, 0.02f, 0.14f, 0.92f);
                ol.effectDistance = new Vector2(2f, -2f);
            }
            return txt;
        }
    }
}
