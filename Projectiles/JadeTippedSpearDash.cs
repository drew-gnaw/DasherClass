using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Projectiles
{
    public class JadeTippedSpearDash : LanceWeaponProjectile
    {
        public override float LungeSpeed => 19f;
        public override float ChargeTime => 1f;
        public override float DashTime => 30f;
        public override float PullBackScale => 0.8f;
        public override float MaxPullBackRate => 0.8f;
        public override int OnHitIFrames => 40;
        public override float HoldMinRadius => 15f;
        public override float HoldMaxRadius => 25f;
        public override float LungingMinRadius => 25f;
        public override float LungingMaxRadius => 35f;
        public override int FrameDelay { get; set; } = 1;
        public override bool CycleChargingSprite => false;
        public override bool CycleLungingSprite => false;
        public int lungeTimer = 0;
        public int plungeTime = 8;
        public bool offsetted = false;
        public bool isRightClickLunge = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 1.0f;
            Projectile.width = (int)(90* Projectile.scale);
            Projectile.height = (int)(90 * Projectile.scale);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DasherDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.rotation -= MathHelper.PiOver4;
        }

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer && Main.mouseRight && !isRightClickLunge)
            {
                PerformRightClickLunge();
            }
            
            if (isRightClickLunge && Main.mouseRight)
            {
                lungeTimer++;
                Projectile.Center = Owner.Center;
                Projectile.rotation = MathHelper.Pi + MathHelper.PiOver2 - MathHelper.PiOver4; // Down direction, accounting for sprite's -45° base
                offsetted = true;
                if(Owner.velocity.Y <= 0)
                {
                    SpawnSpears();
                }
                HandleDust();
            } else if(isRightClickLunge && Main.mouseRightRelease)
            {
                Projectile.Kill();
            }
            else
            {
                base.AI();
                
                if(isMidlunge)
                {
                    offsetted = true;
                }
                
                HandleDust();
            }
        }

        public void PerformRightClickLunge()
        {
            if (Main.myPlayer != Projectile.owner)
                return;
               
            Owner.GiveUniversalIFrames(OnHitIFrames);
            Projectile.tileCollide = true;
            //Aim directly downwards
            Vector2 aim = new Vector2(0,1);
            Owner.velocity = aim * LungeSpeed * 1.5f;
            DasherPlayer.DasherPlayer dasherPlayer = Owner.GetModPlayer<DasherPlayer.DasherPlayer>();
            dasherPlayer.isLunging = true;
            dasherPlayer.lungeSpeed = LungeSpeed * 1.5f; 
            HasPerformedLunge = true;
            isRightClickLunge = true;
        }

        public void HandleDust()
        {
            // Spawn shadow particles along the entire sprite
            if (Main.rand.NextBool(2))
            {
                // Sample multiple points along the lance length
                for (int i = 0; i < 4; i++)
                {
                    // Calculate position along the sprite from projectile center outward
                    float progress = i / 3f;
                    // Use projectile center as the base position
                    Vector2 direction = new Vector2((float)Math.Cos(Projectile.rotation), (float)Math.Sin(Projectile.rotation));
                    Vector2 dustPosition = Projectile.Center - direction * (progress * 40f); // Extend along the sprite
                    
                    Dust d = Dust.NewDustDirect(dustPosition - new Vector2(4, 4), 8, 8, DustID.DungeonWater);
                    d.noGravity = true;
                    
                    if (isMidlunge)
                    {
                        // During dash, dust trails behind
                        d.velocity = Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                        d.scale = 1.6f;
                    }
                    else
                    {
                        // While charging, dust swirls around
                        d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                        d.scale = 1.4f;
                        d.fadeIn = 1.2f;
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            
            // Add some impact particles
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.DungeonWater);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = 1.3f;
            }
        }

        public void SpawnSpears()
        {
            if(lungeTimer > plungeTime)
            {
                for(int i = 0; i < 5; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonWater);
                    d.noGravity = true;
                    d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    d.scale = 1.3f;
                }
                for(int i = 0; i < 10; i++)
                {
                    int[] majorOffsets = [-5, -4, -3, -2, -1, 1, 2, 3, 4, 5];
                    int spearSize;
                    if(i < 2 || i > 7)
                    {
                        spearSize = 0;
                    } else if(i >= 2 && i <= 3 || i >= 6 && i <=7)
                    {
                        spearSize = 1;
                    } else {
                        spearSize = 2;
                    }
                    int spearYoffset;
                    if (spearSize == 0)
                    {
                        spearYoffset = -15;
                    } else if (spearSize == 1)
                    {
                        spearYoffset = 0;
                    } else {
                        spearYoffset = 15;
                    }
                    int minorOffset = Main.rand.Next(-3, 3);
                    Vector2 velocity = Vector2.Zero;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(majorOffsets[i] * 25 + minorOffset, spearYoffset), velocity, 
                        ModContent.ProjectileType<WindFusedSpears>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: spearSize, ai1: majorOffsets[i]);
                }
            }
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects effects;
            if(!Main.mouseRight)
            {
                if (Owner.direction == 1)
                {
                    effects = SpriteEffects.FlipVertically;
                    if(!offsetted)
                    {
                        Projectile.rotation += MathHelper.PiOver4;
                    }
                }
                else
                {
                    effects = SpriteEffects.None;
                    if(!offsetted)
                    {
                        Projectile.rotation -= MathHelper.PiOver4;
                    }
                }
            } else {
                effects = SpriteEffects.None;
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Draw base sprite
            Main.EntitySpriteDraw(texture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            Color glowColor = new Color(40, 130, 85, 0) * 0.5f;
            Main.EntitySpriteDraw(texture, drawPos, frame, glowColor, Projectile.rotation, origin, Projectile.scale * 0.9f, effects, 0);
            return false;
        }
    }
}
