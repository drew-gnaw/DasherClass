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
        private float dashProgress = 0f;
        private const float DashDuration = 25f;
        private const float InitialDelay = 30f;
        private int ownerDirection = 1;

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
            ownerDirection = player.direction;

            if (dashProgress < InitialDelay)
            {
                dashProgress++;
                Projectile.velocity = Vector2.Zero;
                previousPosition = Projectile.Center;
                return;
            }

            float adjustedProgress = dashProgress - InitialDelay;

            if (adjustedProgress < DashDuration)
            {
                previousPosition = Projectile.Center;

                Vector2 directionToPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = 25f;
                Projectile.velocity = directionToPlayer * speed;

                float rotationToPlayer = directionToPlayer.ToRotation();
                Projectile.rotation = rotationToPlayer;

                dashProgress++;
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

            SpriteEffects effects = ownerDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

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

            Vector2 start = previousPosition - Main.screenPosition;
            Vector2 end = Projectile.Center - Main.screenPosition;

            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 1f) return;

            float rotation = direction.ToRotation();

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Color slashColor = new Color(200, 100, 255) * slashOpacity * 0.8f;
            Color slashGlow = new Color(230, 150, 255) * slashOpacity * 0.5f;

            DrawSlashSegment(pixel, start, length, rotation, 12f, slashGlow);
            DrawSlashSegment(pixel, start, length, rotation, 6f, slashColor);
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
