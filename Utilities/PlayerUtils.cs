using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DasherClass.Balancing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Player;

namespace DasherClass
{
    public static partial class DasherClassUtil
    {
        
        #region Immunity Frames
        /// <summary>
        /// Computes the appropriate amount of immunity frames to grant a player when they are struck by an attack.<br />
        /// Accounts for all Calamity effects, but not effects from other mods.
        /// </summary>
        /// <param name="player">The player whose immunity frames are being computed.</param>
        /// <returns>The amount of immunity frames the player should receive if struck.</returns>
        public static int ComputeHitIFrames(this Player player, HurtInfo hurtInfo)
        {
            // Start with vanilla immunity frames.
            int iframes = BalancingConstants.VanillaDefaultIFrames + (player.longInvince ? BalancingConstants.CrossNecklaceIFrameBoost : 0);

            return iframes;
        }
      
        /// <summary>
        /// Computes the appropriate amount of immunity frames to grant a player when they activate a dodge.<br />
        /// Accounts for all Calamity effects, but not effects from other mods.
        /// </summary>
        /// <param name="player">The player whose immunity frames are being computed.</param>
        /// <returns>The amount of immunity frames the player should receive upon dodging.</returns>
        public static int ComputeDodgeIFrames(this Player player)
        {
            int iframes = BalancingConstants.VanillaDodgeIFrames + (player.longInvince ? BalancingConstants.CrossNecklaceIFrameBoost : 0);
            return iframes;
        }

        /// <summary>
        /// Computes the appropriate amount of immunity frames to grant a player when they activate a parry.<br />
        /// Accounts for all Calamity effects, but not effects from other mods.
        /// </summary>
        /// <param name="player">The player whose immunity frames are being computed.</param>
        /// <returns>The amount of immunity frames the player should receive upon parrying.</returns>
        public static int ComputeParryIFrames(this Player player)
        {
            int iframes = BalancingConstants.VanillaParryIFrames + (player.longInvince ? BalancingConstants.CrossNecklaceIFrameBoost_Parry : 0);
            return iframes;
        }

        // Currently, reflects are functionally equivalent to dodges.
        /// <summary>
        /// Computes the appropriate amount of immunity frames to grant a player when they activate a reflect.<br />
        /// Accounts for all Calamity effects, but not effects from other mods.
        /// </summary>
        /// <param name="player">The player whose immunity frames are being computed.</param>
        /// <returns>The amount of immunity frames the player should receive upon reflecting an attack.</returns>
        public static int ComputeReflectIFrames(this Player player) => player.ComputeDodgeIFrames();

        /// <summary>
        /// Checks whether the player has any kind of immunity frames (or "iframes" for short) available.
        /// </summary>
        /// <param name="player">The player whose immunity frames should be checked.</param>
        /// <returns>Whether or not they are currently in any immunity frames.</returns>
        public static bool HasIFrames(this Player player)
        {
            // Check old school iframes first (aka "cooldown timer -1". Regular hits, falling damage, etc.)
            if (player.immune || player.immuneTime > 0)
                return true;

            // Check more particular iframes. This primarily comes from traps, lava, and bosses.
            for (int i = 0; i < player.hurtCooldowns.Length; i++)
                if (player.hurtCooldowns[i] > 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Gives the player the specified number of immunity frames (or "iframes" for short) to a specific cooldown slot.<br />
        /// If the player already has more iframes than you want to give them, this function does nothing.<br />
        /// <br />
        /// <b>This should be used for effects that need to mock or mimic the iframes that would be granted by getting hit.</b>
        /// </summary>
        /// <param name="player">The player who should be given immunity frames.</param>
        /// <param name="cooldownSlot">The immunity cooldown slot to use. See TML documentation for which is which.</param>
        /// <param name="frames">The number of immunity frames to give.</param>
        /// <param name="blink">Whether or not the player should be blinking during this time.</param>
        /// <returns>Whether or not any immunity frames were given.</returns>
        public static bool GiveIFrames(this Player player, int cooldownSlot, int frames, bool blink = false)
        {
            // Check to see if there is any way for the player to get iframes from this operation.
            bool anyIFramesWouldBeGiven = (cooldownSlot < 0) ? player.immuneTime < frames : player.hurtCooldowns[cooldownSlot] < frames;

            // If they would get nothing, don't do it.
            if (!anyIFramesWouldBeGiven)
                return false;

            // Apply iframes thoroughly. Player.AddImmuneTime is not used because iframes should not exceed the intended amount.
            player.immune = true;
            player.immuneNoBlink = !blink;
            if (cooldownSlot < 0)
                player.immuneTime = frames;
            else
                player.hurtCooldowns[cooldownSlot] = frames;

            return true;
        }

        /// <summary>
        /// Gives the player the specified number of immunity frames (or "iframes" for short) to all cooldown slots.<br />
        /// If the player already has more iframes than you want to give them, this function does nothing.<br />
        /// <br />
        /// <b>This should be used for effects like dodges or true invulnerability that should prevent the player from being hit for a predetermined time.</b>
        /// </summary>
        /// <param name="player">The player who should be given immunity frames.</param>
        /// <param name="frames">The number of immunity frames to give.</param>
        /// <param name="blink">Whether or not the player should be blinking during this time.</param>
        /// <returns>Whether or not any immunity frames were given.</returns>
        public static bool GiveUniversalIFrames(this Player player, int frames, bool blink = false)
        {
            // Check to see if there is any way for the player to get iframes from this operation.
            bool anyIFramesWouldBeGiven = false;
            for (int i = 0; i < player.hurtCooldowns.Length; ++i)
                if (player.hurtCooldowns[i] < frames)
                    anyIFramesWouldBeGiven = true;

            // If they would get nothing, don't do it.
            if (!anyIFramesWouldBeGiven)
                return false;

            // Apply iframes thoroughly. Player.AddImmuneTime is not used because iframes should not exceed the intended amount.
            player.immune = true;
            player.immuneNoBlink = !blink;
            player.immuneTime = frames;
            for (int i = 0; i < player.hurtCooldowns.Length; ++i)
                if (player.hurtCooldowns[i] < frames)
                    player.hurtCooldowns[i] = frames;

            return true;
        }

        /// <summary>
        /// Removes all immunity frames (or "iframes" for short) from the specified player immediately.
        /// </summary>
        /// <param name="player">The player whose iframes should be removed.</param>
        public static void RemoveAllIFrames(this Player player)
        {
            player.immune = false;
            player.immuneNoBlink = false;
            player.immuneTime = 0;
            for (int i = 0; i < player.hurtCooldowns.Length; ++i)
                player.hurtCooldowns[i] = 0;
        }

        private static readonly FieldInfo hurtInfoDamageField = typeof(HurtInfo).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Lifted from Fargo's. Sets the damage and knockback of an incoming hit to zero, making it not affect the player.
        /// </summary>
        /// <param name="hurtInfo">The HurtInfo instance to nullify.</param>
        public static void NullifyHit(ref this HurtInfo hurtInfo)
        {
            object unboxedHurtInfo = hurtInfo;
            hurtInfoDamageField.SetValue(unboxedHurtInfo, 0);
            hurtInfo = (Player.HurtInfo)unboxedHurtInfo;
            hurtInfo.Knockback = 0;
        }
        #endregion

        #region Arms Control

        /// <summary>
        /// Gets an arm stretch amount from a number ranging from 0 to 1
        /// </summary>
        public static CompositeArmStretchAmount ToStretchAmount(this float percent)
        {
            if (percent < 0.25f)
                return CompositeArmStretchAmount.None;
            if (percent < 0.5f)
                return CompositeArmStretchAmount.Quarter;
            if (percent < 0.75f)
                return CompositeArmStretchAmount.ThreeQuarters;

            return CompositeArmStretchAmount.Full;
        }

        /// <summary>
        /// The exact same thing as Player.GetFrontHandPosition() except it properly accounts for gravity swaps instead of requiring the coders to do it manually afterwards.
        /// Additionally, it simply takes in the arm data instead of asking for the rotation and stretch separately.
        /// </summary>
        public static Vector2 GetFrontHandPositionImproved(this Player player, CompositeArmData arm)
        {
            Vector2 position = player.GetFrontHandPosition(arm.stretch, arm.rotation * player.gravDir).Floor();

            if (player.gravDir == -1f)
            {
                position.Y = player.position.Y + (float)player.height + (player.position.Y - position.Y);
            }

            return position;
        }

        /// <summary>
        /// The exact same thing as Player.GetBackHandPosition() except it properly accounts for gravity swaps instead of requiring the coders to do it manually afterwards.
        /// Additionally, it simply takes in the arm data instead of asking for the rotation and stretch separately.
        /// </summary>
        public static Vector2 GetBackHandPositionImproved(this Player player, CompositeArmData arm)
        {
            Vector2 position = player.GetBackHandPosition(arm.stretch, arm.rotation * player.gravDir).Floor();

            if (player.gravDir == -1f)
            {
                position.Y = player.position.Y + (float)player.height + (player.position.Y - position.Y);
            }

            return position;
        }

        /// <summary>
        /// Properly sets the player's held item rotation and position by doing the annoying math for you, since vanilla decided to be wholly inconsistent about it!
        /// This all assumes the player is facing right. All the flip stuff is automatically handled in here
        /// </summary>
        /// <param name="player">The player for which we set the hold style</param>
        /// <param name="desiredRotation">The desired rotation of the item</param>
        /// <param name="desiredPosition">The desired position of the item</param>
        /// <param name="spriteSize">The size of the item sprite (used in calculations)</param>
        /// <param name="rotationOriginFromCenter">The offset from the center of the sprite of the rotation origin</param>
        /// <param name="noSandstorm">Should the swirly effect from the sandstorm jump be disabled</param>
        /// <param name="flipAngle">Should the angle get flipped with the player, or should it be rotated by 180 degrees</param>
        /// <param name="stepDisplace">Should the item get displaced with the player's height during the walk anim? </param>
        public static void CleanHoldStyle(Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            if (noSandstorm)
                player.sandStorm = false;

            //Since Vector2.Zero isn't a compile-time constant, we can't use it directly as the default parameter
            if (rotationOriginFromCenter == null)
                rotationOriginFromCenter = Vector2.Zero;

            Vector2 origin = rotationOriginFromCenter.Value;
            //Flip the origin's X position, since the sprite will be flipped if the player faces left.
            origin.X *= player.direction;
            //Additionally, flip the origin's Y position in case the player is in reverse gravity.
            origin.Y *= player.gravDir;

            player.itemRotation = desiredRotation;

            if (flipAngle)
                player.itemRotation *= player.direction;
            else if (player.direction < 0)
                player.itemRotation += MathHelper.Pi;

            //This can anchors the item to rotate around the middle left of its sprite
            //Vector2 consistentLeftAnchor = (player.itemRotation).ToRotationVector2() * -10f * player.direction;

            //This anchors the item to rotate around the center of its sprite.
            Vector2 consistentCenterAnchor = player.itemRotation.ToRotationVector2() * (spriteSize.X / -2f - 10f) * player.direction;

            //This shifts the item so it rotates around the set origin instead
            Vector2 consistentAnchor = consistentCenterAnchor - origin.RotatedBy(player.itemRotation);

            //The sprite needs to be offset by half its sprite size.
            Vector2 offsetAgain = spriteSize * -0.5f;

            Vector2 finalPosition = desiredPosition + offsetAgain + consistentAnchor;

            //Account for the players extra height when stepping
            if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                {
                    finalPosition -= Vector2.UnitY * 2f;
                }
            }

            player.itemLocation = finalPosition + new Vector2(spriteSize.X * 0.5f, 0);
        }
        #endregion

        #region Visual Layers
        public static void HideAccessories(this Player player, bool hideHeadAccs = true, bool hideBodyAccs = true, bool hideLegAccs = true, bool hideShield = true)
        {
            if (hideHeadAccs)
                player.face = -1;

            if (hideBodyAccs)
            {
                player.handon = -1;
                player.handoff = -1;

                player.back = -1;
                player.front = -1;
                player.neck = -1;
            }

            if (hideLegAccs)
            {
                player.shoe = -1;
                player.waist = -1;
            }

            if (hideShield)
                player.shield = -1;
        }
        #endregion

        /// <summary>
        /// A shorthand bool to check if the player can continue using the holdout or not.
        /// </summary>
        /// <param name="player">The player using the holdout.</param>
        /// <returns>Returns <see langword="true"/> if the player CAN'T use the item.</returns>
        public static bool CantUseHoldout(this Player player, bool needsToHold = true) => player == null || !player.active || player.dead || (!player.channel && needsToHold) || player.CCed || player.noItems;

        /// <summary>
        /// Makes the given player send the given packet to all appropriate receivers.<br />
        /// If server is false, the packet is sent only to the multiplayer host.<br />
        /// If server is true, the packet is sent to all clients except the player it pertains to.
        /// </summary>
        /// <param name="player">The player to whom the packet's data pertains.</param>
        /// <param name="packet">The packet to send with certain parameters.</param>
        /// <param name="server">True if a dedicated server is broadcasting information to all players.</param>
        public static void SendPacket(this Player player, ModPacket packet, bool server)
        {
            // Client: Send the packet only to the host.
            if (!server)
                packet.Send();

            // Server: Send the packet to every OTHER client.
            else
                packet.Send(-1, player.whoAmI);
        }
    }
}
