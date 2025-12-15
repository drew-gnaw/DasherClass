using DasherClass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class EtherealLanceDashChild : ModProjectile
    {
        // ai[0]: Parent projectile index (-1 if no parent or parent is gone)
        // ai[1]: Slot index for positioning around the player
        // ai[2]: Total number of slots for angle calculation
        
        private const float OrbitRadius = 60f;         // Distance from player center while orbiting
        private const float LaunchSpeed = 70f;         // Speed when shot forward
        private const float IndicatorLineLength = 1200f; // Length of the aiming indicator line
        private const float IndicatorLineAlpha = 0.4f;  // Transparency of the indicator line
        private const int AfterimageCount = 8;          // Number of afterimages to draw
        private const float SpinSpeed = 0.05f;          // How fast the lances spin (radians per frame)
        
        public Player Owner => Main.player[Projectile.owner];
        public int ParentIndex => (int)Projectile.ai[0];
        public int SlotIndex => (int)Projectile.ai[1];    // Which slot this lance occupies
        public int TotalSlots => (int)Projectile.ai[2];   // Total slots in the orbit circle
        
        private bool hasLaunched = false;
        private Vector2 launchDirection = Vector2.Zero;

        private float spinAngle = 0f; // Current spin angle (shared timing via GlobalTimeWrappedHourly)

        public override void SetStaticDefaults()
        {
            // Enable old position tracking for afterimages
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = AfterimageCount;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // Records old position and rotation
        }

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
            // Calculate the base angle for this slot (evenly distributed around the circle)
            float slotAngle = (SlotIndex / (float)TotalSlots) * MathHelper.TwoPi;
            
            // Add a global spin that's synchronized across all lances
            // Using GlobalTimeWrappedHourly ensures all lances spin together
            float globalSpin = Main.GlobalTimeWrappedHourly * 3f; // Spin speed multiplier
            
            // Final angle = slot's base position + global spin
            float finalAngle = slotAngle + globalSpin;
            
            // Convert angle to direction vector
            Vector2 orbitDirection = finalAngle.ToRotationVector2();
            
            // Position the projectile at orbit radius from player
            Projectile.Center = Owner.Center + orbitDirection * OrbitRadius;
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
            Projectile.timeLeft = 180; // 3 seconds of flight time after launch
            
            // Use the stored launch direction
            if (launchDirection == Vector2.Zero)
                launchDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
            
            Projectile.velocity = launchDirection * LaunchSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            Projectile.netUpdate = true;
        }

        /// <summary>
        /// Returns a rainbow color that cycles over time, with an offset based on slot index.
        /// Empress of Light style: lower saturation for a more pastel/white look.
        /// </summary>
        private Color GetRainbowColor(float timeOffset = 0f)
        {
            // Offset the hue based on SlotIndex for variety
            float hueOffset = (SlotIndex / (float)Math.Max(1, TotalSlots)) + timeOffset;
            float hue = ((Main.GlobalTimeWrappedHourly * 0.5f) + hueOffset) % 1f;
            // Lower saturation (0.4) for pastel/white look, high brightness
            return Main.hslToRgb(hue, 0.4f, 0.65f);
        }

        /// <summary>
        /// Returns a more saturated rainbow color for edge effects.
        /// </summary>
        private Color GetEdgeRainbowColor(float timeOffset = 0f)
        {
            float hueOffset = (SlotIndex / (float)Math.Max(1, TotalSlots)) + timeOffset;
            float hue = ((Main.GlobalTimeWrappedHourly * 0.5f) + hueOffset) % 1f;
            // Higher saturation for the edge glow
            return Main.hslToRgb(hue, 0.85f, 0.55f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Get colors for this projectile
            Color coreColor = GetRainbowColor();         // Pastel/white core
            Color edgeColor = GetEdgeRainbowColor();     // Saturated edge color
            
            // Draw indicator line if not yet launched
            if (!hasLaunched)
            {
                DrawIndicatorLine(edgeColor);
            }
            
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = SpriteEffects.None;
            
            // Draw afterimages when launched and moving
            if (hasLaunched && Projectile.velocity.Length() > 1f)
            {
                for (int i = AfterimageCount - 1; i >= 0; i--)
                {
                    Vector2 afterimagePos = Projectile.oldPos[i] + Projectile.Size * 0.5f;
                    float afterimageRotation = Projectile.oldRot[i];
                    
                    // Calculate fade based on position in trail
                    float progress = (float)i / AfterimageCount;
                    float alpha = (1f - progress) * 0.5f;
                    
                    // Shift hue slightly for each afterimage for a rainbow trail effect
                    Color afterimageColor = GetEdgeRainbowColor(progress * 0.3f) * alpha;
                    
                    Main.EntitySpriteDraw(
                        texture,
                        afterimagePos - Main.screenPosition,
                        null,
                        afterimageColor,
                        afterimageRotation,
                        origin,
                        Projectile.scale * (1f - progress * 0.2f), // Slightly smaller afterimages
                        effects,
                        0
                    );
                }
            }
            
            // Draw the edge color layer (slightly larger, behind the core)
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                edgeColor * 0.6f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.15f, // Slightly larger for edge glow
                effects,
                0
            );
            
            // Draw the main projectile with pastel core color
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                coreColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );
            
            // Draw a bright white center for that ethereal glow
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * 0.4f,
                Projectile.rotation,
                origin,
                Projectile.scale * 0.7f, // Smaller white core
                effects,
                0
            );
            
            return false;
        }

        private void DrawIndicatorLine(Color baseColor)
        {
            // Get the direction this projectile is pointing
            Vector2 direction = launchDirection;
            if (direction == Vector2.Zero)
                direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // Use a simple pixel texture for the line
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Draw multiple segments to create a fading line effect
            int segments = 60;
            float segmentLength = IndicatorLineLength / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 segmentStart = Projectile.Center + direction * (i * segmentLength);
                
                // Fade out the line as it gets further from the projectile
                float alpha = IndicatorLineAlpha * (1f - progress * 0.8f);
                Color segmentColor = baseColor * alpha;
                
                // Calculate rotation for the segment
                float rotation = direction.ToRotation();
                
                // Draw the segment
                Main.EntitySpriteDraw(
                    pixel,
                    segmentStart - Main.screenPosition,
                    new Rectangle(0, 0, 1, 1),
                    segmentColor,
                    rotation,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(segmentLength + 1, 2.5f), // width x height of segment
                    SpriteEffects.None,
                    0
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn rainbow-colored dust on death
            Color rainbowColor = GetEdgeRainbowColor();
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, 0f, 0f, 100, rainbowColor, 1f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }
    }
}