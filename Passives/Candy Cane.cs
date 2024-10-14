using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;

/* NOTES:
 * Would be cool if the casing vfx showed up above the player when collecting a heart - player.BloopItemAboveHead(); 
 * just need to find casing sprite
*/
namespace TF2Stuff
{
    public class Candy_Cane : PassiveItem
    {
        public static int ID;
        public static string consoleID;

        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "Candy Cane";
            string resourceName = "TF2Items/Resources/passives/candy_cane_sprite";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<Candy_Cane>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Sweet!";
            string longDesc = "An oversized candy cane with a trigger and chocolate silencer attached to it. It is advertised " +
                "to bring the wielder great fortune when tending to wounds.\n\nWhen arriving to the hollow, make sure to not lick " +
                "it. Please don't. Just don't.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, MODPREFIX);

            consoleID = MODPREFIX + ":" + item.name.ToID();
            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.B;
        }
        float currentHealth;
        private void OnHealthChanged(float resultHealth, float maxHealth)
        {
            PlayerController player = Owner as PlayerController;
            int moneyToGive;
            float healthChange = resultHealth - currentHealth;

            if (healthChange > 0)
            {
                moneyToGive = (int)(25 * (healthChange / 0.5f));
                player.carriedConsumables.Currency += moneyToGive;
                player.BloopItemAboveHead(this.sprite);
                AkSoundEngine.PostEvent("Play_OBJ_coin_large_01", base.gameObject);
                
            }
            currentHealth = resultHealth;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            currentHealth = player.healthHaver.currentHealth;
            player.healthHaver.OnHealthChanged += OnHealthChanged;
        }
        public override void DisableEffect(PlayerController player)
        {
            player.healthHaver.OnHealthChanged -= OnHealthChanged;
            base.DisableEffect(player);
        }
    }
}