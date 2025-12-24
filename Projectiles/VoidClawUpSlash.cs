using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class VoidClawUpSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.timeLeft = 60;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            UpSlash();

            Lighting.AddLight(Projectile.Center, 0.25f, 0.08f, 0.35f);

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                d.noGravity = true;
                d.scale = 0.9f;
                d.velocity *= 0.2f;
            }
        }

        public void UpSlash()
        {
            Player player = Main.player[Projectile.owner];

            // Initialize on first tick: store start angle and life
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f; // initialized
                float baseUp = -MathHelper.PiOver2;
                float startAngle = baseUp - MathHelper.ToRadians(60f) * player.direction;
                Projectile.ai[0] = startAngle; // starting angle
                Projectile.ai[1] = 0f; // elapsed ticks
                Projectile.localAI[1] = Projectile.timeLeft; // total life
            }

            // Advance elapsed and compute interpolation (0 -> 1) over projectile life
            Projectile.ai[1] += 1f;
            float elapsed = Projectile.ai[1];
            float life = Projectile.localAI[1] > 0f ? Projectile.localAI[1] : 60f;
            float t = MathHelper.Clamp(elapsed / life, 0f, 1f);

            // Sweep ~120 degrees around the player (mirrored by facing)
            float sweep = MathHelper.ToRadians(120f) * player.direction;
            float angle = Projectile.ai[0] + sweep * t;
            float radius = 48f;

            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            Projectile.Center = player.MountedCenter + offset;
            Projectile.rotation = angle + MathHelper.PiOver2;
            Projectile.velocity = Vector2.Zero;
        }
    }
}
