using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

public class DasherClassGlobalNPCLoot : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        switch (npc.type)
        {
            case NPCID.UndeadViking:
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DasherClass.Items.Materials.VikingPlating>(), 1, 6, 10));
                break;
        }
    }
}