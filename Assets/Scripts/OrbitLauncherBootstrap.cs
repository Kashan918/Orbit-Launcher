using UnityEngine;

namespace OrbitLauncher
{
    public class OrbitLauncherBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapOnPlay()
        {
            EnsureSceneSystems();
        }

        public static void EnsureSceneSystems()
        {
            // 1. Setup Main Camera + CameraRig
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = FindFirstObjectByType<Camera>();
            }
            if (mainCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            CameraRig rig = mainCam.GetComponent<CameraRig>();
            if (rig == null)
            {
                rig = mainCam.gameObject.AddComponent<CameraRig>();
            }

            // 2. Setup Core Managers
            if (FindFirstObjectByType<AudioManager>() == null)
            {
                new GameObject("AudioManager").AddComponent<AudioManager>();
            }
            if (FindFirstObjectByType<VFXManager>() == null)
            {
                new GameObject("VFXManager").AddComponent<VFXManager>();
            }
            if (FindFirstObjectByType<WorldSpawner>() == null)
            {
                new GameObject("WorldSpawner").AddComponent<WorldSpawner>();
            }

            // 3. Setup Player Scout Probe
            ProbeController probe = FindFirstObjectByType<ProbeController>();
            if (probe == null)
            {
                GameObject probeGo = new GameObject("ScoutProbe");
                probe = probeGo.AddComponent<ProbeController>();
            }
            rig.targetProbe = probe.transform;

            // 4. Setup UI Manager
            if (FindFirstObjectByType<UIManager>() == null)
            {
                new GameObject("UIManager").AddComponent<UIManager>();
            }

            // 5. Setup Game Manager
            if (FindFirstObjectByType<GameManager>() == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
            }
        }

        private void Awake()
        {
            EnsureSceneSystems();
        }
    }
}

