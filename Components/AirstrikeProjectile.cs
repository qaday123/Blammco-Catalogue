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
    public class AirstrikeProjectile : MonoBehaviour
    {
        public Projectile self;
        public PlayerController player;
        public float ascentTime = 0.5f;
        float timeActive = 0f;
        public GameObject targetEffect;

        GameObject activeEffect;

        bool swapped = false;

        public Vector2 endPosition;
        bool canCollide;
        public void Start()
        {
            self = base.GetComponent<Projectile>();
            player = self.ProjectilePlayerOwner();
            canCollide = false;
            if (self && player)
            {
                endPosition = player.unadjustedAimPoint;
                self.specRigidbody.Velocity = new Vector3(0, 0, self.Speed);
                self.SendInDirection(Vector2.zero, true);
                self.m_currentDirection = Vector3.zero;
                self.sprite.transform.rotation = Quaternion.Euler(0, 0, 90);
                if (targetEffect != null )
                {
                    activeEffect = Instantiate(targetEffect, endPosition, Quaternion.identity);
                }
                ParticleSystem trail = self.ParticleTrail;
                if (trail) BraveUtility.EnableEmission(self.ParticleTrail, enabled: true);
            }
            
            self.specRigidbody.OnPreRigidbodyCollision += OnPreCollision;
        }
        public void Update()
        {
            if (self) // doesnt work
            {
                timeActive += BraveTime.DeltaTime;
                float displacement = self.Speed * BraveTime.DeltaTime;
                Vector3 previousPosition = self.sprite.transform.position;

                if (timeActive < ascentTime)
                {
                    self.sprite.transform.position += new Vector3(0, displacement, 0);
                }
                else
                {
                    if (!swapped)
                    {
                        //ETGModConsole.Log("Swapped");
                        swapped = true;
                        self.sprite.transform.rotation = Quaternion.Euler(0, 0, -90);
                        self.sprite.transform.position = new Vector3(endPosition.x, previousPosition.y);
                    }
                    self.sprite.transform.position += new Vector3(0, -displacement, 0);
                    if (self.sprite.transform.position.y <= endPosition.y)
                    {
                        canCollide = true;
                        self.transform.position = new(endPosition.x, endPosition.y);
                        self.specRigidbody.Position = new(endPosition.x, endPosition.y);
                        self.DieInAir();
                        if (activeEffect != null) Destroy(activeEffect);
                    }
                }
                //ETGModConsole.Log(self.sprite.transform.position + ", " + timeActive);
            }
        }
        public void OnPreCollision(SpeculativeRigidbody body, PixelCollider collider, SpeculativeRigidbody collision, PixelCollider collisionCollider)
        {
            if (!canCollide && collision.GetComponentInParent<DungeonDoorController>() == null && (collision.GetComponent<MajorBreakable>() == null || !collision.GetComponent<MajorBreakable>().IsSecretDoor))
            {
                PhysicsEngine.SkipCollision = true;
                return;
            }
        }
    }
}