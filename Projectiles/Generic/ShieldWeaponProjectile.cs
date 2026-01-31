using System;
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

    public override void AI()
    {
        base.AI();
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            if (Main.projectile[i].active && Main.projectile[i].hostile && Main.projectile[i].damage > 0
                && Projectile.Colliding(Projectile.Hitbox, Main.projectile[i].Hitbox))
            {
                OnBlockProjectile(Main.projectile[i]);
            }
        }
    }

    public virtual void OnBlockProjectile(Projectile proj) {
        proj.hostile = false;
        proj.friendly = true;
        proj.owner = Owner.whoAmI;

        proj.velocity = proj.DirectionFrom(Projectile.Center) * proj.velocity.Length();

        proj.netUpdate = true;
    }

    #region NPC Hit Collision Logic

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ReelBack();
    #endregion
}