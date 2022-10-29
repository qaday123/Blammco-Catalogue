using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;

/* NOTES:
 * Actually feels good to use, thanks AI for giving feedback on the animation :)
 * Idk whether to make the dodge roll effect only happen while holding it or all the time
 * Balloon gun synergy - winger fires balloon gun projectiles, and balloon gun has the dodge roll boosts
 * OnPickedUpByPlayer currently doesn't work. UPDATE: turns out i dont need that method to achieve what i wanted to do
*/
namespace ExampleMod
{
    public class Winger : AdvancedGunBehavior
    {
        public static void Add()
        {


            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Winger", "winger");
            Game.Items.Rename("outdated_gun_mods:winger", "qad:winger");
            gun.gameObject.AddComponent<Winger>();
            
            //Gun descriptions
            gun.SetShortDescription("On The Fly");
            gun.SetLongDescription("Doesn't make you fly, but can bring you close to it.\n\n" +
                "A favoured choice in an arsenal that requires getting close to people. It can give you the edge juuuust in case " +
                "a battle's a bit too close. Or if you're bad at jumping over pits.");
            
            // Sprite setup
            gun.SetupSprite(null, "winger_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 24);
            gun.SetAnimationFPS(gun.reloadAnimation, 16);

            // Projectile setup
            gun.AddProjectileModuleFrom("ak-47", true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.1f;
            gun.DefaultModule.cooldownTime = 0.15f;
            gun.DefaultModule.numberOfShotsInClip = 5;
            gun.SetBaseMaxAmmo(250);
            gun.AddCurrentGunStatModifier(PlayerStats.StatType.DodgeRollDistanceMultiplier, 1.25f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            gun.AddCurrentGunStatModifier(PlayerStats.StatType.DodgeRollSpeedMultiplier, 1.1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            gun.gunClass = GunClass.PISTOL;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "The Winger";
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 9f;
            projectile.baseData.speed = 20f;
            projectile.baseData.range = 30f;
            projectile.baseData.force = 15f;
            projectile.AdditionalScaleMultiplier = 1.25f;
            projectile.transform.parent = gun.barrelOffset;
            
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_winger_shoot", gameObject);
        }
        private bool HasReloaded;
        protected override void Update()
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
        /*
        private void ChangedGun(Gun oldGun, Gun newGun, bool whatthisdo)
        {
            ETGModConsole.Log("Guns changed");
            StatChange(newGun);
        }
        private void StatChange(Gun currentgun)
        {
            currentgun.RemoveStatFromGun(PlayerStats.StatType.DodgeRollDistanceMultiplier);
            currentgun.RemoveStatFromGun(PlayerStats.StatType.DodgeRollSpeedMultiplier);
            if (currentgun == this.gun)
            {
                currentgun.AddStatToGun(PlayerStats.StatType.DodgeRollDistanceMultiplier, 1.25f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                currentgun.AddStatToGun(PlayerStats.StatType.DodgeRollSpeedMultiplier,1.1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                ETGModConsole.Log("Multiplier added successfully");
            }
            PlayerController gunplayer = gun.GunPlayerOwner();
            gunplayer.stats.RecalculateStats(gunplayer, true, false);
        }
    
        protected override void OnPickedUpByPlayer(PlayerController player)
        {
            ETGModConsole.Log("OnPickup Was Triggered");
            base.OnPickedUpByPlayer(player);
            player.GunChanged += this.ChangedGun;
            StatChange(player.CurrentGun);
            player.stats.RecalculateStats(player, true, false);
        }
        protected override void OnPostDroppedByPlayer(PlayerController player)
        {
            player.GunChanged -= this.ChangedGun;
            player.stats.RecalculateStats(player, true, false);
            StatChange(player.CurrentGun);
            base.OnPostDroppedByPlayer(player);
        }*/
        public Winger()
        {

        }
    }
}
