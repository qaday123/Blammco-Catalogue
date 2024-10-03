using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using Alexandria.SoundAPI;
using Alexandria.BreakableAPI;


/*
 *  
*/
namespace TF2Stuff
{
    public class Nailgun : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Nail Gun", "nail");
            Game.Items.Rename("outdated_gun_mods:nail_gun", "qad:nailgun");
            gun.gameObject.AddComponent<Nailgun>();
            
            //Gun descriptions
            gun.SetShortDescription("Construction Not On The Agenda");
            gun.SetLongDescription("It's a nail gun, but... slightly modified?");
            
            // Sprite setup
            gun.SetupSprite(null, "nail_idle_001", 20);
            gun.SetAnimationFPS(gun.shootAnimation, 32);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(26) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(26) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 25;
            gun.DefaultModule.angleVariance = 6f;
            gun.SetBaseMaxAmmo(500);
            gun.gunClass = GunClass.SHITTY;
            gun.barrelOffset.transform.localPosition += new Vector3(5f/16f, 9f/16f, 0);
            gun.carryPixelOffset = new IntVector2(3, 0);
            gun.clipObject = BreakableAPIToolbox.GenerateDebrisObject("TF2Items/Resources/Debris/nailgun_clip.png", true, 1, 5, 60, 20, null, 1, DebrisBounceCount: 1).gameObject;
            gun.clipsToLaunchOnReload = 1;

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_nailgun", "Play_WPN_Gun_Shot_01", "Play_WPN_nailgun_shot_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_nailgun", "Play_WPN_Gun_Reload_01", "Play_pistol_clipin");
            gun.gunSwitchGroup = "qad_nailgun";

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
   
            // More projectile setup
            projectile.baseData.damage = 3f;
            projectile.baseData.speed = 26f;
            projectile.baseData.range = 30f;
            projectile.baseData.force = 8f;
            projectile.transform.parent = gun.barrelOffset;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
    }
}
