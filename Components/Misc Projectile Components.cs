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
using Steamworks;

namespace TF2Stuff
{
    public class ProjectileSpin : MonoBehaviour
    {
        Projectile proj;
        public float degreesPerSecond = 360;
        public bool directionOfSpinDependsOnVelocity = true;
        public float DecayRate = 1f;
        public void Start()
        {
            proj = GetComponent<Projectile>();
            proj.OnDestruction += Disable;
            proj.OnBecameDebris += Disable;
        }
        public void Disable(DebrisObject p)
        {
            this.enabled = false;
        }
        public void Disable(Projectile p)
        {
            this.enabled = false;
        }
        public void Update()
        {
            if (proj != null)
            {
                float z = base.transform.rotation.eulerAngles.z;
                int spinDirection = 1;
                if (directionOfSpinDependsOnVelocity) spinDirection = proj.specRigidbody.Velocity.x > 0f ? -1 : 1;

                transform.rotation = Quaternion.Euler(0f, 0f, z + (degreesPerSecond * spinDirection * BraveTime.DeltaTime));
                degreesPerSecond *= (float)Math.Pow(DecayRate, BraveTime.DeltaTime);
            }
        }
    }
    public class BlockEnemyProjectilesMod : MonoBehaviour // stolen from nn... again...
    {
        public bool projectileSurvives;
        public IntVector2 RangeExtension;
        public bool collidesWithMap;
        public BlockEnemyProjectilesMod()
        {
            projectileSurvives = false;
            RangeExtension = new (0, 0);
            collidesWithMap = true;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();
            this.m_projectile.collidesWithProjectiles = true;
            this.m_projectile.UpdateCollisionMask();
            SpeculativeRigidbody specRigidbody = this.m_projectile.specRigidbody;
            specRigidbody.PixelColliders[0].Dimensions += RangeExtension;
            specRigidbody.CollideWithTileMap = collidesWithMap;
            specRigidbody.OnPreRigidbodyCollision += this.HandlePreCollision;
        }
        private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            if (otherRigidbody && otherRigidbody.projectile)
            {
                if (otherRigidbody.projectile.Owner is AIActor)
                {
                    otherRigidbody.projectile.DieInAir(false, true, true, false);
                }
                if (
                    !projectileSurvives) myRigidbody.projectile.DieInAir(false, true, true, false);
                PhysicsEngine.SkipCollision = true;
            }
        }

        private Projectile m_projectile;
    } //Causes the projectile to destroy enemy projectiles
}