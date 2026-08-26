using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace DasherClass.Projectiles
{
    public class FleshChunkBall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Vertical spritesheet with 3 distinct ball variants
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 0.25f;
            Projectile.width = Projectile.height = (int)(Projectile.scale * 50);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.timeLeft = 250;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Lock a random sprite variant for this ball's lifetime
            Projectile.frame = Main.rand.Next(3);
        }

        public override void AI()
        {
            // Gravity
            if (Projectile.velocity.Y < 16f)
                Projectile.velocity.Y += 0.4f;

            // Slight horizontal drag
            Projectile.velocity.X *= 0.99f;

            // Spin proportional to horizontal speed
            Projectile.rotation += Projectile.velocity.X * 0.05f + (Projectile.velocity.X != 0f ? 0.04f * Math.Sign(Projectile.velocity.X) : 0f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Dampen velocity on surface impact instead of dying
            if (Math.Abs(Projectile.velocity.X) < Math.Abs(oldVelocity.X))
                Projectile.velocity.X = oldVelocity.X * -0.3f;
            if (Math.Abs(Projectile.velocity.Y) < Math.Abs(oldVelocity.Y))
                Projectile.velocity.Y = oldVelocity.Y * -0.3f;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
