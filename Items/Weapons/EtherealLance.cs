using DasherClass.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Items.Weapons
{
    public class EtherealLance : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons";
        public const int OnHitIFrames = (int) WoodenPlankDash.DashTime;

        public override void SetDefaults()
        {
            Item.width = Item.height = 40;
            Item.damage = 400;
            Item.DamageType = DasherDamageClass.Instance;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.useAnimation = Item.useTime = 40;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(gold: 10);
            Item.shoot = ModContent.ProjectileType<EtherealLanceDash>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.Red;
        }

        // Terraria seems to really dislike high crit values in SetDefaults.
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 10;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
    }
}