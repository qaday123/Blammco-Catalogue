using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.IO;
using JetBrains.Annotations;
using HutongGames.PlayMaker;
using System.Diagnostics;
using Alexandria.SoundAPI;

/* NOTES: 
 * Better player feedback on what the boost meter is at (come back when learn how vfx work/repurposed active charge on guns)
 * Error where if you drop the gun bad things occur (and if the dodge roll action is subscribed to it you can no loger dodge roll) - TEMP FIX INSTATIATED
 * Balancing: My main concern is that by making you lose half your boost if you dodge roll means that it might force players into
              constantly holding the gun (and it's not an amazing gun, either), so keep an eye on that.
*/
namespace TF2Stuff
{
    public class BabyFaceBlaster : AdvancedGunBehavior
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
            gun.TrimGunSprites();

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
            gun.quality = PickupObject.ItemQuality.B;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.DecreaseFinalSpeedPercentMin = -25f;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.barrelOffset.transform.localPosition += new Vector3(8f/16f, 10f/16f);
            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_babyface", "Play_WPN_Gun_Shot_01", "babyface_shoot");
            SoundManager.AddCustomSwitchData("WPN_Guns", "qad_babyface", "Play_WPN_Gun_Reload_01", "scatter_gun_reload");
            gun.gunSwitchGroup = "qad_babyface";
            //gun.transform.position += new Vector3(3f/16f, 10f/16f);
            //gun.CanBeDropped = false; // gun breaks when dropped? make it undroppable
            gun.shellCasing = (PickupObjectDatabase.GetById(202) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 1;
            gun.shellsToLaunchOnReload = 0;
            gun.doesScreenShake = true;
            gun.gunScreenShake = new ScreenShakeSettings(0.6f, 10f, 0.1f, 0.02f);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
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
        //override 
        protected override void OnPickup(GameActor owner)
        {
            base.OnPickup(owner);
            PlayerController player_owner = (owner as PlayerController);
            if (player_owner != null)
            {
                isprimary = true;
            }
            else
            {
                isprimary = false;
            }
            (owner as PlayerController).healthHaver.OnDamaged += this.OnDamaged;
            (owner as PlayerController).OnPreDodgeRoll += this.OnDodgeRoll;
            //StatChange((owner as PlayerController).CurrentGun);
        }
        protected override void OnPostDrop(GameActor owner)
        {
            ETGModConsole.Log("OnDrop Triggered");
            (owner as PlayerController).healthHaver.OnDamaged -= this.OnDamaged;
            (owner as PlayerController).OnPreDodgeRoll -= this.OnDodgeRoll;
            base.OnPostDrop(owner);
            //ETGModConsole.Log("Action unsubcsription successful");
        }
        public override void OnDestroy()
        {
            PlayerController player;
            if (isprimary)
            {
                player = GameManager.Instance.PrimaryPlayer;
            }
            else 
            {
                player = GameManager.Instance.SecondaryPlayer;
            }
            //ETGModConsole.Log("OnDestroy triggered");
            player.healthHaver.OnDamaged -= this.OnDamaged;
            player.OnPreDodgeRoll -= this.OnDodgeRoll;
            base.OnDestroy();
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

        /*public override void OnPlayerPickup(PlayerController player)
        {
            //ETGModConsole.Log("OnPickup Was Triggered");

            base.OnPlayerPickup(player);
            player.healthHaver.OnDamaged += this.OnDamaged;
            player.OnPreDodgeRoll += this.OnDodgeRoll;
            StatChange(player.CurrentGun);
        }
        /*public override void OnDroppedByPlayer(PlayerController player)
        {
            base.OnDroppedByPlayer(player);
            //base.OnDroppedByPlayer(player);
            if (this.gun != null)
            {
                //PlayerController player = this.gun.GunPlayerOwner();
                ETGModConsole.Log("Gun does not null");
                //base.OnDroppedByPlayer(player);
                if (player != null)
                {
                    ETGModConsole.Log("Player does not null");
                    bullethits = 0;
                    player.healthHaver.OnDamaged -= this.OnDamaged;
                    player.OnPreDodgeRoll -= this.OnDodgeRoll;
                }
                else if (player == null)
                {
                    ETGModConsole.Log("Player is nulling");
                }
                //base.OnDroppedByPlayer(player);
            }
            else
            {
                ETGModConsole.Log("Gun is nulling");
            }
        }*/
        /*public override void OnDropped()
        {
            //PlayerController player = this.gun.GunPlayerOwner();
            base.OnDropped();
            PlayerController player = this.gun.CurrentOwner as PlayerController;
            if (gun != null)
            {
                //PlayerController player = this.gun.GunPlayerOwner();
                ETGModConsole.Log("Gun does not null");
                //base.OnDroppedByPlayer(player);
                if (player != null)
                {
                    ETGModConsole.Log("Player does not null");
                    bullethits = 0;
                    player.healthHaver.OnDamaged -= this.OnDamaged;
                    player.OnPreDodgeRoll -= this.OnDodgeRoll;
                }
                else if (player == null)
                {
                    ETGModConsole.Log("Player is nulling");
                }
            }
            //player.stats.RecalculateStats(player, true, false);
            //StatChange(player.CurrentGun);
            //base.OnDropped();
            //base.OnDropped();
        }*/
        public int bullethits;
        public float curdamage;
        public static int ID;
        public static bool isprimary;
    }
}
