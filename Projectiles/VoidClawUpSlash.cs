using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class VoidClawUpSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public bool isHit = false;
        public Vector2 targetPosition;
        public float totalSlashTime;
        public float totalWindUpTime;
        public Player Owner => Main.player[Projectile.owner];
        public int originalDirection;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 48;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.timeLeft = 20;
            Projectile.ignoreWater = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            targetPosition = Projectile.position;
            totalWindUpTime = Projectile.timeLeft * 0.2f;
            totalSlashTime = Projectile.timeLeft * 0.8f;
            if(Owner.direction == 1)
            {
                Projectile.position += new Vector2(-20, 40);
                Projectile.rotation += MathHelper.Pi / 4 + MathHelper.Pi / 8;
                Projectile.spriteDirection = -1;
                originalDirection = 1;
            } else
            {
                Projectile.position += new Vector2(20, 40);
                Projectile.rotation -= MathHelper.Pi / 4 + MathHelper.Pi / 8;
                Projectile.spriteDirection = 1;
                originalDirection = -1;
            }
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.67f, 0.08f, 0.41f);
            if(Projectile.timeLeft > totalSlashTime)
            {
                WindUp();
            } else
            {
                UpSlash();
            }
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                d.noGravity = true;
                d.scale = 0.9f;
                d.velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.ShadowFlame, 300);
        }

        public float CresentSlashXOffset(float t)
        {
            if(originalDirection == 1)
            {
                t = 1 - t;
            }
            float curve = (float)Math.Cos(t * MathHelper.Pi);
            return -curve * 1.8f; 
        }

        public void UpSlash()
        {
            float t = (totalSlashTime - Projectile.timeLeft) / totalSlashTime; // 1.5 = 50% faster
            t = MathHelper.Clamp(t, 0f, 1f); 
            float x = CresentSlashXOffset(t);
            float y = MathHelper.Lerp(-1, -1.2f, (float)Math.Pow(3.0, t));
            float rotation = MathHelper.Lerp(MathHelper.ToRadians(3), MathHelper.ToRadians(14), t*t);
            Projectile.rotation += HandleRotationDirection(rotation);
            Projectile.position += new Vector2(x , y) * 10.0f;
            Projectile.velocity = Vector2.Zero;
        }

        public void WindUp()
        {
            float t = (totalWindUpTime - (Projectile.timeLeft * 0.2f)) / totalWindUpTime;
            float y = MathHelper.Lerp(-1, -2, t);
            float rotation = MathHelper.Lerp(MathHelper.ToRadians(0), MathHelper.ToRadians(8), t);
            Projectile.rotation -= HandleRotationDirection(rotation);
            Projectile.position -= new Vector2(0 , y) * 10.0f;
            Projectile.velocity = Vector2.Zero;
        }

        public float HandleRotationDirection(float rotation) 
        {
            return originalDirection == 1 ? -rotation : rotation;
        }
    }
}
