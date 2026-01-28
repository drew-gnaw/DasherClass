using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using DasherClass.Projectiles;

namespace DasherClass.Items.Weapons
{
    public class DarkLance : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DasherDamageClass.Instance;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.Orange;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<DarkLanceDash>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.autoReuse = false;
        }
    }
}
