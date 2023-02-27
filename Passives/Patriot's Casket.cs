using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace ExampleMod
{
    public class Patriots_Casket : PassiveItem
    {
        public static void Register()
        {
            string itemName = "Patriot's Casket";
            // TODO: Add sprite
            string resourceName = "ExampleMod/Resources/passives/patriot's_casket_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Patriots_Casket>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "God Bless";
            string longDesc = "[Rocket jump stuff]" +
                "\nOh, and also boosts your effectiveness with American weapons. Godspeed.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //Adds the actual passive effect to the item
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.MovementSpeed, 2, StatModifier.ModifyMethod.ADDITIVE);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, 1.15f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Rarity of the item
            item.quality = PickupObject.ItemQuality.SPECIAL;
            ID = item.PickupObjectId;
        }
        private void AddStat(PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method)
        {
            StatModifier statModifier = new StatModifier();
            statModifier.amount = amount;
            statModifier.statToBoost = statType;
            statModifier.modifyType = method;
            foreach (StatModifier statModifier2 in this.passiveStatModifiers)
            {
                bool flag = statModifier2.statToBoost == statType;
                if (flag)
                {
                    return;
                }
            }
            bool flag2 = this.passiveStatModifiers == null;
            if (flag2)
            {
                this.passiveStatModifiers = new StatModifier[]
                {
                    statModifier
                };
                return;
            }
            this.passiveStatModifiers = this.passiveStatModifiers.Concat(new StatModifier[]
            {
                statModifier
            }).ToArray<StatModifier>();
        }

        private void RemoveStat(PlayerStats.StatType statType)
        {
            List<StatModifier> list = new List<StatModifier>();
            for (int i = 0; i < this.passiveStatModifiers.Length; i++)
            {
                bool flag = this.passiveStatModifiers[i].statToBoost != statType;
                if (flag)
                {
                    list.Add(this.passiveStatModifiers[i]);
                }
            }
            this.passiveStatModifiers = list.ToArray();
        }

        public void OnGunChanged(Gun previous, Gun current, bool newgun)
        {
            //ETGModConsole.Log("Previous: " + previous.EncounterNameOrDisplayName + $", {previous.PickupObjectId}");
            //ETGModConsole.Log("Current: " + current.EncounterNameOrDisplayName + $", {current.PickupObjectId}");
            RemoveStat(PlayerStats.StatType.Damage);
            RemoveStat(PlayerStats.StatType.Accuracy);
            RemoveStat(PlayerStats.StatType.RateOfFire);
            RemoveStat(PlayerStats.StatType.ReloadSpeed);
            //ETGModConsole.Log($"Stats removed successfully");
            //ETGModConsole.Log($"{american_ids.Contains(current.PickupObjectId)}");
            if (american_ids.Contains(current.PickupObjectId))
            {
                AddStat(PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                AddStat(PlayerStats.StatType.Accuracy, 0.8f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                AddStat(PlayerStats.StatType.RateOfFire, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                AddStat(PlayerStats.StatType.ReloadSpeed, 0.6f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                ETGModConsole.Log($"Stats added successfully");
            }
            Owner.stats.RecalculateStats(Owner, true, false);
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.GunChanged += this.OnGunChanged;
            OnGunChanged(player.CurrentGun, player.CurrentGun, false);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            player.GunChanged -= this.OnGunChanged;
            player.stats.RecalculateStats(player, true, false);
            return base.Drop(player);
        }
        public static int ID;
        public int[] american_ids = new int[]
        {
            25, // M1
            96, // M16
            2, // Thompson
            94, // MAC10
            1, // Winchester
            181, // Winchester Rifle
            19, // Grenade Launcher
            26, // Nail Gun
            50, // SAA
            62, // Colt 1851
            30, // M1911
            56, // 38. Special
            378, // Derringer
            84, // Vulcan Cannon
            23, // Gungeon Eagle
            38, // Magnum
            275, // Flare Gun
            10, // Mega DOuser
        };
    }
}