using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;

/* NOTES:
 *
*/
namespace TF2Stuff
{
    public class LibertyLauncher : AdvancedGunBehavior
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:liberty_launcher";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Liberty Launcher", "liberty");
            Game.Items.Rename("outdated_gun_mods:liberty_launcher", consoleID);
            gun.gameObject.AddComponent<LibertyLauncher>();
            
            //Gun descriptions
            gun.SetShortDescription("Bells and Whistles");
            gun.SetLongDescription("Fire fast fancy flying rockets at foes to fling them far out into familiar family faces to also " +
                "fracture their facade. An absolute staple of modern military equipment!");
            
            // Sprite setup
            gun.SetupSprite(null, "liberty_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 2f;
            gun.DefaultModule.cooldownTime = 0.9f;
            gun.DefaultModule.numberOfShotsInClip = 5;
            gun.SetBaseMaxAmmo(150);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE
            gun.carryPixelOffset += new IntVector2(0, 2);

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
            projectile.baseData.damage = 25f;
            projectile.baseData.speed = 32f;
            projectile.baseData.range = 50f;
            projectile.baseData.force = 50f;
            gun.barrelOffset.transform.localPosition += new Vector3(12f/16f,14f/16f);
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("stock_rocket",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipempty");
            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = rocketexplosion;
            ID = gun.PickupObjectId;
            KilledEnemiesBecomeProjectileModifier casey = projectile.gameObject.GetOrAddComponent<KilledEnemiesBecomeProjectileModifier>();

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "Play_rocket_shoot";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[0].eventAudio = "Play_rocket_reload";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[0].triggerEvent = true;
        }
        public static int ID;
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
            }
        }
        public static ExplosionData rocketexplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 3f,
            pushRadius = 3.5f,
            damage = 10f,
            doDamage = true,
            damageToPlayer = 0,
            secretWallsRadius = 6f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 20f,
            debrisForce = 25f,
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
