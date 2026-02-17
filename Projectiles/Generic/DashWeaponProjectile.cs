using DasherClass.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using DasherClass;
using DasherClass.DasherPlayer;


public abstract class DashWeaponProjectile : ModProjectile, ILocalizedModType
{
    public abstract float LungeSpeed { get; }
    public abstract float ChargeTime { get; }
    public abstract float DashTime { get; }
    public abstract float PullBackScale { get; }
    public abstract float MaxPullBackRate { get; }
    public abstract int OnHitIFrames { get; }

    public abstract float HoldMinRadius { get; }
    public abstract float HoldMaxRadius { get; }
    public abstract float LungingMinRadius { get; }
    public abstract float LungingMaxRadius { get; }
    public abstract int FrameDelay { get; set;}
    public abstract bool CycleLungingSprite { get; }
    public abstract bool CycleChargingSprite { get; }
    public abstract float ChargingFrameDelay { get; }
    public abstract float LungingFrameDelay { get; }

    public new string LocalizationCategory => "Projectiles";
    public Player Owner => Main.player[Projectile.owner];
    public bool HasPerformedLunge
    {
        get => Projectile.ai[0] == 1f;
        set
        {
            int newValue = value.ToInt();
            if (Projectile.ai[0] != newValue)
            {
                Projectile.ai[0] = newValue;
                Projectile.netUpdate = true;
            }
        }
    }

    public ref float Time => ref Projectile.ai[1];

    public float pullBackRate;
    public bool isMidlunge = false;
    public float currentChargeTime = 0f;
    public float currentDashTime = 0f;
    private Vector2 releaseAimDirection = Vector2.Zero;

    public override void AI()
    {
        if (Owner.dead)
        {
            Projectile.Kill();
        }
        if (isMidlunge)
        {
            Projectile.friendly = true;
            HandleProjectileVisuals();
            HandlePositioning();
        } else if (Owner.controlUseItem)
        {
            if (currentChargeTime >= ChargeTime) 
            {
                GenerateChargedParticle(Owner);
            } else
            {
                GenerateChargingParticles(Owner, currentChargeTime);
            }
            currentChargeTime++;
            Projectile.friendly = false;
            float t = MathHelper.Clamp(currentChargeTime / ChargeTime, 0f, 1f);
            pullBackRate = MathHelper.Lerp(1f, MaxPullBackRate, 1 - (1 - t) * (1 - t));
            HandleChargingProjectileVisuals();
            HandleChargingPositioning(pullBackRate);
        } else
        {
            if (currentChargeTime >= ChargeTime) 
            {
                isMidlunge = true;
                pullBackRate = 1f;
                if (!HasPerformedLunge)
                {
                    // Capture aim direction at release on the owning client.
                    if (Projectile.owner == Main.myPlayer)
                    {
                        releaseAimDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
                    }
                    PerformLunge();
                }
            } else
            {
                Projectile.Kill();
            }
        }
    }

    internal static void GenerateChargingParticles(Player player, float time)
        {
            if (Main.dedServ)
                return;

            int amountGenerated = (int) time / 20;
            for (int i = 0; i < amountGenerated; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position + new Vector2(-4, 0), player.width + 15, player.height + 15, DustID.GemRuby, 0f, 0f, 100, default, 0.45f + Main.rand.NextFloat() * 0.35f);
                dust.noGravity = true;
                dust.fadeIn = 1f;

                // Pull toward player center; strength scales with distance and charge progress.
                Vector2 toCenter = player.Center - dust.position;
                float pullFactor = 0.01f; // adjust for stronger/weaker pull
                dust.velocity = toCenter * pullFactor + player.velocity * 0.2f;
            }
        }

        internal static void GenerateChargedParticle(Player player)
        {
            if (Main.dedServ)
                return;
            // Spawn a burst of charged particles that push outward from the player.
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position + new Vector2(-4, 0), player.width + 3, player.height + 3, DustID.GemEmerald, 0f, 0f, 100, default, 0.45f + Main.rand.NextFloat() * 0.35f);
                dust.noGravity = true;
                dust.fadeIn = 1f;

                // Push away from the player's center; add a small random spread and inherit some player velocity.
                Vector2 fromCenter = dust.position - player.Center;
                float pushFactor = 0.03f + Main.rand.NextFloat() * 0.06f;
                dust.velocity = fromCenter * pushFactor + player.velocity * 0.25f + Utils.RandomVector2(Main.rand, -0.5f, 0.5f);
            }
        }

        internal virtual void PerformLunge()
        {
            if (Main.myPlayer != Projectile.owner)
                return;
               
            Owner.GiveUniversalIFrames(OnHitIFrames);

            Vector2 aim = releaseAimDirection;
            if (aim == Vector2.Zero)
                aim = Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction);

            Owner.velocity = aim * LungeSpeed;
            DasherPlayer dasherPlayer = Owner.GetModPlayer<DasherPlayer>();
            dasherPlayer.isLunging = true;
            dasherPlayer.lungeSpeed = LungeSpeed;
            HasPerformedLunge = true;
        }

        internal virtual void HandleChargingProjectileVisuals()
        {
            // Animate frames at a steady rate and point the projectile toward the mouse while charging.
            float velocityAngle = (Main.MouseWorld - Owner.Center).ToRotation();
            Projectile.rotation = velocityAngle + MathHelper.Pi;
            
            Projectile.spriteDirection = Owner.direction == 1 ? 1 : -1;

            // Simple frame timer: advance `Projectile.frame` every `frameDelay` ticks.
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= ChargingFrameDelay)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    if (CycleChargingSprite)
                    {
                        Projectile.frame = 0;
                    } else
                    {
                        Projectile.frame = Main.projFrames[Projectile.type] - 1;
                    }
                }
            }
        }

        internal virtual void HandleChargingPositioning(float pullBackScale)
        {
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Vector2 aimDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
            if (aimDirection == Vector2.Zero)
                aimDirection = Vector2.UnitX * Owner.direction;

            float t = Math.Abs(aimDirection.Y);
            float radius = MathHelper.Lerp(HoldMinRadius, HoldMaxRadius, t) * pullBackScale;
            Projectile.Center += aimDirection * radius;
            Projectile.direction = aimDirection.X >= 0f ? 1 : -1;
            Owner.ChangeDir(Projectile.direction);
        }

        internal virtual void HandleProjectileVisuals()
        {
            // Ensure sprite direction matches owner so PreDraw can flip vertically/horizontally
            Projectile.spriteDirection = Owner.direction == 1 ? 1 : -1;


            Projectile.frameCounter++;
            if (Projectile.frameCounter >= LungingFrameDelay)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    if (CycleLungingSprite)
                    {
                        Projectile.frame = 0;
                    } else
                    {
                        Projectile.frame = Main.projFrames[Projectile.type] - 1;
                    }
                }
            }
            if (currentDashTime < DashTime)
            {
                currentDashTime++;
            }
            else
            {
                releaseAimDirection = Vector2.Zero;
                currentDashTime = 0f;
                isMidlunge = false;
                DasherPlayer dasherPlayer = Owner.GetModPlayer<DasherPlayer>();
                dasherPlayer.isLunging = false;
                dasherPlayer.isLance = false;
                Projectile.Kill();
            }
        }

        internal void HandlePositioning()
        {
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Vector2 aimDirection = releaseAimDirection.SafeNormalize(Vector2.UnitX * Owner.direction);
            if (aimDirection == Vector2.Zero)
                aimDirection = Vector2.UnitX * Owner.direction;

            float minRadius = LungingMinRadius;
            float maxRadius = LungingMaxRadius;
            float t = Math.Abs(aimDirection.Y);
            float radius = MathHelper.Lerp(minRadius, maxRadius, t);
            Projectile.Center += aimDirection * radius;
        }
}