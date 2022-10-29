using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;


// THINGS FOR WHEN BETTER AT CODE: Wind up time?, Extra sound stuff (spin up and spin off)
namespace ExampleMod
{
    public class Sasha : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Sasha", "sasha");
            Game.Items.Rename("outdated_gun_mods:sasha", "qad:sasha");
            gun.gameObject.AddComponent<Sasha>();
            
            //Gun descriptions
            gun.SetShortDescription("Don't Touch My Gun");
            gun.SetLongDescription("Being the first minigun to ever mow down hordes of people in heavy's hands, it continues to be " +
                "used to this day, exactly how it was before, nothing changed at all.\n\nFamously known to weigh one-fifty kilograms and " +
                "cost 400,000 dollars to fire for twelve seconds. Fortunately, as most of the Gungeon's ammo is made of magic, most " +
                "of it can be created perfectly and instantly without costing a fortune.");
            
            // Sprite setup
            gun.SetupSprite(null, "sasha_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 8); //Do i even need this

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.1f;
            gun.DefaultModule.cooldownTime = 0.07f;
            gun.DefaultModule.numberOfShotsInClip = 400;
            gun.DefaultModule.angleVariance = 15f;
            gun.SetBaseMaxAmmo(500);
            gun.gunClass = GunClass.FULLAUTO;
            gun.barrelOffset.transform.localPosition += new Vector3(0.75f, 0.125f, 0);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(86) as Gun).muzzleFlashEffects;
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "sasha";
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SMALL_BULLET;

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 4f;
            projectile.baseData.speed = 25f;
            projectile.baseData.range = 20f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_minigun_shoot", gameObject);
        }
        private bool HasReloaded;
        public override void Update()
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
                AkSoundEngine.PostEvent("Stop_SDB_All", base.gameObject);
                base.OnReloadPressed(player, gun, bSOMETHING);
            }
        }
    }
}
