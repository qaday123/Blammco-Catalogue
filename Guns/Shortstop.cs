using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;

/* NOTES:
 * Synergy with something stupid (Klobbe?) to add the shove
*/
namespace ExampleMod
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
                "with a couple of shots to the back, you can get it over quick and easy. That is, as long as you like to be " +
                "close to that back. Otherwise, it's just a worse pistol.");
            
            // Sprite setup
            gun.SetupSprite(null, "shortstop_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            gun.SetAnimationFPS(gun.reloadAnimation, 8);

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
                proj.angleVariance = 1f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(proj.projectiles[0]);
                proj.projectiles[0] = projectile;
                projectile.baseData.speed = 20f;
                if (iterator == 1)
                {
                    proj.angleFromAim = 10f;
                    projectile.baseData.damage = 5f;
                    projectile.baseData.range = 30f;
                    projectile.baseData.force = 5f;
                }
                if (iterator == 2)
                {
                    proj.angleFromAim = -10f;
                    projectile.baseData.damage = 5f;
                    projectile.baseData.range = 30f;
                    projectile.baseData.force = 5f;
                }
                if (iterator == 3)
                {
                    proj.positionOffset += new Vector3(0f,0.25f,0f); 
                    projectile.baseData.damage = 8f;
                    projectile.baseData.range = 3.5f;
                    projectile.baseData.force = 14f;
                }
                if (iterator == 4)
                {
                    proj.positionOffset += new Vector3(0f, -0.25f, 0f);
                    projectile.baseData.damage = 8f;
                    projectile.baseData.range = 3.5f;
                    projectile.baseData.force = 14f;
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
                "ExampleMod/Resources/CustomGunAmmoTypes/shortstop/inverseshell_clipfull",
                "ExampleMod/Resources/CustomGunAmmoTypes/shortstop/inverseshell_clipempty");
            gun.Volley.UsesShotgunStyleVelocityRandomizer = false;
            gun.carryPixelOffset = new IntVector2(4, 1);
            gun.barrelOffset.transform.localPosition += new Vector3(4f/16f, 7f/16f, 0);
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 4;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }

        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_shortstop_shoot", gameObject);
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
                //AkSoundEngine.PostEvent("Stop_SDB_All", base.gameObject);
                base.OnReloadPressed(player, gun, bSOMETHING);
                AkSoundEngine.PostEvent("Play_shortstop_reload", base.gameObject);
                //gun.SpawnShellCasingAtPosition(new Vector3(0f, 0f, 0f));
            }
        }
    }
}
