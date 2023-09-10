using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;
using System.Collections;
using static ETGMod;
using Alexandria.Misc;

/* NOTES:
 * 
*/
namespace ExampleMod
{
    public class Disciplinary_Action : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Disciplinary Action";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/disciplinary_action_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Disciplinary_Action>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 3f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Violent Encouragement";
            string longDesc = "Whip an enemy or a companion to give yourself a little boost. Enemies are stunned and take more damage and companions are " +
                "\"lightly encouraged\" to up their game.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.C;
            ID = item.PickupObjectId;

            //var speed_icon = SpriteBuilder.SpriteFromResource("ExampleMod/Resources/StatusEffectVFX/speed_effect/speed_effect_001", new GameObject("Speed Icon"));
            //speed_icon.SetActive(false);
            //FakePrefab.MarkAsFakePrefab(speed_icon);
        }
        public static int ID;
        private bool applied = false;
        private bool applied_p2 = false;
        private bool hitObject;
        private float m_timer = 0f;
        StatModifier speed = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.8f);
        StatModifier shotSpeed = StatModifier.Create(PlayerStats.StatType.RateOfFire, StatModifier.ModifyMethod.MULTIPLICATIVE, 1.2f);
        private void EnableVFX(GameObject obj)
        {
            ImprovedAfterImage effect = obj.GetOrAddComponent<ImprovedAfterImage>();
            effect.shadowTimeDelay = 0.1f;
            effect.targetHeight = 1f;
            effect.shadowLifetime = 0.5f;
            effect.dashColor = MoreColours.lightgrey;
            effect.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
            effect.spawnShadows = true;
        }

        private void DisableVFX(GameObject obj)
        {
            ImprovedAfterImage effect = obj.GetOrAddComponent<ImprovedAfterImage>();
            effect.spawnShadows = false;
        }
        public override void DoEffect(PlayerController user)
        {
            hitObject = false;
            SlashData whip = ScriptableObject.CreateInstance<SlashData>();
            whip.damage = 3f * user.stats.GetStatValue(PlayerStats.StatType.Damage);
            whip.projInteractMode = SlashDoer.ProjInteractMode.IGNORE;
            whip.slashRange = 3f;
            whip.enemyKnockbackForce = 2f * user.stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
            user.StartCoroutine(DoSlash(user, whip));
        }
        private IEnumerator DoSlash(PlayerController user, SlashData slashParameters)
        {
            
            Vector2 vector = user.CenterPosition;
            float angleToUse = user.CurrentGun.CurrentAngle;
            slashParameters.OnHitTarget += OnSlashedEnemy;

            if (UnityEngine.Random.value < 0.5f) AkSoundEngine.PostEvent("Play_whip_woosh_01", gameObject);
            else AkSoundEngine.PostEvent("Play_whip_woosh_02", gameObject);
            SlashDoer.DoSwordSlash(user.CenterPosition, angleToUse, user, slashParameters);
            CheckCompanionSlashed(user, slashParameters, angleToUse);
            yield break;
        }

        private void OnSlashedEnemy(GameActor enemy, bool fatal)
        {
            if (UnityEngine.Random.value < 0.5f) AkSoundEngine.PostEvent("Play_whip_impact_01", gameObject);
            else AkSoundEngine.PostEvent("Play_whip_impact_02", gameObject);

            float duration = 2.5f;
            if (enemy.healthHaver && !enemy.healthHaver.IsBoss) enemy.behaviorSpeculator.Stun(3f);
            if (enemy.healthHaver.IsBoss) duration *= 2;

            GameManager.Instance.StartCoroutine(HandleEnemyEffect(enemy));
            PlayerController player = LastOwner;
            DoBuff(player, duration);
            hitObject = true;
        }
        public void CheckCompanionSlashed(PlayerController currentPlayer, SlashData data, float angleOfSlash)
        {
            //ETGModConsole.Log("started");
            List<AIActor> companions = currentPlayer.companions;
            //ETGModConsole.Log(companions.Count);
            PlayerController otherPlayer = null;
            if (GameManager.Instance.SecondaryPlayer != null)
            {
                otherPlayer = (currentPlayer.IsPrimaryPlayer) ? GameManager.Instance.SecondaryPlayer : GameManager.Instance.PrimaryPlayer;
                companions.AddRange(otherPlayer.companions);
            }
            if (otherPlayer != null)
            {
                if (ObjectWasHitBySlash(otherPlayer.sprite.WorldCenter, currentPlayer.CenterPosition, angleOfSlash, data.slashRange, data.slashDegrees))
                {
                    DoBuff(currentPlayer, 3f, otherPlayer);
                    //DoBuff(otherPlayer, 1.5f, true);
                    PixelCollider hitbox = otherPlayer.specRigidbody.HitboxPixelCollider;
                    Vector2 collisionPoint = BraveMathCollege.ClosestPointOnRectangle(currentPlayer.CenterPosition, hitbox.UnitBottomLeft, hitbox.UnitDimensions);
                    data.hitVFX.SpawnAtPosition(new Vector3(collisionPoint.x, collisionPoint.y), 0, otherPlayer.transform);
                    hitObject = true;
                }
            }
            foreach (AIActor companion in companions)
            {
                if (companion && companion.aiActor)
                {
                    PixelCollider hitbox = companion.specRigidbody.HitboxPixelCollider;
                    if (ObjectWasHitBySlash(companion.sprite.WorldCenter, currentPlayer.CenterPosition, angleOfSlash, data.slashRange, data.slashDegrees))
                    {
                        Vector2 collisionPoint = BraveMathCollege.ClosestPointOnRectangle(currentPlayer.CenterPosition, hitbox.UnitBottomLeft, hitbox.UnitDimensions);
                        if (UnityEngine.Random.value < 0.5f) AkSoundEngine.PostEvent("Play_whip_impact_01", gameObject);
                        else AkSoundEngine.PostEvent("Play_whip_impact_02", gameObject);
                        data.hitVFX.SpawnAtPosition(new Vector3(collisionPoint.x, collisionPoint.y), 0, companion.transform);
                        DoBuff(currentPlayer, 2f);
                        GameManager.Instance.StartCoroutine(HandleCompanionEffect(companion));
                        hitObject = true;
                    }
                }
            }
        }
        private IEnumerator HandleEnemyEffect(GameActor enemy)
        {
            DamageTypeModifier damageBonus = new DamageTypeModifier();
            damageBonus.damageMultiplier = (enemy.healthHaver.IsBoss) ? 1.2f : 1.5f;
            damageBonus.damageType = CoreDamageTypes.None;
            enemy.healthHaver.damageTypeModifiers.Add(damageBonus);
            yield return new WaitForSeconds(3f);
            enemy.healthHaver.damageTypeModifiers.Remove(damageBonus);
            yield break;
        }
        private IEnumerator HandleCompanionEffect(AIActor companion)
        {
            companion.MovementSpeed *= 2f;
            companion.behaviorSpeculator.AttackCooldown /= 1.4f;
            EnableVFX(companion.gameObject);
            while (m_timer >= 0)
            {
                //m_timer -= BraveTime.DeltaTime;
                yield return null;
            }
            companion.behaviorSpeculator.AttackCooldown *= 1.4f;
            companion.MovementSpeed /= 2f;
            DisableVFX(companion.gameObject);
            yield break;
        }
        public void DoBuff(PlayerController player, float duration, PlayerController otherPlayer = null)
        {
            if (!hitObject)
            {
                //ETGModConsole.Log("powering up");
                //if (hitBoss) duration *= 2f; hitBoss = false;
                AkSoundEngine.PostEvent("Play_whip_power_up", gameObject);
                m_timer += duration;
                if (!applied)
                {
                    GameManager.Instance.StartCoroutine(ApplyEffect(player));
                    if (otherPlayer)
                    {
                        GameManager.Instance.StartCoroutine(ApplyEffect(otherPlayer, false));
                    }
                }
            }
        }
        private IEnumerator ApplyEffect(PlayerController player, bool doTimer = true)
        {
            //PlayerController player = LastOwner;
            player.ownerlessStatModifiers.Add(speed);
            player.ownerlessStatModifiers.Add(shotSpeed);
            player.stats.RecalculateStats(player);
            EnableVFX(player.gameObject);
            applied = true;
            //ETGModConsole.Log($"stats done");
            while (m_timer >= 0)
            {
                //ETGModConsole.Log($"timer: {m_timer}");
                if (doTimer) m_timer -= BraveTime.DeltaTime;
                yield return null;
            }
            hitObject = false;
            //ETGModConsole.Log($"timer finished");
            DisableEffect(player);
            yield break;
        }
        public void DisableEffect(PlayerController player)
        {
            player.ownerlessStatModifiers.Remove(speed);
            player.ownerlessStatModifiers.Remove(shotSpeed);
            player.stats.RecalculateStats(player);
            DisableVFX(player.gameObject);
            AkSoundEngine.PostEvent("Play_whip_power_down", gameObject);
            applied = false;
            m_timer = 0f;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return true;
        }

        public DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            DisableEffect(LastOwner);
            return debrisObject;
        }
        public override void OnDestroy()
        {
            if (LastOwner)
            {
                DisableEffect(LastOwner);
            }
            base.OnDestroy();
        }
        private static bool ObjectWasHitBySlash(Vector2 ObjectPosition, Vector2 SlashPosition, float slashAngle, float SlashRange, float SlashDimensions)
        {
            if (Vector2.Distance(ObjectPosition, SlashPosition) < SlashRange)
            {
                float num7 = BraveMathCollege.Atan2Degrees(ObjectPosition - SlashPosition);
                float minRawAngle = Math.Min(SlashDimensions, -SlashDimensions);
                float maxRawAngle = Math.Max(SlashDimensions, -SlashDimensions);
                bool isInRange = false;
                float actualMaxAngle = slashAngle + maxRawAngle;
                float actualMinAngle = slashAngle + minRawAngle;

                if (num7.IsBetweenRange(actualMinAngle, actualMaxAngle)) isInRange = true;
                if (actualMaxAngle > 180)
                {
                    float Overflow = actualMaxAngle - 180;
                    if (num7.IsBetweenRange(-180, (-180 + Overflow))) isInRange = true;
                }
                if (actualMinAngle < -180)
                {
                    float Underflow = actualMinAngle + 180;
                    if (num7.IsBetweenRange((180 + Underflow), 180)) isInRange = true;
                }
                return isInRange;
            }
            return false;
        }
    }
}