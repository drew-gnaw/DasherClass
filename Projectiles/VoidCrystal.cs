using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class VoidCrystal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public bool isCracked = false;
        public float chargeTime = 120f;
        public float currentChargeTime = 0f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.scale = 0.8f;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];

            // Follow the player with slight smoothing
            Vector2 followOffset = new Vector2(-5f, -48f);
            Vector2 target = Owner.MountedCenter + followOffset;
            Projectile.Center = target;

            // spawn a little dust trail
            Lighting.AddLight(Projectile.Center, 0.8f, 0.1f, 0.6f);
            Projectile.spriteDirection = Owner.direction == 1 ? 1 : -1;
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity = Projectile.velocity * 0.15f;
                d.scale = 0.8f;
            }

            if (!isCracked && currentChargeTime > chargeTime)
            {
                Projectile.frame = 1;
                isCracked = true;
                // Crystal shard break sound when the crystal cracks
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);
            }
            else if (!isCracked)
            {
                currentChargeTime++;
            }

            // Shake when cracked (frame == 1)
            Projectile.ai[0] += 1f;
            if (isCracked)
            {
                float shakeMag = 1f;
                float shakeX = (float)Math.Sin(Projectile.ai[0] * 0.6f) * shakeMag;
                float shakeY = (float)Math.Cos(Projectile.ai[0] * 0.3f) * (shakeMag * 0.5f);
                Projectile.Center += new Vector2(shakeX, shakeY);
            }
        }

        // Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D punchTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = punchTexture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects directionEffect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Draw base sprite
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(punchTexture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, directionEffect, 0);

            // Draw violet glowmask similar to VoidRuneDash
            Texture2D glowTexture = punchTexture; // Reuse same texture for glowmask
            Color glowColor = new Color(180, 80, 255, 80) * 0.7f; // Moderate violet, semi-transparent
            float glowScale = Projectile.scale * 1.05f;
            Main.EntitySpriteDraw(glowTexture, drawPos, frame, glowColor, Projectile.rotation, origin, glowScale, directionEffect, 0);

            return false;
        }
    }
}
