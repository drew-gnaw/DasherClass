﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace DasherClass.Projectiles
{
    public class FleshRipperDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 60f;
        public override float ChargeTime => 50f;
        public override float DashTime => 15f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.2f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 30);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.frameCounter = 0;
        }


        #region Drawing

        // Manual drawing is used to correct the origin of the projectile when drawn.
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            SpriteEffects effects;
            float drawRotation = Projectile.rotation;

            if (Owner.direction == 1)
            {
                effects = SpriteEffects.FlipVertically;
                drawRotation += MathHelper.PiOver4;
            }
            else
            {
                effects = SpriteEffects.None;
                drawRotation -= MathHelper.PiOver4;
            }

            Main.EntitySpriteDraw(
                punchTexture,
                Projectile.Center - Main.screenPosition,
                frame,
                lightColor,
                drawRotation, 
                origin,
                Projectile.scale,
                effects,
                0
            );
            return false;
        }
        #endregion

        #region NPC Hit Collision Logic

        #endregion
    }
}