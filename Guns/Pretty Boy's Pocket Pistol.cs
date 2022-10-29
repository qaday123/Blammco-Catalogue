using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;

namespace ExampleMod
{
    public class PocketPistol : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Pretty Boy's Pocket Pistol", "pocket");
            Game.Items.Rename("outdated_gun_mods:pretty_boy's_pocket_pistol", "qad:pbpp");
            gun.gameObject.AddComponent<PocketPistol>();
            
            //Gun descriptions
            gun.SetShortDescription("Little And Fierce");
            gun.SetLongDescription("Rapid firing pistol of doom. Brought to the Gungeon by a fast, arrogant mercenary.\n\n" +
                "This weapon used to be the smallest weapon known to man. And Scout loved to use it because killing people with the smallest "+
                "gun known to man was the best thing ever. Naturally outclassed (in lack of size) by other weapons in the Gungeon, the pistol no longer held that title.\n"+
                "Yet, this fierce little gun always held a place in his heart, and so he stuck with it.");
            
            // Sprite setup
            gun.SetupSprite(null, "pocket_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 24);
            gun.SetAnimationFPS(gun.reloadAnimation, 18);

            // Projectile setup
            gun.AddProjectileModuleFrom("ak-47", true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.7f;
            gun.DefaultModule.cooldownTime = 0.12f;
            gun.DefaultModule.numberOfShotsInClip = 9;
            gun.SetBaseMaxAmmo(300);
            gun.InfiniteAmmo = true;
            gun.gunClass = GunClass.SHITTY; //when as starting weapon reclass to 'SHITTY'
            gun.CanBeDropped = false;

            
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "PBPP";
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            gun.PlaceItemInAmmonomiconAfterItemById(88);

            // More projectile setup
            projectile.baseData.damage = 4f; //when as starting weapon remodify to 3-4 damage + others
            projectile.baseData.speed = 26f;
            projectile.baseData.range = 10f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            gun.barrelOffset.transform.localPosition += new Vector3(1f/16f, 11f/16f, 0);
            gun.carryPixelOffset += new IntVector2(3, 0);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_pistol_shoot", gameObject);
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
                AkSoundEngine.PostEvent("Play_pistol_clipin", base.gameObject);
            }
        }
    }
}
