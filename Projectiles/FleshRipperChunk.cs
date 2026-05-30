using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class FleshRipperChunk : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 1000;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.scale = 0.6f;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.05f;
            Projectile.velocity.Y += 0.3f;
            Projectile.velocity.X *= 0.98f; 
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {   
            SpawnCollisionDust();
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnCollisionDust();
        }


        private void SpawnCollisionDust()
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Blood,
                    Projectile.velocity.X * 0.5f,
                    Projectile.velocity.Y * 0.5f
                );
            }
        }
    }
}
