using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class VoidPortal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public Vector2 targetPosition;
        public int FrameDelay = 4;
        public int FrameDelayCounter = 4;
        public int totalAllowedSpirits = 5;
        public int spiritsSpawned = 0;
        public int spiritSpawnDelay = 10;
        public int spiritSpawnDelayCounter = 10;
        public Player Owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 24;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.0f;
            Projectile.width = (int)(68 * Projectile.scale);
            Projectile.height = (int)(71 * Projectile.scale);
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.67f, 0.08f, 0.41f);
            HandleFrames();
            if (spiritsSpawned < totalAllowedSpirits)
            {
                CyclePortalSprite();
                spiritSpawnDelayCounter--;
                if (spiritSpawnDelayCounter <= 0)
                {
                    SpawnVoidSpirits();
                    spiritSpawnDelayCounter = spiritSpawnDelay;
                }
            }
            Projectile.velocity *= 0f;
        }

        public void HandleFrames()
        {
            if (Projectile.frame == 0)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item100, Projectile.Center); // Clinger Staff cast
            }
            if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
            {
                Projectile.frame++;
            }
            else
            {
                Projectile.Kill();
            }
        }

        public void SpawnVoidSpirits()
        {
            
            Vector2 spawnPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-10f, 10f));
            // VoidSpirit initializes its own velocity, so spawn with zero velocity
            Vector2 velocity = Vector2.Zero;
            int spiritType = ModContent.ProjectileType<VoidSpirit>();
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item8, spawnPos); // Demon Scythe cast
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity, spiritType, Projectile.damage, 0f, Projectile.owner);
            spiritsSpawned++;
        }
        public void CyclePortalSprite()
        {
            FrameDelayCounter++;
            if (FrameDelayCounter >= FrameDelay)
            {
                if (Projectile.frame > 12)
                {
                    Projectile.frame = 10;
                }
                Projectile.frame++;
                FrameDelayCounter = 0;
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
            Texture2D glowTexture = punchTexture; // Reuse same texture as simple glowmask
            Color glowColor = new Color(180, 80, 255, 80) * 0.7f; // Moderate violet, semi-transparent
            float glowScale = Projectile.scale * 1.05f;
            Main.EntitySpriteDraw(glowTexture, drawPos, frame, glowColor, Projectile.rotation, origin, glowScale, directionEffect, 0);

            return false;
        }
    }
}
