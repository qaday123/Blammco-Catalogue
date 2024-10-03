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
    public class BaseballProjectile : MonoBehaviour
    {
        float m_timeActive = 0f;
        float m_debrisTime = 0f;
        bool hasHitEnemy = false;

        public Projectile self;
        public float MaxTimeForStun = 1f;
        public float MaxStunDuration = 8f;
        public float MinStunDuration = 1f;

        public void Start()
        {
            self = base.GetComponent<Projectile>();
            self.specRigidbody.OnTileCollision += Bounced;
            self.specRigidbody.OnRigidbodyCollision += OnBallCollided;
        }
        public void Update()
        {
            if (self && self.gameObject)
            {
                    m_timeActive += BraveTime.DeltaTime;
            }
        }
        public void OnBallCollided(CollisionData collisiondata)
        {
            SpeculativeRigidbody otherrigidbody = collisiondata.OtherRigidbody;
            if (otherrigidbody != null && otherrigidbody.healthHaver != null)
            {
                if (otherrigidbody.aiActor != null)
                {
                    hasHitEnemy = true;
                    if (!otherrigidbody.healthHaver.IsBoss)
                    {
                        float stunPercentage = (m_timeActive < MaxTimeForStun) ? m_timeActive / MaxTimeForStun : 1;
                        float stunDuration = Mathf.Lerp(MinStunDuration, MaxStunDuration, stunPercentage);

                        if (otherrigidbody.aiActor.behaviorSpeculator.IsStunned)
                            otherrigidbody.aiActor.behaviorSpeculator.UpdateStun(stunDuration);
                        else
                            otherrigidbody.aiActor.behaviorSpeculator.Stun(stunDuration, true);

                        if (m_timeActive < MaxTimeForStun) AkSoundEngine.PostEvent("baseball_stun", base.gameObject);
                        else AkSoundEngine.PostEvent("baseball_moonshot", base.gameObject);
                    }
                }
            }
        }
        public void Bounced(CollisionData tileCollision)
        {
            string[] bounceSounds = { "baseball_hitworld1", "baseball_hitworld2", "baseball_hitworld3" };
            AkSoundEngine.PostEvent(bounceSounds[UnityEngine.Random.Range(0, bounceSounds.Length)], base.gameObject);
        }
    }
}