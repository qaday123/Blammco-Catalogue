using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;


/* NOTES:
 * how tf do you do the active charge thingy
*/
namespace TF2Stuff

{
    public class SodaPopper : GunBehaviour
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:soda_popper";

            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Soda Popper", "sodapop");
            Game.Items.Rename("outdated_gun_mods:soda_popper", consoleID);
            gun.gameObject.AddComponent<SodaPopper>();
            
            //Gun descriptions
            gun.SetShortDescription("Hype Hype Hype");
            gun.SetLongDescription("Like the Force-A-Nature's less arrogant, more competent sibling.\n\n" +
                "More consistent, less shove-y and reloads faster. It also stores some mysterious energy that can get " +
                "released once reloading at full... (heads up: this doesn't work yet, so don't try it)\n\nI'm not even winded!");
            
            // Sprite setup
            gun.SetupSprite(null, "sodapop_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 20);
            gun.SetAnimationFPS(gun.reloadAnimation, 14);
            gun.TrimGunSprites();

            // gun setup
            gun.reloadTime = 0.8f;
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.SHOTGUN;
            gun.UsesRechargeLikeActiveItem = true;
            gun.ActiveItemStyleRechargeAmount = 0f;

            for (int i = 0; i < 5; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            }
            foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                //System.Random randspeed = new System.Random();
                //float speed = randspeed.Next(20, 31);
                projectileModule.ammoCost = 1;
                projectileModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                projectileModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                projectileModule.cooldownTime = 0.3125f;
                projectileModule.numberOfShotsInClip = 2;
                projectileModule.angleVariance = 15f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 5.5f;
                projectile.baseData.speed = 26f;
                projectile.baseData.range = 9f;
                projectile.baseData.force = 4f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }
            
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.Volley.DecreaseFinalSpeedPercentMin = -30f;
            gun.barrelOffset.transform.localPosition += new Vector3(5f/16f, 5f/16f, 0);
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.carryPixelOffset += new IntVector2(4,1);
            gun.shellCasing = (PickupObjectDatabase.GetById(202) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 2;
            gun.reloadShellLaunchFrame = 3;
            gun.gunScreenShake = new(0.4f, 14f, 0.09f, 0.009f);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public float _hype = 0;
        public const float MAX_HYPE = 350; 
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            //currentcharge = gun.RemainingActiveCooldownAmount;
            //gun.ClearCooldowns();
            AkSoundEngine.PostEvent("Play_scatter_gun_double_shoot", gameObject);
            //gun.RemainingActiveCooldownAmount = currentcharge;
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
                base.OnReloadPressed(player, gun, bSOMETHING);
                AkSoundEngine.PostEvent("Play_scatter_gun_double_tube_close", base.gameObject);
                //gun.SpawnShellCasingAtPosition(new Vector3(0f, 0f, 0f));
            }
        }
        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            base.OnPlayerPickup(playerOwner);
        }
        public override void OnDropped()
        {
            base.OnDropped();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
