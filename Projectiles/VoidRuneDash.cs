using System;
using System.Linq;
using Humanizer;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace DasherClass.Projectiles
{
    public class VoidRuneDash : ShieldWeaponProjectile
    {
        public override float LungeSpeed => 17f;
        public override float ChargeTime => 120f;
        public override float DashTime => 65f;
        public override float PullBackScale => 1.0f; // No pullback 
        public override float MaxPullBackRate => 1.0f;
        public override int OnHitIFrames => 30;
        public override float HoldMinRadius => 55f;
        public override float HoldMaxRadius => 75f;
        public override float LungingMinRadius => 80f;
        public override float LungingMaxRadius => 100f;
        public override int FrameDelay {get; set;} = 2;
        public override bool CycleChargingSprite => false;
        public override bool CycleLungingSprite => false;
        public int voidClawIndex = -1;
        public int voidCrystalIndex = -1;
        public bool onReelback = false;
        public int holdFrameCount = 7;
        public int holdFrameCounter = 7;
        public int slashUpSlashIndex = -1;
        public int clawUpSlashIndex = -1;
        public int[] KEYFRAMES = [4, 9, 14, 18];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 62;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.0f;
            Projectile.width = 23;
            Projectile.height = 56;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.frameCounter = 0;
        }

        public override void AI()
        {
            base.AI();
            if (isMidlunge)
            {
                Projectile.width = 59;
                if(Projectile.frame >= Main.projFrames[Projectile.type] - 1)
                {
                    Projectile.Kill();
                }
            }
            FrameDelay = frameDelayHandler();
            if (voidCrystalIndex == -1)
            {
                voidCrystalIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + new Vector2(-10, -48), Projectile.velocity * 0, ModContent.ProjectileType<VoidCrystal>(), 0, 0, Projectile.owner);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.ShadowFlame, 300);
            Projectile crystalProjectile = Main.projectile[voidCrystalIndex];
            if (((VoidCrystal)crystalProjectile.ModProjectile).isCracked)
            {
                SpawnUpSlash(target);
            }
        }

        public override void OnKill(int timeLeft)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
            if (voidCrystalIndex >= 0)
            {
                Projectile crystalProjectile = Main.projectile[voidCrystalIndex];
                if (crystalProjectile.active && crystalProjectile.type == ModContent.ProjectileType<VoidCrystal>())
                {
                    crystalProjectile.Kill();
                }
            }
            base.OnKill(timeLeft);
        }

        internal override void HandleChargingProjectileVisuals()
        {
            if (Projectile.frame >= 35 && Projectile.frame <= 37)
            {
                float velocityAngle = (Main.MouseWorld - Owner.Center).ToRotation();
                Projectile.rotation = velocityAngle + MathHelper.Pi;
                // Ensure sprite direction matches owner so PreDraw can flip vertically/horizontally
                Projectile.spriteDirection = Owner.direction == 1 ? 1 : -1;
                if (Projectile.frame >= 37)
                {
                    Projectile.frame = 35;
                }
                Projectile.frame++;
            } else if (KEYFRAMES.Contains(Projectile.frame))
            {
                float velocityAngle = (Main.MouseWorld - Owner.Center).ToRotation();
                Projectile.rotation = velocityAngle + MathHelper.Pi;
                // Ensure sprite direction matches owner so PreDraw can flip vertically/horizontally
                Projectile.spriteDirection = Owner.direction == 1 ? 1 : -1;

                if (holdFrameCounter > 0)
                {
                    holdFrameCounter--;
                    return;
                } else
                {
                    Projectile.frameCounter++;
                    Projectile.frame++;
                    holdFrameCounter = holdFrameCount;
                }

            } else
            {
                base.HandleChargingProjectileVisuals();
            }
        }

        public void SpawnUpSlash(NPC target)
        {
            if(clawUpSlashIndex == -1 && slashUpSlashIndex == -1)
            {
                Vector2 clawSpawnPos = target.Center + new Vector2(0, Projectile.height * 0.5f + 3f);
                Vector2 slashSpawnPos = target.Center + new Vector2(0, Projectile.height * 0.5f + 8f);
                Vector2 slowUpVel = new Vector2(0f, -2f);
                Vector2 fastUpVel = new Vector2(0f, -4f);
                clawUpSlashIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), clawSpawnPos, fastUpVel, ModContent.ProjectileType<VoidClawUpSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                slashUpSlashIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashSpawnPos, slowUpVel, ModContent.ProjectileType<VoidUpSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }

        public int frameDelayHandler()
        {
            int FrameDelay;
            if (Projectile.frame >= 0 && Projectile.frame <= 19)
            {
                FrameDelay = 2;
            } else if (Projectile.frame == 20)
            {
                FrameDelay = 3;
            } else if (Projectile.frame >= 21 && Projectile.frame <= 27)
            {
                FrameDelay = 2;
            } else if (Projectile.frame >= 28 && Projectile.frame <= 34)
            {
                FrameDelay = 6;
            } else if (Projectile.frame >= 38 && Projectile.frame <= 50)
            {
                FrameDelay = 1;
            } else
            {
                FrameDelay = 3;
            }
            return FrameDelay;
        }

        #region Drawing

        // Manual drawing is used to correct the origin of the projectile when drawn.
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect;
            if (Owner.direction == 1)
            {
                directionEffect = SpriteEffects.FlipVertically;
            } else
            {
                directionEffect = SpriteEffects.None;
            }
            Main.EntitySpriteDraw(punchTexture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);
            return false;
        }
        #endregion
    }
}