using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using System.Linq;
using System.ComponentModel;

/* NOTES:
 * Am failing hard pls work this out
*/
namespace TF2Stuff
{
    public class Airstrike : GunBehaviour
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static void Add()
        {
            // New gun base
            consoleID = MODPREFIX + ":air_strike";

            Gun gun = ETGMod.Databases.Items.NewGun("The Air Strike", "airstrike");
            Game.Items.Rename("outdated_gun_mods:the_air_strike", consoleID);
            gun.gameObject.AddComponent<Airstrike>();
            
            //Gun descriptions
            gun.SetShortDescription("Don't Confuse It For The Item");
            gun.SetLongDescription("Fire a barrage of rockets into the air which shortly land at the required target! They certainly " +
                "won't see what's coming now!");
            
            // Sprite setup
            gun.SetupSprite(null, "airstrike_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.TrimGunSprites();

            // Projectile setup
            //gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(56) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.6f;
            gun.DefaultModule.cooldownTime = 0.9f;//0.28f;
            gun.DefaultModule.numberOfShotsInClip = 4;
            gun.SetBaseMaxAmmo(120);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = "qad_airstrike";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "Play_rocket_shoot");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_rocket_reload");
            gun.muzzleFlashEffects = CodeShortcuts.Empty;
            gun.preventRotation = true;

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = gun.DefaultModule.projectiles[0].InstantiateAndFakeprefab();
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 15f;
            projectile.baseData.speed = 40f;
            projectile.baseData.range = 10000000f;
            projectile.baseData.force = 2f;
            gun.barrelOffset.transform.localPosition += new Vector3(-11f/16f,20f/16f);
            gun.barrelOffset.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.AnimateProjectile(new List<string>
            {
                "airstrike_rocket_001",
                "airstrike_rocket_002",
                "airstrike_rocket_003",
                "airstrike_rocket_004",
            }, 16, true, AnimateBullet.ConstructListOfSameValues(new IntVector2(17, 9), 4), // sprite size
            AnimateBullet.ConstructListOfSameValues(false, 4), //Lightened??
            AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 4), //Anchors
            AnimateBullet.ConstructListOfSameValues(false, 4), //Anchors Change Colliders
            AnimateBullet.ConstructListOfSameValues(false, 4), //Fixes Scales
            AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 4),  //Manual Offsets
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(new IntVector2(17, 9), 4), //Collider Pixel Sizes?
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4), //Override Collider Offsets
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));

            Projectile baseProjectile = (PickupObjectDatabase.GetById(39) as Gun).DefaultModule.projectiles[0];

            projectile.ParticleTrail = baseProjectile.ParticleTrail;
            projectile.TrailRenderer = baseProjectile.TrailRenderer;
            projectile.CustomTrailRenderer = baseProjectile.CustomTrailRenderer;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("airstrike_rocket",
                "TF2Items/Resources/CustomGunAmmoTypes/airstrike/airstrike_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/airstrike/airstrike_clipempty");
            //ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            /*SkyRocket rocket = projectile.sprite.gameObject.GetOrAddComponent<SkyRocket>();
            rocket.AscentTime = 0.4f;
            rocket.DescentTime = 0.6f;
            rocket.Variance = 0f;//*/

            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = airstrikeExplosion;
            AirstrikeProjectile strike = projectile.gameObject.GetOrAddComponent<AirstrikeProjectile>();
            GameObject _NapalmReticle = ResourceManager.LoadAssetBundle("shared_auto_002").LoadAsset<GameObject>("NapalmStrikeReticle");
            GameObject target = VFXBuilder.CreateVFX("airstrike_target", CodeShortcuts.GenerateFilePaths("TF2Items/Resources/OtherVFX/airstrike_target/airstrike_target_", 6), 5, new(29, 29), tk2dBaseSprite.Anchor.MiddleCenter, false, 0.2f, persist: true);
            tk2dSprite targetSprite = target.GetComponent<tk2dSprite>();

            int spriteId = targetSprite.spriteId;
            tk2dSpriteCollectionData spriteCollection = targetSprite.collection;
            Destroy(targetSprite);

            GameObject reticle = UnityEngine.Object.Instantiate(_NapalmReticle);
            tk2dSlicedSprite quad = reticle.GetComponent<tk2dSlicedSprite>();
            //quad.SetSprite(spriteCollection, spriteId);
            //ETGModConsole.Log("TEST: " + spriteId.ToString() + ", " + (spriteCollection != null).ToString());

            tk2dSlicedSprite slicedSpriteTarget = target.GetOrAddComponent<tk2dSlicedSprite>();
            slicedSpriteTarget.SetSprite(spriteCollection, spriteId);
            slicedSpriteTarget.anchor = tk2dBaseSprite.Anchor.MiddleCenter;
            target.GetOrAddComponent<ReticleRiserEffect>();
           

            strike.targetEffect = target;
            //strike.targetEffect.GetComponent<tk2dSprite>().renderLayer = 0;//

            ID = gun.PickupObjectId;
        }
        public static int ID;
        public static string consoleID;
        public bool boostedFirerate = false;
        float time_boosted = 0f;
        float BoostTimePerBulletInClip = 0.5f;

        public float normalCooldown = 0.9f;
        public float boostMult = 0.31f;
        public override void Update()
        {
            if (gun && PlayerOwner)
            {
                if (boostedFirerate)
                {
                    if (gun.DefaultModule.shootStyle != ProjectileModule.ShootStyle.Automatic)
                    {
                        gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
                        gun.DefaultModule.cooldownTime *= boostMult;
                    }
                    if (time_boosted < BoostTimePerBulletInClip * gun.ClipCapacity)
                        time_boosted += BraveTime.DeltaTime;
                    else
                    {
                        boostedFirerate = false;
                        gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                        gun.DefaultModule.cooldownTime = normalCooldown;
                    }
                }
            }
            base.Update();
        }
        public void BoostFireRate(PlayerController player)
        {
            boostedFirerate = true;
            time_boosted = 0f;
        }
        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            playerOwner.OnPreDodgeRoll += BoostFireRate;
            base.OnPlayerPickup(playerOwner);
        }
        public override void DisableEffectPlayer(PlayerController player)
        {
            player.OnPreDodgeRoll -= BoostFireRate;
            base.DisableEffectPlayer(player);
        }
        public static ExplosionData airstrikeExplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 2f,
            pushRadius = 2.5f,
            damage = 25f,
            doDamage = true,
            damageToPlayer = 0,
            secretWallsRadius = 6f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 4f,
            debrisForce = 8f,
            preventPlayerForce = false,
            explosionDelay = 0.1f,
            usesComprehensiveDelay = false,
            comprehensiveDelay = 0f,
            doScreenShake = true,
            playDefaultSFX = true,
            effect = genericSmallExplosion.effect,
            ignoreList = genericSmallExplosion.ignoreList,
            ss = genericSmallExplosion.ss,
        };
    }
}
