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

namespace ExampleMod
{
    public class BlockEnemyProjectilesMod : MonoBehaviour // stolen from nn... again...
    {
        public bool projectileSurvives;
        public BlockEnemyProjectilesMod()
        {
            this.projectileSurvives = false;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();
            this.m_projectile.collidesWithProjectiles = true;
            this.m_projectile.UpdateCollisionMask();
            SpeculativeRigidbody specRigidbody = this.m_projectile.specRigidbody;
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