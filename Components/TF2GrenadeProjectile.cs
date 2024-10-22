using DaikonForge.Tween.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Tilemaps;

namespace TF2Items
{
    public class TF2GrenadeProjectile : MonoBehaviour
    {
        float activeTime = 0f;
        ExplosiveModifier explode;
        bool becameDebris = false;

        public Projectile self;
        public float fuseTime = 2.5f;
        public ExplosionData expsionData;

        public void Start()
        {
            self = GetComponent<Projectile>();
            if (expsionData != null ) 
            {
                explode = self.gameObject.GetOrAddComponent<ExplosiveModifier>();
                explode.explosionData = expsionData;
            }
            self.specRigidbody.OnTileCollision += OnCollision;
            //self.OnBecameDebrisGrounded += CorrectDirection;
        }
        
        public void Update()
        {
            if (base.gameObject != null)
            {
                fuseTime -= BraveTime.DeltaTime;

                if (fuseTime <= 0f && explode != null)
                {
                    //explode.Explode(self.SafeCenter);
                    if (self) self.DieInAir(true);
                }
                if (self && becameDebris)
                {
                    self.Speed *= Mathf.Pow(0.8f, BraveTime.DeltaTime);
                }
            }
        }
        public void CorrectDirection(DebrisObject debris)
        {
            if (debris && self) debris.transform.rotation = Quaternion.Euler(0, 0, self.Direction.ToAngle() + 90);
        }
        public void OnCollision(CollisionData collisiondata)
        {
            if (!becameDebris)
            {
                DebrisObject debris;
                Vector2 newVel = collisiondata.MyRigidbody.Velocity;
                if (collisiondata.CollidedX)
                    newVel = newVel.WithX(-newVel.x);
                if (collisiondata.CollidedY)
                    newVel = newVel.WithY(-newVel.y);
                PhysicsEngine.PostSliceVelocity = newVel;
                self.OnBecameDebris(debris = self.BecomeDebris(newVel / 2, 1f));
                self.Speed = 1f;
                debris.OnGrounded += CorrectDirection;
                debris.angularVelocity = 180f;
                becameDebris = true;
            }
        }
    }
}
