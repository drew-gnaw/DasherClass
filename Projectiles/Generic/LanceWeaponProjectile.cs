using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;

public abstract class LanceWeaponProjectile : DashWeaponProjectile
{
    public const float EndOfLungeVelocityScale = 0.2f;

    internal override void PerformLunge()
    {
        Owner.maxFallSpeed = 0f;
        base.PerformLunge();
    }   

    internal override void HandleProjectileVisuals()
    {
        if (currentDashTime >= DashTime)
        {
            Owner.velocity *= EndOfLungeVelocityScale;
            Owner.maxFallSpeed = 10f; // default max fall speed
        }
        base.HandleProjectileVisuals();
    }
}