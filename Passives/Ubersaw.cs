using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;

/* NOTES:
 * This item is kinda boring ngl.
 * In its current state it could do with some VFX player feedback.
 * A potential future rework is to have enemies drop some item/effect (like the GUNNER thing) on death to collect, which restores
   current active charge in a range of values
 * NEW REWORK: Increase dodge roll damage by 5? On dodge roll kill immediately recharge currently held active item
   use -> player.OnRolledIntoEnemy; // REWORKED
 * Add a stock slash vfx for when the effect takes place for more player feedback + it looks cool :)
*/
namespace TF2Stuff
{
    public class Ubersaw : PassiveItem
    {
        public static int ID;
        public static string consoleID;
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "Ubersaw";
            string resourceName = "TF2Items/Resources/passives/ubersaw_sprite";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<Ubersaw>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "It Vill Saw Through Your Bones";
            string longDesc = "Roll into an enemy to stab them with the Ubersaw and recharge your currently held item if they die.\n\n " +
                "This thing swiftly fell out of common use when guns proved to be a much more efficient way of charging things. " +
                "Now viewed more as a tool than a weapon, it still retains some of its siphoning ability.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, MODPREFIX);

            consoleID = MODPREFIX + ":" + item.name.ToID();
            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.D;

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.DodgeRollDamage, 2, StatModifier.ModifyMethod.ADDITIVE);
        }
        
        private void OnRollHit(PlayerController player, AIActor enemy)
        {
            if (player.CurrentItem != null && enemy.healthHaver.IsDead)
            {
                PlayerItem item = player.CurrentItem;
                if (item.IsOnCooldown)
                {
                    item.ClearCooldowns();
                    AkSoundEngine.PostEvent("Play_ubersaw_hit", gameObject);
                }
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnRolledIntoEnemy += OnRollHit;
           //player.OnAnyEnemyReceivedDamage += OnHit;
        }
        public override void DisableEffect(PlayerController player)
        {
            player.OnRolledIntoEnemy -= OnRollHit;
            base.DisableEffect(player);
        }
    }
}