using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.Misc;

namespace TF2Stuff
{
    public class Patriots_Casket : PassiveItem
    {
        public static void Register()
        {
            string itemName = "Patriot's Casket";
            // TODO: Add sprite
            string resourceName = "TF2Items/Resources/passives/patriot's_casket_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Patriots_Casket>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "God Bless";
            string longDesc = "Permits you to rocket jump. To perform one, dodge roll in the blast radius of any explosion. Dodge roll " +
                "while in the air to prematurely end the jump. Deal much more damage while jumping and gain a small boost for while after jumping.\n" +
                "Oh, and also perform more effectively with American weapons. Godspeed.\n\nThe Soldier insists he was in the military, " +
                "but ultimately was deemed too crazy to be accepted, so his companions say. Still, he jumps into battle bravely and swiftly, " +
                "getting the jump on many enemies who don't expect his unconventional tactics.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //Rarity of the item
            item.quality = PickupObject.ItemQuality.SPECIAL;
            ID = item.PickupObjectId;
        }
        StatModifier damageBoost_jump = StatModifier.Create(PlayerStats.StatType.Damage, StatModifier.ModifyMethod.MULTIPLICATIVE, 3f);
        StatModifier damageBoost_after = StatModifier.Create(PlayerStats.StatType.Damage, StatModifier.ModifyMethod.MULTIPLICATIVE, 2f);
        StatModifier speedBoost = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.4f);
        List<StatModifier> statMods = new List<StatModifier>()
        {
            StatModifier.Create(PlayerStats.StatType.Damage, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.5f),
            StatModifier.Create(PlayerStats.StatType.Accuracy, StatModifier.ModifyMethod.MULTIPLICATIVE, 0.8f),
            StatModifier.Create(PlayerStats.StatType.Accuracy, StatModifier.ModifyMethod.MULTIPLICATIVE, 0.8f),
            StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 0.6f),
        };
        public bool activeOutline;
        private void EnableVFX(PlayerController user)
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(60, 60, 60));
        }

        private void DisableVFX(PlayerController user)
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
        }
        // these two are to prevent the outline breaking when taking damage
        private IEnumerator GainOutline()
        {
            PlayerController user = this.Owner;
            yield return new WaitForSeconds(0.05f);
            EnableVFX(user);
            yield break;
        }

        private IEnumerator LoseOutline()
        {
            PlayerController user = this.Owner;
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
        public void OnGunChanged(Gun previous, Gun current, bool newgun)
        {
            if (current)
            {
                if (Owner != null)
                {
                    foreach (StatModifier modifier in statMods) { Owner.ownerlessStatModifiers.Remove(modifier); }
                    if (american_ids.Contains(current.PickupObjectId))
                    { foreach (StatModifier modifier in statMods) { Owner.ownerlessStatModifiers.Add(modifier); } }
                   Owner.stats.RecalculateStats(Owner, true, false);
                }
            }
        }
        private void OnRocketJump(PlayerController player)
        {
            //ETGModConsole.Log("Rocket Jump Logged");
            RemoveStats(player);
            player.ownerlessStatModifiers.Add(damageBoost_jump);
            player.stats.RecalculateStats(player);
            AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Trigger_01", base.gameObject);
            GameManager.Instance.StartCoroutine(GainOutline());
            activeOutline = true;
            GameManager.Instance.StartCoroutine(HandleStatusEffects(player));
        }
        private IEnumerator HandleStatusEffects(PlayerController player)
        {
            RocketJumpDoer jump = player.gameObject.GetOrAddComponent<RocketJumpDoer>();
            while (jump.isRocketJumping) { yield return null; }
            AddStats(player);
            float timer = 2f;
            while (!jump.isRocketJumping && timer > 0f) { timer -= 0.1f; yield return new WaitForSeconds(0.1f); }
            RemoveStats(player);
            yield break;
        }
        private void AddStats(PlayerController player)
        {
            player.ownerlessStatModifiers.Remove(damageBoost_jump);
            player.ownerlessStatModifiers.Add(damageBoost_after);
            player.ownerlessStatModifiers.Add(speedBoost);
            player.stats.RecalculateStats(player);
        }
        private void RemoveStats(PlayerController player)
        {
            RocketJumpDoer jump = player.gameObject.GetOrAddComponent<RocketJumpDoer>();
            player.ownerlessStatModifiers.Remove(damageBoost_jump);
            player.ownerlessStatModifiers.Remove(damageBoost_after);
            player.ownerlessStatModifiers.Remove(speedBoost);
            player.stats.RecalculateStats(player);
            if (!jump.isRocketJumping)
            {
                GameManager.Instance.StartCoroutine(LoseOutline());
                activeOutline = false;
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            RocketJumpDoer jump = player.gameObject.GetOrAddComponent<RocketJumpDoer>();
            jump.OnRocketJump += OnRocketJump;
            player.healthHaver.OnDamaged += PlayerTookDamage;
            player.GunChanged += this.OnGunChanged;
            player.lostAllArmorVFX = null;
            OnGunChanged(player.CurrentGun, player.CurrentGun, false);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            RocketJumpDoer jump = player.gameObject.GetOrAddComponent<RocketJumpDoer>();
            player.GunChanged -= this.OnGunChanged;
            player.healthHaver.OnDamaged -= PlayerTookDamage;
            jump.OnRocketJump -= OnRocketJump;
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
            10, // Mega Douser
        };
    }
}