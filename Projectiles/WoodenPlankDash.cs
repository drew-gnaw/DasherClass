﻿using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace DasherClass.Projectiles
{
    public class WoodenPlankDash : ShieldWeaponProjectile
    {
        public override float LungeSpeed => 10f;
        public override float ChargeTime => 50f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.995f;
        public override float MaxPullBackRate => 0.75f;
        public override int OnHitIFrames => 30;
        public override float HoldMinRadius => 23f;
        public override float HoldMaxRadius => 38f;
        public override float LungingMinRadius => 23f;
        public override float LungingMaxRadius => 38f;
        public override int FrameDelay { get; set; } = 1;
        public override bool CycleChargingSprite => false;
        public override bool CycleLungingSprite => false;
        public override float ChargingFrameDelay => 1f;
        public override float LungingFrameDelay => 1f;

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