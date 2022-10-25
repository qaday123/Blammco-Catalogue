using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.IO;
using JetBrains.Annotations;
using HutongGames.PlayMaker;

/* NOTES: 
 * Better player feedback on what the boost meter is at (come back when learn how vfx work/repurposed active charge on guns)
 * Error where if you drop the gun bad things occur (and if the dodge roll action is subscribed to it you can no loger dodge roll) - TEMP FIX INSTATIATED
*/
namespace ExampleMod
{
    public class BabyFaceBlaster : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Baby Face's Blaster", "babyface");
            Game.Items.Rename("outdated_gun_mods:baby_face's_blaster", "qad:baby_faces_blaster");
            gun.gameObject.AddComponent<BabyFaceBlaster>();
            
            //Gun descriptions
            gun.SetShortDescription("BAM! YOU'RE DEAD PAL!");
            gun.SetLongDescription("Hitting an enemy grants you a speed boost. This speed boost is carried across guns but is fully lost when you " +
                "take damage, and slightly lost when dodgerolling.\n\nThis gun makes its wielder more and more overconfident with every hit, " +
                "making them think they don't need a dodge roll to dodge things, hence, why all is lost when they're proved wrong.");
            
            // Sprite setup
            gun.SetupSprite(null, "babyface_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 14);
            gun.SetAnimationFPS(gun.reloadAnimation, 10);

            // gun setup
            gun.reloadTime = 1.1f;
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.SHOTGUN;

            for (int i = 0; i < 5; i++)
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
                projectileModule.cooldownTime = 0.6f;
                projectileModule.numberOfShotsInClip = 4;
                projectileModule.angleVariance = 14f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 5f;
                projectile.baseData.speed = 26f; //speed;
                projectile.baseData.range = 15f;
                projectile.baseData.force = 6f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "thespeedgun";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.DecreaseFinalSpeedPercentMin = -25f;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.barrelOffset.transform.localPosition += new Vector3(8f/16f, 10f/16f);
            ID = gun.PickupObjectId;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            //gun.transform.position += new Vector3(3f/16f, 10f/16f);
            gun.CanBeDropped = false; // gun breaks when dropped? make it undroppable

            ETGMod.Databases.Items.Add(gun, false, "ANY");
        }
        //if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += this.OnHitEnemy;
            base.PostProcessProjectile(projectile);
            //ETGModConsole.Log("Proj hit");
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (bullethits < 40)
            {
                bullethits++;
            }
            StatChange(proj.PossibleSourceGun);
        }

        private void StatChange(Gun gun)
        {
            //ETGModConsole.Log($"bullethits: {bullethits}");
            PlayerController player = gun.GunPlayerOwner();
            gun.RemoveStatFromGun(PlayerStats.StatType.MovementSpeed);
            gun.AddStatToGun(PlayerStats.StatType.MovementSpeed, 1f + (bullethits * 0.0125f), StatModifier.ModifyMethod.MULTIPLICATIVE);
            player.stats.RecalculateStats(player, true, false);
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_babyface_shoot", gameObject);
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

        private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
        {
            bullethits = 0;
            StatChange(this.gun);
        }
        private void OnDodgeRoll(PlayerController player)
        {
            bullethits -= 20;
            if (bullethits < 0)
            {
                bullethits = 0;
            }
            StatChange(this.gun);
        }
        public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
        {
            if (gun.IsReloading && this.HasReloaded)
            {
                HasReloaded = false;
                AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
                base.OnReloadPressed(player, gun, bSOMETHING);
                AkSoundEngine.PostEvent("Play_scatter_gun_reload", base.gameObject);
            }
        }
        //override        
        public override void OnPlayerPickup(PlayerController player)
        {
            //ETGModConsole.Log("OnPickup Was Triggered");

            base.OnPlayerPickup(player);
            player.healthHaver.OnDamaged += this.OnDamaged;
            player.OnPreDodgeRoll += this.OnDodgeRoll;
            StatChange(player.CurrentGun);
        }
        public override void OnDropped()
        {
            base.OnDropped();
            PlayerController player = gun.CurrentOwner as PlayerController;
            //ETGModConsole.Log("OnDrop Was Triggered");
            //base.OnDroppedByPlayer(player);
            bullethits = 0;
            player.healthHaver.OnDamaged -= this.OnDamaged;
            player.OnPreDodgeRoll -= this.OnDodgeRoll;
            //player.stats.RecalculateStats(player, true, false);
            //StatChange(player.CurrentGun);
            //base.OnDropped();
        }
        public int bullethits;
        public float curdamage;
        public static int ID;
    }
}
