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
namespace ExampleMod
{
    public class Sandman : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Sandman";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "ExampleMod/Resources/actives/sandman_sprite";

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
            string longDesc = "Launch a ball that will stun your enemies in amazement! Nothin' beats your skills on the field.\n\n" +
                "Normally, melee weapons would behold the weilder with a great curse, however this bat had found a certain " +
                "loophole by 'shooting' a projectile as it's main function. The laws of the Gungeon really ought to be made " +
                "clearer.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;

            /*Projectile ball = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            ball.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(ball.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(ball);
            ball.baseData.damage = 4f;
            ball.baseData.speed = 24f;
            ball.AppliesStun = true;
            ball.StunApplyChance = 100f;
            ball.AppliedStunDuration = 14f;
            ball.AnimateProjectile(new List<string>
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
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));
            BounceProjModifier bounce = ball.gameObject.GetOrAddComponent<BounceProjModifier>();
            bounce.numberOfBounces = +1;*/
        }
        //private static Projectile ball; // this is causing the registered projectile to not work
        public static int ID;
        public override void DoEffect(PlayerController user)
        {
            hitenemy = false;
            elapsed = 0f;
            //user.PostProcessProjectile += PostProcessProjectile;
            AkSoundEngine.PostEvent("Play_baseball_shoot", base.gameObject);

            Projectile ball = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            ball.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(ball.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(ball);
            ball.baseData.damage = 4f;
            ball.baseData.speed = 28f;
            ball.baseData.range = 10000f;
            ball.AppliesStun = true;
            ball.StunApplyChance = 100f;
            ball.AppliedStunDuration = 6f;
            ball.AnimateProjectile(new List<string>
            {
                "baseball_001",
                "baseball_002",
                "baseball_003",
                "baseball_004",
            }, 10, true, AnimateBullet.ConstructListOfSameValues(new IntVector2(6, 6), 4), // sprite size
            AnimateBullet.ConstructListOfSameValues(true, 4), //Lightened??
            AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 4), //Anchors
            AnimateBullet.ConstructListOfSameValues(true, 4), //Anchors Change Colliders
            AnimateBullet.ConstructListOfSameValues(false, 4), //Fixes Scales
            AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 4),  //Manual Offsets
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(new IntVector2(4, 4), 4), //Collider Pixel Sizes?
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4), //Override Collider Offsets
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));
            BounceProjModifier bounce = ball.gameObject.GetOrAddComponent<BounceProjModifier>();
            bounce.numberOfBounces = +1;
            GameObject gameObject = SpawnManager.SpawnProjectile(ball.gameObject, user.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (user.CurrentGun == null) ? 0f : user.CurrentGun.CurrentAngle), true);
            Projectile component = gameObject.GetComponent<Projectile>();
            if (component != null)
            {
                component.Owner = user;
                component.Shooter = user.specRigidbody;
            }
            //ball.OnHitEnemy += this.OnHitEnemy;
        }
        public float elapsed;
        public bool hitenemy;

        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            ETGModConsole.Log($"hit");
            //enemy.aiActor.behaviorSpeculator.Stun(elapsed + 4f, true);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

        }
        /*public override bool CanBeUsed(PlayerController user)
        {

        }*/

        /*public override DebrisObject Drop(PlayerController player)
        {
            Tools.Print($"Player dropped {this.DisplayName}");
            return base.Drop(player);
        }*/
    }
}