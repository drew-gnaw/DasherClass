using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.Localization;

namespace DasherClass
{
    public class DasherDamageClass : DamageClass
    {
        internal static DasherDamageClass Instance;

        public override void Load() => Instance = this;
        public override void Unload() => Instance = null;
    }

    
}