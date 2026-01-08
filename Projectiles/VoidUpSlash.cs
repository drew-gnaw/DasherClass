using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace DasherClass.Projectiles
{
    public class VoidUpSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public int holdFrameCount = 5;
        public int holdPerFrame = 2;
        public int holdPerFrameCounter;
        public int startHoldFrames = 15;
        public Vector2 targetPosition;
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 17;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.8f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 60f);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            targetPosition = Projectile.position;
            Projectile.position += new Vector2(28, 20);
            holdPerFrameCounter = holdPerFrame;
            if(Owner.direction == 1)
            {
                Projectile.spriteDirection = -1;
                Projectile.position += new Vector2(-32, 20);
            } else
            {
                Projectile.spriteDirection = 1;
                Projectile.position += new Vector2(27, 20);
            }
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.67f, 0.08f, 0.41f);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                d.noGravity = true;
                d.scale = 0.9f;
                d.velocity *= 0.2f;
            }
            if(startHoldFrames > 0)
            {
                startHoldFrames--;
                Projectile.velocity = Vector2.Zero;
                Projectile.hide = true;
                return;
            } else
            {
                Projectile.hide = false;
            }
            if(holdPerFrameCounter > 0)
            {
                holdPerFrameCounter--;
            } else {
                Projectile.frame = Projectile.frameCounter;
                holdPerFrameCounter = holdPerFrame;
                if(Projectile.frameCounter == 8 && holdFrameCount > 0)
                {
                    holdFrameCount--;
                    Projectile.frame = Projectile.frameCounter;
                    return;
                }
                Projectile.frameCounter++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.Kill();
                }
            }
            Projectile.velocity = Vector2.Zero;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.ShadowFlame, 300);
        }
    }
}
