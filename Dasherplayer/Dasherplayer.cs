using Terraria;
using Terraria.ModLoader;

namespace DasherClass.DasherPlayer
{
    public partial class DasherPlayer : ModPlayer
    {
        public bool isLunging = false;
        public bool isCharging = false;
        public bool isLance = false;
        public float lungeSpeed = 0f;
        public float lanceLungeGravity = 0.4f;

        public override void PreUpdate()
        {
            if (isLunging)
            {
                Player.maxFallSpeed = lungeSpeed;
                if (isLance)
                {
                    Player.gravity = lanceLungeGravity;
                }
            } else
            {
                base.PreUpdate();
            }
        }
    }
}