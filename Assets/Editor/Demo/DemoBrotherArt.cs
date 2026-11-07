using UnityEngine;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Genera por código los fotogramas del hermano pequeño (8 años): gorro de lana con pompón,
    /// chaqueta mostaza, mochila pequeña. Pixel art de perfil derecho, mismo lienzo (128x160) y
    /// pivote que el protagonista para integrarse con sus animaciones.
    /// </summary>
    public static class DemoBrotherArt
    {
        public const string Folder = "Assets/Art/Characters/LittleBrother";
        public const int NativeW = 64;
        public const int NativeH = 80;

        static readonly Color Skin = new Color(0.95f, 0.76f, 0.62f);
        static readonly Color SkinShade = new Color(0.78f, 0.57f, 0.46f);
        static readonly Color Jacket = new Color(0.86f, 0.63f, 0.22f);
        static readonly Color JacketShade = new Color(0.58f, 0.40f, 0.14f);
        static readonly Color Pants = new Color(0.24f, 0.29f, 0.37f);
        static readonly Color PantsShade = new Color(0.14f, 0.17f, 0.24f);
        static readonly Color Boots = new Color(0.34f, 0.23f, 0.15f);
        static readonly Color BootsShade = new Color(0.19f, 0.12f, 0.09f);
        static readonly Color Beanie = new Color(0.70f, 0.40f, 0.22f);
        static readonly Color BeanieShade = new Color(0.46f, 0.24f, 0.14f);
        static readonly Color BeanieBand = new Color(0.84f, 0.56f, 0.32f);
        static readonly Color Hair = new Color(0.36f, 0.22f, 0.14f);
        static readonly Color Pack = new Color(0.48f, 0.33f, 0.19f);
        static readonly Color PackShade = new Color(0.30f, 0.19f, 0.11f);
        static readonly Color Outline = new Color(0.09f, 0.08f, 0.11f, 1f);
        static readonly Color Ink = new Color(0.16f, 0.10f, 0.10f, 1f);

        struct Pose
        {
            public float thighF, shinF, thighB, shinB;
            public float armF, elbowF, armB, elbowB;
            public float lean, headDx, headDy;
            public int mouth; // 0 sonrisa suave, 1 boca abierta
            public bool lookUp;
            public bool grounded;
            public float lift;
        }

        public static string FramePath(string name)
        {
            return $"{Folder}/{name}.png";
        }

        public static void Generate()
        {
            Save("idle_0", IdlePose(false));
            Save("idle_1", IdlePose(true));

            for (int i = 0; i < 6; i++)
            {
                Save($"walk_{i}", WalkPose(i / 6f * Mathf.PI * 2f));
            }

            Save("jump", new Pose
            {
                thighF = 42f, shinF = 6f, thighB = -14f, shinB = -52f,
                armF = 125f, elbowF = 135f, armB = 150f, elbowB = 158f,
                lean = 1f, mouth = 1, grounded = false, lift = 2f
            });
            Save("fall", new Pose
            {
                thighF = 22f, shinF = 14f, thighB = -22f, shinB = -30f,
                armF = 105f, elbowF = 112f, armB = -100f, elbowB = -108f,
                lean = 0f, mouth = 1, grounded = false, lift = 2f
            });

            Save("ask_0", new Pose
            {
                thighF = 6f, shinF = 3f, thighB = -6f, shinB = -4f,
                armF = 168f, elbowF = 172f, armB = 150f, elbowB = 156f,
                lean = 0.5f, headDy = 0.5f, mouth = 1, lookUp = true, grounded = true
            });
            Save("ask_1", new Pose
            {
                thighF = 8f, shinF = -4f, thighB = -8f, shinB = -14f,
                armF = 150f, elbowF = 158f, armB = 168f, elbowB = 172f,
                lean = 0.5f, headDy = 0.5f, mouth = 0, lookUp = true, grounded = false, lift = 1.5f
            });

            Save("cheer_0", new Pose
            {
                thighF = 14f, shinF = 6f, thighB = -14f, shinB = -20f,
                armF = 145f, elbowF = 155f, armB = -145f, elbowB = -155f,
                lean = 0f, headDy = 0.5f, mouth = 1, lookUp = true, grounded = false, lift = 3f
            });
            Save("cheer_1", new Pose
            {
                thighF = 8f, shinF = 4f, thighB = -8f, shinB = -6f,
                armF = 130f, elbowF = 140f, armB = -130f, elbowB = -140f,
                lean = 0f, mouth = 1, lookUp = true, grounded = true
            });
        }

        static Pose IdlePose(bool alt)
        {
            return new Pose
            {
                thighF = 5f, shinF = 2f, thighB = -5f, shinB = -3f,
                armF = alt ? 7f : 3f, elbowF = alt ? 14f : 10f,
                armB = alt ? -6f : -3f, elbowB = alt ? -1f : -3f,
                lean = 0f, headDy = alt ? -1f : 0f, mouth = 0, grounded = true
            };
        }

        static Pose WalkPose(float phase)
        {
            const float amp = 34f;
            float sF = Mathf.Sin(phase);
            float cF = Mathf.Cos(phase);
            float sB = Mathf.Sin(phase + Mathf.PI);
            float cB = Mathf.Cos(phase + Mathf.PI);

            var p = new Pose
            {
                thighF = amp * sF,
                thighB = amp * sB,
                armF = -26f * sF,
                armB = -26f * sB,
                lean = 1.2f,
                mouth = 0,
                grounded = true
            };
            p.shinF = p.thighF - 55f * Mathf.Max(0f, cF);
            p.shinB = p.thighB - 55f * Mathf.Max(0f, cB);
            p.elbowF = p.armF + 18f;
            p.elbowB = p.armB + 18f;
            p.headDy = Mathf.Abs(sF) < 0.5f ? 0.5f : 0f;
            return p;
        }

        static void Save(string name, Pose pose)
        {
            DemoImg native = Draw(pose);
            native.OuterOutline(Outline);
            DemoImg big = native.Upscale(2);
            big.Save(FramePath(name), 48f, FilterMode.Point, new Vector2(0.5f, 0.075f));
        }

        static Vector2 Dir(float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(r), -Mathf.Cos(r));
        }

        static void Limb(DemoImg img, Vector2 a, Vector2 b, float r, Color baseC, Color shade)
        {
            img.Capsule(a.x, a.y, b.x, b.y, r, r, shade);
            img.Capsule(a.x + 0.7f, a.y + 0.7f, b.x + 0.7f, b.y + 0.7f, Mathf.Max(0.6f, r - 0.9f), Mathf.Max(0.6f, r - 0.9f), baseC);
        }

        static DemoImg Draw(Pose p)
        {
            var img = new DemoImg(NativeW, NativeH);
            const float thighLen = 8.5f;
            const float shinLen = 8.5f;
            const float hipX = 30f;
            const float restAnkleY = 8f;

            float hipY = restAnkleY + thighLen + shinLen;
            if (p.grounded)
            {
                float ayF = hipY - thighLen * Mathf.Cos(p.thighF * Mathf.Deg2Rad) - shinLen * Mathf.Cos(p.shinF * Mathf.Deg2Rad);
                float ayB = hipY - thighLen * Mathf.Cos(p.thighB * Mathf.Deg2Rad) - shinLen * Mathf.Cos(p.shinB * Mathf.Deg2Rad);
                hipY += restAnkleY - Mathf.Min(ayF, ayB);
            }
            else
            {
                hipY += p.lift;
            }

            Vector2 hip = new Vector2(hipX, hipY);
            Vector2 kneeF = hip + thighLen * Dir(p.thighF);
            Vector2 ankleF = kneeF + shinLen * Dir(p.shinF);
            Vector2 kneeB = hip + thighLen * Dir(p.thighB);
            Vector2 ankleB = kneeB + shinLen * Dir(p.shinB);

            Vector2 torsoBase = new Vector2(hipX + p.lean * 0.2f, hipY + 1.5f);
            Vector2 torsoTop = new Vector2(hipX + p.lean, hipY + 12f);
            Vector2 shoulder = new Vector2(hipX + p.lean * 0.85f + 0.5f, hipY + 10.5f);
            Vector2 head = new Vector2(hipX + p.lean + 1f, hipY + 19.5f + p.headDy);

            // Mochila pequeña (detrás)
            img.Ellipse(hipX - 5.4f + p.lean * 0.5f, hipY + 8f, 3.7f, 5.3f, PackShade);
            img.Ellipse(hipX - 5.0f + p.lean * 0.5f, hipY + 8.6f, 2.9f, 4.4f, Pack);

            // Brazo trasero
            DrawArm(img, shoulder, p.armB, p.elbowB, JacketShade, new Color(0.62f, 0.45f, 0.36f));

            // Pierna trasera
            DrawLeg(img, hip, kneeB, ankleB, PantsShade, new Color(0.10f, 0.12f, 0.17f), BootsShade, new Color(0.12f, 0.08f, 0.06f));

            // Torso
            img.Capsule(torsoBase.x, torsoBase.y, torsoTop.x, torsoTop.y, 5.4f, 4.9f, JacketShade);
            img.Capsule(torsoBase.x + 0.8f, torsoBase.y + 0.4f, torsoTop.x + 0.8f, torsoTop.y + 0.4f, 4.3f, 3.9f, Jacket);
            // Cremallera / cuello
            img.Capsule(torsoBase.x + 2.6f, torsoBase.y + 1f, torsoTop.x + 2.2f, torsoTop.y - 0.5f, 0.5f, 0.5f, JacketShade);

            // Pierna delantera
            DrawLeg(img, hip, kneeF, ankleF, Pants, PantsShade, Boots, BootsShade);

            // Cabeza
            img.Ellipse(head.x - 3.4f, head.y - 0.6f, 4.6f, 5.2f, Hair); // pelo de la nuca
            img.Ellipse(head.x, head.y, 7.4f, 7.2f, SkinShade);
            img.Ellipse(head.x + 0.5f, head.y + 0.4f, 6.6f, 6.4f, Skin);
            img.Disc(head.x - 2.6f, head.y - 0.8f, 1.5f, SkinShade); // oreja

            // Cara
            float eyeY = head.y + 0.6f + (p.lookUp ? 1f : 0f);
            Put(img, head.x + 3.6f, eyeY, Ink);
            Put(img, head.x + 3.6f, eyeY + 1f, Ink);
            Put(img, head.x + 4.6f, eyeY, Ink);
            Put(img, head.x + 4.6f, eyeY + 1f, Ink);
            Put(img, head.x + 3.6f, eyeY + 2f, Hair); // ceja
            Put(img, head.x + 4.6f, eyeY + 2f, Hair);
            Put(img, head.x + 4.6f, eyeY - 2.4f, new Color(0.96f, 0.55f, 0.50f, 0.9f)); // mejilla
            Put(img, head.x + 3.6f, eyeY - 2.4f, new Color(0.96f, 0.55f, 0.50f, 0.9f));
            Put(img, head.x + 7.2f, head.y - 0.8f, SkinShade); // nariz
            float my = head.y - 3.6f;
            if (p.mouth == 1)
            {
                for (int dx = 0; dx < 3; dx++)
                {
                    Put(img, head.x + 3.4f + dx, my, new Color(0.45f, 0.12f, 0.12f));
                    Put(img, head.x + 3.4f + dx, my - 1f, new Color(0.45f, 0.12f, 0.12f));
                }
            }
            else
            {
                Put(img, head.x + 3.4f, my + 0.6f, Ink);
                Put(img, head.x + 4.4f, my, Ink);
                Put(img, head.x + 5.4f, my, Ink);
                Put(img, head.x + 6.4f, my + 0.6f, Ink);
            }

            // Gorro de lana con pompón
            DrawBeanie(img, head);

            // Brazo delantero
            DrawArm(img, shoulder, p.armF, p.elbowF, Jacket, Skin);

            return img;
        }

        static void Put(DemoImg img, float x, float y, Color c)
        {
            img.Set(Mathf.FloorToInt(x), Mathf.FloorToInt(y), c);
        }

        static void DrawArm(DemoImg img, Vector2 shoulder, float upperAngle, float lowerAngle, Color sleeve, Color hand)
        {
            Vector2 elbow = shoulder + 6.5f * Dir(upperAngle);
            Vector2 wrist = elbow + 6f * Dir(lowerAngle);
            Limb(img, shoulder, elbow, 2.5f, sleeve, DemoImg.Mul(sleeve, 0.7f));
            Limb(img, elbow, wrist, 2.3f, sleeve, DemoImg.Mul(sleeve, 0.7f));
            img.Disc(wrist.x, wrist.y, 2.0f, DemoImg.Mul(hand, 0.85f));
            img.Disc(wrist.x + 0.4f, wrist.y + 0.4f, 1.5f, hand);
        }

        static void DrawLeg(DemoImg img, Vector2 hip, Vector2 knee, Vector2 ankle, Color pants, Color pantsShade, Color boots, Color bootsShade)
        {
            Limb(img, hip, knee, 3.1f, pants, pantsShade);
            Limb(img, knee, ankle, 2.7f, pants, pantsShade);
            Vector2 toe = ankle + new Vector2(3.6f, -0.4f);
            img.Capsule(ankle.x - 1f, ankle.y, toe.x, toe.y, 2.3f, 2.1f, bootsShade);
            img.Capsule(ankle.x - 0.4f, ankle.y + 0.5f, toe.x, toe.y + 0.5f, 1.7f, 1.5f, boots);
        }

        static void DrawBeanie(DemoImg img, Vector2 head)
        {
            float cx = head.x - 0.4f;
            float cy = head.y + 2.4f;
            const float rx = 8.4f;
            const float ry = 8.0f;
            float bandBottom = head.y + 1.6f;
            float bandTop = head.y + 4.4f;

            for (int y = Mathf.FloorToInt(bandBottom); y <= Mathf.CeilToInt(cy + ry); y++)
            {
                for (int x = Mathf.FloorToInt(cx - rx); x <= Mathf.CeilToInt(cx + rx); x++)
                {
                    float nx = (x + 0.5f - cx) / rx;
                    float ny = (y + 0.5f - cy) / ry;
                    if (nx * nx + ny * ny > 1f || y + 0.5f < bandBottom)
                    {
                        continue;
                    }

                    float lightT = Mathf.Clamp01((nx + 1f) * 0.5f);
                    Color c = Color.Lerp(BeanieShade, Beanie, Mathf.Clamp01(lightT * 1.15f));
                    if (y + 0.5f <= bandTop)
                    {
                        bool rib = ((x / 2) & 1) == 0;
                        c = Color.Lerp(BeanieShade, BeanieBand, lightT);
                        if (rib)
                        {
                            c = DemoImg.Mul(c, 0.88f);
                        }
                    }
                    else if (((x + y) & 3) == 0)
                    {
                        c = DemoImg.Mul(c, 0.93f); // trama de lana
                    }

                    img.Set(x, y, c);
                }
            }

            // Pompón
            img.Disc(cx - 0.6f, cy + ry + 1.4f, 3.4f, BeanieShade);
            img.Disc(cx - 0.2f, cy + ry + 1.9f, 2.7f, BeanieBand);
            Put(img, cx + 0.3f, cy + ry + 2.8f, new Color(0.95f, 0.72f, 0.5f));
        }
    }
}
