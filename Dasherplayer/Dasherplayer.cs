using Terraria;
using Terraria.ModLoader;

namespace DasherClass.DasherPlayer
{
    public partial class DasherPlayer : ModPlayer
    {
        public bool isLunging = false;
        public float lungeSpeed = 0f;

        public override void PreUpdate()
        {
            if (isLunging)
            {
                Player.maxFallSpeed = lungeSpeed;
            } else
            {
                base.PreUpdate();
            }
        }
    }
}