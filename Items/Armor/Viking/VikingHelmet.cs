using DasherClass.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DasherClass.Items.Armor.Viking
{
    [AutoloadEquip(EquipType.Head)]
    public class VikingHelmet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<VikingBreastplate>() && legs.type == ModContent.ItemType<VikingLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalizedValue("SetBonus");
            player.GetDamage<DasherDamageClass>() += 1f;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<DasherDamageClass>() += 0.04f;
            player.GetCritChance<DasherDamageClass>() += 2;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<VikingPlating>(10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
