using System;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace DasherClass.Projectiles
{
    public class NightsVeilDash : ShieldWeaponProjectile
    {
        public override float LungeSpeed => 15f;
        public override float ChargeTime => 30f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.997f;
        public override float MaxPullBackRate => 0.85f;
        public override int OnHitIFrames => 30;
        public override float HoldMinRadius => 21f;
        public override float HoldMaxRadius => 35f;
        public override float LungingMinRadius => 35f;
        public override float LungingMaxRadius => 45f;
        public override int FrameDelay => 2;
        public override bool CycleChargingSprite => true;
        public override bool CycleLungingSprite => false;
        public int voidClawIndex = -1;
        public int voidCrystalIndex = -1;
        public bool onReelback = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.0f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 30f);
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
                Console.WriteLine(LungingMinRadius);
                Projectile.type = ModContent.ProjectileType<VoidClaw>();
                Projectile.scale = 1.5f;
                Main.projFrames[Projectile.type] = 10;
            }
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

        public void SpawnUpSlash(NPC target)
        {
            Vector2 clawSpawnPos = target.Center + new Vector2(0, Projectile.height * 0.5f + 3f);
            Vector2 slashSpawnPos = target.Center + new Vector2(0, Projectile.height * 0.5f + 8f);
            Vector2 slowUpVel = new Vector2(0f, -2f);
            Vector2 fastUpVel = new Vector2(0f, -4f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), clawSpawnPos, fastUpVel, ModContent.ProjectileType<VoidClawUpSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashSpawnPos, slowUpVel, ModContent.ProjectileType<VoidUpSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
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