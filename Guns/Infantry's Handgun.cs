using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;

namespace ExampleMod
{
    public class InfantryHandgun : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Infantry's Handgun", "infantry");
            Game.Items.Rename("outdated_gun_mods:infantry's_handgun", "qad:infantry's_handgun");
            gun.gameObject.AddComponent<InfantryHandgun>();
            
            //Gun descriptions
            gun.SetShortDescription("American");
            gun.SetLongDescription("It's old, beaten, rusty, and doesn't even fire normally. Yet Soldier refused to use any other, " +
                "at least usable weapon as a starter because they all weren't \"American\" according to him.\n\nNobody actually knows " +
                "what that means except for the band of mercenaries who arrived here. They all looked really annoyed when asked, so " +
                "they weren't bothered any further but it sounds like a planet. Quite an old one at that.");
            
            // Sprite setup
            gun.SetupSprite(null, "infantry_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            gun.SetAnimationFPS(gun.reloadAnimation, 18);

            // Projectile setup
            gun.AddProjectileModuleFrom("ak-47", true, false);
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.9f;
            gun.SetBaseMaxAmmo(300);
            gun.InfiniteAmmo = true;
            gun.gunClass = GunClass.SHITTY; //when as starting weapon reclass to 'SHITTY'
            gun.CanBeDropped = false;
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 1;
            gun.clipObject = (PickupObjectDatabase.GetById(86) as Gun).clipObject;
            gun.clipsToLaunchOnReload = 1;

            
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.SPECIAL;
            gun.encounterTrackable.EncounterGuid = "Infantry's Handgun";
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.PlaceItemInAmmonomiconAfterItemById(88);

            // More projectile setup
            for (int i = 0; i < 2; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            }
            foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                //System.Random randspeed = new System.Random();
                //float speed = randspeed.Next(4, 28);
                projectileModule.ammoCost = 1;
                projectileModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                projectileModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                projectileModule.cooldownTime = 0.25f;
                projectileModule.numberOfShotsInClip = 7;
                projectileModule.angleVariance = 10f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 2f;
                projectile.baseData.speed *= 0.8f; //speed;
                projectile.baseData.range = 14f;
                projectile.baseData.force = 5f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) { projectileModule.ammoCost = 0; }
            }

            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.DecreaseFinalSpeedPercentMin = -20f;
            gun.Volley.IncreaseFinalSpeedPercentMax = 20f;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            gun.barrelOffset.transform.localPosition += new Vector3(4f / 16f, 8f / 16f);
            gun.carryPixelOffset += new IntVector2(2, 0);
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_WPN_m1911_shot_01", gameObject);
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
                AkSoundEngine.PostEvent("Play_WPN_m1911_reload_01", base.gameObject);
            }
        }
    }
}
