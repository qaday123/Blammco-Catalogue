﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;
using Alexandria.Misc;

/* NOTES:
 * Synergy with blast helmet
 * Maybe do its effect while rocket jumping
*/
namespace TF2Stuff
{
    public class Gunboats : PassiveItem
    {
        public static int ID;
        public static string consoleID;
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "Gunboats";
            string resourceName = "TF2Items/Resources/passives/gunboats_sprite";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<Gunboats>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Fly Away";
            string longDesc = "These thin fabric boots laced with some metal plating allows you to rocket jump. Dodge roll in the " +
                "blast radius of an explosion to do so, and dodge roll mid-air to cancel the jump prematurey.\n\n" +
                "How to these marvels of technology function? Nobody knows. Most people suspect magic is involved. Others say the " +
                "metal was sourced from great depths.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, MODPREFIX);

            consoleID = MODPREFIX + ":" + item.name.ToID();
            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.B;
        }
        public void OnRocketJump(PlayerController player)
        {

        }
        public override void Pickup(PlayerController player)
        {
            RocketJumpDoer jump = player.gameObject.GetOrAddComponent<RocketJumpDoer>();
            jump.SetPlayer(player);
            base.Pickup(player);
            //jump.OnRocketJump += OnRocketJump;
        }
        public override void DisableEffect(PlayerController player)
        {
            RocketJumpDoer jump = player.gameObject.GetComponent<RocketJumpDoer>();
            jump.SetPlayer(null);
            base.DisableEffect(player);
        }
    }
}