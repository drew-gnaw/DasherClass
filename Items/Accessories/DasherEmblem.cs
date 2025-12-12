using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Items.Accessories
{
    public class DasherEmblem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage<DasherDamageClass>() += 0.15f;
        }

        public override void AddRecipes()
        {
            Recipe r = Recipe.Create(ItemID.AvengerEmblem);
            r.AddIngredient<DasherEmblem>();
            r.AddIngredient(ItemID.SoulofMight, 5);
            r.AddIngredient(ItemID.SoulofSight, 5);
            r.AddIngredient(ItemID.SoulofFright, 5);
            r.AddTile(TileID.TinkerersWorkbench);
            r.Register();
        }
    }
}
