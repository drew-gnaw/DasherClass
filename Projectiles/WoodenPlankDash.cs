﻿using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

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

        public const float LungeSpeed = 9f;
        public const float ChargeTime = 50f;
        public float currentTime = 0f;

        // Stored player movement stats so we can restore them after charging.
        private float originalMoveSpeed = -1f;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 14;
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

            if (Owner.controlUseItem)
            {
                Time++;
                currentTime++;
                // Save original values the first time we start charging.
                if (originalMoveSpeed < 0f)
                {
                    originalMoveSpeed = Owner.moveSpeed;
                }

                // Reduce movement while charging. Adjust multipliers as desired.
                Owner.moveSpeed = originalMoveSpeed - 0.4f;
                if (currentTime >= ChargeTime) 
                {
                    GenerateDustOnOwnerHand(Owner);
                }
            } else
            {
                // Restore original movement values when not charging.
                if (originalMoveSpeed >= 0f)
                {
                    Owner.moveSpeed = originalMoveSpeed;
                    originalMoveSpeed = -1f;
                }

                if (currentTime >= ChargeTime) 
                {
                    if (!HasPerformedLunge)
                        PerformLunge();

                    // Vector2 topLeft = Projectile.Center + Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * 20f;
                    // Vector2 topRight = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * 20f;
                    // if (Time >= 8f && !Collision.CanHitLine(topLeft, 8, 8, topRight, 8, 8))
                    //     ReelBack();
                    HandleProjectileVisuals();
                    HandlePositioning();
                    Time++;
                    currentTime = 0f;
                }    
                Time++;  
                currentTime = 0f;
            }
        }

        internal static void GenerateDustOnOwnerHand(Player player)
        {
            if (Main.dedServ)
                return;

            Vector2 handOffset = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
            if (player.direction != 1)
                handOffset.X = player.bodyFrame.Width - handOffset.X;
            if (player.gravDir != 1f)
                handOffset.Y = player.bodyFrame.Height - handOffset.Y;

            handOffset -= new Vector2(player.bodyFrame.Width - player.width, player.bodyFrame.Height - player.height) / 2f;
            Vector2 rotatedHandPosition = player.RotatedRelativePoint(player.position + handOffset, true);
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Electric, 0f, 0f, 150, default, 1.0f);
                    dust.position = rotatedHandPosition;
                    dust.velocity = Vector2.Zero;
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                    dust.velocity += player.velocity;
                    if (Main.rand.NextBool())
                    {
                        dust.position += Utils.RandomVector2(Main.rand, -4f, 4f);
                        dust.scale += Main.rand.NextFloat() * 0.5f;
                    }
                }
            }
        }

        internal void PerformLunge()
        {
            if (Main.myPlayer != Projectile.owner)
                return;
            Owner.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction) * LungeSpeed;
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

        internal void HandleProjectileVisuals()
        {
            float velocityAngle = Projectile.velocity.ToRotation();
            Projectile.rotation = velocityAngle + MathHelper.Pi;
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 3 == 2)
            {
                Projectile.frame++;

                // Die at the end of the final punch.
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.Kill();
            }
        }

        internal void HandlePositioning()
        {
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Projectile.Center += Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction) * 30f;
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