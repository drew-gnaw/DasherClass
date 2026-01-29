using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class DarkLanceDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 15f;
        public override float ChargeTime => 12f;
        public override float DashTime => 18f;
        public override float PullBackScale => 0.8f;
        public override float MaxPullBackRate => 0.8f;
        public override int OnHitIFrames => 20;
        public override float HoldMinRadius => 15f;
        public override float HoldMaxRadius => 25f;
        public override float LungingMinRadius => 25f;
        public override float LungingMaxRadius => 35f;
        public override int FrameDelay { get; set; } = 1;
        public override bool CycleChargingSprite => false;
        public override bool CycleLungingSprite => false;
        public bool offsetted = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1; // Adjust based on your sprite
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.2f;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.rotation -= MathHelper.PiOver4;
        }

        public override void AI()
        {
            base.AI();

            // Spawn shadow particles while charging - concentrated at spear tip
            if (!isMidlunge && Owner.controlUseItem && Main.rand.NextBool(2))
            {
                // Calculate spear tip position (tip is at 0,0 of sprite, which is top-left corner of frame)
                Vector2 tipOffset = new Vector2(-Projectile.width / 2f, -Projectile.height / 2f).RotatedBy(Projectile.rotation);
                Vector2 tipPosition = Projectile.position + tipOffset;
                
                Dust d = Dust.NewDustDirect(tipPosition - new Vector2(4, 4), 8, 8, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                d.scale = 1.4f;
                d.fadeIn = 1.2f;
            }
            
            if(isMidlunge)
            {
                offsetted = true;
            }

            // Spawn shadow particles during dash - also at tip
            if (isMidlunge && Main.rand.NextBool(3))
            {
                Vector2 tipOffset = new Vector2(-Projectile.width / 2f, -Projectile.height / 2f).RotatedBy(Projectile.rotation);
                Vector2 tipPosition = Projectile.position + tipOffset;
                
                Dust d = Dust.NewDustDirect(tipPosition - new Vector2(4, 4), 8, 8, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity = Projectile.velocity * 0.3f;
                d.scale = 1.4f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.ShadowFlame, 240);
            
            // Add some impact particles
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = 1.3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects effects;
            if (Owner.direction == 1)
            {
                effects = SpriteEffects.FlipVertically;
                if(!offsetted)
                {
                    Projectile.rotation += MathHelper.PiOver4;
                }
            }
            else
            {
                effects = SpriteEffects.None;
                if(!offsetted)
                {
                    Projectile.rotation -= MathHelper.PiOver4;
                }
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Draw base sprite
            Main.EntitySpriteDraw(texture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            // Draw dark purple glow
            Color glowColor = new Color(120, 50, 200, 0) * 0.6f;
            Main.EntitySpriteDraw(texture, drawPos, frame, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, effects, 0);

            return false;
        }
    }
}
