using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;

/* NOTES:
 * Synergise with any shield in order to gain a speed boost 
*/
namespace TF2Stuff
{
    public class Demoknight_Boots : PassiveItem
    {
        public static int ID;
        public static string consoleID;
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "Ali Baba's Wee Booties";
            string resourceName = "TF2Items/Resources/passives/ali_babas_booties_sprite";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<Demoknight_Boots>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Steel Toe-d";
            string longDesc = "Makes your toes much more fortified.\n\nA prized possession in the hands of madmen who charge " +
                "with no fear at their enemies. If only you had some sort of shield...";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, MODPREFIX);
            consoleID = MODPREFIX + ":" + item.name.ToID();

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, 1, StatModifier.ModifyMethod.ADDITIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Coolness, 2, StatModifier.ModifyMethod.ADDITIVE);

            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.C;
        }
        
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
        }
    }
}