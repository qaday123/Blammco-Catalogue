using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;
using System.Collections;

/* NOTES:
 * Add hit count and speed debuff at end (4s)
*/
namespace ExampleMod
{
    public class Bonk_Soda : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "BONK! Atomic Punch";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/bonk_can_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Bonk_Soda>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 1750f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Now With Isotopes!";
            string longDesc = "Cherry Fission flavoured. Drinking this will allow you to dodge every bullet that comes your way, " +
                "and roll with great power. Unfortunately, muscle spasms in your hand will mean this is your only offensive option.\n\n" +
                "Are you tired of time moving too fast for your tastes? Well BONK! proudly presents the Atomic Punch - now with REAL radioactive " +
                "isotopes, allowing you to remain alert for every second.\n" +
                "Warning: side effects may include heavy muscle cramps and tiredness after the drink's effects wear off, and long-term use may cause " +
                " glowing skin, extreme cancer, and possible organ failure. BONK! is not responsible for any injuries caused by consumption.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.C;
            ID = item.PickupObjectId;
        }
        public static int ID;
        public static float timer = 10f;
        StatModifier rollDamage = StatModifier.Create(PlayerStats.StatType.DodgeRollDamage, StatModifier.ModifyMethod.ADDITIVE, 50f);
        StatModifier speed = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.6f);
        public override void DoEffect(PlayerController user)
        {
            AkSoundEngine.PostEvent("Play_soda_drink", base.gameObject);
            StartEffect(user);

            StartCoroutine(ItemBuilder.HandleDuration(this, timer, user, EndEffect));
        }
        // VFX stolen from nn as a placeholder feedback VFX
        bool activeOutline = false;
        private void EnableVFX(PlayerController user)
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(217f, 209f, 48f));
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
            player.ownerlessStatModifiers.Add(rollDamage);
            player.ownerlessStatModifiers.Add(speed);
            player.healthHaver.TriggerInvulnerabilityPeriod(timer);
            player.IsGunLocked = true;
            player.stats.RecalculateStats(player, true, false);
            EnableVFX(player);
            activeOutline = true;
        }
        private void EndEffect(PlayerController player)
        {
            player.ownerlessStatModifiers.Remove(rollDamage);
            player.ownerlessStatModifiers.Remove(speed);
            player.stats.RecalculateStats(player, true, false);
            player.IsGunLocked = false;
            DisableVFX(player);
            activeOutline = false;
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
            return debrisObject;
        }
        public override void OnDestroy()
        {
            if (LastOwner)
            {
                LastOwner.healthHaver.OnDamaged -= this.PlayerTookDamage;
            }
            base.OnDestroy();
        }
    }
}