﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace TF2Stuff
{
    public class Recon_Pouch : PassiveItem
    {
        public static int ID;
        public static string consoleID;
        public static void Register()
        {
            string itemName = "Recon Pouch";
            // TODO: Add sprite
            string resourceName = "TF2Items/Resources/passives/scout_bag_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Recon_Pouch>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "I'm runnin' circles around ya!";
            string longDesc = "Increases movement and firing speed. Owned by that obnoxious Boston Boy. \n\n" +
                              "Scout never really used this pouch very much. Nevertheless, he always brought it with him during his mercenary days. "+
                              "Too bad it can't hold much, though. A memento of his childhood, perhaps?";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, MODPREFIX);

            //Adds the actual passive effect to the item
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.MovementSpeed, 2, StatModifier.ModifyMethod.ADDITIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, 1.15f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Rarity of the item
            consoleID = MODPREFIX + ":" + item.name.ToID();
            item.quality = PickupObject.ItemQuality.SPECIAL;
            ID = item.PickupObjectId;
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