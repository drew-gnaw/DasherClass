using DasherClass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class EtherealLanceDashChild : ModProjectile
    {
        // ai[0]: Parent projectile index (-1 if no parent or parent is gone)
        // ai[1]: Side offset multiplier (+1 for one side, -1 for the other)
        
        private const float OrbitRadius = 40f;         // Distance from player center while orbiting
        private const float LaunchSpeed = 70f;         // Speed when shot forward
        private const float RotationOffset = MathHelper.PiOver2; // 90 degrees perpendicular to aim direction
        
        public Player Owner => Main.player[Projectile.owner];
        public int ParentIndex => (int)Projectile.ai[0];
        public float SideMultiplier => Projectile.ai[1];
        public int ChargeStage => (int)Projectile.ai[2]; // integer from 1-max indicating charge stage. 1 is closest to player, max is farthest.
        
        private bool hasLaunched = false;
        private Vector2 launchDirection = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (hasLaunched)
            {
                // Already launched - fly in a straight line
                Projectile.friendly = true;
                Projectile.tileCollide = true;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                return;
            }

            // Check if parent projectile still exists and is valid
            Projectile parent = GetParentProjectile();
            if (parent == null || !parent.active)
            {
                // Parent is gone, kill self
                Projectile.Kill();
                return;
            }

            // Get the parent's lunge state by checking if it has performed the lunge
            bool parentHasLunged = parent.ai[0] == 1f;

            if (parentHasLunged)
            {
                // Parent has lunged - launch this projectile
                LaunchProjectile();
                return;
            }

            // Still orbiting - position around player and point towards cursor
            PositionAroundPlayer();
            PointTowardsCursor();
        }

        private Projectile GetParentProjectile()
        {
            int index = ParentIndex;
            if (index < 0 || index >= Main.maxProjectiles)
                return null;

            Projectile parent = Main.projectile[index];
            if (!parent.active || parent.type != ModContent.ProjectileType<EtherealLanceDash>())
                return null;

            return parent;
        }

        private void PositionAroundPlayer()
        {
            // Calculate position perpendicular to the aim direction
            Vector2 aimDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
            
            // Get perpendicular direction (rotate 90 degrees)
            Vector2 perpendicular = aimDirection.RotatedBy(RotationOffset * SideMultiplier);
            
            // Position the projectile at orbit radius from player
            Projectile.Center = Owner.Center + perpendicular * (OrbitRadius * ChargeStage);
        }

        private void PointTowardsCursor()
        {
            // Calculate rotation to point towards cursor
            Vector2 toMouse = Main.MouseWorld - Projectile.Center;
            Projectile.rotation = toMouse.ToRotation() + MathHelper.Pi;
            
            // Store the launch direction for when we fire
            launchDirection = toMouse.SafeNormalize(Vector2.UnitX * Owner.direction);
        }

        private void LaunchProjectile()
        {
            hasLaunched = true;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180; // 3 seconds of flight time after launch
            
            // Use the stored launch direction
            if (launchDirection == Vector2.Zero)
                launchDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
            
            Projectile.velocity = launchDirection * LaunchSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = SpriteEffects.None;
            
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn some dust on death
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
        }
    }
}