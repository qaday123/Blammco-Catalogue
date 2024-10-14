using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Alexandria.Misc;

/* NOTES:
 *
*/
namespace TF2Stuff
{
    public class StockRocket : AdvancedGunBehavior
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:eagle";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("The Eagle", "eagle");
            Game.Items.Rename("outdated_gun_mods:the_eagle", consoleID);
            gun.gameObject.AddComponent<StockRocket>();
            
            //Gun descriptions
            gun.SetShortDescription("Maggots!");
            gun.SetLongDescription("Shoots 4 rockets in one clip. Boy these guns make even less sense than some of the ones " +
                "native to the gungeon!\n\nBeing as old and as used as it is, it's not as effective as a rocket launcher as you'd " +
                "think. But it still works, so in that case... last one alive locks the doors!");
            
            // Sprite setup
            gun.SetupSprite(null, "eagle_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 2.2f;
            gun.DefaultModule.cooldownTime = 0.9f;
            gun.DefaultModule.numberOfShotsInClip = 4;
            gun.SetBaseMaxAmmo(120);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 10f;
            projectile.baseData.speed = 22f;
            projectile.baseData.range = 50f;
            projectile.baseData.force = 10f;
            gun.barrelOffset.transform.localPosition += new Vector3(12f/16f,10f/16f);
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);

            Projectile baseProjectile = (PickupObjectDatabase.GetById(39) as Gun).DefaultModule.projectiles[0];

            projectile.ParticleTrail = baseProjectile.ParticleTrail;
            projectile.TrailRenderer = baseProjectile.TrailRenderer;
            projectile.CustomTrailRenderer = baseProjectile.CustomTrailRenderer;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("stock_rocket",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipempty");
            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = rocketexplosion;
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
            damageRadius = 4f,
            pushRadius = 4.5f,
            damage = 10f,
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
            doScreenShake = false,
            playDefaultSFX = true,
            effect = genericLargeExplosion.effect,
            ignoreList = genericLargeExplosion.ignoreList,
            ss = genericLargeExplosion.ss,
        };
    }
}
