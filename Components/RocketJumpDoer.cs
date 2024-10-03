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

/*
 * Jumping should decrease your fire bar over tiem if on fire // done
 * Some trap tiles count as tile collisions- find a way to check that and circumvent it // done
*/ 
namespace TF2Stuff
{
    public class RocketJumpDoer : MonoBehaviour
    {
        public RocketJumpDoer()
        {
            isAbleToRocketJump = false;
            isRocketJumping = false;
            cancelJump = false;
            AirstrafeControl = 0.1f; // normal amount
            cancelJumpSpeedThreshold = 7f;
            jumpMaxMagnitude = 150f;
        }
        // find the item, or player the component is attached to.
        private void Start()
        {
            item = base.GetComponent<PassiveItem>();
            gun = base.GetComponent<Gun>();
            if (item != null)
                player = item.Owner;
            else if (gun != null)
                player = gun.GunPlayerOwner();
            else
                player = base.GetComponent<PlayerController>();
            if (player) 
                CustomActions.OnExplosionComplex += OnExplosion;
            OnRocketJump += RocketJumpAction;
            PostRocketJump += AfterRocketJump;
        }
        public void OnExplosion(Vector3 position, ExplosionData data, Vector2 dir, Action onbegin, bool ignoreQueues, CoreDamageTypes damagetypes, bool ignoreDamageCaps)
        {
            if (player && !isRocketJumper) { GameManager.Instance.StartCoroutine(CheckForRocketJump(player, position, data)); }
        }
        // checks if the player is able to rocket jump when an explosion occurs
        private IEnumerator CheckForRocketJump(PlayerController player, Vector3 position, ExplosionData data)
        {
            bool isAbleToRocketJump = false;
            float timer = 0.25f;
            while (!isAbleToRocketJump && timer > 0)
            {
                BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(player.PlayerIDX);
                timer -= BraveTime.DeltaTime;
                float distance = Vector3.Distance(position, player.sprite.WorldCenter.ToVector3ZUp());
                if (distance <= (data.damageRadius + 0.2f) && ((instanceForPlayer.ActiveActions.DodgeRollAction.IsPressed && timer > 0.125f) || player.IsDodgeRolling))
                {
                    Vector3 vector = player.sprite.WorldCenter.ToVector3ZUp() - position;
                    jumpDirection = new Vector2(vector.x, vector.y);
                    jumpDirection.x *= jumpMaxMagnitude * (timer / distance);
                    jumpDirection.y *= jumpMaxMagnitude * (timer / distance);
                    isAbleToRocketJump = true;
                    GameManager.Instance.StartCoroutine(HandleRocketJump(player));
                }
                yield return null;
            }

            yield break;
        }
        public void CheckForJump(PlayerController player, Vector3 position, ExplosionData data)
        {
            if (player) GameManager.Instance.StartCoroutine(CheckForRocketJump(player, position, data));
        }
        // does the actual "jump" itself
        private IEnumerator HandleRocketJump(PlayerController player)
        {
            //ETGModConsole.Log("HandleRocketJump started");
            cancelJump = false;
            dodgesCancelsJump = false;
            if (player != null)
            {
                ImprovedAfterImage effect = player.gameObject.GetOrAddComponent<ImprovedAfterImage>();
                float delay = 0f;
                effect.shadowLifetime = 0.2f;
                effect.shadowTimeDelay = 0.1f;
                effect.dashColor = MoreColours.lightgrey;
                effect.targetHeight = 1f;
                effect.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
                effect.minTranslation = 8f / 16f;
                // ^ handles all the VFX
                player.ForceStopDodgeRoll();
                player.ToggleHandRenderers(true);
                player.ToggleGunRenderers(true);
                player.stats.RecalculateStats(player);
                player.OnPreDodgeRoll += OnDodgeRoll; // assigns all the actions that would cancel the jump
                player.healthHaver.OnDamaged += OnDamaged;
                player.specRigidbody.OnTileCollision += OnHitWall;
                player.specRigidbody.OnPreRigidbodyCollision += OnPreRigidbodyCollision;
                player.SetIsFlying(true, "rocketjump");
                effect.spawnShadows = true;
                
                player.MovementModifiers += MovementMod;
                isRocketJumping = true;
                if (OnRocketJump != null) OnRocketJump(player); // invokes the OnRocketJump action
                // main "loop" of the jump, descelerates over time and checks for a low enough velocity to cancel the jump
                while (!cancelJump && jumpDirection.magnitude > cancelJumpSpeedThreshold)
                {
                    //ETGModConsole.Log($"Loop looping, {jumpDirection}");
                    if (delay > 0.2f) player.AdditionalCanDodgeRollWhileFlying.SetOverride("rocketjump", true);
                    if (player.IsOnFire)
                    {
                        if (player.CurrentFireMeterValue > 0f) player.CurrentFireMeterValue = Mathf.Max(0f, player.CurrentFireMeterValue -= 0.0005f * player.Velocity.magnitude);
                        if (player.CurrentFireMeterValue == 0f)
                        {
                            player.IsOnFire = false;
                        }
                    }
                    jumpDirection.x *= Mathf.Pow(0.8f, BraveTime.DeltaTime);
                    jumpDirection.y *= Mathf.Pow(0.8f, BraveTime.DeltaTime);

                    if (canWallScrape)
                    {
                        if (CellCheckIHateThisAAAA())
                        {
                            jumpDirection *= Mathf.Pow(0.5f, BraveTime.DeltaTime);
                            //GameManager.Instance.Dungeon.dungeonDustups.InstantiateDodgeDustup(jumpDirection / jumpMaxMagnitude, player.CenterPosition);
                            //CodeShortcuts.DoSmokeAt(player.CenterPosition);
                            // play a particle effect against the wall here // sadness
                        }
                        else canWallScrape = false;
                    }
                    if (!player.AdditionalCanDodgeRollWhileFlying.BaseValue) delay += BraveTime.DeltaTime;
                    yield return null;
                }
                if (cancelJump && !player.IsDodgeRolling)
                {
                    GameManager.Instance.MainCameraController.DoScreenShake(new ScreenShakeSettings(jumpDirection.magnitude / jumpMaxMagnitude, 5f, 0.1f, 0.3f), player.specRigidbody.UnitCenter);
                }
                player.AdditionalCanDodgeRollWhileFlying.SetOverride("rocketjump", false);
                player.MovementModifiers -= MovementMod;
                player.SetIsFlying(false, "rocketjump");
                if (effect != null) effect.spawnShadows = false;
                player.stats.RecalculateStats(player);
                player.specRigidbody.OnPreRigidbodyCollision -= OnPreRigidbodyCollision;
                player.healthHaver.OnDamaged -= OnDamaged;
                player.specRigidbody.OnTileCollision -= OnHitWall;
                player.OnPreDodgeRoll -= OnDodgeRoll;
                isRocketJumping = false;
                if (PostRocketJump != null) PostRocketJump(player);
            }
            yield break;
        }
        // fly past unflipped table + other stuff with collisions
        public void OnPreRigidbodyCollision(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, SpeculativeRigidbody otherrigidbody, PixelCollider otherpixelcollider)
        { // do the fun thing to do damage to enemies.dw
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
                    //otherrigidbody.RegisterTemporaryCollisionException(myrigidbody);
                    PhysicsEngine.SkipCollision = true;
                }
                else if (otherrigidbody.healthHaver && otherrigidbody.aiActor)
                {
                    if (!cancelJump)
                    {
                        otherrigidbody.knockbackDoer.ApplyKnockback(jumpDirection.normalized, jumpDirection.magnitude);
                        otherrigidbody.healthHaver.ApplyDamageDirectional((player.stats.rollDamage / 8f) * jumpDirection.magnitude, otherrigidbody.sprite.WorldCenter - myrigidbody.sprite.WorldCenter, "Rocket Jump", CoreDamageTypes.None);
                        cancelJump = true;
                    }
                }
                else if (!otherrigidbody.minorBreakable)
                {
                    cancelJump = true;
                }
            }
            else
            {
                PhysicsEngine.SkipCollision = true;
            }
        }

        public bool AnyDistanceToNearestWall(SpeculativeRigidbody body)
        {
            bool NearWall = false;
            IntVector2[] vectorsToCheck = new IntVector2[] { IntVector2.Up, IntVector2.Down, IntVector2.Left, IntVector2.Right };
            int dist = 4;

            foreach (IntVector2 i in vectorsToCheck) 
            { 
                NearWall |= (body.DistanceToNearestWall(i, dist) >= 0 && body.DistanceToNearestWall(i, dist) < dist);
            }
            return NearWall;
        }
        public bool CellCheckIHateThisAAAA()
        {
            RoomHandler room = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(Vector2Extensions.ToIntVector2(player.CenterPosition, VectorConversions.Round));
            if (room != null)
            {
                CellData cellaim = room.GetNearestCellToPosition(player.CenterPosition);
                //CellData cellaimminus = room.GetNearestCellToPosition(player.CenterPosition - new Vector2(0, 1f));
                return cellaim.HasWallNeighbor(true, true); //|| cellaimminus.HasWallNeighbor(true, true);
            }
            else
            {
                return false;
            }
            
        }
        // these are the things that will end a jump
        public void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
        {
            //cancelJump = true;
        }
        public void OnHitWall(CollisionData collisionData)
        {
            //cancelJump = true;
            canWallScrape = true;
            Vector2 normalComponent = Vector2.Dot(jumpDirection, collisionData.Normal) * collisionData.Normal;
            //collisionDirection = new IntVector2((int)collisionData.Normal.x, (int)collisionData.Normal.y) * -1; 
            GameManager.Instance.MainCameraController.DoScreenShake(new ScreenShakeSettings(normalComponent.magnitude / jumpMaxMagnitude, 5f, 0.1f, 0.3f), player.specRigidbody.UnitCenter);
            jumpDirection -= normalComponent;
            jumpDirection *= 0.8f;
        }
        public void OnDodgeRoll(PlayerController player)
        {
            //ETGModConsole.Log("ondodge running");
            cancelJump = true;
        }
        // the jump itself
        public void MovementMod(ref Vector2 voluntaryVal, ref Vector2 involuntaryVal)
        {
            involuntaryVal += jumpDirection;
            // NEW AND BETTER WAY OF DOING THIS WITH THE POWER OF \o/ DOT PRODUCTS!!!!
            if (voluntaryVal != Vector2.zero && involuntaryVal != Vector2.zero) // make sure you're not dividing by 0 ._.
            {
                float CosTheta = Vector2.Dot(involuntaryVal, voluntaryVal) / (voluntaryVal.magnitude * involuntaryVal.magnitude);
                jumpDirection = involuntaryVal + voluntaryVal * AirstrafeControl * (1 - Mathf.Abs(CosTheta)) * (involuntaryVal.magnitude / jumpMaxMagnitude);
                //involuntaryVal += voluntaryVal * 1.5f * (1 - Mathf.Abs(CosTheta));
                voluntaryVal = Vector2.zero;
                
            }
            // scales the control over character based on the angle between the rocket jump vector and player vector
        }
        public void SetPlayer(PlayerController target)
        {
            this.player = target;
        }
        public void RocketJumpAction(PlayerController player)
        {

        }
        public void AfterRocketJump(PlayerController player)
        {

        }

        private PlayerController player;
        private PassiveItem item;
        private Gun gun;

        public PlayerController Player
        {
            get
            {
                return player;
            }
        }

        public bool isAbleToRocketJump;
        public bool isRocketJumping;
        public bool cancelJump;
        public bool isRocketJumper;
        public bool dodgesCancelsJump;
        public float AirstrafeControl;
        public float cancelJumpSpeedThreshold;
        public float jumpMaxMagnitude;
        bool canWallScrape = false;

        IntVector2 collisionDirection = IntVector2.Zero;
        public Vector2 jumpDirection;
        public Action<PlayerController> OnRocketJump;
        public Action<PlayerController> PostRocketJump;
    }
}