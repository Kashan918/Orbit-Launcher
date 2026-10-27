#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OrbitLauncher.Editor
{
    public class OrbitSpritePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.Contains("Resources/Sprites/"))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                string spriteName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                Vector4 border = OrbitAssetFactory.GetSpriteBorder(spriteName);

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                if (border != Vector4.zero)
                {
                    importer.spriteBorder = border;
                }
            }
        }
    }

    [CustomEditor(typeof(UIManager))]
    public class UIManagerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UIManager ui = (UIManager)target;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("QUICK PANEL SWITCHER (EDIT MODE)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "All UI Panels live under UIManager -> OrbitLauncherCanvas in the Hierarchy. " +
                "Any changes you make to positions, sizes, texts, or colors in the Hierarchy/Scene View are saved in the scene and preserved when you press Play!",
                MessageType.Info);

            if (!ui.HasBuiltHierarchy)
            {
                GUI.backgroundColor = new Color(0.3f, 1f, 0.4f);
                if (GUILayout.Button("Build UI Panels in Hierarchy Now", GUILayout.Height(34)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Build UI Hierarchy");
                    ui.RebuildHierarchyInEditor();
                    EditorUtility.SetDirty(ui);
                    EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Main Menu", GUILayout.Height(26))) SwitchPanel(ui, "Option3MainMenu");
                if (GUILayout.Button("Splash Screen", GUILayout.Height(26))) SwitchPanel(ui, "SplashScreenOverlay");
                if (GUILayout.Button("Gameplay HUD", GUILayout.Height(26))) SwitchPanel(ui, "GameplayHUD");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Upgrades", GUILayout.Height(26))) SwitchPanel(ui, "UpgradesModal");
                if (GUILayout.Button("Skins", GUILayout.Height(26))) SwitchPanel(ui, "HullsModal");
                if (GUILayout.Button("Ranks", GUILayout.Height(26))) SwitchPanel(ui, "LeaderboardModal");
                if (GUILayout.Button("Missions", GUILayout.Height(26))) SwitchPanel(ui, "MissionsModal");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Settings", GUILayout.Height(26))) SwitchPanel(ui, "SettingsModal");
                if (GUILayout.Button("Pause", GUILayout.Height(26))) SwitchPanel(ui, "PauseModal");
                if (GUILayout.Button("Game Over", GUILayout.Height(26))) SwitchPanel(ui, "GameOverScreen");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);
                GUI.backgroundColor = new Color(1f, 0.75f, 0.3f);
                if (GUILayout.Button("Reset / Rebuild Default UI Panels in Hierarchy", GUILayout.Height(26)))
                {
                    if (EditorUtility.DisplayDialog("Reset UI Hierarchy?",
                        "This will rebuild the default OrbitLauncherCanvas and all UI panels in the Hierarchy. Continue?",
                        "Yes, Rebuild", "Cancel"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Rebuild UI Hierarchy");
                        ui.RebuildHierarchyInEditor();
                        EditorUtility.SetDirty(ui);
                        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
                    }
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("GAME BUILD ICON SELECTOR (1024x1024)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Icon 1: Cosmic", GUILayout.Height(26)))
                OrbitLauncherEditorSetup.ApplyBuildIcon("build_icon_1_cosmic_arcade");
            if (GUILayout.Button("Icon 2: Neon", GUILayout.Height(26)))
                OrbitLauncherEditorSetup.ApplyBuildIcon("build_icon_2_neon_slingshot");
            if (GUILayout.Button("Icon 3: Black Hole", GUILayout.Height(26)))
                OrbitLauncherEditorSetup.ApplyBuildIcon("build_icon_3_blackhole_warp");
            if (GUILayout.Button("Icon 4: Gold Crest", GUILayout.Height(26)))
                OrbitLauncherEditorSetup.ApplyBuildIcon("build_icon_4_golden_crest");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            DrawDefaultInspector();
        }

        private static void SwitchPanel(UIManager ui, string panelName)
        {
            Transform canvasTr = ui.transform.Find("OrbitLauncherCanvas");
            if (canvasTr == null) return;
            Transform targetPanel = canvasTr.Find(panelName);
            if (targetPanel != null)
            {
                ui.ShowOnlyPanelInEditor(targetPanel.gameObject);
                Selection.activeGameObject = targetPanel.gameObject;
                EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            }
        }
    }

    [InitializeOnLoad]
    public static class OrbitLauncherEditorSetup
    {
        static OrbitLauncherEditorSetup()
        {
            EditorApplication.delayCall += AutoConfigureOnLoad;
        }

        private static void AutoConfigureOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            UIManager ui = Object.FindFirstObjectByType<UIManager>();
            if (Object.FindFirstObjectByType<GameManager>() == null || ui == null || !ui.HasBuiltHierarchy)
            {
                SetupSceneAndPrefabs();
            }
        }

        [MenuItem("Orbit Launcher/Setup Scene & Build UI Hierarchy")]
        public static void SetupSceneAndPrefabs()
        {
            if (EditorApplication.isPlaying) return;

            // Ensure Sprites are imported as Sprite type with 9-Slice borders
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    string spriteName = System.IO.Path.GetFileNameWithoutExtension(path);
                    Vector4 border = OrbitAssetFactory.GetSpriteBorder(spriteName);
                    bool needsReimport = ti.textureType != TextureImporterType.Sprite ||
                                         (border != Vector4.zero && ti.spriteBorder != border);
                    if (needsReimport)
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        ti.spriteImportMode = SpriteImportMode.Single;
                        ti.alphaIsTransparency = true;
                        ti.mipmapEnabled = false;
                        if (border != Vector4.zero) ti.spriteBorder = border;
                        ti.SaveAndReimport();
                    }
                }
            }

            // Populate current scene hierarchy
            Camera mainCam = Camera.main;
            if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = OrbitAssetFactory.DeepVoidBlue;
                mainCam.orthographic = true;
                mainCam.orthographicSize = 8.2f;
                if (mainCam.GetComponent<CameraRig>() == null)
                {
                    mainCam.gameObject.AddComponent<CameraRig>();
                }
            }

            if (Object.FindFirstObjectByType<AudioManager>() == null)
                new GameObject("AudioManager").AddComponent<AudioManager>();

            if (Object.FindFirstObjectByType<VFXManager>() == null)
                new GameObject("VFXManager").AddComponent<VFXManager>();

            if (Object.FindFirstObjectByType<WorldSpawner>() == null)
                new GameObject("WorldSpawner").AddComponent<WorldSpawner>();

            ProbeController probe = Object.FindFirstObjectByType<ProbeController>();
            if (probe == null)
                probe = new GameObject("ScoutProbe").AddComponent<ProbeController>();

            if (mainCam != null && mainCam.GetComponent<CameraRig>() != null)
                mainCam.GetComponent<CameraRig>().targetProbe = probe.transform;

            UIManager ui = Object.FindFirstObjectByType<UIManager>();
            if (ui == null)
                ui = new GameObject("UIManager").AddComponent<UIManager>();

            // Build all UI panels directly into the Scene Hierarchy in Edit Mode!
            if (!ui.HasBuiltHierarchy)
            {
                ui.RebuildHierarchyInEditor();
                EditorUtility.SetDirty(ui);
            }

            if (Object.FindFirstObjectByType<GameManager>() == null)
                new GameObject("GameManager").AddComponent<GameManager>();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            ApplyBuildIcon("build_icon_1_cosmic_arcade");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Orbit Launcher] All UI Panels built in Scene Hierarchy under UIManager -> OrbitLauncherCanvas! You can freely edit any panel in Edit Mode.");
        }

        [MenuItem("Orbit Launcher/Select Build Icon/Icon 1 - Vibrant Cosmic Arcade")]
        public static void SelectBuildIcon1() => ApplyBuildIcon("build_icon_1_cosmic_arcade");

        [MenuItem("Orbit Launcher/Select Build Icon/Icon 2 - Neon Cyber Slingshot")]
        public static void SelectBuildIcon2() => ApplyBuildIcon("build_icon_2_neon_slingshot");

        [MenuItem("Orbit Launcher/Select Build Icon/Icon 3 - Black Hole Singularity Warp")]
        public static void SelectBuildIcon3() => ApplyBuildIcon("build_icon_3_blackhole_warp");

        [MenuItem("Orbit Launcher/Select Build Icon/Icon 4 - Golden Orbit Crest Emblem")]
        public static void SelectBuildIcon4() => ApplyBuildIcon("build_icon_4_golden_crest");

        public static void ApplyBuildIcon(string iconFileName)
        {
            string iconPath = $"Assets/Resources/Icons/{iconFileName}.png";
            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);
            Texture2D iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (iconTex != null)
            {
                PlayerSettings.companyName = "Kashan Studios";
                PlayerSettings.productName = "Orbit Launcher";
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { iconTex }, IconKind.Any);
                Debug.Log($"[Orbit Launcher] Build Icon set to: {iconPath}");
            }
        }

        [MenuItem("Orbit Launcher/Rebuild Default UI Panels in Hierarchy")]
        public static void ForceRebuildUIInHierarchy()
        {
            if (EditorApplication.isPlaying) return;
            UIManager ui = Object.FindFirstObjectByType<UIManager>();
            if (ui == null)
                ui = new GameObject("UIManager").AddComponent<UIManager>();

            ui.RebuildHierarchyInEditor();
            EditorUtility.SetDirty(ui);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }
    }
}
#endif
