using System.Collections.Generic;
using UnityEngine;

namespace OrbitLauncher
{
    public struct CosmicTierInfo
    {
        public int tierNumber;
        public string title;
        public string subtitle;
        public string playerTip;
        public Color bannerColor;
    }

    public class WorldSpawner : MonoBehaviour
    {
        public static WorldSpawner Instance { get; private set; }

        public static readonly CosmicTierInfo[] Tiers = new CosmicTierInfo[]
        {
            new CosmicTierInfo
            {
                tierNumber = 1,
                title = "NEBULA",
                subtitle = "Static planets, open space. Learn the timing.",
                playerTip = "Release when the dotted tangent line turns LIME and points at the next planet!",
                bannerColor = new Color(0f, 0.94f, 1f, 1f)
            },
            new CosmicTierInfo
            {
                tierNumber = 2,
                title = "ASTEROID RING",
                subtitle = "Alternating orbit directions. Read the rotation.",
                playerTip = "Watch CW / CCW arrows on each ring. Drifting asteroids patrol transit lanes.",
                bannerColor = new Color(0f, 1f, 0.53f, 1f)
            },
            new CosmicTierInfo
            {
                tierNumber = 3,
                title = "BINARY STARS",
                subtitle = "Decaying orbits and laser barriers. Move fast.",
                playerTip = "Decaying planets collapse after 2 spins! Time your shot through laser pulses.",
                bannerColor = new Color(1f, 0.68f, 0.18f, 1f)
            },
            new CosmicTierInfo
            {
                tierNumber = 4,
                title = "PULSAR STORM",
                subtitle = "Pulsing gravity, spinning debris, magnetic mines.",
                playerTip = "Strong pulses spin your probe faster. Feel the rhythm before tapping!",
                bannerColor = new Color(0.75f, 0.35f, 1f, 1f)
            },
            new CosmicTierInfo
            {
                tierNumber = 5,
                title = "SINGULARITY",
                subtitle = "Moving planets, lensing, wandering black holes.",
                playerTip = "Lead your target on moving planets. Missed shots near a Black Hole spiral in!",
                bannerColor = new Color(1f, 0.165f, 0.373f, 1f)
            }
        };

        private readonly List<GravityAnchor> _activeAnchors = new List<GravityAnchor>();
        private readonly List<GameObject> _spawnedObjects = new List<GameObject>();

        private System.Random _rng;
        private int _nextHopIndex;
        private Vector2 _lastSpawnPos;
        private int _startTierOffset;

        private void Awake()
        {
            Instance = this;
        }

        public static CosmicTierInfo GetTierInfo(int tierNumber)
        {
            int idx = Mathf.Clamp(tierNumber - 1, 0, Tiers.Length - 1);
            return Tiers[idx];
        }

        public int GetTierForHop(int hopIndex)
        {
            int effectiveHop = hopIndex + _startTierOffset * 6;
            if (effectiveHop < 6) return 1;
            if (effectiveHop < 13) return 2;
            if (effectiveHop < 21) return 3;
            if (effectiveHop < 30) return 4;
            return 5;
        }

        public void ResetAndGenerateInitialWorld(int seed, int startingTier = 1)
        {
            ClearWorld();
            _rng = new System.Random(seed);
            _startTierOffset = Mathf.Clamp(startingTier - 1, 0, 4);
            _nextHopIndex = 0;
            _lastSpawnPos = Vector2.zero;

            // Spawn first 8 planets ahead
            for (int i = 0; i < 8; i++)
            {
                SpawnNextAnchor();
            }
        }

        public void EnsureWorldAhead(int currentHopIndex)
        {
            while (_nextHopIndex <= currentHopIndex + 6)
            {
                SpawnNextAnchor();
            }

            // Clean up old anchors far behind
            for (int i = _activeAnchors.Count - 1; i >= 0; i--)
            {
                if (_activeAnchors[i] == null)
                {
                    _activeAnchors.RemoveAt(i);
                }
                else if (_activeAnchors[i].hopIndex < currentHopIndex - 2)
                {
                    Destroy(_activeAnchors[i].gameObject);
                    _activeAnchors.RemoveAt(i);
                }
            }
        }

        public GravityAnchor GetAnchorByHopIndex(int hopIndex)
        {
            for (int i = 0; i < _activeAnchors.Count; i++)
            {
                if (_activeAnchors[i] != null && _activeAnchors[i].hopIndex == hopIndex)
                    return _activeAnchors[i];
            }
            return null;
        }

        public GravityAnchor GetNextTargetAnchor(Vector2 referencePos)
        {
            if (ProbeController.Instance != null && ProbeController.Instance.CurrentAnchor != null)
            {
                GravityAnchor next = GetAnchorByHopIndex(ProbeController.Instance.CurrentAnchor.hopIndex + 1);
                if (next != null) return next;
            }
            if (ProbeController.Instance != null && ProbeController.Instance.LastReleasedAnchor != null)
            {
                GravityAnchor next = GetAnchorByHopIndex(ProbeController.Instance.LastReleasedAnchor.hopIndex + 1);
                if (next != null) return next;
            }
            return GetAnchorByHopIndex(0);
        }

        private void SpawnNextAnchor()
        {
            int idx = _nextHopIndex++;
            int tier = GetTierForHop(idx);

            Vector2 prevPos = _lastSpawnPos;
            Vector2 pos;
            if (idx == 0)
            {
                pos = Vector2.zero;
            }
            else
            {
                // Progress forward-right and slightly upward with organic cosmic variation
                float dist = Mathf.Lerp(7.4f, 9.2f, (float)_rng.NextDouble());
                float angleDeg = Mathf.Lerp(-24f, 34f, (float)_rng.NextDouble());
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector2 step = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * dist;
                pos = prevPos + step;
            }
            _lastSpawnPos = pos;

            float planetRadius = Mathf.Lerp(0.88f, 1.15f, (float)_rng.NextDouble());
            float orbitRadius = Mathf.Lerp(2.15f, 2.55f, (float)_rng.NextDouble());

            // Direction rule per tier
            int dir = -1; // Default Clockwise in Tier 1
            if (tier >= 2 && idx > 0)
            {
                dir = (idx % 2 == 1) ? 1 : -1;
                if (tier >= 4 && _rng.NextDouble() > 0.5) dir = -dir;
            }

            bool decaying = (tier == 3 && idx > 0 && (idx % 2 == 0)) || (tier >= 4 && idx > 0 && _rng.NextDouble() < 0.30);
            bool pulsing = (tier == 4 && idx > 0) || (tier == 5 && _rng.NextDouble() < 0.45);
            bool moving = (tier == 5 && idx > 0 && (idx % 2 == 1));

            Vector2 toHere = idx > 0 ? (pos - prevPos).normalized : Vector2.right;
            Vector2 perpAxis = new Vector2(-toHere.y, toHere.x);
            float moveAmp = moving ? Mathf.Lerp(1.1f, 1.75f, (float)_rng.NextDouble()) : 0f;

            GameObject anchorGo = new GameObject($"GravityAnchor_{idx}_Tier{tier}");
            anchorGo.transform.SetParent(transform, false);
            GravityAnchor anchor = anchorGo.AddComponent<GravityAnchor>();
            anchor.Initialize(idx, tier, pos, planetRadius, orbitRadius, dir, decaying, pulsing, moving, perpAxis, moveAmp);

            _activeAnchors.Add(anchor);
            _spawnedObjects.Add(anchorGo);

            if (idx > 0)
            {
                SpawnTransitShardsAndHazards(idx, tier, prevPos, pos, toHere, perpAxis);
            }
        }

        private void SpawnTransitShardsAndHazards(int hopIdx, int tier, Vector2 fromPos, Vector2 toPos, Vector2 forward, Vector2 perp)
        {
            Vector2 midPoint = Vector2.Lerp(fromPos, toPos, 0.5f);
            float gapDist = Vector2.Distance(fromPos, toPos);

            // 1. Spawn Star Shards along the transit line or slightly curved arc
            int shardCount = (_rng.NextDouble() < 0.65) ? 2 : 1;
            for (int s = 0; s < shardCount; s++)
            {
                float t = shardCount == 1 ? 0.5f : Mathf.Lerp(0.38f, 0.62f, s);
                Vector2 shardPos = Vector2.Lerp(fromPos, toPos, t) + perp * Mathf.Lerp(-0.35f, 0.35f, (float)_rng.NextDouble());
                GameObject shardGo = new GameObject($"StarShard_{hopIdx}_{s}");
                shardGo.transform.SetParent(transform, false);
                StarShard shard = shardGo.AddComponent<StarShard>();
                shard.Initialize(shardPos, hopIdx + s * 0.9f);
                _spawnedObjects.Add(shardGo);
            }

            // Tier 1 has zero obstacles per GDD Page 6 ("Obstacles: None. Open-space margins.")
            if (tier <= 1) return;

            // Tier 2: Slow asteroid rocks drifting along transit lines
            if (tier == 2)
            {
                float side = (_rng.NextDouble() > 0.5) ? 1.55f : -1.55f;
                Vector2 hPos = midPoint + perp * side;
                SpawnHazard(HazardType.Asteroid, hPos, perp, 1.35f, 0.95f, hopIdx * 0.7f);
            }
            // Tier 3: Laser barriers bridging planetary gaps + occasional asteroid
            else if (tier == 3)
            {
                if (hopIdx % 2 == 1)
                {
                    Vector2 hPos = midPoint + perp * 1.15f;
                    GameObject laserGo = SpawnHazard(HazardType.LaserBarrier, hPos, perp, 0.8f, 1.1f, hopIdx * 1.1f);
                    float rot = Mathf.Atan2(perp.y, perp.x) * Mathf.Rad2Deg - 90f;
                    laserGo.transform.rotation = Quaternion.Euler(0f, 0f, rot);
                }
                else
                {
                    SpawnHazard(HazardType.Asteroid, midPoint + perp * -1.4f, perp, 1.5f, 1.25f, hopIdx);
                }
            }
            // Tier 4: Pulsar Storm - Spinning Debris & Magnetic Mines
            else if (tier == 4)
            {
                if (hopIdx % 2 == 0)
                {
                    Vector2 hPos = midPoint + perp * 1.75f;
                    SpawnHazard(HazardType.SpinningDebris, hPos, perp, 0.6f, 0.9f, hopIdx);
                }
                else
                {
                    Vector2 hPos = midPoint + perp * -1.65f;
                    SpawnHazard(HazardType.MagneticMine, hPos, perp, 1.1f, 1.3f, hopIdx);
                }
            }
            // Tier 5: Singularity - Wandering Black Holes + High-speed hazards
            else if (tier >= 5)
            {
                float side = (hopIdx % 2 == 0) ? 2.45f : -2.45f;
                Vector2 bhPos = midPoint + perp * side;
                SpawnHazard(HazardType.BlackHole, bhPos, perp, 0.85f, 0.8f, hopIdx);

                if (_rng.NextDouble() > 0.45 && gapDist > 7.8f)
                {
                    SpawnHazard(HazardType.Asteroid, midPoint - perp * (side * 0.65f), perp, 1.2f, 1.4f, hopIdx + 2f);
                }
            }
        }

        private GameObject SpawnHazard(HazardType type, Vector2 pos, Vector2 axis, float amp, float freq, float phase)
        {
            GameObject go = new GameObject($"Hazard_{type}");
            go.transform.SetParent(transform, false);
            Hazard h = go.AddComponent<Hazard>();
            h.Initialize(type, pos, axis, amp, freq, phase);
            _spawnedObjects.Add(go);
            return go;
        }

        public void ClearWorld()
        {
            for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (_spawnedObjects[i] != null)
                {
                    Destroy(_spawnedObjects[i]);
                }
            }
            _spawnedObjects.Clear();
            _activeAnchors.Clear();
        }
    }
}

