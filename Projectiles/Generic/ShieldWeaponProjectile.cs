using DasherClass;
using Microsoft.Xna.Framework;
using Terraria;

public abstract class ShieldWeaponProjectile : DashWeaponProjectile
{
    internal void ReelBack()
    {
        Owner.GiveUniversalIFrames(OnHitIFrames);

        if (Main.myPlayer != Projectile.owner)
            return;

        // Reel back after collision.
        Owner.velocity = Vector2.Reflect(Owner.velocity.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero)) * Owner.velocity.Length();

        // Create on-hit tile dust.
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width + 16, Projectile.height + 16);
        Projectile.Kill();
    }

    #region NPC Hit Collision Logic

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ReelBack();
    #endregion
}