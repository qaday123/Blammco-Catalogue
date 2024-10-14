using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Alexandria.Misc;
using Alexandria.VisualAPI;

/* NOTES:
 *
*/
namespace TF2Stuff
{
    public class RocketJumper : GunBehaviour
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:rocket_jumper";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Rocket Jumper", "jumper");
            Game.Items.Rename("outdated_gun_mods:rocket_jumper", consoleID);
            gun.gameObject.AddComponent<RocketJumper>();
            
            //Gun descriptions
            gun.SetShortDescription("Reach For The Star");
            gun.SetLongDescription("A very ineffective weapon, but a very useful mobility tool. Dodge roll in the presence of it's projectiles' " +
                " explosions to perform a rocket jump\n\nThis weapon was supposedly used to train young rocket jumpees the ropes, which resulted in " +
                "a lot of broken legs. Hm.");
            
            // Sprite setup
            gun.SetupSprite(null, "jumper_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.9f;
            gun.DefaultModule.numberOfShotsInClip = 4;
            gun.SetBaseMaxAmmo(1000);
            gun.gunClass = GunClass.SILLY;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE
            
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            ExplosionEffect = VFXToolbox.CreateVFX("Explosion Ring", new()
            {
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_001.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_002.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_003.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_004.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_005.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_006.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_007.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_008.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_009.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_010.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_011.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_012.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_013.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_014.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_015.png",
                "TF2Items/Resources/OtherVFX/explosion_ring/explosion_ring_cloud_016.png",
            }, 48, new(101, 101), tk2dBaseSprite.Anchor.MiddleCenter, false, 0f);

            // More projectile setup
            projectile.baseData.damage = 3;
            projectile.baseData.speed = 22f;
            projectile.baseData.range = 100f;
            projectile.baseData.force = 30f;
            gun.barrelOffset.transform.localPosition += new Vector3(12f/16f,10f/16f);
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);
            rocketexplosion.effect = ExplosionEffect;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("stock_rocket",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipempty");
            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = rocketexplosion;
            ID = gun.PickupObjectId;

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "rocket_jumper_shoot";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[0].eventAudio = "Play_rocket_reload";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[0].triggerEvent = true;
        }
        public static int ID;
        static GameObject ExplosionEffect;
        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            projectile.OnDestruction += OnDestroy;
        }
        public void OnDestroy(Projectile projectile)
        {
            //SpawnManager.SpawnVFX((GameObject)BraveResources.Load("Global VFX/GhostVFX_blank", ".prefab"), projectile.specRigidbody.UnitCenter, Quaternion.identity);
            AkSoundEngine.PostEvent("rocket_jumper_explode1", projectile.gameObject);
            /*RocketJumpDoer jump = projectile.PossibleSourceGun.GunPlayerOwner().gameObject.GetOrAddComponent<RocketJumpDoer>();
            ETGModConsole.Log($"From jumper: {jump.isRocketJumper}");
            jump.ForceCheckForJump(projectile.PossibleSourceGun.GunPlayerOwner(), projectile.specRigidbody.UnitCenter, rocketexplosion);
            */
            //PlayerController player = projectile.PossibleSourceGun.GunPlayerOwner();
            if (PlayerOwner)
            {
                RocketJumpDoer jump = PlayerOwner.gameObject.GetComponent<RocketJumpDoer>();
                if (jump == null || jump.Player == null)
                {
                    jump = gun.gameObject.GetOrAddComponent<RocketJumpDoer>();
                    jump.CheckForJump(PlayerOwner, projectile.SafeCenter, rocketexplosion); 
                }
            }
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            //AkSoundEngine.PostEvent("Play_rocket_shoot", gameObject);
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
            }
        }
        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            base.OnPlayerPickup(playerOwner);
            RocketJumpDoer jump = gun.gameObject.GetOrAddComponent<RocketJumpDoer>();
            jump.SetPlayer(playerOwner);
            jump.isRocketJumper = true;
        }
        public override void DisableEffect(GameActor owner)
        {
            RocketJumpDoer jump = gun.gameObject.GetComponent<RocketJumpDoer>();
            jump.SetPlayer(null);
            base.DisableEffect(owner);
        }

        public static ExplosionData rocketexplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 4f,
            pushRadius = 4.5f,
            damage = 0f,
            doDamage = true,
            damageToPlayer = 0,
            secretWallsRadius = 6f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 20f,
            debrisForce = 24f,
            preventPlayerForce = false,
            explosionDelay = 0.1f,
            usesComprehensiveDelay = false,
            comprehensiveDelay = 0f,
            doScreenShake = false,
            playDefaultSFX = false,
            effect = null,
            ignoreList = genericLargeExplosion.ignoreList,
            ss = genericLargeExplosion.ss,
        };
    }
}
