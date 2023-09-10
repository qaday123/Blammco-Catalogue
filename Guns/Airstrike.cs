using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using System.Linq;
using System.ComponentModel;

/* NOTES:
 * Am failing hard pls work this out
*/
namespace ExampleMod
{
    public class Airstrike : AdvancedGunBehavior
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("The Air Strike", "airstrike");
            Game.Items.Rename("outdated_gun_mods:the_air_strike", "qad:air_strike");
            gun.gameObject.AddComponent<Airstrike>();
            
            //Gun descriptions
            gun.SetShortDescription("Don't Confuse It For The Item");
            gun.SetLongDescription("Fire a barrage of rockets into the air which shortly land at the required target! They certainly " +
                "won't see what's coming now!");
            
            // Sprite setup
            gun.SetupSprite(null, "airstrike_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);

            // Projectile setup
            //gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.6f;
            gun.DefaultModule.cooldownTime = 0.28f;
            gun.DefaultModule.numberOfShotsInClip = 4;
            gun.SetBaseMaxAmmo(120);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 15f;
            projectile.baseData.speed = 40f;
            projectile.baseData.range = 0.1f;
            projectile.baseData.force = 0f;
            gun.barrelOffset.transform.localPosition += new Vector3(12f/16f,10f/16f);
            gun.barrelOffset.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("stock_rocket",
                "ExampleMod/Resources/CustomGunAmmoTypes/rocket/rocket_clipfull",
                "ExampleMod/Resources/CustomGunAmmoTypes/rocket/rocket_clipempty");
            //ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            SkyRocket rocket = projectile.sprite.gameObject.GetOrAddComponent<SkyRocket>();
            rocket.AscentTime = 0.4f;
            rocket.DescentTime = 0.6f;
            rocket.Variance = 0f;//*/
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_rocket_shoot", gameObject);
        }
        private bool HasReloaded;
        protected override void Update()
        {
            if (gun.CurrentOwner)
            {

                if (!gun.PreventNormalFireAudio)
                {
                    this.gun.PreventNormalFireAudio = true;
                }
                if (!gun.IsReloading && !HasReloaded)
                {
                    this.HasReloaded = true;
                }
            }
        }
        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            GameObject Rocket;
            var cm = UnityEngine.Object.Instantiate<GameObject>((GameObject)BraveResources.Load("Global Prefabs/_ChallengeManager", ".prefab"));
            Rocket = (cm.GetComponent<ChallengeManager>().PossibleChallenges.Where(c => c.challenge is SkyRocketChallengeModifier).First().challenge as SkyRocketChallengeModifier).Rocket;
            UnityEngine.Object.Destroy(cm);
            SkyRocket component = SpawnManager.SpawnProjectile(Rocket, Vector3.zero, Quaternion.identity, true).gameObject.GetOrAddComponent<SkyRocket>();
            component.SpawnObject = projectile.gameObject;
            component.ExplosionData = rocketexplosion;
            component.TargetVector2 = projectile.PossibleSourceGun.GunPlayerOwner().unadjustedAimPoint;
            component.AscentTime = 0.2f;
            component.DescentTime = 0.4f;
            component.Variance = 0f;//*/
            //SkyRocket rocket = projectile.sprite.gameObject.GetOrAddComponent<SkyRocket>();
            //rocket.TargetVector2 = projectile.PossibleSourceGun.GunPlayerOwner().unadjustedAimPoint;
        }
        public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
        {
            if (gun.IsReloading && this.HasReloaded)
            {
                HasReloaded = false;
                AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
                base.OnReloadPressed(player, gun, bSOMETHING);
                AkSoundEngine.PostEvent("Play_rocket_reload", base.gameObject);
            }
        }
        public static ExplosionData rocketexplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 2f,
            pushRadius = 2.5f,
            damage = 20f,
            doDamage = true,
            damageToPlayer = 0,
            secretWallsRadius = 6f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 4f,
            debrisForce = 8f,
            preventPlayerForce = false,
            explosionDelay = 0.1f,
            usesComprehensiveDelay = false,
            comprehensiveDelay = 0f,
            doScreenShake = true,
            playDefaultSFX = true,
            effect = genericSmallExplosion.effect,
            ignoreList = genericSmallExplosion.ignoreList,
            ss = genericSmallExplosion.ss,
        };
    }
}
