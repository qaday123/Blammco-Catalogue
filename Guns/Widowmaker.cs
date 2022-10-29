using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.IO;
using JetBrains.Annotations;
using HutongGames.PlayMaker;

/* NOTES: 
 * Everything works, which is great - clip refills automatically
 * The fix for reloading is a little clunky, but isn't that noticable (you can see the flash of the clip changing)
 * Small bug, whenever you gain ammo, clip size increases with it - great (though it decides to update it whenever which is not good) 
    - but when you decrease the ammo, the clip size will not change at all. dunno how to fix
    UPDATE: the clip size does actually work both ways but it requires changing guns
    UPDATE UPDATE: tis been fixed! It actually works!!
 * Debate on whether to make A tier (8 damage) or B tier (5 damage) for balancing purposes
*/
namespace ExampleMod
{
    public class Widowmaker : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Widowmaker", "widow");
            Game.Items.Rename("outdated_gun_mods:widowmaker", "qad:widowmaker");
            gun.gameObject.AddComponent<Widowmaker>();
            
            //Gun descriptions
            gun.SetShortDescription("Scrapbooking");
            gun.SetLongDescription("Bullets that hit enemies will get recycled back into your clip. Aim true!" +
                "\n\nThe art of rewarding good aim and immediately screwing over those with none is in the true nature " +
                "of the Gungeon, so it'll sure fit right in!");
            
            // Sprite setup
            gun.SetupSprite(null, "widow_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);

            // gun setup
            gun.reloadTime = 0f;
            gun.SetBaseMaxAmmo(24);
            gun.gunClass = GunClass.SHOTGUN;

            for (int i = 0; i < 6; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            }
            foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                projectileModule.ammoCost = 4;
                projectileModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                projectileModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                projectileModule.cooldownTime = 0.625f;
                projectileModule.numberOfShotsInClip = 6;
                projectileModule.angleVariance = 18f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 5f;
                projectile.baseData.speed *= 1f;
                projectile.baseData.range = 50f;
                projectile.baseData.force = 6f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.encounterTrackable.EncounterGuid = "widowmaker";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.DecreaseFinalSpeedPercentMin = -25f;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.barrelOffset.transform.localPosition += new Vector3(0, 0.375f, 0);
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            gun.carryPixelOffset += new IntVector2(4,-1);
            //gun.transform.position += new Vector3(2f, 0.5f, 0f);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += this.OnHitEnemy;
            base.PostProcessProjectile(projectile);
            //ETGModConsole.Log("Proj hit");
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            gun.GainAmmo(1);
            gun.ForceImmediateReload();
            gun.ClipShotsRemaining = gun.CurrentAmmo / 4;
            //ETGModConsole.Log($"basemaxammo: {gun.GetBaseMaxAmmo()}");
            //ETGModConsole.Log($"adjustedammo: {gun.AdjustedMaxAmmo}");
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            if (UnityEngine.Random.value < 0.33f) { AkSoundEngine.PostEvent("Play_widowmaker_shot_01", gameObject); }
            else if (UnityEngine.Random.value < 0.66f) { AkSoundEngine.PostEvent("Play_widowmaker_shot_02", gameObject); }
            else { AkSoundEngine.PostEvent("Play_widowmaker_shot_03", gameObject); }
            //ETGModConsole.Log($"clipcapacity: {gun.AdditionalClipCapacity}");
        }
        private bool HasReloaded;
        public override void Update()
        {
            if (gun.CurrentOwner)
            {
                maxammo = gun.AdjustedMaxAmmo;
                gun.AdditionalClipCapacity = (gun.AdjustedMaxAmmo - gun.GetBaseMaxAmmo()) / 4;
                gun.ClipShotsRemaining = gun.CurrentAmmo / 4;
                if (maxammo != old_maxammo)
                {
                    PlayerController player = gun.GunPlayerOwner();
                    player.stats.RecalculateStats(player, true, false);
                    old_maxammo = maxammo;
                }
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
                //gun.SpawnShellCasingAtPosition(new Vector3(0f, 0f, 0f));
            }
        }
        public int maxammo;
        public int old_maxammo = 24;
        public static int ID;
    }
}
