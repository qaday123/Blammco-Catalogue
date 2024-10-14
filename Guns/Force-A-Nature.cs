using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;


// Done for now, but for future: Player Knockback?; Shoot enemies into each other, causing damage?
// Frost and Gunfire has force-a-nature. question life choices.
namespace TF2Stuff

{
    public class ForceANature : GunBehaviour
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:force_a_nature";

            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Force A Nature", "foanat");
            Game.Items.Rename("outdated_gun_mods:force_a_nature", consoleID);
            gun.gameObject.AddComponent<ForceANature>();
            
            //Gun descriptions
            gun.SetShortDescription("Pushover");
            gun.SetLongDescription("Pushes enemies back. A lot. Also pushes you back, a bit less.\n\n" +
                "Being annoying is this gun's prime. Not to the weilder, of course, but to whoever is at the other end of the barrel. "+
                "Renowned by many for high burst damage and funny ragdolls, and hated by others.");
            
            // Sprite setup
            gun.SetupSprite(null, "foanat_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 18);
            gun.SetAnimationFPS(gun.reloadAnimation, 8);
            gun.TrimGunSprites();

            // gun setup
            gun.reloadTime = 1f;
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.SHOTGUN;

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
                projectile.baseData.damage = 4f;
                projectile.baseData.speed = 26f;
                projectile.baseData.range = 7.5f;
                projectile.baseData.force = 30f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                projectile.AppliesKnockbackToPlayer = true;
                projectile.PlayerKnockbackForce = 15f;
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.Volley.DecreaseFinalSpeedPercentMin = -30f;
            gun.barrelOffset.transform.localPosition += new Vector3(4f/16f, 6f/16f, 0);
            gun.carryPixelOffset = new IntVector2(4, 1);
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.shellCasing = (PickupObjectDatabase.GetById(202) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 2;
            gun.reloadShellLaunchFrame = 2;
            gun.gunScreenShake = new(0.6f, 12f, 0.09f, 0.009f);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_scatter_gun_double_shoot", gameObject);
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
    }
}
