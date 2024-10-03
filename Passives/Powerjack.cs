using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Security.Policy;

/* NOTES:
 * It may be perhaps a little unbalanced, but for now I just slapped it on S tier and called it a day
 * fix speedmodconst    DONE?
*/
namespace TF2Stuff
{
    public class Powerjack : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Powerjack";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/passives/powerjack_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Powerjack>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Fuel To The Engine";
            string longDesc = "For every enemy felled, you will become more nimble.\n\n" +
                "This funky device, an old car battery attatched to a vehicle jack via some crude rubber bands still works " +
                "despite its age and crudeness. It's battery will need some recharging though.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //Adds the actual passive effect to the item


            //Set the rarity of the item
            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.S;
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
        private void CalculateConst(PlayerController player)
        {
            if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON) { speedmodconst = 0.1f; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.SEWERGEON) { speedmodconst = 0.115f; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON) { speedmodconst = 0.13f; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATHEDRALGEON) { speedmodconst = 0.145f; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON) { speedmodconst = 0.16f; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.RATGEON || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON) { speedmodconst = 0.175f; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON) { speedmodconst = 0.2f; }
            else
            {
                speedmodconst = 0.15f; // if its a floor it doesn't recognise, it should just do this amount (idk how to find the previous floor)
            }
        }
        private void OnEnemyDamaged(float damage, bool fatal, HealthHaver enemyHealth)
        {
            if (enemyHealth.aiActor && enemyHealth && fatal) { killedenemies++; }
            ChangeStats();
        }
        private void ChangeStats()
        {
            RemoveStat(PlayerStats.StatType.MovementSpeed);
            RemoveStat(PlayerStats.StatType.DodgeRollSpeedMultiplier);
            AddStat(PlayerStats.StatType.MovementSpeed, -(100f / (killedenemies + 200f)) + 1.5f + speedmodconst, StatModifier.ModifyMethod.MULTIPLICATIVE );
            AddStat(PlayerStats.StatType.DodgeRollSpeedMultiplier, -(100f / (killedenemies + 200f)) + 1.5f + speedmodconst, StatModifier.ModifyMethod.MULTIPLICATIVE);
            Owner.stats.RecalculateStats(Owner, true, false);
        }  

        public override void Pickup(PlayerController player)
        {
            //speedmodconst = 0.1f; //added when picked up (but will scale based on floor)
            base.Pickup(player);
            killedenemies = 0;
            ChangeStats();
            player.OnAnyEnemyReceivedDamage += OnEnemyDamaged;
            //player.OnNewFloorLoaded += OnNewFloor;
            CalculateConst(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            ChangeStats();
            player.OnAnyEnemyReceivedDamage -= OnEnemyDamaged;
            //player.OnNewFloorLoaded -= OnNewFloor;
            return base.Drop(player);
        }
        public override void OnDestroy()
        {
            ChangeStats();
            Owner.OnAnyEnemyReceivedDamage -= OnEnemyDamaged;
            //Owner.OnNewFloorLoaded -= OnNewFloor;
            base.OnDestroy();
        }
        public static int ID;
        public float killedenemies;
        public float speedmodconst;
    }
}