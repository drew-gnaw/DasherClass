using DasherClass.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Items.Armor.Viking
{
    [AutoloadEquip(EquipType.Body)]
    public class VikingBreastplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 20;
            Item.value = Item.buyPrice(gold: 1);
            Item.defense = 6;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<DasherDamageClass>() += 0.06f;
            player.GetCritChance<DasherDamageClass>() += 4;
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
