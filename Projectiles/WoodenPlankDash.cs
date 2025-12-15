﻿using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace DasherClass.Projectiles
{
    public class WoodenPlankDash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public Player Owner => Main.player[Projectile.owner];
        public bool HasPerformedLunge
        {
            get => Projectile.ai[0] == 1f;
            set
            {
                int newValue = value.ToInt();
                if (Projectile.ai[0] != newValue)
                {
                    Projectile.ai[0] = newValue;
                    Projectile.netUpdate = true;
                }
            }
        }

        public ref float Time => ref Projectile.ai[1];

        public const float LungeSpeed = 10f;
        public const float ChargeTime = 50f;
        public const float DashTime = 30f;
        public const float MaxPullBackRate = 0.75f;
        public float pullBackRate;
        public bool isMidlunge = false;
        public float currentChargeTime = 0f;
        public float currentDashTime = 0f;

        // Direction captured at the moment the charge ends (player release).
        private Vector2 releaseAimDirection = Vector2.Zero;
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

        #region AI
        public override void AI()
        {
            if (isMidlunge)
            {
                Projectile.friendly = true;
                HandleProjectileVisuals();
                HandlePositioning();
            } else if (Owner.controlUseItem)
            {
                if (currentChargeTime >= ChargeTime) 
                {
                    GenerateChargedParticle(Owner);
                } else
                {
                    GenerateChargingParticles(Owner, currentChargeTime);
                }
                currentChargeTime++;
                Projectile.friendly = false;
                float t = MathHelper.Clamp(currentChargeTime / ChargeTime, 0f, 1f);
                pullBackRate = MathHelper.Lerp(1f, MaxPullBackRate, 1 - (1 - t) * (1 - t));
                HandleChargingProjectileVisuals();
                HandleChargingPositioning(pullBackRate);
            } else
            {
                if (currentChargeTime >= ChargeTime) 
                {
                    isMidlunge = true;
                    pullBackRate = 1f;
                    if (!HasPerformedLunge)
                    {
                        // Capture aim direction at release on the owning client.
                        if (Projectile.owner == Main.myPlayer)
                        {
                            releaseAimDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
                        }
                        PerformLunge();
                    }
                } else
                {
                    Projectile.Kill();
                }
            }
        }

        internal static void GenerateChargingParticles(Player player, float time)
        {
            if (Main.dedServ)
                return;

            int amountGenerated = (int) time / 20;
            for (int i = 0; i < amountGenerated; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position + new Vector2(-4, 0), player.width + 15, player.height + 15, DustID.GemRuby, 0f, 0f, 100, default, 0.45f + Main.rand.NextFloat() * 0.35f);
                dust.noGravity = true;
                dust.fadeIn = 1f;

                // Pull toward player center; strength scales with distance and charge progress.
                Vector2 toCenter = player.Center - dust.position;
                float pullFactor = 0.01f; // adjust for stronger/weaker pull
                dust.velocity = toCenter * pullFactor + player.velocity * 0.2f;
            }
        }

        internal static void GenerateChargedParticle(Player player)
        {
            if (Main.dedServ)
                return;
            // Spawn a burst of charged particles that push outward from the player.
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position + new Vector2(-4, 0), player.width + 3, player.height + 3, DustID.GemEmerald, 0f, 0f, 100, default, 0.45f + Main.rand.NextFloat() * 0.35f);
                dust.noGravity = true;
                dust.fadeIn = 1f;

                // Push away from the player's center; add a small random spread and inherit some player velocity.
                Vector2 fromCenter = dust.position - player.Center;
                float pushFactor = 0.03f + Main.rand.NextFloat() * 0.06f;
                dust.velocity = fromCenter * pushFactor + player.velocity * 0.25f + Utils.RandomVector2(Main.rand, -0.5f, 0.5f);
            }
        }

        internal void PerformLunge()
        {
            if (Main.myPlayer != Projectile.owner)
                return;
               
            Owner.GiveUniversalIFrames(WoodenPlank.OnHitIFrames);

            Vector2 aim = releaseAimDirection;
            if (aim == Vector2.Zero)
                aim = Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction);

            Owner.velocity = aim * LungeSpeed;
            HasPerformedLunge = true;
        }

        internal void ReelBack()
        {
            Owner.GiveUniversalIFrames(WoodenPlank.OnHitIFrames);

            if (Main.myPlayer != Projectile.owner)
                return;

            // Reel back after collision.
            Owner.velocity = Vector2.Reflect(Owner.velocity.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero)) * Owner.velocity.Length();

            // Create on-hit tile dust.
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width + 16, Projectile.height + 16);
            Projectile.Kill();
        }

        internal void HandleChargingProjectileVisuals()
        {
            float velocityAngle = (Main.MouseWorld - Owner.Center).ToRotation();
            Projectile.rotation = velocityAngle + MathHelper.Pi;
        }

        internal void HandleChargingPositioning(float pullBackScale)
        {
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Vector2 aimDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
            if (aimDirection == Vector2.Zero)
                aimDirection = Vector2.UnitX * Owner.direction;

            float minRadius = 23f;
            float maxRadius = 38f;
            float t = Math.Abs(aimDirection.Y);
            float radius = MathHelper.Lerp(minRadius, maxRadius, t) * pullBackScale;
            Projectile.Center += aimDirection * radius;
            Projectile.direction = aimDirection.X >= 0f ? 1 : -1;
            Owner.ChangeDir(Projectile.direction);
        }

        internal void HandleProjectileVisuals()
        {
            float velocityAngle = releaseAimDirection.ToRotation();
            Projectile.rotation = velocityAngle + MathHelper.Pi;
            if (currentDashTime < DashTime)
            {
                currentDashTime++;
            }
            else
            {
                releaseAimDirection = Vector2.Zero;
                currentDashTime = 0f;
                isMidlunge = false;
                Projectile.Kill();
            }
        }

        internal void HandlePositioning()
        {
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Vector2 aimDirection = releaseAimDirection.SafeNormalize(Vector2.UnitX * Owner.direction);
            if (aimDirection == Vector2.Zero)
                aimDirection = Vector2.UnitX * Owner.direction;

            float minRadius = 23f;
            float maxRadius = 38f;
            float t = Math.Abs(aimDirection.Y);
            float radius = MathHelper.Lerp(minRadius, maxRadius, t);
            Projectile.Center += aimDirection * radius;
        }
        
        #endregion

        #region Drawing

        // Manual drawing is used to correct the origin of the projectile when drawn.
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(punchTexture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);
            return false;
        }
        #endregion

        #region NPC Hit Collision Logic

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ReelBack();
        #endregion
    }
}