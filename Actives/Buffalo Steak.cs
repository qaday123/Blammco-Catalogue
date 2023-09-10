using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;
using System.Collections;

/* NOTES:
 * Obvi add corresponding shaders and vfx - if you can find out how maybe do the convict photo rage vfx as well
*/
namespace ExampleMod
{
    public class Buffalo_Steak : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Buffalo Steak Sandvich";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/steak_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Buffalo_Steak>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 750f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Prime Blood";
            string longDesc = "A raw T-bone steak, a perfect sandwich. Who even needs bread?. If you look at it from the right angle, it even looks like a gun.\n\nFills the eater with a primal " +
                "rage, improving their combat effectiveness, but inhibits greater cognitive function to the point where only muscle memory to " +
                "the gungeoneer's most used and reliable weapon through their lifetime can be used.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;
        }
        public static int ID;
        public static float timer = 10f;
        public bool isActive = false;
        StatModifier speed = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.4f);
        StatModifier firerate = StatModifier.Create(PlayerStats.StatType.RateOfFire, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.5f);
        StatModifier damage = StatModifier.Create(PlayerStats.StatType.Damage, StatModifier.ModifyMethod.MULTIPLICATIVE, 2f);
        StatModifier reloadspeed = StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 0.5f);
        public override void DoEffect(PlayerController user)
        {
            AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
            StartEffect(user);

            StartCoroutine(ItemBuilder.HandleDuration(this, 10f, user, EndEffect));
        }
        // VFX stolen from nn as a placeholder feedback VFX
        bool activeOutline = false;
        private void EnableVFX(PlayerController user)
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(255f, 200f, 74f));
        }

        private void DisableVFX(PlayerController user)
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
        }
        // these two are to prevent the outline breaking when taking damage
        private IEnumerator GainOutline()
        {
            PlayerController user = this.LastOwner;
            yield return new WaitForSeconds(0.05f);
            EnableVFX(user);
            yield break;
        }

        private IEnumerator LoseOutline()
        {
            PlayerController user = this.LastOwner;
            yield return new WaitForSeconds(0.05f);
            DisableVFX(user);
            yield break;
        }
        private void PlayerTookDamage(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
        {
            if (activeOutline)
            {
                GameManager.Instance.StartCoroutine(this.GainOutline());
            }

            else if (!activeOutline)
            {
                GameManager.Instance.StartCoroutine(this.LoseOutline());
            }
        }
        private void StartEffect(PlayerController player)
        {
            isActive = true;
            player.ChangeToGunSlot(0);
            player.GunChanged += OnGunChanged;
            //ETGModConsole.Log(player.startingGunIds);
            player.ownerlessStatModifiers.Add(speed);
            player.ownerlessStatModifiers.Add(firerate);
            player.ownerlessStatModifiers.Add(damage);
            player.ownerlessStatModifiers.Add(reloadspeed);
            player.stats.RecalculateStats(player, true, false);
            EnableVFX(player);
            activeOutline = true;
        }
        private void EndEffect(PlayerController player)
        {
            if (player && isActive)
            {
                player.GunChanged -= OnGunChanged;
                player.IsGunLocked = false;
                player.ownerlessStatModifiers.Remove(speed);
                player.ownerlessStatModifiers.Remove(firerate);
                player.ownerlessStatModifiers.Remove(damage);
                player.ownerlessStatModifiers.Remove(reloadspeed);
                player.stats.RecalculateStats(player, true, false);
                DisableVFX(player);
                activeOutline = false;
                isActive = false;
            }
        }


        private void OnGunChanged(Gun previous, Gun current, bool GunChanged)
        {
            PlayerController player = current.GunPlayerOwner();
            //ETGModConsole.Log(GunChanged);
            if (current != previous)
            {
                player.ChangeToGunSlot(0);
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.healthHaver.OnDamaged += PlayerTookDamage;
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat;
        }

        public DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.healthHaver.OnDamaged -= this.PlayerTookDamage;
            EndEffect(player);
            return debrisObject;
        }
        public override void OnDestroy()
        {
            if (LastOwner)
            {
                EndEffect(LastOwner);
                LastOwner.healthHaver.OnDamaged -= this.PlayerTookDamage;
            }
            base.OnDestroy();
        }
    }
}