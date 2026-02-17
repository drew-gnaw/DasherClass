using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using Terraria.Audio;

namespace DasherClass.Projectiles
{
    public class ShadeBasherDash : ShieldWeaponProjectile
    {
        public override float LungeSpeed => 15f;
        public override float ChargeTime => 20f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.993f;
        public override float MaxPullBackRate => 0.85f;
        public override int OnHitIFrames => 30;
        public override float HoldMinRadius => 30f;
        public override float HoldMaxRadius => 40f;
        public override float ChargingFrameDelay => 1f;
        public override float LungingFrameDelay => 1f;
        public override bool CycleChargingSprite => true;
        public override bool CycleLungingSprite => true;
        public override float LungingMinRadius => 30f;
        public override float LungingMaxRadius => 40f;
        public override int FrameDelay { get; set; }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 0.8f;
            Projectile.width = (int)(Projectile.scale * 14);
            Projectile.height = (int)(Projectile.scale * 45);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.frameCounter = 0;
        }

        public override void AI()
        {
            base.AI();
            
            // Spawn corruption particles while charging
            if (Owner.controlUseItem && !isMidlunge && Main.rand.NextBool(3))
            {
                if (!Main.dedServ)
                {
                    Dust dust = Dust.NewDustDirect(Owner.position, Owner.width, Owner.height, DustID.Corruption, 0f, 0f, 100, default, 0.8f);
                    dust.noGravity = true;
                    dust.velocity = (Owner.Center - dust.position) * 0.02f;
                }
            }
        }

        #region Drawing

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
            
            // Spawn explosion of Corruption dusts on hit
            if (!Main.dedServ)
            {
                for (int i = 0; i < 15; i++)
                {
                    Vector2 velocity = Utils.RandomVector2(Main.rand, -5f, 5f);
                    Dust dust = Dust.NewDustDirect(target.Center - new Vector2(16, 16), 32, 32, DustID.Corruption, velocity.X, velocity.Y, 100, default, 1.2f + Main.rand.NextFloat() * 0.8f);
                    dust.noGravity = true;
                }
            }
            
            // Spawn slash at target with rotation based on player's lunge direction
            float rotation = Owner.velocity.ToRotation();
            int proj = Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<ShadeBasherSlash>(), (int)(Projectile.damage * 1.5f), 0f, Projectile.owner, rotation);
        }

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