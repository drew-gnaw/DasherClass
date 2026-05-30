﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class ShadowRipperDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 30f;
        public override float ChargeTime => 60f;
        public override float DashTime => 10f;
        private bool shadowSpawned = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 0.6f;
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

        internal override void PerformLunge()
        {
            base.PerformLunge();

            if (Main.myPlayer == Projectile.owner && !shadowSpawned)
            {
                Vector2 shadowSpawnPos = Owner.Center;
                Projectile.NewProjectile(
                    new EntitySource_Parent(Projectile),
                    shadowSpawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<ShadowRipperShadow>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
                shadowSpawned = true;
            }
        }
    }
}