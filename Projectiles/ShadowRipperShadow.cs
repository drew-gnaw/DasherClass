using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class ShadowRipperShadow : ModProjectile
    {
        private Vector2 previousPosition;
        private Vector2 dashStartPosition;
        private System.Collections.Generic.List<Vector2> slashPath = new System.Collections.Generic.List<Vector2>();
        private float dashProgress = 0f;
        private const float DashDuration = 25f;
        private const float InitialDelay = 30f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (dashProgress < InitialDelay)
            {
                dashProgress++;
                Projectile.velocity = Vector2.Zero;
                previousPosition = Projectile.Center;

                Vector2 directionToPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.rotation = directionToPlayer.ToRotation();

                return;
            }

            float adjustedProgress = dashProgress - InitialDelay;

            if (adjustedProgress < DashDuration)
            {
                if (adjustedProgress == 0)
                {
                    dashStartPosition = Projectile.Center;
                    slashPath.Clear();
                    slashPath.Add(Projectile.Center);
                }

                previousPosition = Projectile.Center;

                Vector2 directionToPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = 25f;
                Projectile.velocity = directionToPlayer * speed;

                float rotationToPlayer = directionToPlayer.ToRotation();
                Projectile.rotation = rotationToPlayer;

                if (Vector2.Distance(Projectile.Center, player.Center) < 30f)
                {
                    Projectile.Kill();
                }

                dashProgress++;
                slashPath.Add(Projectile.Center);
            }
            else
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float adjustedProgress = dashProgress - InitialDelay;

            if (adjustedProgress > 0 && adjustedProgress < DashDuration)
            {
                DrawSlash();
            }

            Texture2D shadowTexture = ModContent.Request<Texture2D>("DasherClass/Projectiles/ShadowRipperShadow").Value;
            Rectangle frame = shadowTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            Color shadowColor = Color.White * (1f - adjustedProgress / DashDuration);

            SpriteEffects effects = System.Math.Cos(Projectile.rotation) < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(
                shadowTexture,
                Projectile.Center - Main.screenPosition,
                frame,
                shadowColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }

        private void DrawSlash()
        {
            float adjustedProgress = dashProgress - InitialDelay;
            float slashOpacity = 1f - (adjustedProgress / DashDuration);

            if (slashPath.Count < 2) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            for (int i = 0; i < slashPath.Count - 1; i++)
            {
                Vector2 start = slashPath[i] - Main.screenPosition;
                Vector2 end = slashPath[i + 1] - Main.screenPosition;

                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;

                float rotation = direction.ToRotation();

                Color slashColor = new Color(200, 100, 255) * slashOpacity;
                Color slashGlow = new Color(255, 180, 255) * slashOpacity;

                DrawSlashSegment(pixel, start, length, rotation, 12f, slashGlow);
                DrawSlashSegment(pixel, start, length, rotation, 6f, slashColor);
            }
        }

        private void DrawSlashSegment(Texture2D texture, Vector2 start, float length, float rotation, float width, Color color)
        {
            Vector2 origin = new Vector2(0, texture.Height / 2f);
            Vector2 scale = new Vector2(length / texture.Width, width / texture.Height);

            Main.EntitySpriteDraw(
                texture,
                start,
                null,
                color,
                rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );
        }
    }
}
