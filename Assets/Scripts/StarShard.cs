using UnityEngine;

namespace OrbitLauncher
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class StarShard : MonoBehaviour
    {
        private Vector2 _basePos;
        private float _phase;
        private Transform _visual;
        private bool _collected;

        public void Initialize(Vector2 pos, float phaseOffset = 0f)
        {
            _basePos = pos;
            transform.position = pos;
            _phase = phaseOffset;
            _collected = false;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;

            GameObject vis = new GameObject("ShardVisual");
            vis.transform.SetParent(transform, false);
            _visual = vis.transform;
            _visual.localScale = Vector3.one * 0.48f;

            SpriteRenderer sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = OrbitAssetFactory.GetSprite("star_shard");
            sr.sortingOrder = 6;
        }

        private void Update()
        {
            if (_collected) return;

            // Gentle float & spin
            float bob = Mathf.Sin(Time.time * 3.8f + _phase) * 0.12f;
            float scalePulse = 0.46f + Mathf.Sin(Time.time * 5.2f + _phase) * 0.05f;
            if (_visual != null)
            {
                _visual.localPosition = new Vector3(0f, bob, 0f);
                _visual.localScale = new Vector3(scalePulse, scalePulse, 1f);
                _visual.Rotate(0f, 0f, 55f * Time.deltaTime);
            }

            // Magnetic attraction when probe is nearby (scaled by Gravity Magnet Upgrade!)
            if (ProbeController.Instance != null && ProbeController.Instance.CurrentState != ProbeState.DeathAndReset)
            {
                int magnetLvl = GameManager.Instance != null ? GameManager.Instance.GetUpgradeLevel(0) : 1;
                float pullDist = 1.65f + (magnetLvl - 1) * 0.75f;
                Vector2 probePos = ProbeController.Instance.transform.position;
                float dist = Vector2.Distance(transform.position, probePos);
                if (dist < pullDist)
                {
                    transform.position = Vector2.MoveTowards(transform.position, probePos, (9.5f + magnetLvl * 1.5f) * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector2.Lerp(transform.position, _basePos, 4f * Time.deltaTime);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            ProbeController probe = other.GetComponent<ProbeController>();
            if (probe == null) return;

            _collected = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectStarShard(1);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShardPickup();
            }
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnShardPickup(transform.position);
                VFXManager.Instance.SpawnFloatingPopup(transform.position, "+1 SHARD", OrbitAssetFactory.StarGold, 0.85f);
            }
            Destroy(gameObject);
        }
    }
}

