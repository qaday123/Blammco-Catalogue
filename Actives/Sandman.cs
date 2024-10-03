using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;

/* NOTES:
 * debate on whether this should be an active or passive
 * Make stun effect scale based on time in air? how do
 * should this be cursed??
 * Casey syngergy - idk what it would do tho
*/
namespace TF2Stuff
{
    public class Sandman : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Sandman";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/actives/sandman_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Sandman>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 8f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "I Love My Ball";
            string longDesc = "Launch a ball that will stun your enemies in amazement! Nothin' beats your skills on the field. Stun duration increases " +
                "based on how long your shot was. If you miss - no worries! Pick that ball up and try again!\n\n" +
                "HOME RUN! He heard them say. Round the bases faster than a speeding bullet. Good times, they were. Better times they are now.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            item.AddPassiveStatModifier(StatModifier.Create(PlayerStats.StatType.Curse, StatModifier.ModifyMethod.ADDITIVE, 1));
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;

            ball = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            ball.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(ball.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(ball);
            ball.baseData.damage = 4f;
            ball.baseData.speed = 24f;
            ball.baseData.range = 2000f;
            ball.collidesWithPlayer = true;
            ball.allowSelfShooting = true;
            ball.SetProjectileSpriteRight("baseball", 4, 4, lightened: false, anchor: tk2dBaseSprite.Anchor.MiddleCenter, 2, 2);
            /*ball.AnimateProjectile(new List<string>
            {
                "baseball_001",
                "baseball_002",
                "baseball_003",
                "baseball_004",
            }, 6, true, AnimateBullet.ConstructListOfSameValues(new IntVector2(4, 4), 4), // sprite size
            AnimateBullet.ConstructListOfSameValues(true, 4), //Lightened??
            AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 4), //Anchors
            AnimateBullet.ConstructListOfSameValues(true, 4), //Anchors Change Colliders
            AnimateBullet.ConstructListOfSameValues(false, 4), //Fixes Scales
            AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 4),  //Manual Offsets
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(new IntVector2(4, 4), 4), //Collider Pixel Sizes?
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4), //Override Collider Offsets
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));*/
            

            PickupableProjectile pickup = ball.gameObject.GetOrAddComponent<PickupableProjectile>();

            BaseballProjectile baseball = ball.gameObject.GetOrAddComponent<BaseballProjectile>();
            baseball.MaxStunDuration = 8f;
            baseball.MaxTimeForStun = 1f;

            
        }
        //private static Projectile ball; // this is causing the registered projectile to not work
        public static int ID;
        public static Projectile ball;
        public override void DoEffect(PlayerController user)
        {
            bool hasBalls = user.PlayerHasActiveSynergy("I Love My Balls");

            int BallsToSpawn = hasBalls ? 3 : 1;
            float angleVariance = hasBalls ? 30f : 0;

            AkSoundEngine.PostEvent("Play_baseball_shoot", base.gameObject);

            for (int i = 0; i < BallsToSpawn; i++)
            {
                float variance = UnityEngine.Random.Range(-1f, 1f) * angleVariance;
                float angle = (user.CurrentGun == null) ? 0f : user.CurrentGun.CurrentAngle;
                Vector2 offset = Vector2.zero;//new(Mathf.Cos(angle * (Mathf.PI / 180)), Mathf.Sin(angle * (Mathf.PI / 180)));
                GameObject gameObject = SpawnManager.SpawnProjectile(ball.gameObject, user.sprite.WorldCenter + offset, Quaternion.Euler(0f, 0f, angle + variance), true);
                Projectile proj = gameObject.GetComponent<Projectile>();
                PickupableProjectile component = gameObject.GetComponent<PickupableProjectile>();
                BaseballProjectile baseball = gameObject.GetComponent<BaseballProjectile>();
                if (proj != null)
                {
                    proj.Owner = user;
                    proj.Shooter = user.specRigidbody;
                    //proj.specRigidbody.RegisterTemporaryCollisionException(user.specRigidbody);
                }
                if (component != null && baseball != null)
                {
                    if (hasBalls)
                    {
                        baseball.MaxStunDuration /= 1.5f;
                        component.DespawnTime /= 4;
                    }
                    if (user.PlayerHasActiveSynergy("Home Run All The Time"))
                    {
                        component.DespawnTime /= 2;
                        baseball.MaxTimeForStun = 0f;
                    }
                    component.OnPickup += OnBallPickup;
                }
            }
        }
        
        public void OnBallPickup()
        {
            if (this && this.gameObject)
            {
                AkSoundEngine.PostEvent("recharged", base.gameObject);
                ClearCooldowns();
            }
        }
    }
}