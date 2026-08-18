using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class MoltenMantleDash : ShieldWeaponProjectile
    {
        public override float ChargeTime => 26f;
        public override float LungeSpeed => 15f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.993f;
        public override float MaxPullBackRate => 0.85f;
        public override int OnHitIFrames => 30;
        public override int frameDelay => 3;

        // Radii and counts for the 3 rings: triangle, hexagon, near-circle
        private static readonly float[] RingRadii = { 80f, 160f, 260f };
        private static readonly int[] RingCounts = { 3, 6, 12 };

        private int _spawnedRingCount = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 0.8f;
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

            // Spawn rings at each third of charge: ring 0 ~tick 9, ring 1 ~tick 17, ring 2 ~tick 26
            if (!isMidlunge && Owner.controlUseItem && Main.myPlayer == Projectile.owner)
            {
                int targetRing = (int)(currentChargeTime / (ChargeTime / 3f));
                while (_spawnedRingCount < targetRing && _spawnedRingCount < 3)
                {
                    SpawnExplosionRing(_spawnedRingCount);
                    _spawnedRingCount++;
                }
            }
        }

        private void SpawnExplosionRing(int ringIndex)
        {
            float radius = RingRadii[ringIndex];
            int count = RingCounts[ringIndex];
            float angleStep = MathHelper.TwoPi / count;

            // Offset so ring 0 (triangle) points up, others align nicely
            float startAngle = -MathHelper.PiOver2;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 offset = Vector2.UnitX.RotatedBy(angle) * radius;

                // Sprite apex (down at rotation=0) should point away from the player
                float rotation = angle - MathHelper.PiOver2;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Owner.Center + offset,
                    Vector2.Zero,
                    ModContent.ProjectileType<MoltenMantleExplosion>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    rotation
                );
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            SoundEngine.PlaySound(SoundID.Item20, Projectile.position);

            if (!Main.dedServ)
            {
                for (int i = 0; i < 18; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust dust = Dust.NewDustDirect(target.Center - new Vector2(16), 32, 32,
                        DustID.Torch, vel.X, vel.Y, 100, default, 1.4f);
                    dust.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(punchTexture, Projectile.Center - Main.screenPosition, frame,
                lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);
            return false;
        }
    }
}
