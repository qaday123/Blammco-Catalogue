using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;

namespace ExampleMod
{
    public class TheAgent : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("The Agent", "agent");
            Game.Items.Rename("outdated_gun_mods:the_agent", "qad:agent");
            gun.gameObject.AddComponent<TheAgent>();

            //Gun descriptions
            gun.SetShortDescription("Gentlemen");
            gun.SetLongDescription("This rusty, old fashioned revolver belongs to a mysterious figure who doesn't like to " +
                "conform to the conventions of the Gungeon.\n\nLightweight, and packs a punch. Perfect in espionage for those " +
                "moments where you're found out.");
            
            // Sprite setup
            gun.SetupSprite(null, "agent_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 6);

            // Projectile setup
            gun.AddProjectileModuleFrom("ak-47", true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.9f;
            gun.DefaultModule.cooldownTime = 0.5f;
            gun.DefaultModule.numberOfShotsInClip = 6;
            gun.DefaultModule.angleVariance = 5f;
            gun.SetBaseMaxAmmo(200);
            gun.InfiniteAmmo = false;
            gun.gunClass = GunClass.PISTOL;
            
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;
            gun.encounterTrackable.EncounterGuid = "spy revolver";

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 10f; //when as starting weapon remodify to 3-4 damage + others
            projectile.baseData.speed = 22f;
            projectile.baseData.range = 30f;
            projectile.baseData.force = 10f;
            projectile.AdditionalScaleMultiplier = 1.25f;
            projectile.transform.parent = gun.barrelOffset;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BULLET;
            gun.barrelOffset.transform.localPosition += new Vector3(0.5f, 0.75f, 0);
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 6;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_revolver_shoot", gameObject);
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
                AkSoundEngine.PostEvent("Play_revolver_reload", base.gameObject);
            }
        }
    }
}
