using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using BepInEx;

/* NOTES:
 * Synergy with something stupid (Klobbe?) to add the shove
*/
namespace TF2Stuff
{
    public class Shortstop : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Shortstop", "shortstop");
            Game.Items.Rename("outdated_gun_mods:shortstop", "qad:shortstop");
            gun.gameObject.AddComponent<Shortstop>();
            
            //Gun descriptions
            gun.SetShortDescription("Handheld Shotgun");
            gun.SetLongDescription("Nobody ever questioned the validity of making a handgun shoot like a shotgun.\n\nBut now, " +
                "with a couple of shots to the back, you can get it over quick and easy. Just like the milkman said.");
            
            // Sprite setup
            gun.SetupSprite(null, "shortstop_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            gun.SetAnimationFPS(gun.reloadAnimation, 8);
            gun.TrimGunSprites();

            // gun setup
            gun.reloadTime = 1.4f;
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.PISTOL;

            for (int i = 0; i < 4; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            }
            int iterator = 0;
            foreach (ProjectileModule proj in gun.Volley.projectiles)
            {
                //System.Random randspeed = new System.Random();
                //float speed = randspeed.Next(20, 31);
                proj.ammoCost = 1;
                proj.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                proj.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                proj.cooldownTime = 0.36f;
                proj.numberOfShotsInClip = 4;
                proj.angleVariance = 0f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(proj.projectiles[0]);
                proj.projectiles[0] = projectile;
                projectile.baseData.speed = 20f;
                projectile.baseData.damage = 4f;
                projectile.baseData.range = 30f;
                projectile.baseData.force = 5f;
                switch(iterator)
                {
                    case 0:
                        proj.positionOffset += new Vector3(0.2f, 0.2f, 0);
                        break;
                    case 1:
                        proj.positionOffset += new Vector3(0.2f, -0.2f, 0);
                        break;
                    case 2:
                        proj.positionOffset += new Vector3(-0.2f, 0.2f, 0);
                        break;
                    case 3:
                        proj.positionOffset += new Vector3(-0.2f, -0.2f, 0);
                        break;
                }
                iterator++;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                bool flag = proj != gun.DefaultModule;
                if (flag)
                {
                    proj.ammoCost = 0;
                }
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;
            gun.encounterTrackable.EncounterGuid = "shortstop";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Inverse Shell",
                "TF2Items/Resources/CustomGunAmmoTypes/shortstop/inverseshell_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/shortstop/inverseshell_clipempty");
            gun.Volley.UsesShotgunStyleVelocityRandomizer = false;
            gun.carryPixelOffset = new IntVector2(4, 1);
            gun.barrelOffset.transform.localPosition += new Vector3(4f/16f, 9f/16f, 0);
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(53) as Gun).muzzleFlashEffects;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 4;
            gun.reloadShellLaunchFrame = 3;
            gun.gunScreenShake = new ScreenShakeSettings(0.5f, 8f, 0.06f, 0.006f);

            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_shortstop", "Play_WPN_Gun_Shot_01", "shortstop_shoot");
            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_shortstop", "Play_WPN_Gun_Reload_01", "shortstop_reload");
            gun.gunSwitchGroup = "qad_shortstop";

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }

        public static int ID;
    }
}
