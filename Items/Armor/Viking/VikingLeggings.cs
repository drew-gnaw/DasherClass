using DasherClass.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Items.Armor.Viking
{
    [AutoloadEquip(EquipType.Legs)]
    public class VikingLeggings : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 16;
            Item.value = Item.buyPrice(gold: 1);
            Item.defense = 5;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += Collision.DrownCollision(player.position, player.width, player.height, player.gravDir) ? 0.35f : 0.1f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<VikingPlating>(20).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
