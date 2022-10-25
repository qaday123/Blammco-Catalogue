using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;

/* NOTES:
 * GetMaxHealth is inaccessible from method... hooray! It doesn't work. FIXED!
 * Game loads floor twice for some reason... hooray for gungeon spaghetti!
 * Synergises with any australium-themed item, supercharging it and doubling it's effects
*/
namespace ExampleMod
{
    public class LEM_MkGRAY : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "Life Extender Machine";
            string resourceName = "ExampleMod/Resources/passives/LEM";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<LEM_MkGRAY>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "100 More Years";
            string longDesc = "Powered by what seems to be a mysterious gold-like metal, meant to be attatched through the spine, " +
                "this contraption will imbue more life force into you.\n\nIt's running low on fuel, though, but perhaps it's just " +
                "enough to last one... more... run...\nInscribed on the side is 'PROPERTY OF GRAY MANN, DO NOT TOUCH OR I WILL KILL YOU.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.B;
        }
        private void OnNewFloor(PlayerController player)
        {
            PlayableCharacters characterIdentity = Owner.characterIdentity;
            float maxhealth = Owner.healthHaver.GetMaxHealth();
            ETGModConsole.Log("New Floor Loaded");
            if (oldfloor != GameManager.Instance.Dungeon.tileIndices.tilesetId)
            {
                if (characterIdentity != PlayableCharacters.Robot)
                {
                    if (Owner.healthHaver.currentHealth != maxhealth)
                    {
                        AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
                        Owner.healthHaver.ApplyHealing(1);
                    }
                    else
                    {
                        Owner.carriedConsumables.Currency += 15;
                    }
                }
                else
                {
                    Owner.carriedConsumables.Currency += 20;
                    Owner.AcquirePassiveItemPrefabDirectly(PickupObjectDatabase.GetById(127) as PassiveItem);
                }
                oldfloor = GameManager.Instance.Dungeon.tileIndices.tilesetId;
            }
        }
       private void CalculateFloor()
       {
            oldfloor = GameManager.Instance.Dungeon.tileIndices.tilesetId;
       }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnNewFloorLoaded += OnNewFloor;
            CalculateFloor();
        }

        public override DebrisObject Drop(PlayerController player)
        {
            player.OnNewFloorLoaded -= OnNewFloor;
            return base.Drop(player);
        }
        public static int ID;
        public GlobalDungeonData.ValidTilesets oldfloor;
        //public GlobalDungeonData.ValidTilesets currentfloor = GlobalDungeonData.ValidTilesets.CASTLEGEON;
    }
}