using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
	public class VoidSpirit : ModProjectile, ILocalizedModType
	{
		public new string LocalizationCategory => "Projectiles";

		private const float HomingRange = 900f;
		private const float HomingLerp = 0.15f;
		private const int ExplosionRadius = 120;
		private const int FrameDelay = 3;
		private const float VelocityDecay = 0.92f;
		private const float RotationSpeed = 0.08f;
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

		public Player Owner => Main.player[Projectile.owner];
		private bool isForming = true;
		private bool hasInitialized = false;

		public override void SetStaticDefaults()
		{
			// Total frame count should match the sprite sheet
			Main.projFrames[Projectile.type] = 19;
		}

		public override void SetDefaults()
		{
			Projectile.scale = 0.6f;
            Projectile.width = (int)(36 * Projectile.scale);
			Projectile.height = (int)(52 * Projectile.scale);
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DasherDamageClass.Instance;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, 0.6f, 0.1f, 0.9f);
			SpawnTrailDust();

			// Initialize velocity on first frame
			if (!hasInitialized)
			{
				Projectile.velocity.X = Main.rand.NextFloat(-1f, 1f);
				Projectile.velocity.Y = Main.rand.NextFloat(1f, 5f);
				hasInitialized = true;
			}

			// Formation phase: slow down and rotate
			if (isForming)
			{
				HandleFormation();
			}

			HandleFrames();

			// Only home after fully formed (frame 14+)
			if (!isForming && Projectile.frame >= 14)
			{
                Projectile.friendly = true;
				HandleHoming();
			}
		}

		private void HandleFormation()
		{
			// Slowly decelerate
			Projectile.velocity *= VelocityDecay;

			// Rotate based on x velocity direction (only before frame 14)
			if (Projectile.frame < 14)
			{
				if (Projectile.velocity.X >= 0)
				{
					Projectile.rotation += RotationSpeed;
				}
				else
				{
					Projectile.rotation -= RotationSpeed;
				}
			}

			// Check if velocity is near zero
			if (Projectile.velocity.LengthSquared() < 0.1f)
			{
				Projectile.velocity = Vector2.Zero;
				isForming = false;
			}
		}

		private void HandleFrames()
		{
			// Stay at frame 0 during formation
			if (isForming)
			{
				Projectile.frame = 0;
				return;
			}

			Projectile.frameCounter++;
			if (Projectile.frameCounter < FrameDelay)
			{
				return;
			}

			Projectile.frameCounter = 0;
			Projectile.frame++;

			int maxFrames = Main.projFrames[Projectile.type];
			if (Projectile.frame >= maxFrames)
			{
				// Once we reach the end, loop only the last 4 frames
				int loopStart = Math.Max(0, maxFrames - 4);
				Projectile.frame = loopStart;
			}

			// After frame 14, rotate with velocity for homing
			if (Projectile.frame >= 14 && Projectile.velocity.LengthSquared() > 0.01f)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
		}

		private void HandleHoming()
		{
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

		private void SpawnTrailDust()
		{
			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
				d.noGravity = true;
				d.scale = 1.1f;
				d.velocity = Projectile.velocity * 0.2f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			base.OnHitNPC(target, hit, damageDone);
			Explode();
            Projectile.Kill();
		}

		public override void OnKill(int timeLeft)
		{
			Explode();
		}

		private void Explode()
		{
			if (!Projectile.active)
			{
				return;
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

			Vector2 center = Projectile.Center;
			int oldWidth = Projectile.width;
			int oldHeight = Projectile.height;

			Projectile.position = center;
			Projectile.width = Projectile.height = ExplosionRadius * 2;
			Projectile.Center = center;

			for (int i = 0; i < 25; i++)
			{
				Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f);
				Dust d = Dust.NewDustDirect(center - new Vector2(ExplosionRadius), ExplosionRadius * 2, ExplosionRadius * 2, DustID.Shadowflame, velocity.X, velocity.Y, 150, default, 1.4f);
				d.noGravity = true;
			}

			Projectile.Damage();

			Projectile.width = oldWidth;
			Projectile.height = oldHeight;
			Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
			Vector2 origin = frame.Size() * 0.5f;
			SpriteEffects effects = SpriteEffects.None;

			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

			Texture2D glowTexture = texture;
			Color glowColor = new Color(180, 80, 255, 80) * 0.8f;
			float glowScale = Projectile.scale * 1.1f;
			Main.EntitySpriteDraw(glowTexture, drawPos, frame, glowColor, Projectile.rotation, origin, glowScale, effects, 0);

			return false;
		}
	}
}

