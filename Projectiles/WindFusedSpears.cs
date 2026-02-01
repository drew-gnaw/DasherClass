using System;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class WindFusedSpears : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles";
        public int randomFrame;
        public int randomRotationOffset;
        public bool flippedOrNotHorizontal;
        public bool flippedOrNotVertical;
        public bool usingSpearOne;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 0.7f;
            Projectile.width = (int)(80 * Projectile.scale);
            Projectile.height = (int)(80 * Projectile.scale);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            randomFrame = Main.rand.Next(0, Main.projFrames[Projectile.type]);
            flippedOrNotHorizontal = Projectile.ai[1] > 0 ? true : false;
            flippedOrNotVertical = Main.rand.NextBool();
            usingSpearOne = Main.rand.NextBool();
            if(usingSpearOne)
            {
                Projectile.frame = (int)Projectile.ai[0];
            } else
            {
                Projectile.frame = (int)Projectile.ai[0] + 3;
            }
            if(flippedOrNotHorizontal)
            {
                if(flippedOrNotVertical)
                {
                    Projectile.rotation += MathHelper.ToRadians(80);
                } else
                {
                    Projectile.rotation -= MathHelper.ToRadians(10);
                }
            }
            else
            {
                if(flippedOrNotVertical)
                {
                    Projectile.rotation -= MathHelper.ToRadians(80);
                } else
                {
                    Projectile.rotation += MathHelper.ToRadians(10);
                }
            }
        }

        public override void AI()
        {
            // Add turquoise lighting
            Lighting.AddLight(Projectile.Center, 0.3f, 0.9f, 0.85f);

            // Spawn dust effects
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = 1.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Create additional visual effects on hit
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.DungeonSpirit);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                d.scale = 1.8f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, 6, 0, randomFrame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects;

            if(flippedOrNotHorizontal)
            {
                if(flippedOrNotVertical)
                {
                    effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                } else
                {
                    effects = SpriteEffects.FlipHorizontally;
                }
            }
            else
            {
                if(flippedOrNotVertical)
                {
                    effects = SpriteEffects.FlipVertically;
                } else
                {
                    effects = SpriteEffects.None;
                }
            }

            Main.EntitySpriteDraw(texture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            // Draw additive glow layer
            Color glowColor = new Color(150, 240, 230, 230) * 0.3f;
            Main.EntitySpriteDraw(texture, drawPos, frame, glowColor, Projectile.rotation, origin, Projectile.scale * 1.15f, effects, 0);
            return false;
        }
    }
}
