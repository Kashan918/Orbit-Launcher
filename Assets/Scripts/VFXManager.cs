using System.Collections;
using UnityEngine;

namespace OrbitLauncher
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnReleaseBurst(Vector2 position, Vector2 launchDir, Color primaryColor, Color secondaryColor)
        {
            GameObject go = new GameObject("VFX_ReleaseBurst");
            go.transform.position = new Vector3(position.x, position.y, -0.5f);
            float angle = Mathf.Atan2(-launchDir.y, -launchDir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 0.35f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 6.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.24f);
            main.startColor = new ParticleSystem.MinMaxGradient(primaryColor, secondaryColor);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 24) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 32f;
            shape.radius = 0.08f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            SetupParticleRenderer(ps, 15);
            ps.Play();
            Destroy(go, 0.8f);
        }

        public void SpawnCapturePulse(Vector2 planetPos, float orbitRadius, bool isPerfect)
        {
            StartCoroutine(AnimateRingShockwave(planetPos, orbitRadius * 0.4f, orbitRadius * 1.15f,
                isPerfect ? OrbitAssetFactory.LimeGreen : OrbitAssetFactory.NeonCyan, 0.38f));

            if (isPerfect)
            {
                SpawnRadialSparks(planetPos, 20, OrbitAssetFactory.LimeGreen, OrbitAssetFactory.NeonCyan, 3.5f, 0.45f);
            }
        }

        public void SpawnDeathShatter(Vector2 position, Color hullPrimary, Color hullSecondary)
        {
            SpawnRadialSparks(position, 65, hullPrimary, OrbitAssetFactory.AlertCrimson, 8.5f, 0.75f);
            StartCoroutine(AnimateRingShockwave(position, 0.2f, 3.6f, OrbitAssetFactory.AlertCrimson, 0.48f));
        }

        public void SpawnShardPickup(Vector2 position)
        {
            SpawnRadialSparks(position, 22, OrbitAssetFactory.StarGold, OrbitAssetFactory.NeonCyan, 4.5f, 0.45f);
            StartCoroutine(AnimateRingShockwave(position, 0.15f, 1.4f, OrbitAssetFactory.StarGold, 0.30f));
        }

        public void SpawnRadialSparks(Vector2 position, int count, Color colorA, Color colorB, float maxSpeed, float lifetime)
        {
            GameObject go = new GameObject("VFX_RadialSparks");
            go.transform.position = new Vector3(position.x, position.y, -0.6f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.45f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, maxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.28f);
            main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.12f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.dampen = 0.18f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            SetupParticleRenderer(ps, 18);
            ps.Play();
            Destroy(go, lifetime + 0.3f);
        }

        private void SetupParticleRenderer(ParticleSystem ps, int sortingOrder)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = OrbitAssetFactory.GetDefaultSpriteMaterial();
            renderer.sortingOrder = sortingOrder;
        }

        private IEnumerator AnimateRingShockwave(Vector2 pos, float startRadius, float endRadius, Color color, float duration)
        {
            GameObject ringObj = new GameObject("VFX_RingShockwave");
            ringObj.transform.position = new Vector3(pos.x, pos.y, -0.4f);
            LineRenderer lr = ringObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            int segments = 48;
            lr.positionCount = segments;
            lr.sharedMaterial = OrbitAssetFactory.GetDefaultSpriteMaterial();
            lr.sortingOrder = 16;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                float r = Mathf.Lerp(startRadius, endRadius, ease);
                float alpha = (1f - t) * 0.85f;
                Color c = new Color(color.r, color.g, color.b, alpha);
                lr.startColor = c;
                lr.endColor = c;
                lr.startWidth = Mathf.Lerp(0.14f, 0.02f, t);
                lr.endWidth = lr.startWidth;

                for (int i = 0; i < segments; i++)
                {
                    float ang = (i / (float)segments) * Mathf.PI * 2f;
                    lr.SetPosition(i, new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f));
                }
                yield return null;
            }
            Destroy(ringObj);
        }

        public void SpawnFloatingPopup(Vector2 worldPos, string text, Color textColor, float scale = 1f)
        {
            StartCoroutine(AnimateFloatingPopup(worldPos, text, textColor, scale));
        }

        private IEnumerator AnimateFloatingPopup(Vector2 startPos, string message, Color color, float scale)
        {
            GameObject popupGo = new GameObject("VFX_FloatingPopup");
            popupGo.transform.position = new Vector3(startPos.x, startPos.y, -2f);
            popupGo.transform.localScale = Vector3.one * (0.09f * scale);

            // Background pill sprite
            GameObject bgGo = new GameObject("PillBg");
            bgGo.transform.SetParent(popupGo.transform, false);
            SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
            bgSr.sprite = OrbitAssetFactory.GetSprite("solid_disc");
            bgSr.color = new Color(0.02f, 0.05f, 0.12f, 0.85f);
            bgSr.sortingOrder = 29;
            bgGo.transform.localScale = new Vector3(message.Length * 0.55f + 1.2f, 1.45f, 1f);

            TextMesh tm = popupGo.AddComponent<TextMesh>();
            tm.text = message;
            tm.fontSize = 36;
            tm.characterSize = 0.25f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            MeshRenderer mr = popupGo.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 30;

            float duration = 0.95f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (popupGo == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                popupGo.transform.position = new Vector3(startPos.x, startPos.y + t * 1.35f, -2f);
                float alpha = t < 0.7f ? 1f : 1f - ((t - 0.7f) / 0.3f);
                tm.color = new Color(color.r, color.g, color.b, alpha);
                bgSr.color = new Color(0.02f, 0.05f, 0.12f, 0.85f * alpha);
                yield return null;
            }
            if (popupGo != null) Destroy(popupGo);
        }
    }
}

