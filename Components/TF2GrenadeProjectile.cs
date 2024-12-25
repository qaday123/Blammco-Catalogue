using DaikonForge.Tween.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Tilemaps;

namespace TF2Items // make proper roller behaviour later
{
    public class TF2GrenadeProjectile : MonoBehaviour
    {
        float activeTime = 0f;
        ExplosiveModifier explode;
        bool becameDebris = false;
        bool debris_grounded = false;
        bool startedRolling;
        Vector2 lastVelocity = Vector2.zero;

        public Projectile self;
        public float fuseTime = 2.3f;
        public ExplosionData expsionData;
        public BounceProjModifier bounce;


        PhysicsEngine.Tile cachedLastCollidedTile;
        SpeculativeRigidbody cachedLastCollidedBody;

        public void Start()
        {
            self = GetComponent<Projectile>();
            bounce = GetComponent<BounceProjModifier>();
            if (expsionData != null ) 
            {
                explode = self.gameObject.GetOrAddComponent<ExplosiveModifier>();
                explode.explosionData = expsionData;
            }
            //self.specRigidbody.OnTileCollision += OnCollision;
            self.specRigidbody.OnCollision += OnCollision;
            self.OnHitEnemy += ExplodeOnDirect;
            self.specRigidbody.OnPreRigidbodyCollision += CheckValidBodyCollision;
            self.specRigidbody.OnPreTileCollision += CheckValidTileCollision;
            //self.OnBecameDebrisGrounded += CorrectDirection;

            self.BulletScriptSettings.surviveRigidbodyCollisions = true;
            self.BulletScriptSettings.surviveTileCollisions = true;
        }
        
        public void Update()
        {
            if (base.gameObject != null && self)
            {
                fuseTime -= BraveTime.DeltaTime;
                //ETGModConsole.Log("Velocity: " + self.specRigidbody.Velocity.magnitude);

                if (fuseTime <= 0f && explode != null)
                {
                    //explode.Explode(self.SafeCenter);
                    self.enabled = true;
                    self.DieInAir(true);
                }
                /*if (self && becameDebris)
                {
                    self.Speed *= Mathf.Pow(0.8f, BraveTime.DeltaTime);
                }*/
            }
        }
        public void ExplodeOnDirect(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (!becameDebris && proj)
            {
                proj.DieInAir();
            }
        }
        public void CorrectDirection(DebrisObject debris)
        {
            debris_grounded = true;
            //ETGModConsole.Log(debris.specRigidbody.Velocity.ToAngle());
            //if (debris && self) debris.transform.rotation = Quaternion.Euler(0, 0, debris.specRigidbody.Velocity.ToAngle() + 90);
        }
        public IEnumerator DelayedTurnIntoDebris()
        {
            float deadTime = 0.2f;
            //becameDebris = true;
            while (deadTime > 0f)
            {
                self.specRigidbody.Velocity *= Mathf.Pow(0.6f, BraveTime.DeltaTime);
                deadTime -= BraveTime.DeltaTime;
                yield return null;
            }
            DebrisObject debris;

            if (self)
            {
                becameDebris = true;
                if (bounce) { Destroy(bounce); bounce = null; }
                self.OnBecameDebris(debris = self.BecomeDebris(self.LastVelocity, 1f));
                debris.gameObject.GetOrAddComponent<PipeRoller>();
                //debris.StartCoroutine(HandleRoller(debris));
                //debris.OnGrounded += CorrectDirection;
                self.enabled = false;
            }

            yield break;
        }
        public IEnumerator HandleRoller(DebrisObject roller)
        {
            //bool startedRolling = false;
            float correctionRotationRate = 180f;
            //Vector2 lastVelocity = Vector2.zero;

            roller.angularVelocity = 540f;
            roller.angularVelocityVariance = 360f;
            roller.killTranslationOnBounce = false;
            roller.doesDecay = false;
            roller.removeSRBOnGrounded = true;

            while (fuseTime > 0) // WHILE LOOPS IN ENUMERATORS DO NOT WORK HOW I THOUGHT THEY WORK
            {
                roller.specRigidbody.Velocity *= Mathf.Pow(0.8f, BraveTime.DeltaTime);
                roller.angularVelocity *= Mathf.Pow(0.8f, BraveTime.DeltaTime);
                //ETGModConsole.Log(lastVelocity + ", " + roller.specRigidbody.Velocity);

                /*if (roller.specRigidbody.Velocity == Vector2.zero)
                {
                    ETGModConsole.Log("Started");
                    roller.ApplyVelocity(lastVelocity.normalized);
                }*/






                if (debris_grounded && !startedRolling)
                {
                    //ETGModConsole.Log("Started Rolling.");
                    startedRolling = true;
                    //roller.ApplyVelocity(lastVelocity.normalized);
                    /*roller.angularVelocity = 360; // i don't think this does anything when its grounded :(
                    Vector2 travelVector = (lastVelocity != Vector2.zero) ? lastVelocity : Vector2.left;
                    travelVector.Normalize();
                    roller.specRigidbody.Velocity = travelVector;
                    roller.transform.rotation = Quaternion.Euler(0, 0, travelVector.ToAngle() + 90);*/
                }
                /*if (startedRolling) //&& roller.transform.rotation.z.IsBetweenRange(lastVelocity.ToAngle() + 89, lastVelocity.ToAngle() + 91)) // ew
                {
                    //ETGModConsole.Log(lastVelocity.ToAngle() + 90 + ", " + roller.transform.rotation.z);
                    roller.angularVelocity = 0;
                    //roller.transform.rotation = Quaternion.Euler(0, 0, roller.transform.rotation.z + correctionRotationRate * BraveTime.DeltaTime);
                }*/
                if (roller.specRigidbody.Velocity != Vector2.zero) lastVelocity = roller.specRigidbody.Velocity;
                //yesETGModConsole.Log(lastVelocity);

                yield return null; 
            }
            self.enabled = true;
            self.DieInAir();

            yield break;
        }
        public void CheckValidBodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            if (otherRigidbody && cachedLastCollidedBody == otherRigidbody)
                PhysicsEngine.SkipCollision = true;
            else
                cachedLastCollidedBody = otherRigidbody;
        }
        public void CheckValidTileCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, PhysicsEngine.Tile tile, PixelCollider tilePixelCollider)
        {
            if (cachedLastCollidedTile == tile)
            {
                PhysicsEngine.SkipCollision = true;
            }
            else
            {
                cachedLastCollidedTile = tile;
            }
        }
        public void OnCollision(CollisionData collisiondata)
        {

            Vector2 newVel = collisiondata.MyRigidbody.Velocity;
            if (collisiondata.CollidedX)
                newVel = newVel.WithX(-newVel.x);
            if (collisiondata.CollidedY)
                newVel = newVel.WithY(-newVel.y);
            if (newVel.magnitude > 4) newVel /= 5f;

            PhysicsEngine.PostSliceVelocity = newVel;
            collisiondata.MyRigidbody.Velocity = newVel;

            self.SendInDirection(newVel, false);
            self.m_currentSpeed = newVel.magnitude;
            self.LastVelocity = newVel;

            if (!becameDebris && self && self.enabled)
            {
                self.StartCoroutine(DelayedTurnIntoDebris()); 
            }
        }
    }
    public class PipeRoller : DebrisObject
    {
        Vector2 lastVelocity;
        public override void Start()
        {
            //Console.Log("Start Ran");
            base.Start();
            debris.angularVelocity = 450;
            debris.OnGrounded += OnBecameGrounded;
            debris.removeSRBOnGrounded = true;
            //debris.specRigidbody.MovementRestrictor.
        } 
        public override void InvariantUpdate(float realDeltaTime)
        {
            //Console.Log(lastVelocity);
            doesDecay = false;
            //if (m_velocity != Vector3.zero) lastVelocity = base.specRigidbody.Velocity;
            //else m_velocity = lastVelocity.normalized;
            base.InvariantUpdate(realDeltaTime);
            //Console.Log(m_velocity);
        }
        public void OnBecameGrounded(DebrisObject debris)
        {
            //Console.Log(lastVelocity);
            m_velocity = new(1, 1);
        }
    }
}
