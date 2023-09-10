using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;
using System.Collections;
using static ETGMod;

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
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Now With Isotopes!";
            string longDesc = "Cherry Fission flavoured. Drinking this will allow you to dodge every bullet that comes your way, " +
                "and roll with great power. Unfortunately, this is your only offensive option.\n\n" +
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

            customVFXPrefab = VFXToolbox.CreateOverheadVFX(new List<string>() { "ExampleMod/Resources/StatusEffectVFX/slowed_effect_icon" }, "SlowedEffect", 1);
            GameObject.DontDestroyOnLoad(customVFXPrefab);
            FakePrefab.MarkAsFakePrefab(customVFXPrefab);
            customVFXPrefab.SetActive(false);
        }
        public static int ID;
        public static float timer = 10f;
        public static GameObject customVFXPrefab;
        StatModifier rollDamage = StatModifier.Create(PlayerStats.StatType.DodgeRollDamage, StatModifier.ModifyMethod.ADDITIVE, 50f);
        StatModifier speed = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.6f);
        public override void DoEffect(PlayerController user)
        {
            AkSoundEngine.PostEvent("Play_soda_drink", base.gameObject);
            StartEffect(user);

            StartCoroutine(ItemBuilder.HandleDuration(this, timer, user, EndEffect));
        }
        bool isCurrentlyActive;
        float damageWhileActive = 0f;
        private void EnableVFX(PlayerController player)
        {
            ImprovedAfterImage effect = player.gameObject.GetOrAddComponent<ImprovedAfterImage>();
            effect.shadowTimeDelay = 0.05f;
            effect.targetHeight = 1f;
            effect.shadowLifetime = 0.5f;
            effect.dashColor = Color.yellow;
            effect.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
            effect.spawnShadows = true;
        }

        private void DisableVFX(PlayerController player)
        {
            ImprovedAfterImage effect = player.gameObject.GetOrAddComponent<ImprovedAfterImage>();
            effect.spawnShadows = false;
        }

        private void OnHit(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            bool jammed = false;
            if (otherRigidbody.projectile != null) // checks if the body is a projectile
            {
                VFXToolbox.DoStringSquirt("Miss!", myRigidbody.sprite.WorldTopRight, Color.red);
                Projectile projectile = otherRigidbody.projectile as Projectile;
                jammed = projectile.IsBlackBullet;
                if (jammed) { damageWhileActive += 1f; }
                else { damageWhileActive += 0.5f; }
            }
        }
        private void StartEffect(PlayerController player)
        {
            isCurrentlyActive = true;
            damageWhileActive = 0f;
            player.ownerlessStatModifiers.Add(rollDamage);
            player.ownerlessStatModifiers.Add(speed);
            player.healthHaver.TriggerInvulnerabilityPeriod(timer);
            player.IsGunLocked = true;
            player.stats.RecalculateStats(player, true, false);
            player.specRigidbody.OnPreRigidbodyCollision += OnHit;
            EnableVFX(player);
        }
        private void EndEffect(PlayerController player)
        {
            if (player != null && isCurrentlyActive)
            {
                player.ownerlessStatModifiers.Remove(rollDamage);
                player.ownerlessStatModifiers.Remove(speed);
                player.stats.RecalculateStats(player, true, false);
                player.IsGunLocked = false;
                DisableVFX(player);
                player.specRigidbody.OnPreRigidbodyCollision -= OnHit;
                isCurrentlyActive = false;
                GameManager.Instance.StartCoroutine(HandleSlowdown(player));
            }
        }

        private IEnumerator HandleSlowdown(PlayerController player)
        {
            if (player != null)
            {
                if (damageWhileActive > 0f)
                {
                    float value = -(damageWhileActive / 30f) + 0.9f;
                    if (value < 0.5f) { value = 0.65f; }
                    StatModifier speedMultiplier = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, value);
                    player.ownerlessStatModifiers.Add(speedMultiplier);
                    player.stats.RecalculateStats(player, true, false);
                    Vector2 offset = new Vector2(0, 0.2f);
                    var obj = GameObject.Instantiate(customVFXPrefab);
                    obj.SetActive(true);
                    var sprite = obj.GetComponent<tk2dSprite>();
                    sprite.PlaceAtPositionByAnchor(player.sprite.WorldTopCenter + offset, tk2dBaseSprite.Anchor.LowerCenter);
                    sprite.transform.SetParent(player.transform);
                    AkSoundEngine.PostEvent("Play_stun_effect", base.gameObject);
                    yield return new WaitForSeconds(5);
                    player.ownerlessStatModifiers.Remove(speedMultiplier);
                    player.stats.RecalculateStats(player, true, false);
                    GameObject.Destroy(sprite);
                }
            }
            yield break;
        }
        private void OnGunChanged(Gun previous, Gun current, bool GunChanged)
        {
            PlayerController player = current.GunPlayerOwner();
            //ETGModConsole.Log(GunChanged);
            if (player != null && isCurrentlyActive)
            {
                if (current != previous)
                {
                    player.ChangeToGunSlot(0);
                }
            }
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
            EndEffect(player);
            return debrisObject;
        }
        public override void OnDestroy()
        {
            if (LastOwner)
            {
                EndEffect(LastOwner);
            }
            base.OnDestroy();
        }
    }
}