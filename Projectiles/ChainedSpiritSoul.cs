using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    // A small "soul" that can be spawned when an enemy is hit. It will home in on the nearest
    // valid NPC after being spawned.
    public class ChainedSpiritSoul : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";

        // Tunable parameters
        private const float MaxSpeed = 7f;
        private const float TurnStrength = 0.12f; // how quickly the velocity rotates toward the target
        private const int SearchRadius = 900;
        // Number of ticks to wait before homing activates
        private const int HomingDelay = 120;
        private int homingTimer = 0;

        // Pre-homing speed (fast -> slow) and post-homing acceleration duration (slow -> max)
        private const float PreHomingStartSpeed = 5f;
        private const float PreHomingEndSpeed = 1.5f;
        private const int PostHomingAccelDuration = 20;
        // How many ticks to disable homing after a successful hit
        private const int PostHitHomingCooldown = 20;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = 20;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // spawn a little dust trail
            Lighting.AddLight(Projectile.Center, 0.3f, 0.1f, 0.4f);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SpectreStaff);
                d.noGravity = true;
                d.velocity = Projectile.velocity * 0.15f;
                d.scale = 0.8f;
            }

            // delay homing for a few frames so souls are staggered before locking on
            homingTimer++;
            if (homingTimer < HomingDelay)
            {
                // Gradually reduce speed from PreHomingStartSpeed to PreHomingEndSpeed over HomingDelay
                float t = homingTimer / (float)HomingDelay;
                float currentSpeed = MathHelper.Lerp(PreHomingStartSpeed, PreHomingEndSpeed, t);
                if (Projectile.velocity.LengthSquared() > 0.001f)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * currentSpeed;
                else
                    Projectile.velocity = Utils.RandomVector2(Main.rand, -1f, 1f) * currentSpeed;
            }
            else
            {
                // Post-homing: find a target and accelerate from PreHomingEndSpeed to MaxSpeed over PostHomingAccelDuration
                int target = FindTarget();
                float postTimer = homingTimer - HomingDelay;
                float accelT = MathHelper.Clamp(postTimer / (float)PostHomingAccelDuration, 0f, 1f);
                float currentSpeed = MathHelper.Lerp(PreHomingEndSpeed, MaxSpeed, accelT);

                if (target >= 0 && Main.npc[target].active && !Main.npc[target].dontTakeDamage)
                {
                    Vector2 wanted = Main.npc[target].Center - Projectile.Center;
                    float distance = wanted.Length();
                    if (distance > 0.001f)
                        wanted = wanted / distance * currentSpeed;

                    // smoothly change velocity toward wanted
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, wanted, TurnStrength);
                }
                else
                {
                    // If no target, maintain current direction but accelerate toward currentSpeed
                    if (Projectile.velocity.LengthSquared() > 0.001f)
                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * currentSpeed;
                }
            }

            // rotate sprite in flight
            if (Projectile.velocity.LengthSquared() > 0.001f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                // Ensure spriteDirection matches horizontal velocity so the sprite doesn't appear flipped.
                Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            }
        }

        private int FindTarget()
        {
            int best = -1;
            float bestDistSq = SearchRadius * SearchRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.life <= 0 || npc.friendly || npc.dontTakeDamage)
                    continue;
                float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = i;
                }
            }
            return best;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // souls shouldn't collide with tiles; keep moving
            Projectile.tileCollide = false;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // small hit effect and die
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                d.noGravity = true;
            }
            // Disable homing for a short cooldown so the soul doesn't immediately re-target
            homingTimer = HomingDelay - PostHitHomingCooldown;
            if (homingTimer < 0)
                homingTimer = 0;

            // Make sure facing is consistent after hit: prefer current velocity direction if available.
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
        }

        // Helper: spawn this soul from other code (example):
        // Projectile.NewProjectile(source, spawnPosition, initialVelocity, ModContent.ProjectileType<ChainedSpiritSoul>(), damage, knockback, player.whoAmI);
    }
}
