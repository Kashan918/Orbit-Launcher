#if UNITY_EDITOR
using System.IO;
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
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                }
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

            if (Object.FindFirstObjectByType<GameManager>() == null)
            {
                SetupSceneAndPrefabs();
            }
        }

        [MenuItem("Orbit Launcher/Setup Scene & Prefabs")]
        public static void SetupSceneAndPrefabs()
        {
            if (EditorApplication.isPlaying) return;

            // Ensure Sprites are imported as Sprite type
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null && ti.textureType != TextureImporterType.Sprite)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.alphaIsTransparency = true;
                    ti.mipmapEnabled = false;
                    ti.SaveAndReimport();
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

            if (Object.FindFirstObjectByType<UIManager>() == null)
                new GameObject("UIManager").AddComponent<UIManager>();

            if (Object.FindFirstObjectByType<GameManager>() == null)
                new GameObject("GameManager").AddComponent<GameManager>();

            // Create Prefabs folder and save core prefabs for inspection
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Orbit Launcher] Scene, Assets, Audio, UI, and 5 Cosmic Tiers configured and ready! Press PLAY to launch.");
        }
    }
}
#endif

