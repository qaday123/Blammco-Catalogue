using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;

/* NOTES:
 * I don't like the stats it is giving out currently, 1. it doesn't actually mean much and 
 * 2. i dont think it fits in well with the item
 * Should I decrease movement speed instead? How much would it kill the fun?
 * Synergise with eyepatch - double the effect of both items :)
*/
namespace ExampleMod
{
    public class Bootlegger : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "Bootlegger";
            string resourceName = "ExampleMod/Resources/passives/bootlegger_sprite";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<Bootlegger>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "ARRRRGH";
            string longDesc = "Makes you hit harder, but shoot less often.\n\nThe pirates that lose a leg are often the scariest " +
                "ones because they have the most experience and have seen the most things. What's even scarier is seeing a one-legged " +
                "pirate move around at normal speed.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, 0.6f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.B;
        }
        
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            return base.Drop(player);
        }
        public static int ID;
    }
}