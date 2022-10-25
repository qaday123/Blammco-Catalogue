using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;

/* NOTES:
 * 
*/
namespace ExampleMod
{
    public class Quick_Fix : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Quick Fix";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/quick-fix_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Quick_Fix>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 1500f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "The First";
            string longDesc = "The only medigun that is unmodified and still serves it's original purpose. It's bulky, " +
                "it's rusty, and barely functional, but it'll heal you up in a pinch.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;
        }
        public static int ID;
        public float healthtoheal;
        public override void DoEffect(PlayerController user)
        {
            healthtoheal = (float)(Math.Round(user.healthHaver.GetMaxHealth() / 2, MidpointRounding.AwayFromZero) / 2);
            // heals roughly a quarter of max health
            //ETGModConsole.Log($"healthtoheal:{healthtoheal}");
            AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
            user.healthHaver.ApplyHealing(healthtoheal);
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.healthHaver.GetCurrentHealth() != user.healthHaver.GetMaxHealth();
        }

        /*public override DebrisObject Drop(PlayerController player)
        {
            Tools.Print($"Player dropped {this.DisplayName}");
            return base.Drop(player);
        }*/
    }
}