using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class MoltenMantleExplosion : ModProjectile
    {
        private const int FrameCount = 6;
        private const int FrameDelay = 4;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FrameCount;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.timeLeft = FrameCount * FrameDelay + 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // damage once per NPC
        }

        public override void OnSpawn(IEntitySource source)
        {
            // ai[0] carries the rotation set at spawn time
            Projectile.rotation = Projectile.ai[0];
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameDelay)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FrameCount)
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, FrameCount, 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            // Glow in orange-red for molten feel
            Color glowColor = new Color(255, 120, 0, 180);
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame,
                    glowColor * (0.35f - i * 0.1f), Projectile.rotation, origin,
                    Projectile.scale * (1f + i * 0.06f), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame,
                lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
