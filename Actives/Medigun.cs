using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Steamworks;
using System.Collections;

/* NOTES:
 * 
*/
namespace TF2Stuff
{
    public class Medigun : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Medigun";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/actives/medigun_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Medigun>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 400f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Fatal Overdose";
            string longDesc = "Turns out combining piss, medicine, and gunpowder doesn't bode well for the gundead. So the medigun " +
                "was modified to shoot healing bullets that make enemies explode instead.\n\nAll it took was a little tinkering " +
                "to make the medigun more 'gun' like. Now it's a medicine shotgun of doom! \nThanks for @Bird Boi#0549 for coming " +
                "up with the idea";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.C;
            ID = item.PickupObjectId;

            medibullets = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(56) as Gun).DefaultModule.projectiles[0]);
            medibullets.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(medibullets.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(medibullets);
            medibullets.baseData.damage = 10f; //0f;
            medibullets.baseData.speed = 28f;
            medibullets.baseData.range = 1000f;
            medibullets.collidesWithEnemies = true;
            medibullets.collidesWithPlayer = false;
            medibullets.collidesWithProjectiles = false;
            medibullets.pierceMinorBreakables = true;
            BounceProjModifier Bouncing = medibullets.gameObject.GetOrAddComponent<BounceProjModifier>();
            Bouncing.numberOfBounces += 3;
            medibullets.SetProjectileSpriteRight("medibullet", 20, 7, false, tk2dBaseSprite.Anchor.MiddleCenter, 18, 5);
        }
        private static Projectile medibullets;
        public static int ID;
        public override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);
            AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
            user.healthHaver.ApplyHealing(1);
            AkSoundEngine.PostEvent("Play_WPN_blunderbuss_shot_01", gameObject);
            for (int i = -4; i < 4; i++)
            {
                if (user != null)
                {
                    GameObject gameObject = SpawnManager.SpawnProjectile(Medigun.medibullets.gameObject, user.specRigidbody.UnitCenter + new Vector2(0f, 0.125f), Quaternion.Euler(0, 0, (user.CurrentGun == null) ? 0f : user.CurrentGun.CurrentAngle + i * 6));
                    Projectile projectile = medibullets.gameObject.GetComponent<Projectile>();
                    if (projectile != null)
                    {
                        projectile.Owner = user;
                        projectile.Shooter = user.specRigidbody;
                        user.DoPostProcessProjectile(projectile);
                        //projectile.OnHitEnemy += this.OnHitEnemy;
                    }
                }
            }
        }
        public void PostProcessProjectile(Projectile projectile)
        {
            //projectile.OnHitEnemy += this.OnHitEnemy;
            //base.PostProcessProjectile(projectile);
            ETGModConsole.Log("Proj hitst process'd");
        }

        /*IEnumerator MediShoot(PlayerController user)
        {
            AkSoundEngine.PostEvent("Play_WPN_blunderbuss_shot_01", gameObject);
            for (int i = -4; i < 4; i++)
            {
                if (user != null)
                {
                    GameObject gameObject = SpawnManager.SpawnProjectile(Medigun.medibullets.gameObject, user.specRigidbody.UnitCenter + new Vector2(0f,0.125f), Quaternion.Euler(0, 0, (user.CurrentGun == null) ? 0f : user.CurrentGun.CurrentAngle + i * 6));
                    Projectile projectile = medibullets.gameObject.GetComponent<Projectile>();
                    if (projectile != null)
                    {
                        projectile.Owner = user;
                        projectile.Shooter = user.specRigidbody;
                        user.DoPostProcessProjectile(projectile);
                        projectile.OnHitEnemy += this.OnHitEnemy;
                    }
                }
            }
            yield break;
        }*/
        public void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            ETGModConsole.Log("Enemy hit");
            if (enemy && enemy.healthHaver && !fatal)
            {
                enemy.healthHaver.ApplyHealing(8f);
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

        }
        /*public override bool CanBeUsed(PlayerController user)
        {
            return user.healthHaver.GetCurrentHealth() != user.healthHaver.GetMaxHealth();
        }*/

        /*public override DebrisObject Drop(PlayerController player)
        {
            Tools.Print($"Player dropped {this.DisplayName}");
            return base.Drop(player);
        }*/
    }
}