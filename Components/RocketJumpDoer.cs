using Dungeonator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System.Text;
using UnityEngine;
using Alexandria.EnemyAPI;
using HutongGames.PlayMaker.Actions;
using static UnityEngine.UI.GridLayoutGroup;

/*
 * Jumping should decrease your fire bar over tiem if on fire
 * Some trap tiles count as tile collisions- find a way to check that and circumvent it
*/ 
namespace ExampleMod
{
    public class RocketJumpDoer : MonoBehaviour
    {
        public RocketJumpDoer()
        {
            isAbleToRocketJump = false;
            isRocketJumping = false;
            cancelJump = false;

        }
        private void Start()
        {
            this.item = base.GetComponent<PassiveItem>();
            if (item != null) { player = item.Owner; }
            else if (item == null) { this.player = base.GetComponent<PlayerController>(); }
            if (player) CustomActions.OnExplosionComplex += OnExplosion;
            OnRocketJump += RocketJumpAction;
        }
        public void OnExplosion(Vector3 position, ExplosionData data, Vector2 dir, Action onbegin, bool ignoreQueues, CoreDamageTypes damagetypes, bool ignoreDamageCaps)
        {
            if (player) { GameManager.Instance.StartCoroutine(CheckForRocketJump(player, position, data)); }
        }
        private IEnumerator CheckForRocketJump(PlayerController player, Vector3 position, ExplosionData data)
        {
            bool isAbleToRocketJump = false;
            float timer = 0.25f;
            while (!isAbleToRocketJump && timer > 0)
            {
                BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(player.PlayerIDX);
                timer -= 0.05f;
                float distance = Vector3.Distance(position, player.sprite.WorldCenter.ToVector3ZUp());
                if (distance <= (data.damageRadius + 0.2f) && (instanceForPlayer.ActiveActions.DodgeRollAction.IsPressed || player.IsDodgeRolling))
                {
                    Vector3 vector = player.sprite.WorldCenter.ToVector3ZUp() - position;
                    jumpDirection = new Vector2(vector.x, vector.y);
                    jumpDirection.x *= 240 * (timer / distance);
                    jumpDirection.y *= 240 * (timer / distance);
                    isAbleToRocketJump = true;
                    GameManager.Instance.StartCoroutine(HandleRocketJump(player));
                }
                yield return new WaitForSeconds(0.05f);
            }

            yield break;
        }
        private IEnumerator HandleRocketJump(PlayerController player)
        {
            //ETGModConsole.Log("HandleRocketJump started");
            cancelJump = false;
            if (player != null && jumpDirection != null)
            {
                ImprovedAfterImage effect = player.gameObject.GetOrAddComponent<ImprovedAfterImage>();
                effect.shadowLifetime = 0.2f;
                effect.shadowTimeDelay = 0.1f;
                effect.dashColor = MoreColours.lightgrey;
                effect.targetHeight = 1f;
                effect.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
                effect.minTranslation = 8f / 16f;

                player.ForceStopDodgeRoll();
                player.ToggleHandRenderers(true);
                player.ToggleGunRenderers(true);
                player.stats.RecalculateStats(player);
                player.OnPreDodgeRoll += OnDodgeRoll;
                player.healthHaver.OnDamaged += OnDamaged;
                player.specRigidbody.OnPreTileCollision += OnHitWall;
                player.specRigidbody.OnPreRigidbodyCollision += OnPreRigidbodyCollision;
                player.SetIsFlying(true, "rocketjump");
                effect.spawnShadows = true;
                player.AdditionalCanDodgeRollWhileFlying.SetOverride("rocketjump", true);
                isRocketJumping = true;
                OnRocketJump.Invoke(player);
                while (!cancelJump && Mathf.Abs(jumpDirection.x) + Mathf.Abs(jumpDirection.y) > 7f && (Mathf.Abs(player.Velocity.x) + Mathf.Abs(player.Velocity.y) > 7f))
                {
                    if (player.IsOnFire)
                    {
                        if (player.CurrentFireMeterValue > 0f) player.CurrentFireMeterValue = Mathf.Max(0f, player.CurrentFireMeterValue -= 0.005f * player.Velocity.magnitude);
                        if (player.CurrentFireMeterValue == 0f)
                        {
                            player.IsOnFire = false;
                        }
                    }
                    player.MovementModifiers += MovementMod;
                    jumpDirection.x /= 1.1f;
                    jumpDirection.y /= 1.1f;
                    yield return new WaitForSeconds(0.1f);
                    player.MovementModifiers -= MovementMod;
                }
                player.AdditionalCanDodgeRollWhileFlying.SetOverride("rocketjump", false);
                player.SetIsFlying(false, "rocketjump");
                effect.spawnShadows = false;
                player.stats.RecalculateStats(player);
                player.specRigidbody.OnPreRigidbodyCollision -= OnPreRigidbodyCollision;
                player.healthHaver.OnDamaged -= OnDamaged;
                player.specRigidbody.OnPreTileCollision -= OnHitWall;
                player.OnPreDodgeRoll -= OnDodgeRoll;
                isRocketJumping = false;
                //PostRocketJump.Invoke(player);
            }
            yield break;
        }
        // fly past unflipped table + other stuff with collisions
        public void OnPreRigidbodyCollision(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, SpeculativeRigidbody otherrigidbody, PixelCollider otherpixelcollider)
        {
            if (otherrigidbody.projectile == null)
            {
                if (otherrigidbody.majorBreakable != null)
                {
                    if (otherrigidbody.majorBreakable.GetComponentInParent<FlippableCover>() != null)
                    {
                        FlippableCover table = otherrigidbody.GetComponentInParent<FlippableCover>();
                        if (!table.IsFlipped) { otherrigidbody.RegisterTemporaryCollisionException(myrigidbody, 1f / 150f); }
                        else { cancelJump = true; }
                    }
                }
                
                else if (otherrigidbody.GetComponent<ConveyorBelt>() != null || otherrigidbody.GetComponent<MovingPlatform>())
                {
                    otherrigidbody.RegisterTemporaryCollisionException(myrigidbody);
                }
                else if (!otherrigidbody.minorBreakable)
                {
                    cancelJump = true;
                }
            }
        }

        // these are the things that will end a jump
        public void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
        {
            cancelJump = true;
        }
        public void OnHitWall(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, PhysicsEngine.Tile tile, PixelCollider tilepixelcollider)
        {
            cancelJump = true;
        }
        public void OnDodgeRoll(PlayerController player)
        {
            cancelJump = true;
        }
        // the jump itself
        public void MovementMod(ref Vector2 voluntaryVal, ref Vector2 involuntaryVal)
        {
            //ETGModConsole.Log(voluntaryVal);
            float involuntaryGradient = involuntaryVal.y / involuntaryVal.x;
            float voluntaryGradient = voluntaryVal.y / voluntaryVal.x;
            involuntaryVal += jumpDirection;
            if (voluntaryGradient.IsBetweenRange(involuntaryGradient - (involuntaryGradient / 4), involuntaryGradient + (involuntaryGradient/4)))
            {
                voluntaryVal /= 3f;
            }
            //ETGModConsole.Log($"expl grad: {involuntaryGradient}, move grad: {voluntaryGradient}, move: {voluntaryVal}");
        }
        public void SetPlayer(PlayerController target)
        {
            this.player = target;
        }
        public void RocketJumpAction(PlayerController player)
        {

        }

        private PlayerController player;
        private PassiveItem item;

        public bool isAbleToRocketJump;
        public bool isRocketJumping;
        public bool cancelJump;
        public bool isRocketJumper;

        public Vector2 jumpDirection;
        public Action<PlayerController> OnRocketJump;
        //public Action<PlayerController> PostRocketJump;
    }
}