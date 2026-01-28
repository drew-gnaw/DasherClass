using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class VoidExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        private const int MaxFrames = 5;
        private const int FrameDelay = 3;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = MaxFrames;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.0f;
            Projectile.width = (int)(25 * Projectile.scale);
            Projectile.height = (int)(25 * Projectile.scale);
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxFrames * FrameDelay;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Only hit once
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Add purple/void lighting
            Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 1.2f);

            // Spawn dust effects
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = 1.5f;
            }

            // Handle animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameDelay)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= MaxFrames)
                {
                    Projectile.Kill();
                }
            }

            // Slowly expand
            Projectile.scale += 0.02f;
            Projectile.rotation += 0.05f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            // Create additional visual effects on hit
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                d.scale = 1.8f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, MaxFrames, 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Draw base explosion
            Main.EntitySpriteDraw(texture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            // Draw additive glow layer
            Color glowColor = new Color(200, 100, 255, 0) * 0.8f;
            Main.EntitySpriteDraw(texture, drawPos, frame, glowColor, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);

            return false;
        }
    }
}
