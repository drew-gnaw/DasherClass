using System;
using System.Collections.Generic;
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
        public override float ChargeTime => 42f;
        public override float DashTime => 42f;
        public override float PullBackScale => 1.0f; // No pullback 
        public override float MaxPullBackRate => 1.0f;
        public override int OnHitIFrames => 30;    
        public override float HoldMinRadius => 80f;
        public override float HoldMaxRadius => 100f;
        public override float LungingMinRadius => 80f;
        public override float LungingMaxRadius => 100f;
        public override int FrameDelay {get; set;} = 2;
        public override bool CycleChargingSprite => false;
        public override bool CycleLungingSprite => false;
        public bool crystalCharged = false;
        public int voidClawIndex = -1;
        public int voidCrystalIndex = -1;
        public bool onReelback = false;
        public int holdFrameCount = 3;
        public int holdFrameCounter = 0;
        public int clawSlashIndex = -1;
        public bool spawnedPortal = false;
        public int[] KEYFRAMES = [4, 9, 14, 18];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 35;
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
                if(!crystalCharged)
                {
                    Projectile crystalProjectile = Main.projectile[voidCrystalIndex];
                    if(((VoidCrystal)crystalProjectile.ModProjectile).isCracked)
                    {
                        crystalCharged = true;
                    }
                    crystalProjectile.Kill();
                }
                Projectile.width = 76;
                // Spawn 3 shadowflame dusts at random positions around the center (not exactly center)
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(Projectile.width * 0.3f, Projectile.width * 0.6f);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Vector2 dustPos = Projectile.Center + offset.RotatedBy(Projectile.rotation);
                    Dust dust = Dust.NewDustDirect(dustPos, 4, 4, DustID.Shadowflame, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 120, default, 1.1f);
                    dust.noGravity = true;
                    dust.velocity *= 0.7f;
                }
                if(Projectile.frame >= Main.projFrames[Projectile.type] - 1)
                {
                    Projectile.Kill();
                }
            }
            FrameDelay = FrameDelayHandler();
            if (voidCrystalIndex == -1)
            {
                voidCrystalIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + new Vector2(-10, -48), Projectile.velocity * 0, ModContent.ProjectileType<VoidCrystal>(), 0, 0, Projectile.owner);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.ShadowFlame, 300);
            
            // Spawn void explosion at target with 1.5x damage
            if (Main.myPlayer == Projectile.owner)
            {
                int explosionDamage = (int)(Projectile.damage * 1.5f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<VoidExplosion>(),
                    explosionDamage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
            
            if (crystalCharged)
            {
                SpawnPortal(target);
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
            if (Projectile.frame >= 14 && Projectile.frame <= 16)
            {
                float velocityAngle = (Main.MouseWorld - Owner.Center).ToRotation();
                Projectile.rotation = velocityAngle + MathHelper.Pi;
                // Ensure sprite direction matches owner so PreDraw can flip vertically/horizontally
                Projectile.spriteDirection = Owner.direction == 1 ? 1 : -1;
                int frameDelay = FrameDelayHandler();
                if (holdFrameCounter >= frameDelay)
                {
                    if (Projectile.frame >= 16)
                    {
                        Projectile.frame = 14;
                    }
                    Projectile.frame++;
                    holdFrameCounter = 0;
                } else
                {
                    holdFrameCounter++;
                }
            }else
            {
                base.HandleChargingProjectileVisuals();
            }
        }

        public void SpawnPortal(NPC target)
        {
            if(!spawnedPortal)
            {
                 Vector2 clawSpawnPos = target.Center - new Vector2(0, target.height / 2 + Projectile.height / 2);
                clawSlashIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), clawSpawnPos, Vector2.Zero, ModContent.ProjectileType<VoidPortal>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                spawnedPortal = true;
            }
        }

        public int FrameDelayHandler()
        {
            int FrameDelay;
            if (Projectile.frame >= 0 && Projectile.frame <= 13)
            {
                FrameDelay = 3;
            } else if (Projectile.frame >= 14 && Projectile.frame <= 16)
            {
                FrameDelay = 5;
            }
            else if (Projectile.frame >= 17 && Projectile.frame <= 23)
            {
                if (Projectile.frame == 17)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item20, Projectile.Center); // unholy trident cast
                }
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
            SpriteEffects directionEffect = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // Draw base sprite
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(punchTexture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);

            // Draw violet glowmask perfectly aligned
            Texture2D glowTexture = punchTexture; // Use the same texture for now, ideally use a separate glowmask asset
            Color glowColor = new Color(180, 80, 255, 80) * 0.7f; // Moderate violet, semi-transparent
            float glowScale = Projectile.scale * 1.05f;
            Main.EntitySpriteDraw(glowTexture, drawPos, frame, glowColor, Projectile.rotation, origin, glowScale, directionEffect, 0);

            return false;
        }
        #endregion
    }
}