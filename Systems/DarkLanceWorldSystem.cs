using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using DasherClass.Items.Weapons;

namespace DasherClass.Systems
{
    // Replaces vanilla Dark Lance in shadow chests with the DasherClass version on world gen.
    public class DarkLanceWorldSystem : ModSystem
    {
        public override void PostWorldGen()
        {
            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null)
                    continue;

                for (int slot = 0; slot < Chest.maxItems; slot++)
                {
                    if (chest.item[slot].type == ItemID.DarkLance)
                        chest.item[slot].SetDefaults(ModContent.ItemType<DarkLance>());
                }
            }
        }
    }

    // Replaces vanilla Dark Lance in shadow lockbox loot (dropped by obsidian/hell crates).
    public class DarkLanceLootGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type != ItemID.ObsidianLockbox)
                return;

            foreach (IItemDropRule rule in itemLoot.Get())
            {
                if (rule is OneFromOptionsDropRule pool)
                {
                    for (int i = 0; i < pool.dropIds.Length; i++)
                    {
                        if (pool.dropIds[i] == ItemID.DarkLance)
                            pool.dropIds[i] = ModContent.ItemType<DarkLance>();
                    }
                }
            }
        }
    }
}
