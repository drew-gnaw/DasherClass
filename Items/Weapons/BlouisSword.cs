using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Items.Weapons
{ 
	public class BlouisSword : ModItem, ILocalizedModType
	{
		public new string LocalizationCategory => "Items.Weapons";
		public override void SetDefaults()
		{
			Item.damage = 50;
			Item.DamageType = DasherDamageClass.Instance;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			// Allow holding the use button to charge. We'll spawn the projectile on release from a ModPlayer.
			Item.channel = true;
			// Prevent the swing graphic while charging and prevent the item from dealing melee during charge.
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

	}
}
