using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitLauncher
{
    [Serializable]
    public struct LeaderboardEntry
    {
        public string pilotName;
        public int score;
        public int tier;
        public string hullName;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsSplashActive { get; private set; } = true;

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }
        public int TotalStarShards { get; private set; }
        public int RunStarShards { get; private set; }
        public int ConsecutivePerfectHops { get; private set; }
        public int ComboMultiplier { get; private set; } = 1;
        public int CurrentTier { get; private set; } = 1;
        public int HighestTierUnlocked { get; private set; } = 1;
        public int SelectedStartingTier { get; private set; } = 1;
        public int EquippedHullId { get; private set; }
        public bool UseDailySeed { get; private set; }

        private readonly List<LeaderboardEntry> _leaderboard = new List<LeaderboardEntry>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Application.targetFrameRate = 60;
            LoadPersistentData();
        }

        private void Start()
        {
            // Prepare background world while the animated splash screen runs
            IsSplashActive = true;
            IsPlaying = false;
            int seed = Environment.TickCount;
            if (WorldSpawner.Instance != null)
            {
                WorldSpawner.Instance.ResetAndGenerateInitialWorld(seed, SelectedStartingTier);
            }
            if (CameraRig.Instance != null)
            {
                CameraRig.Instance.SnapToTarget(Vector2.zero);
            }
        }

        public void OnSplashCompleted()
        {
            IsSplashActive = false;
            StartNewRun(false);
        }

        private void LoadPersistentData()
        {
            BestScore = PlayerPrefs.GetInt("OL_BestScore", 0);
            TotalStarShards = PlayerPrefs.GetInt("OL_TotalShards", 30);
            EquippedHullId = PlayerPrefs.GetInt("OL_EquippedHull", 0);
            HighestTierUnlocked = Mathf.Max(5, PlayerPrefs.GetInt("OL_HighestTier", 5));
            PlayerPrefs.SetInt("OL_HullUnlocked_0", 1);

            LoadLeaderboard();
        }

        private void LoadLeaderboard()
        {
            _leaderboard.Clear();
            _leaderboard.Add(new LeaderboardEntry { pilotName = "NOVA-9", score = 18920, tier = 5, hullName = "PULSAR CORE" });
            _leaderboard.Add(new LeaderboardEntry { pilotName = "VORTEX", score = 14200, tier = 4, hullName = "ION DART" });
            _leaderboard.Add(new LeaderboardEntry { pilotName = "LYRA-X", score = 9800, tier = 3, hullName = "NEON GLIDER" });
            _leaderboard.Add(new LeaderboardEntry { pilotName = "KASHAN", score = Mathf.Max(5400, BestScore), tier = HighestTierUnlocked, hullName = OrbitAssetFactory.Hulls[Mathf.Clamp(EquippedHullId, 0, 3)].name });
            _leaderboard.Add(new LeaderboardEntry { pilotName = "ORION-7", score = 3200, tier = 2, hullName = "SCOUT PROBE" });
            SortLeaderboard();
        }

        private void SortLeaderboard()
        {
            _leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        }

        public List<LeaderboardEntry> GetLeaderboard()
        {
            return _leaderboard;
        }

        public void SetStartingTier(int tier)
        {
            SelectedStartingTier = Mathf.Clamp(tier, 1, 5);
        }

        public void ToggleDailySeedMode()
        {
            UseDailySeed = !UseDailySeed;
        }

        public void StartNewRun(bool fromRetryButton = true)
        {
            Time.timeScale = 1f;
            IsPaused = false;
            IsSplashActive = false;
            IsPlaying = true;

            CurrentScore = 0;
            RunStarShards = 0;
            ConsecutivePerfectHops = 0;
            ComboMultiplier = 1;
            CurrentTier = SelectedStartingTier;

            int seed = UseDailySeed
                ? (DateTime.UtcNow.Year * 10000 + DateTime.UtcNow.Month * 100 + DateTime.UtcNow.Day)
                : Environment.TickCount;

            if (WorldSpawner.Instance != null)
            {
                WorldSpawner.Instance.ResetAndGenerateInitialWorld(seed, SelectedStartingTier);
            }

            if (ProbeController.Instance != null)
            {
                ProbeController.Instance.ApplyHull(EquippedHullId);
                GravityAnchor firstAnchor = WorldSpawner.Instance != null ? WorldSpawner.Instance.GetAnchorByHopIndex(0) : null;
                Vector2 targetPos = firstAnchor != null ? (Vector2)firstAnchor.transform.position : Vector2.zero;
                Vector2 launchOrigin = targetPos + new Vector2(-4.2f, -1.15f);
                Vector2 launchDir = (targetPos + new Vector2(0f, -1.2f) - launchOrigin).normalized;
                ProbeController.Instance.ResetAndLaunch(launchOrigin, launchDir);

                if (CameraRig.Instance != null)
                {
                    CameraRig.Instance.SnapToTarget(targetPos);
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnRunStarted();
                UIManager.Instance.UpdateHUD();
                UIManager.Instance.ShowTierBanner(WorldSpawner.GetTierInfo(CurrentTier));
            }
        }

        public void OnPlanetCaptured(GravityAnchor anchor, bool wasCleanPerfectAim)
        {
            if (!IsPlaying) return;

            if (WorldSpawner.Instance != null)
            {
                WorldSpawner.Instance.EnsureWorldAhead(anchor.hopIndex);
            }

            if (anchor.tierIndex > CurrentTier)
            {
                CurrentTier = anchor.tierIndex;
                if (CurrentTier > HighestTierUnlocked)
                {
                    HighestTierUnlocked = CurrentTier;
                    PlayerPrefs.SetInt("OL_HighestTier", HighestTierUnlocked);
                }
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowTierBanner(WorldSpawner.GetTierInfo(CurrentTier));
                }
            }

            if (anchor.hopIndex == 0)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCaptureChime(0);
                if (VFXManager.Instance != null) VFXManager.Instance.SpawnCapturePulse(anchor.transform.position, anchor.orbitRadius, false);
                if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
                return;
            }

            if (wasCleanPerfectAim)
            {
                ConsecutivePerfectHops++;
                if (ConsecutivePerfectHops == 1) ComboMultiplier = 1;
                else if (ConsecutivePerfectHops == 2) ComboMultiplier = 2;
                else ComboMultiplier = 4;

                int points = 100 * ComboMultiplier;
                AddScore(points);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCaptureChime(ConsecutivePerfectHops);
                }
                if (VFXManager.Instance != null)
                {
                    VFXManager.Instance.SpawnCapturePulse(anchor.transform.position, anchor.orbitRadius, true);
                    string popText = $"PERFECT! +{points}";
                    VFXManager.Instance.SpawnFloatingPopup(
                        (Vector2)anchor.transform.position + Vector2.up * (anchor.orbitRadius + 0.55f),
                        popText,
                        OrbitAssetFactory.LimeGreen,
                        ComboMultiplier >= 4 ? 1.15f : 1.0f
                    );
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.TriggerComboPunch();
                }

                if (ConsecutivePerfectHops > 0 && ConsecutivePerfectHops % 5 == 0 && ProbeController.Instance != null)
                {
                    ProbeController.Instance.ActivateWarpInvulnerability(3.5f);
                }
            }
            else
            {
                ConsecutivePerfectHops = 0;
                ComboMultiplier = 1;
                AddScore(100);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCaptureChime(0);
                }
                if (VFXManager.Instance != null)
                {
                    VFXManager.Instance.SpawnCapturePulse(anchor.transform.position, anchor.orbitRadius, false);
                    VFXManager.Instance.SpawnFloatingPopup(
                        (Vector2)anchor.transform.position + Vector2.up * (anchor.orbitRadius + 0.45f),
                        "CAPTURED +100",
                        OrbitAssetFactory.NeonCyan,
                        0.9f
                    );
                }
            }

            if (ProbeController.Instance != null)
            {
                ProbeController.Instance.UpdateTrailGradient(ComboMultiplier);
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHUD();
            }
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                PlayerPrefs.SetInt("OL_BestScore", BestScore);
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHUD();
                UIManager.Instance.TriggerScorePunch();
            }
        }

        public void CollectStarShard(int count)
        {
            RunStarShards += count;
            TotalStarShards += count;
            AddScore(50 * ComboMultiplier);
            PlayerPrefs.SetInt("OL_TotalShards", TotalStarShards);
            PlayerPrefs.Save();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHUD();
                UIManager.Instance.TriggerShardPunch();
            }
        }

        public void OnProbeDied(string causeOfDeath)
        {
            if (!IsPlaying) return;
            IsPlaying = false;

            for (int i = 0; i < _leaderboard.Count; i++)
            {
                if (_leaderboard[i].pilotName == "KASHAN")
                {
                    if (BestScore >= _leaderboard[i].score)
                    {
                        _leaderboard[i] = new LeaderboardEntry
                        {
                            pilotName = "KASHAN",
                            score = BestScore,
                            tier = CurrentTier,
                            hullName = OrbitAssetFactory.Hulls[Mathf.Clamp(EquippedHullId, 0, 3)].name
                        };
                    }
                    break;
                }
            }
            SortLeaderboard();
            PlayerPrefs.Save();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOverScreen(causeOfDeath);
            }
        }

        public void TogglePause()
        {
            if (!IsPlaying) return;
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetPauseMenuVisible(IsPaused);
            }
        }

        public bool IsHullUnlocked(int hullId)
        {
            if (hullId == 0) return true;
            return PlayerPrefs.GetInt("OL_HullUnlocked_" + hullId, 0) == 1;
        }

        public bool TryUnlockOrEquipHull(int hullId)
        {
            if (hullId < 0 || hullId >= OrbitAssetFactory.Hulls.Length) return false;
            HullDefinition def = OrbitAssetFactory.Hulls[hullId];

            if (IsHullUnlocked(hullId))
            {
                EquippedHullId = hullId;
                PlayerPrefs.SetInt("OL_EquippedHull", EquippedHullId);
                PlayerPrefs.Save();
                if (ProbeController.Instance != null)
                {
                    ProbeController.Instance.ApplyHull(EquippedHullId);
                }
                return true;
            }

            if (TotalStarShards >= def.cost)
            {
                TotalStarShards -= def.cost;
                EquippedHullId = hullId;
                PlayerPrefs.SetInt("OL_TotalShards", TotalStarShards);
                PlayerPrefs.SetInt("OL_HullUnlocked_" + hullId, 1);
                PlayerPrefs.SetInt("OL_EquippedHull", EquippedHullId);
                PlayerPrefs.Save();
                if (ProbeController.Instance != null)
                {
                    ProbeController.Instance.ApplyHull(EquippedHullId);
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayWarpShield();
                }
                return true;
            }
            return false;
        }

        void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }
    }
}
