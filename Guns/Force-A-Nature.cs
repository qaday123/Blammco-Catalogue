using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;


// Done for now, but for future: Player Knockback?; Shoot enemies into each other, causing damage?
// Frost and Gunfire has force-a-nature. question life choices.
namespace ExampleMod

{
    public class ForceANature : GunBehaviour
    {
        public static void Add()
        {


            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Force A Nature", "foanat");
            Game.Items.Rename("outdated_gun_mods:force_a_nature", "qad:force_a_nature");
            gun.gameObject.AddComponent<ForceANature>();
            
            //Gun descriptions
            gun.SetShortDescription("Pushover");
            gun.SetLongDescription("Pushes enemies back. A lot.\n\n" +
                "Being annoying is this gun's prime. Not to the weilder, of course, but to whoever is at the other end of the barrel. "+
                "Renowned by many for high burst damage and funny ragdolls, and hated by others.");
            
            // Sprite setup
            gun.SetupSprite(null, "foanat_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 18);
            gun.SetAnimationFPS(gun.reloadAnimation, 8);

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
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "FaN";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.Volley.DecreaseFinalSpeedPercentMin = -30f;
            gun.barrelOffset.transform.localPosition += new Vector3(0, 0.125f, 0);
            ID = gun.PickupObjectId;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD

            ETGMod.Databases.Items.Add(gun, false, "ANY");
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
