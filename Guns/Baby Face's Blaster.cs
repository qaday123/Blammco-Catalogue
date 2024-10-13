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
using static UnityEngine.UI.GridLayoutGroup;

/* NOTES: 
* Better player feedback on what the boost meter is at (come back when learn how vfx work/repurposed active charge on guns)
* Error where if you drop the gun bad things occur (and if the dodge roll action is subscribed to it you can no loger dodge roll) - TEMP FIX INSTATIATED
* Balancing: My main concern is that by making you lose half your boost if you dodge roll means that it might force players into
          constantly holding the gun (and it's not an amazing gun, either), so keep an eye on that.
*/

namespace TF2Stuff
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
            gun.SetLongDescription("Hitting an enemy grants you a speed boost (Visible on the side). This speed boost is carried across guns but is fully lost when you " +
                "take damage, and slightly lost when dodgerolling in combat.\n\nThis gun makes its wielder more and more overconfident with every hit, " +
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
                if (projectileModule != gun.DefaultModule) { projectileModule.ammoCost = 0; }
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.DecreaseFinalSpeedPercentMin = -25f;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.barrelOffset.transform.localPosition += new Vector3(8f / 16f, 10f / 16f);
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

            gun.gameObject.AddComponent<BabyFaceDisplay>();

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
            /*if (bullethits < 40)
            {
                bullethits++;
            }*/
            curdamage += proj.baseData.damage;
            if (curdamage > maxdamage) curdamage = maxdamage;
            StatChange(proj.PossibleSourceGun);
        }

        private void StatChange(Gun gun)
        {
            //ETGModConsole.Log($"bullethits: {bullethits}");
            PlayerController player = gun.GunPlayerOwner();
            gun.RemoveStatFromGun(PlayerStats.StatType.MovementSpeed);
            gun.AddStatToGun(PlayerStats.StatType.MovementSpeed, 1f + (curdamage / maxdamage), StatModifier.ModifyMethod.MULTIPLICATIVE);  //(bullethits * 0.0125f)
            player.stats.RecalculateStats(player, true, false);
        }
        //override 
        public override void OnPlayerPickup(PlayerController owner)
        {
            base.OnPlayerPickup(owner);
            owner.healthHaver.OnDamaged += this.OnDamaged;
            owner.OnPreDodgeRoll += this.OnDodgeRoll;
            //StatChange((owner as PlayerController).CurrentGun);
        }
        public override void DisableEffectPlayer(PlayerController player)
        {
            player.healthHaver.OnDamaged -= this.OnDamaged;
            player.OnPreDodgeRoll -= this.OnDodgeRoll;
            base.DisableEffectPlayer(player);
        }
        private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
        {
            //bullethits = 0;
            curdamage = 0;
            StatChange(this.gun);
        }
        private void OnDodgeRoll(PlayerController player)
        {
            /*bullethits -= 20;
            if (bullethits < 0)
            {
                bullethits = 0;
            }*/
            if (!player.IsInCombat)
                return;

            curdamage -= (maxdamage / 2f);
            if (curdamage < 0) curdamage = 0;
            StatChange(this.gun);
        }

        public int bullethits;
        public float curdamage = 0;
        public float maxdamage = 400;
        public static int ID;
        

        private class BabyFaceDisplay : CustomAmmoDisplay
        {
            private Gun _gun;
            private BabyFaceBlaster babyface;
            private PlayerController _owner;
            private bool changed_gun = false;

            public Vector3 cooldown_foreground_offset = new Vector3(0, -3f);
            public Vector3 cooldown_fill_offset = new Vector3(2.75f, 3f);
            private void Start()
            {
                this._gun = base.GetComponent<Gun>();
                this.babyface = this._gun.GetComponent<BabyFaceBlaster>();
                this._owner = this._gun.CurrentOwner as PlayerController;
                _owner.GunChanged += ChangedGun;
            }

            public override bool DoCustomAmmoDisplay(GameUIAmmoController uic)
            {
                if (!this._owner)
                {
                    ETGModConsole.Log("byebye");
                    ResetDisplay(uic);
                    return false;
                }
                //ETGModConsole.Log("running");
                uic.GunCooldownForegroundSprite.RelativePosition = uic.GunBoxSprite.RelativePosition + cooldown_foreground_offset; // + babyface.offset1;
                uic.GunCooldownForegroundSprite.flip = dfSpriteFlip.FlipVertical | dfSpriteFlip.FlipHorizontal;
                uic.GunCooldownFillSprite.RelativePosition = uic.GunBoxSprite.RelativePosition + cooldown_fill_offset;//+ new Vector3(123f, 3f, 0f);
                uic.GunCooldownFillSprite.ZOrder = uic.GunBoxSprite.ZOrder + 1;
                uic.GunCooldownForegroundSprite.ZOrder = uic.GunCooldownFillSprite.ZOrder + 1;
                uic.GunCooldownFillSprite.IsVisible = true;
                uic.GunCooldownForegroundSprite.IsVisible = true;
                uic.GunCooldownFillSprite.FillAmount = babyface.curdamage / babyface.maxdamage;
                uic.GunAmmoCountLabel.Text = $"[color #ff8811] Boost:  [/color]{this._owner.VanillaAmmoDisplay()}";
                //ResetDisplay(uic);
                return true;
            }

            public void ResetDisplay(GameUIAmmoController uic)
            {
                uic.GunCooldownForegroundSprite.RelativePosition = uic.GunBoxSprite.RelativePosition;
                uic.GunCooldownForegroundSprite.flip = dfSpriteFlip.None;
                uic.GunCooldownFillSprite.RelativePosition = uic.GunBoxSprite.RelativePosition + new Vector3(123f, 3f, 0f);
                uic.GunCooldownFillSprite.IsVisible = false;
                uic.GunCooldownForegroundSprite.IsVisible = false;
            }
            public void ChangedGun(Gun gun, Gun gun2, bool newgun)
            {
                ETGModConsole.Log("changed");
                changed_gun = gun2.GetComponent<BabyFaceBlaster>() == null;
            }
        }
    } 
}



