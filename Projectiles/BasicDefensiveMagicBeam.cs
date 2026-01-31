using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace DasherClass.Projectiles
{
    // A static laserbeam that appears for a short time, dealing damage along its length, with advanced rendering
    public class BasicDefensiveMagicBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_466";

        public ref float Time => ref Projectile.ai[0];
        public ref float LaserLength => ref Projectile.ai[1];
        public const int Lifetime = 30; // 0.5 seconds
        public const float MaxLaserWidth = 38f;

        public override void SetDefaults()
        {
            Projectile.width = (int)MaxLaserWidth;
            Projectile.height = (int)MaxLaserWidth;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = ModContent.GetInstance<DasherDamageClass>();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Time++;
            if (Time >= Lifetime)
                Projectile.Kill();
        }

        public override bool ShouldUpdatePosition() => false;

        // Laser width function (tapered)
        public float LaserWidthFunction(float completionRatio)
        {
            // Taper at ends, max in middle
            float width = MathHelper.Lerp(10f, MaxLaserWidth, 1f - Math.Abs(completionRatio * 2f - 1f));
            return width;
        }

        // Laser color function (cyan core, blue edge)
        public Color LaserColorFunction(float completionRatio)
        {
            Color core = Color.Cyan;
            Color edge = Color.DeepSkyBlue;
            return Color.Lerp(core, edge, completionRatio) * 0.85f;
        }

        // Bloom width function
        public float BloomWidthFunction(float completionRatio) => LaserWidthFunction(completionRatio) * 1.7f;

        // Bloom color function
        public Color BloomColorFunction(float completionRatio)
        {
            Color bloom = Color.White;
            Color blue = Color.Cyan;
            float opacity = MathHelper.Lerp(0.18f, 0.08f, Math.Abs(completionRatio * 2f - 1f));
            return Color.Lerp(bloom, blue, completionRatio) * opacity;
        }

        // Get control points for the laser (straight line)
        public List<Vector2> GetLaserControlPoints(int points, float length)
        {
            List<Vector2> result = new();
            Vector2 start = Projectile.Center;
            Vector2 unit = Projectile.rotation.ToRotationVector2();
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)(points - 1);
                result.Add(start + unit * (length * t));
            }
            return result;
        }

        // Draw bloom behind the beam
        public void DrawBackBloom()
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float scale = MaxLaserWidth / tex.Height * 2.5f;
            Color bloomColor = Color.White * 0.18f;
            Main.spriteBatch.Draw(tex, pos, null, bloomColor, Projectile.rotation, new Vector2(0, tex.Height / 2), scale, SpriteEffects.None, 0);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Use a line collision with width at the closest point
            Vector2 start = Projectile.Center;
            Vector2 unit = Projectile.rotation.ToRotationVector2();
            Vector2 end = start + unit * LaserLength;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, MaxLaserWidth * 0.5f, ref _);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Optionally add effects
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBackBloom();
            int points = 12;
            List<Vector2> laserPositions = GetLaserControlPoints(points, LaserLength);

            // Draw bloom trail
            for (int i = 1; i < laserPositions.Count; i++)
            {
                float t = (i - 1) / (float)(laserPositions.Count - 1);
                float width = BloomWidthFunction(t);
                Color color = BloomColorFunction(t);
                Vector2 a = laserPositions[i - 1] - Main.screenPosition;
                Vector2 b = laserPositions[i] - Main.screenPosition;
                DrawLaserSegment(a, b, width, color);
            }

            // Draw main laser trail
            for (int i = 1; i < laserPositions.Count; i++)
            {
                float t = (i - 1) / (float)(laserPositions.Count - 1);
                float width = LaserWidthFunction(t);
                Color color = LaserColorFunction(t);
                Vector2 a = laserPositions[i - 1] - Main.screenPosition;
                Vector2 b = laserPositions[i] - Main.screenPosition;
                DrawLaserSegment(a, b, width, color);
            }

            // Draw flashes at ends
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            for (int j = 0; j < 2; j++)
            {
                Vector2 flashPos = (j == 0) ? laserPositions[0] - Main.screenPosition : laserPositions[^1] - Main.screenPosition;
                float flashScale = MaxLaserWidth / tex.Height * 2.2f;
                Color flashColor = Color.White * 0.5f;
                Main.EntitySpriteDraw(tex, flashPos, null, flashColor, Projectile.rotation, new Vector2(0, tex.Height / 2), flashScale, SpriteEffects.None, 0);
            }

            return false;
        }

        // Helper to draw a laser segment as a thick line (rectangle)
        public void DrawLaserSegment(Vector2 a, Vector2 b, float width, Color color)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 dir = b - a;
            float length = dir.Length();
            if (length < 1f) return;
            float rotation = dir.ToRotation();
            Main.spriteBatch.Draw(tex, a, null, color, rotation, new Vector2(0, tex.Height / 2), new Vector2(length / tex.Width, width / tex.Height), SpriteEffects.None, 0);
        }
    }
}