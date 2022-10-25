using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace ExampleMod
{
    public class InvisWatch : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Invis Watch";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/inviswatch_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<InvisWatch>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 10f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Right Behind You";
            string longDesc = "Makes you invisible for a shot time. Unable to shot while invisible.\n\n" +
                "Cloaking technology has been around in the gungeon for a long time. But no one ever actually thought of how " +
                "to make it readily available at all times when you need it. The watch may disable your weapons but it can get " +
                "you out of a pinch. Or get enemies into them.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            item.consumable = false;
            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;
        }

        public float duration = 10f;
        public static int ID;
        public override void DoEffect(PlayerController user)
        {
            turninvis(user);
            StartCoroutine(ItemBuilder.HandleDuration(this, duration, user, BreakStealth));
        }

        private void turninvis(PlayerController user)
        {
            user.SetIsStealthed(true, "InvisWatch");
            user.SetCapableOfStealing(true, "InvisWatch");
        }

        private void BreakStealth(PlayerController user)
        {
            user.SetIsStealthed(false, "InvisWatch");
            user.SetCapableOfStealing(false, "InvisWatch");
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        /*public override bool CanBeUsed(PlayerController user)
        {
            return user.healthHaver.GetCurrentHealth() != user.healthHaver.GetMaxHealth();
        }*/

        /*public override DebrisObject Drop(PlayerController player)
        {
            Tools.Print($"Player dropped {this.DisplayName}");
            return base.Drop(player);
        }*/
    }
}