using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;

namespace DasherClass.Projectiles
{
    public class ShadeBasherSlash : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 12;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.scale = 0.8f;
            Projectile.timeLeft = 36; // Dies after 12 frames * 3 delay
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit once per NPC
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            int randomOffset = Main.rand.Next(-10, 11);
            Projectile.rotation = Projectile.ai[0] + MathHelper.ToRadians(randomOffset); // Add random offset to rotation for visual variety
        }

        public override void AI()
        {
            // Cycle through frames with delay
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                
                // Kill projectile when it reaches the last frame
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.Kill();
                }
            }

            // Lock to spawn position
            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            
            // Draw with bright magenta glow mask effect
            Color glowColor = new Color(255, 0, 255, 255); // Bright magenta glow
            
            // Draw multiple glow layers for stronger effect
            for (int i = 0; i < 3; i++)
            {
                float glowScale = Projectile.scale * (1f + i * 0.05f);
                Main.EntitySpriteDraw(
                    texture,
                    Projectile.Center - Main.screenPosition,
                    frame,
                    glowColor * 0.5f,
                    Projectile.rotation,
                    origin,
                    glowScale,
                    SpriteEffects.None,
                    0
                );
            }
            
            // Draw the main sprite on top
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Make it slightly darker overall
            return new Color(lightColor.R * 0.7f / 255f, lightColor.G * 0.7f / 255f, lightColor.B * 0.7f / 255f, lightColor.A / 255f);
        }
    }
}
