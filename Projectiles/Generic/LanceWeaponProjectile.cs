using System.Diagnostics;
using DasherClass.DasherPlayer;
using Microsoft.Xna.Framework;
using Terraria;

public abstract class LanceWeaponProjectile : DashWeaponProjectile
{
    public const float EndOfLungeVelocityScale = 0.2f;

    internal override void PerformLunge()
    {
        base.PerformLunge();
        DasherPlayer dasherPlayer = Owner.GetModPlayer<DasherPlayer>();
        dasherPlayer.lanceLungeGravity = 0f;
        dasherPlayer.isLance = true;
    }

    internal override void HandleProjectileVisuals()
    {
        if (currentDashTime >= DashTime)
        {
            Owner.velocity *= EndOfLungeVelocityScale;
        }
        base.HandleProjectileVisuals();
    }

    protected override float GetDrawRotation()
    {
        float rotation = Projectile.rotation;
        if (Owner.direction == 1)
            rotation += MathHelper.PiOver4;
        else
            rotation -= MathHelper.PiOver4;
        return rotation;
    }
}