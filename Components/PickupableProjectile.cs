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
using UnityEngine.Experimental.UIElements;

namespace TF2Stuff
{
    // probably easier to spawn in a new gameobject (debris?) for this instead when the speed threshold is reached //done
    public class PickupableProjectile : MonoBehaviour
    {
        float m_timeActive = 0f;
        float m_debrisTime = 0f;
        bool hasHitEnemy = false;
        bool becameDebris = false;
        bool hasHitWall;

        public Action OnPickup;
        public Projectile self;
        public float DebrisTime = 2f;
        public float DespawnTime = 6f;
        public bool BecomesDebris = true;
        public bool hasAfterImageEffect = true;
        public GameObject DeathVFX;
        public float speedDecayRate = 0.5f;

        ImprovedAfterImage effect;
        ProjectileSpin spin;
        public float spinDegreeRate = 900f;
        public float spinDecayRate = 0.8f;

        public void Start()
        {
            self = base.GetComponent<Projectile>();
            if (BecomesDebris) self.DestroyMode = Projectile.ProjectileDestroyMode.BecomeDebris;
            self.specRigidbody.OnPreRigidbodyCollision += OnPreBallCollided;
            self.specRigidbody.OnRigidbodyCollision += OnBallCollided;
            self.specRigidbody.RegisterTemporaryCollisionException(self.Shooter);
            self.specRigidbody.OnTileCollision += Bounced;
            //fps_tracker = self.spriteAnimator.ClipFps;
            DeathVFX = self.hitEffects.overrideMidairDeathVFX;

            OnPickup += OnBallPlayerPickup;

            BounceProjModifier bounce = self.gameObject.GetOrAddComponent<BounceProjModifier>();
            bounce.numberOfBounces += 1;
            bounce.percentVelocityToLoseOnBounce = 0.2f;

            spin = self.gameObject.GetOrAddComponent<ProjectileSpin>();
            spin.directionOfSpinDependsOnVelocity = true;
            spin.degreesPerSecond = spinDegreeRate;
            spin.DecayRate = spinDecayRate;

            if (hasAfterImageEffect)
            {
                effect = self.gameObject.GetOrAddComponent<ImprovedAfterImage>();
                float delay = 0f;
                effect.shadowLifetime = 0.04f;
                effect.shadowTimeDelay = 0f;
                effect.dashColor = Color.white;
                effect.targetHeight = 1f;
                effect.maxEmission = 0.2f;
                //effect.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
                effect.minTranslation = 0f / 16f;
                effect.spawnShadows = true;
            }
            hasHitWall = false;
        }
        public void Update()
        {
            if (gameObject && self && self.gameObject)
            {
                if (!becameDebris)
                {
                    m_timeActive += BraveTime.DeltaTime;
                    self.Speed *= Mathf.Pow(speedDecayRate, BraveTime.DeltaTime);

                    if (m_timeActive > DebrisTime && !becameDebris)
                    {
                        self.Speed = 0;
                        DropBall(self.LastVelocity);
                    }
                }
            }
            if (gameObject && becameDebris)
            {
                m_debrisTime += BraveTime.DeltaTime;
                if (m_debrisTime > DespawnTime)
                {
                    DespawnDebris();
                }
            }
        }
        public void DropBall(Vector2 NewVelocity)
        {
            becameDebris = true;
            //self.spriteAnimator.Stop();
            spin.enabled = false;
            if (effect && hasAfterImageEffect) effect.spawnShadows = false;
            if (BecomesDebris) self.BecomeDebris(NewVelocity, self.specRigidbody.UnitHeight);
        }
        public void OnPreBallCollided(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, SpeculativeRigidbody otherrigidbody, PixelCollider otherpixelcollider)
        {  
            if (otherrigidbody != null && otherrigidbody.healthHaver != null)
            {
                if (otherrigidbody.healthHaver.isPlayerCharacter)
                {
                    PhysicsEngine.SkipCollision = true;
                    if (OnPickup != null && hasHitWall)
                    {
                        OnPickup(); 
                    }
                }
            }
        }
        public void OnBallCollided(CollisionData collisiondata)
        {
            SpeculativeRigidbody otherrigidbody = collisiondata.OtherRigidbody;
            if (otherrigidbody != null && otherrigidbody.healthHaver != null)
            {
                if (otherrigidbody.aiActor != null)
                {
                    if (!otherrigidbody.healthHaver.IsBoss) DespawnTime *= 0.75f * m_timeActive;

                    Vector2 newVel = collisiondata.MyRigidbody.Velocity;
                    if (collisiondata.CollidedX)
                        newVel = newVel.WithX(-newVel.x);
                    if (collisiondata.CollidedY)
                        newVel = newVel.WithY(-newVel.y);
                    PhysicsEngine.PostSliceVelocity = newVel;
                    DropBall(newVel / 2);
                }
            }
        }
        public void Bounced(CollisionData data)
        {
            hasHitWall = true;
        }
        public void OnBallPlayerPickup()
        {
            DespawnDebris();
        }
        public void DespawnDebris()
        {
            SpawnManager.SpawnVFX(DeathVFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}