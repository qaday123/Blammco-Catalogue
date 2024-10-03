using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.Misc;

namespace TF2Stuff
{
    public class ExamplePassive : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Test";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/passives/example_item_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<ExamplePassive>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Proof of Concept";
            string longDesc = "A stupid person tried to mod.\n\n" +
                "It's not working too well...";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //Adds the actual passive effect to the item
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.MovementSpeed, 2, StatModifier.ModifyMethod.ADDITIVE);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, 1, StatModifier.ModifyMethod.ADDITIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
            ID = item.PickupObjectId;
        }
        public static int ID;
        public int _kills;

        public void EnemyTookDamage(float damage, bool fatal, HealthHaver enemy)
        {
            if (fatal && enemy != null)
            {
                _kills++; 
            }
        }
        public void SpawnMeal(PlayerController player)
        {
            if (_kills >= 10)
            {
                //I deleted this but spawn it here
                _kills -= 10;
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnAnyEnemyReceivedDamage += EnemyTookDamage;
            player.OnRoomClearEvent += SpawnMeal;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            return base.Drop(player);
        }
    }
}