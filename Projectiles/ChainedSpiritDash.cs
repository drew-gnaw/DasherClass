using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;


namespace DasherClass.Projectiles
{
    public class ChainedSpiritDash : ShieldWeaponProjectile
    {
        public override float LungeSpeed => 20f;
        public override float ChargeTime => 15f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.995f;
        public override float MaxPullBackRate => 0.75f;
        public override int OnHitIFrames => 60;
        public override float HoldMinRadius => 23f;
        public override float HoldMaxRadius => 38f;
        public override float LungingMinRadius => 23f;
        public override float LungingMaxRadius => 38f;
        public override bool CycleChargingSprite => true;
        public override bool CycleLungingSprite => true;
        public override int ChargingFrameDelay => 7;
        public override int LungingFrameDelay => 6;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.6f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 50);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.frameCounter = 0;
        }

        #region Drawing

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Spawn several homing souls knocked out of the target
            if (Main.myPlayer == Projectile.owner)
            {
                int soulCount = 3;
                int projType = ModContent.ProjectileType<ChainedSpiritSoul>();
                Player ownerPlayer = Main.player[Projectile.owner];
                for (int i = 0; i < soulCount; i++)
                {
                    // spawn just behind the target relative to the player direction
                    Vector2 spawnPos = target.Center + ownerPlayer.velocity * 3f;
                    // small outward velocity plus random spread
                    Vector2 dirFromCenter = spawnPos - target.Center;
                    Vector2 baseVel = dirFromCenter.LengthSquared() > 0.001f ? Vector2.Normalize(dirFromCenter) * 2f : new Vector2(ownerPlayer.direction * -2f, 0f);
                    Vector2 initialVel = baseVel + Utils.RandomVector2(Main.rand, -0.8f, 0.8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, initialVel, projType, Projectile.damage / 2, 0f, Projectile.owner);
                }
            }

            // Call base behavior (if any) after spawning
            base.OnHitNPC(target, hit, damageDone);
        }
        #endregion
    }
}