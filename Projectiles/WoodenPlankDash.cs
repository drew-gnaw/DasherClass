using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

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
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 14;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.6f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 60);
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
            if (!HasPerformedLunge)
                PerformLunge();

            Vector2 topLeft = Projectile.Center + Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * 40f;
            Vector2 topRight = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * 40f;
            if (Time >= 8f && !Collision.CanHitLine(topLeft, 8, 8, topRight, 8, 8))
                ReelBack();
            HandleProjectileVisuals();
            HandlePositioning();
            Time++;
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