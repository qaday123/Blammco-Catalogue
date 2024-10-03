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
 * Explosion doesn't do self damage for some reason even when the variable contains a value other than 0.
*/
namespace TF2Stuff
{
    public class Caber : AdvancedGunBehavior
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Ullapool Caber", "caber");
            Game.Items.Rename("outdated_gun_mods:ullapool_caber", "qad:caber");
            gun.gameObject.AddComponent<Caber>();
            
            //Gun descriptions
            gun.SetShortDescription("KABLOOEH");
            gun.SetLongDescription("boom");
            
            // Sprite setup
            gun.SetupSprite(null, "caber_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.SetAnimationFPS(gun.chargeAnimation, 8);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(56) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(656) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.4f;
            gun.DefaultModule.cooldownTime = 0.5f;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.SetBaseMaxAmmo(75);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.encounterTrackable.EncounterGuid = "Caber";
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 10f;
            projectile.baseData.speed = 20f;
            projectile.baseData.range = 10f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            //projectile.baseData.UsesCustomAccelerationCurve = true;
            //projectile.baseData.AccelerationCurve = (PickupObjectDatabase.GetById(761) as Gun).projectile.baseData.AccelerationCurve;

            projectile.AnimateProjectile(new List<string> {
                "caber_projectile_001",
                "caber_projectile_002",
                "caber_projectile_003",
                "caber_projectile_004",
            }, 6, true, new List<IntVector2> {
                new IntVector2(18, 17), //1
                new IntVector2(18, 17), //2            
                new IntVector2(18, 17), //3
                new IntVector2(18, 17), //4
            }, AnimateBullet.ConstructListOfSameValues(false, 4), AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 4),
            AnimateBullet.ConstructListOfSameValues(true, 4),
            AnimateBullet.ConstructListOfSameValues(false, 4),
            AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 4),
            new List<IntVector2?> {
                new IntVector2(16, 15), //1
                new IntVector2(16, 15), //2            
                new IntVector2(16, 15), //3
                new IntVector2(16, 15), //3
            },
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4),
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));

            ProjectileModule.ChargeProjectile chargeProj = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = 0.5f,
                VfxPool = null,
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> { chargeProj };

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 3;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Caber",
                "TF2Items/Resources/CustomGunAmmoTypes/caber/caber_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/caber/caber_clipempty");

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "Play_OBJ_item_throw_01";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;

            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.doExplosion = true;
            explode.explosionData = caberexplosion;
            explode.explosionData.damageToPlayer = 0.5f;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
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
                //AkSoundEngine.PostEvent("Play_pistol_clipin", base.gameObject);
            }
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            //ETGModConsole.Log(projectile.m_currentSpeed);
            GameManager.Instance.StartCoroutine(HandleSlowDown(projectile));
        }

        private IEnumerator HandleSlowDown(Projectile projectile)
        {
            //ETGModConsole.Log("Starting Coroutine");
            while (projectile.m_distanceElapsed < projectile.baseData.range - 2f)
            {
                yield return new WaitForSeconds(0); 
            }

            while (projectile.m_currentSpeed > 3f)
            {
                projectile.m_currentSpeed /= 2f;
                //ETGModConsole.Log($"Speed: {projectile.m_currentSpeed}");
                yield return new WaitForSeconds(0.1f);
            }
            //ETGModConsole.Log("Ending Coroutine");
            yield break;
        }

        public static ExplosionData caberexplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 8f,
            pushRadius = 8f,
            damage = 45f,
            doDamage = true,
            damageToPlayer = 0.5f,
            secretWallsRadius = 10f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 10f,
            debrisForce = 12f,
            preventPlayerForce = false,
            explosionDelay = 0.1f,
            usesComprehensiveDelay = false,
            comprehensiveDelay = 0f,
            doScreenShake = true,
            playDefaultSFX = true,
            effect = genericLargeExplosion.effect,
            ignoreList = genericLargeExplosion.ignoreList,
            ss = genericLargeExplosion.ss,
            breakSecretWalls = true
        };
        public float minimum_speed;
        public float current_speed;
    }
}
