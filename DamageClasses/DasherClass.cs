using Terraria.ModLoader;

namespace DasherClass
{
    public class DasherDamageClass : DamageClass
    {
        internal static DasherDamageClass Instance;

        public override void Load() => Instance = this;
        public override void Unload() => Instance = null;
    }
    
}