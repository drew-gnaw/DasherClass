﻿using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using System.Collections;

namespace DasherClass.Projectiles
{
    public class EtherealLanceDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 60f;
        public override float ChargeTime => 50f;
        public override float DashTime => 15f;
        public override float PullBackScale => 0.995f;
        public override float MaxPullBackRate => 0.90f;
        public override int OnHitIFrames => 15;
        public override float HoldMinRadius => 67f;
        public override float HoldMaxRadius => 80f;
        public override float LungingMinRadius => 67f;
        public override float LungingMaxRadius => 80f;
        public override int FrameDelay => 1;
        public override bool CycleChargingSprite => false;
        public override bool CycleLungingSprite => false;

        // consts specific to Ethereal Lance: charge stages
        public const int MaxChargeStages = 10;
        public const float ChargeStageInterval = 50f; 

        private const float ChildDamageMultiplier = 0.5f; // Child projectiles deal 50% of lance damage
        private int SpawnedChildrenForStage = 0; // represents the largest chatge stage for which children have been spawned

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

        public override void AI()
        {
            int currentChargeStage = Math.Clamp((int)(currentChargeTime - ChargeTime) / (int)ChargeStageInterval, 0, MaxChargeStages);
            if (SpawnedChildrenForStage < currentChargeStage && Owner.controlUseItem)
            {
                SpawnChildProjectiles(++SpawnedChildrenForStage);
            }

            base.AI();
        }


        private void SpawnChildProjectiles(int chargeStage)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            int childType = ModContent.ProjectileType<EtherealLanceDashChild>();
            int childDamage = (int)(Projectile.damage * ChildDamageMultiplier);
            float knockback = Projectile.knockBack * 0.5f;

            int totalSlots = MaxChargeStages;
            
            int slotIndex = chargeStage - 1; // 0, 1, 2, 3...

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Owner.Center,
                Vector2.Zero,
                childType,
                childDamage,
                knockback,
                Projectile.owner,
                ai0: Projectile.whoAmI,   // Parent projectile index
                ai1: slotIndex,           // Slot index for positioning
                ai2: totalSlots           // Total number of slots for angle calculation
            );
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

        #region NPC Hit Collision Logic

        // public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ReelBack();
        #endregion
    }
}