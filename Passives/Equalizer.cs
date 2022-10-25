using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace ExampleMod
{
    public class Equalizer : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Equalizer";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/passives/equalizer_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Equalizer>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Now We're Even";
            string longDesc = "As health decreases, other stats will increase to aid you and your lack of skills.\n\n" +
                "Before the Equalizer was introduced into the gungeon, it had its fair share of ups and downs. Split into " +
                "two when deemed too powerful, having one be the better and the other left in the dust. In the gungeon, it's " +
                "two forms have been reunited back into one, ready to embue its wielder with heavy rebalancing.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //Adds the actual passive effect to the item


            //Set the rarity of the item
            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.A;
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

        private void OnHealthChanged(float resultvalue, float maxvalue)
        {
            ChangeStats();
        }
        private void ChangeStats()
        {
            {
                PlayableCharacters characterIdentity = Owner.characterIdentity;
                //ETGModConsole.Log($"Player character: {characterIdentity}");
                RemoveStat(PlayerStats.StatType.MovementSpeed);
                RemoveStat(PlayerStats.StatType.Damage);
                RemoveStat(PlayerStats.StatType.RateOfFire);
                RemoveStat(PlayerStats.StatType.Coolness);
                RemoveStat(PlayerStats.StatType.ReloadSpeed);
                if (characterIdentity != PlayableCharacters.Robot)
                {
                    float healthpercentage = Owner.healthHaver.GetCurrentHealthPercentage();
                    //ETGModConsole.Log($"health at {healthpercentage}");
                    if (healthpercentage < 0.75)
                    {                                               //hopefully the math works out       it WORKS!
                        AddStat(PlayerStats.StatType.MovementSpeed, 1f + (1.1f - healthpercentage) * speedmod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        AddStat(PlayerStats.StatType.Damage, 1f + (1.1f - healthpercentage) * damagemod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        AddStat(PlayerStats.StatType.RateOfFire, 1f + (1.1f - healthpercentage) * fireratemod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        AddStat(PlayerStats.StatType.Coolness, (1.1f - healthpercentage) * coolnessmod, StatModifier.ModifyMethod.ADDITIVE);
                        AddStat(PlayerStats.StatType.ReloadSpeed, (0.3f + healthpercentage) * reloadspeedmod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        /*ETGModConsole.Log($"Speed: {1f + (1.1f - healthpercentage) * speedmod}, Damage: {1f + (1.1f - healthpercentage) * damagemod}\n" +
                            $"Fire rate: {1f + (1.1f - healthpercentage) * fireratemod}, Coolness: {(1.1f - healthpercentage) * coolnessmod}\n" +
                            $"Reload Speed: {(0.3f + healthpercentage) * reloadspeedmod}");*/
                    }
                }
                else if (characterIdentity == PlayableCharacters.Robot)
                {
                    this.armor = this.Owner.healthHaver.Armor;
                    float healthpercentage = armor / 6;
                    //ETGModConsole.Log("calculations calculated");
                    if (healthpercentage < 0.75)
                    {                                               //hopefully the math works out
                        AddStat(PlayerStats.StatType.MovementSpeed, 2f - (healthpercentage/speedconst) * speedmod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        AddStat(PlayerStats.StatType.Damage, 2f - (healthpercentage/damageconst) * damagemod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        AddStat(PlayerStats.StatType.RateOfFire, 2f - (healthpercentage/firerateconst) * fireratemod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        AddStat(PlayerStats.StatType.Coolness, 1f - (healthpercentage/coolnessconst) * coolnessmod, StatModifier.ModifyMethod.ADDITIVE);
                        AddStat(PlayerStats.StatType.ReloadSpeed, (reloadspeedconst + healthpercentage) * reloadspeedmod, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        //ETGModConsole.Log("stats statted");
                    }
                }
                Owner.stats.RecalculateStats(Owner, true, false);
            }
        }  
        //}

        //void PostProcessHealthChange(PlayerController player)
        //ItemBuilder.RemovePassiveStatModifier(item, PlayerStats.StatType.MovementSpeed);

        public override void Pickup(PlayerController player)
        {
            ETGModConsole.Log("OnPickup triggered");
            base.Pickup(player);
            ChangeStats();
            player.healthHaver.OnHealthChanged += OnHealthChanged;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            ETGModConsole.Log("OnDrop triggered");
            ChangeStats();
            player.healthHaver.OnHealthChanged -= OnHealthChanged;
            return base.Drop(player);
        }
        public override void OnDestroy()
        {
            ChangeStats();
            Owner.healthHaver.OnHealthChanged -= OnHealthChanged;
            base.OnDestroy();
        }
        // max for multiplicative stats is 1 + variable
        public float speedmod = 0.4f;
        public float speedconst = 0.76f;
        public float damagemod = 0.5f;
        public float damageconst = 0.76f;
        public float fireratemod = 0.4f;
        public float firerateconst = 0.8f;
        public float coolnessmod = 5;
        public float coolnessconst = 1;
        public float reloadspeedmod = 0.75f; // except for reload speed
        public float reloadspeedconst = 0.5f;
        private float armor;
        public static int ID;
    }
}