using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using BepInEx;


// THINGS FOR WHEN BETTER AT CODE: Wind up time?, Extra sound stuff (spin up and spin off)
namespace TF2Stuff
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
            gun.SetLongDescription("Powerful for its type and rarity. Takes time to rev up and slows you down while firing.\n\n " +
                "Being the first minigun to ever mow down hordes of people in Heavy's hands, it continues to be " +
                "used to this day, exactly how it was before, nothing changed at all.\n\nFamously known to weigh one-fifty kilograms and " +
                "cost 400,000 dollars to fire for twelve seconds. Fortunately, as most of the Gungeon's ammo is made of magic, most " +
                "of it can be created perfectly and instantly without costing a fortune.");
            
            // Sprite setup
            gun.SetupSprite(null, "sasha_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 8); //Do i even need this
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.1f;
            gun.DefaultModule.cooldownTime = 0.06f;
            gun.DefaultModule.numberOfShotsInClip = -1;
            gun.DefaultModule.angleVariance = 15f;
            gun.SetBaseMaxAmmo(600);
            gun.gunClass = GunClass.FULLAUTO;
            gun.barrelOffset.transform.localPosition += new Vector3(0.75f, 0.125f, 0);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(86) as Gun).muzzleFlashEffects;
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 1;
            gun.usesContinuousFireAnimation = true;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).loopStart = 9;
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);
            gun.gunScreenShake = new(0.2f, 8f, 0.04f, 0.004f);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(478) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SMALL_BULLET;

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            rev = gun.gameObject.GetOrAddComponent<GunRevDoer>(); // custom :D
            rev.RevTime = 0.75f;
            rev.FireLoopStartIndex = 9;
            rev.SetAudioMessages(start: "minigun_wind_up", end: "minigun_wind_down", revLoop: "minigun_spin", shootLoop: "minigun_shoot");
            rev.SlowDownMultiplier = 0.6f;

            // More projectile setup
            projectile.baseData.damage = 5f;
            projectile.baseData.speed = 25f;
            projectile.baseData.range = 20f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;

            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_sasha", "Play_WPN_Gun_Shot_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_sasha", "Play_WPN_Gun_Reload_01");
            gun.gunSwitchGroup = "qad_sasha";

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            
        }
        public static int ID;
        private bool HasReloaded;
        public static GunRevDoer rev;
    }
}
