using System.Collections.Generic;
using UnityEngine;

namespace OrbitLauncher
{
    public struct HullDefinition
    {
        public int id;
        public string name;
        public string subtitle;
        public string description;
        public int cost;
        public Color primaryColor;
        public Color secondaryColor;
        public string spriteResource;
    }

    public struct UpgradeDefinition
    {
        public int id;
        public string key;
        public string name;
        public string description;
        public int baseCost;
        public Color themeColor;
        public string iconSprite;
    }

    public static class OrbitAssetFactory
    {
        // Option 3: Vibrant Cosmic Arcade Palette
        public static readonly Color DeepVoidBlue = new Color(0.055f, 0.024f, 0.145f, 1f); // Deep Cosmic Purple-Blue #0e0625
        public static readonly Color NeonCyan = new Color(0f, 0.941f, 1f, 1f);            // #00f0ff
        public static readonly Color ArcadePink = new Color(1f, 0.18f, 0.78f, 1f);        // #ff2ec7
        public static readonly Color LimeGreen = new Color(0.25f, 1f, 0.35f, 1f);         // #40ff59
        public static readonly Color AlertCrimson = new Color(1f, 0.165f, 0.373f, 1f);    // #ff2a5f
        public static readonly Color StarGold = new Color(1f, 0.84f, 0.25f, 1f);          // #ffd640
        public static readonly Color PulsarPurple = new Color(0.72f, 0.32f, 1f, 1f);

        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private static Material _spriteDefaultMat;
        private static Material _additiveMat;
        private static Sprite _dottedLineSprite;

        public static readonly HullDefinition[] Hulls = new HullDefinition[]
        {
            new HullDefinition
            {
                id = 0,
                name = "SCOUT ROCKET",
                subtitle = "Starter skin",
                description = "Lime / cyan delta cruiser with luminous gradient trail.",
                cost = 0,
                primaryColor = new Color(0f, 1f, 0.533f, 1f),
                secondaryColor = new Color(0f, 0.941f, 1f, 1f),
                spriteResource = "hull_scout"
            },
            new HullDefinition
            {
                id = 1,
                name = "NEON GLIDER",
                subtitle = "Pink / cyan twin trail",
                description = "Arcade wing with vibrant magenta and cyan twin trail.",
                cost = 25,
                primaryColor = new Color(1f, 0.28f, 0.78f, 1f),
                secondaryColor = new Color(0f, 0.941f, 1f, 1f),
                spriteResource = "hull_glider"
            },
            new HullDefinition
            {
                id = 2,
                name = "ION DART",
                subtitle = "Gold / blue ion flame",
                description = "High-velocity golden dart with electric blue ion flame.",
                cost = 60,
                primaryColor = new Color(1f, 0.85f, 0.25f, 1f),
                secondaryColor = new Color(0.15f, 0.65f, 1f, 1f),
                spriteResource = "hull_dart"
            },
            new HullDefinition
            {
                id = 3,
                name = "PULSAR CORE",
                subtitle = "Purple pulse aura",
                description = "Glowing cosmic orb with golden rings and purple aura.",
                cost = 120,
                primaryColor = new Color(0.75f, 0.35f, 1f, 1f),
                secondaryColor = new Color(1f, 0.85f, 0.3f, 1f),
                spriteResource = "hull_pulsar"
            }
        };

        public static readonly UpgradeDefinition[] Upgrades = new UpgradeDefinition[]
        {
            new UpgradeDefinition
            {
                id = 0,
                key = "OL_Upg_Magnet",
                name = "GRAVITY MAGNET",
                description = "Pulls nearby Star Shards & widens Perfect Hop sweet-spot.",
                baseCost = 15,
                themeColor = new Color(1f, 0.62f, 0.18f, 1f),
                iconSprite = "icon_lightning"
            },
            new UpgradeDefinition
            {
                id = 1,
                key = "OL_Upg_Shield",
                name = "WARP SHIELD CORE",
                description = "Extends Warp Invulnerability duration (+0.8s per level).",
                baseCost = 20,
                themeColor = new Color(0f, 0.94f, 1f, 1f),
                iconSprite = "icon_star_badge"
            },
            new UpgradeDefinition
            {
                id = 2,
                key = "OL_Upg_Boost",
                name = "ION THRUSTERS",
                description = "Boosts all Hop Score points (+20% per level) & flight range.",
                baseCost = 25,
                themeColor = new Color(1f, 0.22f, 0.78f, 1f),
                iconSprite = "icon_rocket"
            }
        };

        public static Material GetDefaultSpriteMaterial()
        {
            if (_spriteDefaultMat == null)
            {
                Shader s = Shader.Find("Sprites/Default");
                if (s == null) s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (s == null) s = Shader.Find("UI/Default");
                _spriteDefaultMat = new Material(s);
            }
            return _spriteDefaultMat;
        }

        public static Material GetAdditiveMaterial()
        {
            if (_additiveMat == null)
            {
                Shader s = Shader.Find("Mobile/Particles/Additive");
                if (s == null) s = Shader.Find("Particles/Standard Unlit");
                if (s == null) s = Shader.Find("Sprites/Default");
                _additiveMat = new Material(s);
            }
            return _additiveMat;
        }

        public static Vector4 GetSpriteBorder(string name)
        {
            switch (name)
            {
                case "ui_card_arcade":
                    return new Vector4(64f, 64f, 64f, 64f);
                case "ui_panel_inset":
                    return new Vector4(44f, 44f, 44f, 44f);
                case "ui_pill_gold":
                case "ui_pill_purple":
                    return new Vector4(56f, 38f, 56f, 38f);
                case "ui_btn_play_green":
                    return new Vector4(70f, 46f, 70f, 46f);
                case "ui_bar_track":
                    return new Vector4(28f, 20f, 28f, 20f);
                case "ui_bar_fill":
                    return new Vector4(20f, 14f, 20f, 14f);
                case "ui_squircle_orange":
                case "ui_squircle_pink":
                case "ui_squircle_gold":
                case "ui_squircle_blue":
                    return new Vector4(44f, 44f, 44f, 44f);
                default:
                    return Vector4.zero;
            }
        }

        public static Sprite GetSprite(string name)
        {
            if (_spriteCache.TryGetValue(name, out Sprite cached) && cached != null)
                return cached;

            Vector4 border = GetSpriteBorder(name);
            Sprite sp = Resources.Load<Sprite>("Sprites/" + name);

            if (sp != null && border != Vector4.zero && sp.border == Vector4.zero && sp.texture != null)
            {
                Texture2D t = sp.texture;
                sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            }

            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("Sprites/" + name);
                if (tex != null)
                {
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                }
            }

            if (sp == null)
            {
                sp = GenerateProceduralSprite(name);
            }

            _spriteCache[name] = sp;
            return sp;
        }

        public static Sprite GetDottedLineSprite()
        {
            if (_dottedLineSprite != null) return _dottedLineSprite;
            Texture2D tex = new Texture2D(64, 16, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < 16; y++)
            {
                float dy = Mathf.Abs(y - 7.5f) / 7.5f;
                float edge = Mathf.Clamp01(1f - dy * dy);
                for (int x = 0; x < 64; x++)
                {
                    bool on = x < 34;
                    float a = on ? edge : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            _dottedLineSprite = Sprite.Create(tex, new Rect(0, 0, 64, 16), new Vector2(0.5f, 0.5f), 100f);
            return _dottedLineSprite;
        }

        private static Sprite GenerateProceduralSprite(string name)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    if (name == "orbit_ring")
                    {
                        float ringDist = Mathf.Abs(r - 0.88f);
                        float angle = Mathf.Atan2(dy, dx);
                        bool dash = Mathf.Sin(angle * 24f) > -0.2f;
                        float a = (ringDist < 0.035f && dash) ? 1f : (ringDist < 0.08f ? 0.25f : 0f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                    else if (name == "solid_disc")
                    {
                        float a = r <= 0.95f ? 1f : 0f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                    else
                    {
                        float a = Mathf.Clamp01(1f - r);
                        a = a * a;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
