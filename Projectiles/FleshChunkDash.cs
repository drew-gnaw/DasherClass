using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class FleshChunkDash : ShieldWeaponProjectile
    {
        public override float ChargeTime => 26f;
        public override float LungeSpeed => 15f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.993f;
        public override float MaxPullBackRate => 0.85f;
        public override int OnHitIFrames => 30;

        // Tracks how many 2-ball waves have been spawned during charge (max 4 = 8 balls)
        private int _spawnedWaveCount = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 0.5f;
            Projectile.width = (int)(Projectile.scale * 14);
            Projectile.height = (int)(Projectile.scale * 45);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.frameCounter = 0;
        }

        public override void AI()
        {
            base.AI();

            // Spawn 2 balls at each quarter of charge (4 waves * 2 balls = 8 total)
            if (!isMidlunge && Owner.controlUseItem && Main.myPlayer == Projectile.owner)
            {
                int targetWave = (int)(currentChargeTime / (ChargeTime / 4f));
                while (_spawnedWaveCount < targetWave && _spawnedWaveCount < 2)
                {
                    SpawnChargingBalls();
                    _spawnedWaveCount++;
                }
            }
        }

        internal override void PerformLunge()
        {
            base.PerformLunge();

            // Fire 4 balls in a spread when the lunge triggers
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 aim = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
            for (int i = 0; i < 2; i++)
            {
                float spread = MathHelper.ToRadians(MathHelper.Lerp(-30f, 30f, i / 3f));
                Vector2 ballVelocity = aim.RotatedBy(spread) * (LungeSpeed * 0.7f);
                ballVelocity += Main.rand.NextVector2Circular(1f, 1f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Owner.Center,
                    ballVelocity,
                    ModContent.ProjectileType<FleshChunkBall>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        private void SpawnChargingBalls()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 spawnOffset = new Vector2(
                    Main.rand.NextFloat(-70f, 70f),
                    Main.rand.NextFloat(-50f, 20f)
                );
                Vector2 velocity = new Vector2(
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-7f, -2f)
                );
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Owner.Center + spawnOffset,
                    velocity,
                    ModContent.ProjectileType<FleshChunkBall>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.position);

            if (!Main.dedServ)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust.NewDustDirect(target.Center - new Vector2(16), 32, 32, DustID.Blood, vel.X, vel.Y, 100, default, 1.2f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(punchTexture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);
            return false;
        }
    }
}
