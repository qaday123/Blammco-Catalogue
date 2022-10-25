using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;

namespace ExampleMod
{
    public class TestActive : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Free Heart";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/example_active_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<TestActive>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.None, 0f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "One-Time Heal";
            string longDesc = "A young scientist engineered a pale-looking heart in order to start off their career in making things in the gungeon.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //item.consumable = true;
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        public override void DoEffect(PlayerController user)
        {
            List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (activeEnemies != null)
            {
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    AIActor enemy = activeEnemies[i];
                    enemy.ApplyEffect(StaticStatusEffects.StandardJarateEffect);

                }
                /*float maxHealth = user.healthHaver.GetMaxHealth();
                float curHealth = user.healthHaver.GetCurrentHealth();
                if (curHealth != maxHealth)
                {
                    AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
                    user.healthHaver.ApplyHealing(1);
                }*/

            }
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