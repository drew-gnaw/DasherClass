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
        public override int ChargingFrameDelay => 1;
        public override int LungingFrameDelay => 1;

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
    }
}