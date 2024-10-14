using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;
using System.Collections;
using static ETGMod;
using Dungeonator;

/* NOTES:
 * 
*/
namespace TF2Stuff
{
    public class Dispenser : PlayerItem
    {
        public static int ID;
        public static string consoleID;
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Dispenser";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/actives/dispenser_001";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Dispenser>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item as PlayerItem, ItemBuilder.CooldownType.Damage, 500f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Erectin'";
            string longDesc = "UNCLE DANE THEME INTENSIFIES";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, MODPREFIX);
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.B;
            consoleID = MODPREFIX + ":" + item.name.ToID();
            ID = item.PickupObjectId;

            DispenserObject = SpriteBuilder.SpriteFromResource("TF2Items/Resources/actives/dispenser_001", new GameObject("Spawned_Dispenser"));
            DispenserObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(DispenserObject);
            DispenserObject.GetComponent<tk2dSprite>().HeightOffGround = 0.1f;

            var DispenserBody = DispenserObject.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(new IntVector2(0, -3), new IntVector2(20, 23));
            DispenserBody.CollideWithTileMap = false;
            DispenserBody.CollideWithOthers = true;
            DispenserBody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.HighObstacle;

            DispenserObject.AddComponent<DispenserController>();
            /*var DispenserHealth = DispenserObject.AddComponent<HealthHaver>();
            DispenserHealth.healthHaver.maximumHealth = 100;
            DispenserHealth.IsVulnerable = true;*/
        }
        public static GameObject DispenserObject;
        public override void DoEffect(PlayerController user)
        {
            Instantiate(DispenserObject, LastOwner.transform.position + LastOwner.m_cachedAimDirection.normalized * 20f/16, Quaternion.identity);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat;
        }

        public DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            return debrisObject;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
    public class DispenserController : SpawnObjectItem
    {
        private void Start()
        {
            RoomHandler currentRoom = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
            for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
            {
                base.specRigidbody.RegisterSpecificCollisionException(GameManager.Instance.AllPlayers[i].specRigidbody);
            }
            AttractEnemies(currentRoom);
        }

        private void AttractEnemies(RoomHandler room)
        {
            List<AIActor> activeEnemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (activeEnemies == null)
            {
                return;
            }
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i].OverrideTarget == null)
                {
                    activeEnemies[i].OverrideTarget = base.specRigidbody;
                }
            }
        }

    }
}