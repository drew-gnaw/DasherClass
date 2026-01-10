using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class VoidClawSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public Vector2 targetPosition;
        public int FrameDelay = 2;
        public int FrameDelayCounter = 2;
        public Player Owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 50;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.4f;
            Projectile.width = (int)(68 * Projectile.scale);
            Projectile.height = (int)(71 * Projectile.scale);
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.67f, 0.08f, 0.41f);
            HandleFrames();
            SpawnPortalDust();
            Projectile.velocity *= 0f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.ShadowFlame, 300);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item20, Projectile.Center); // Unholy Trident cast
        }

        public void HandleFrames()
        {
            if (FrameDelayCounter <= 0)
            {
                FrameDelayCounter = FrameDelayHandler();
                if (Projectile.frame == 0)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item100, Projectile.Center); // Clinger Staff cast
                }
                if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
                {
                    Projectile.frame++;
                }
                else
                {
                    Projectile.Kill();
                }
            }
            else
            {
                FrameDelayCounter--;
            }
        }

        public int FrameDelayHandler()
        {
            int frameDelay;
            if (Projectile.frame >= 0 && Projectile.frame <= 17)
            {
                frameDelay = 2;
                Projectile.friendly = false;
            }
            else if (Projectile.frame >= 18 && Projectile.frame <= 30)
            {
                Projectile.friendly = true;
                frameDelay = 1;
            }
            else
            {
                frameDelay = 3;
                Projectile.friendly = false;
            }

            return frameDelay;
        }

        private void SpawnPortalDust()
        {
            // Only spawn dust between frames 15 and 43 to sync with the animation
            if (Projectile.frame <= 14 || Projectile.frame > 40)
            {
                return;
            }

            // Dense Shadowflame dust being pulled upward into the slash like a portal
            // Reduced count, smaller size, faster upward motion, and higher spawn
            for (int i = 0; i < 3; i++)
            {
                // Spawn higher above/around the center of the slash
                float offsetX = Main.rand.NextFloat(-Projectile.width * 0.7f, Projectile.width * 0.7f);
                // Negative Y is upward from the projectile center; move another ~10 units up
                float offsetY = Main.rand.NextFloat(-56f, -44f);
                Vector2 spawnPos = Projectile.Center + new Vector2(offsetX, offsetY);

                // Velocity towards the center (generally upward from below), 40% faster
                Vector2 dir = (Projectile.Center - spawnPos).SafeNormalize(new Vector2(0f, -1f));
                Vector2 vel = dir * Main.rand.NextFloat(2.52f, 5.04f);

                // Dust 30% larger width (scale)
                float baseScale = 1.0f + Main.rand.NextFloat(0.3f);
                float finalScale = baseScale * 1.3f;
                Dust d = Dust.NewDustDirect(spawnPos, 0, 0, DustID.Shadowflame, vel.X, vel.Y, 150, default, finalScale);
                d.noGravity = true;
                d.velocity *= 1.1f;
            }
        }

        // Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Draw base sprite
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(punchTexture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);

            // Draw violet glowmask similar to VoidRuneDash
            Texture2D glowTexture = punchTexture; // Reuse same texture as simple glowmask
            Color glowColor = new Color(180, 80, 255, 80) * 0.7f; // Moderate violet, semi-transparent
            float glowScale = Projectile.scale * 1.05f;
            Main.EntitySpriteDraw(glowTexture, drawPos, frame, glowColor, Projectile.rotation, origin, glowScale, directionEffect, 0);

            return false;
        }
    }
}
