﻿using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace DasherClass.Projectiles
{
    public class BasicDefensiveMagicDash : ShieldWeaponProjectile
    {
        public override float LungeSpeed => 10f;
        public override float ChargeTime => 50f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.995f;
        public override float MaxPullBackRate => 0.75f;
        public override int OnHitIFrames => 30;
        public override float HoldMinRadius => 23f;
        public override float HoldMaxRadius => 38f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 25;
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

        public override void OnBlockProjectile(Projectile proj)
        {
            base.OnBlockProjectile(proj);

            // Only spawn the laser on the server or singleplayer
            if (Main.myPlayer == Projectile.owner)
            {
                // Find the target: the owner of the reflected projectile, if valid
                int targetPlayer = proj.owner;
                if (targetPlayer >= 0 && targetPlayer < Main.maxPlayers && Main.player[targetPlayer].active)
                {
                    Player owner = Main.player[Projectile.owner];
                    Vector2 playerCenter = owner.Center;
                    // Pick a random point around the player (circle)
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(40f, 80f);
                    Vector2 spawnPos = playerCenter + radius * angle.ToRotationVector2();
                    Vector2 targetPos = Main.player[targetPlayer].Center;
                    Vector2 direction = (targetPos - spawnPos).SafeNormalize(Vector2.UnitX);
                    float beamLength = (targetPos - spawnPos).Length();
                    // Spawn the laserbeam
                    int beam = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        direction,
                        ModContent.ProjectileType<BasicDefensiveMagicBeam>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        0f, // ai[0] = time
                        beamLength // ai[1] = length
                    );
                    if (beam != Main.maxProjectiles)
                    {
                        Main.projectile[beam].rotation = direction.ToRotation();
                    }
                }
            }
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