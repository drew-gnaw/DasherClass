﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class FleshRipperDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 30f;
        public override float ChargeTime => 200f;
        public override float DashTime => 10f;

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

        #region NPC Hit Collision Logic

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => OnHitEnemy(target);

        #endregion

        private void OnHitEnemy(NPC enemy)
        {
            GrantHealthRegen();
            CreateFleshChunks(enemy);
        }

        private void GrantHealthRegen()
        {
            Owner.AddBuff(BuffID.Regeneration, 300);
        }

        private void CreateFleshChunks(NPC enemy)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 chunkVelocity = Owner.velocity / 1.5f + new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -2f));

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    enemy.Center,
                    chunkVelocity,
                    ModContent.ProjectileType<FleshRipperChunk>(),
                    30,
                    0f,
                    Owner.whoAmI
                );
            }
        }
    }
}