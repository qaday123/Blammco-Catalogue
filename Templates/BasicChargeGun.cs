using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using Alexandria.BreakableAPI;
using BepInEx;
using System.Collections.Generic;


namespace TF2Stuff
{
    public class BasicChargeGun : AdvancedGunBehavior
    {
        public static string internalName;
        public static int ID;
        public static void Add()
        {
            string FULLNAME = "Basic Charge Gun";
            string SPRITENAME = "basicchargegun";
            internalName = $"{Module.MODPREFIX}:{FULLNAME.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<TemplateGunExpanded>();
            gun.SetShortDescription("I made a charge gun!");
            gun.SetLongDescription("It builds power because it's economical and definitely not because I lost the original powersource somewhere."); 
            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 10);
            tk2dSpriteAnimationClip clip = gun.spriteAnimator.GetClipByName($"{SPRITENAME}_intro"); 
            gun.SetAnimationFPS(gun.introAnimation, 15);
            gun.SetAnimationFPS(gun.chargeAnimation, 10); 
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 2;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).fps = 15;
            /* List of IDs and names: https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects: https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(41) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(228) as Gun).muzzleFlashEffects;
            /* List of default sound files: https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./sound-list
             * Instructions on setting up custom sound files: https://mtgmodders.gitbook.io/etg-modding-guide/misc/using-custom-sounds */
            gun.gunSwitchGroup = $"{Module.MODPREFIX}_{FULLNAME.ToID()}";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "Play_WPN_spacerifle_shot_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_WPN_crossbow_reload_01");
            gun.DefaultModule.angleVariance = 0; 
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged;
            gun.gunClass = GunClass.CHARGE;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.1f; 
            gun.DefaultModule.numberOfShotsInClip = 15;
            gun.SetBaseMaxAmmo(150);
            gun.gunHandedness = GunHandedness.AutoDetect;
            gun.carryPixelOffset += new IntVector2(0, 0);
            gun.barrelOffset.transform.localPosition += new Vector3(0.75f, 0.2f);

            //PROJECTILE BLOCK

            /* List of IDs and names: https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects: https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            Projectile projectile = gun.DefaultModule.projectiles[0].InstantiateAndFakeprefab();
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 5f;
            projectile.baseData.speed = 25f;
            projectile.baseData.range = 100f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;

            /* List of ammo types: https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./all-custom-ammo-types */
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "mega";

            // CHARGE PROJECTILE
            
            Projectile chargeprojectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(228) as Gun).DefaultModule.projectiles[0]); //Initialize chargedprojectile.
            chargeprojectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeprojectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeprojectile);
            chargeprojectile.baseData.damage = 30f;
            chargeprojectile.baseData.speed = 25f;
            chargeprojectile.baseData.range = 100f;
            chargeprojectile.baseData.force = 5f;
            chargeprojectile.transform.parent = gun.barrelOffset;
            chargeprojectile.AdditionalScaleMultiplier = 1f;
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_02", "Play_WPN_grasshopper_shot_01");

            Projectile chargeprojectile2 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(39) as Gun).DefaultModule.projectiles[0]); //Initialize chargedprojectile.
            chargeprojectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeprojectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeprojectile2);
            chargeprojectile2.baseData.damage = 70f;
            chargeprojectile2.baseData.speed = 25f;
            chargeprojectile2.baseData.range = 100f;
            chargeprojectile2.baseData.force = 10f;
            chargeprojectile2.transform.parent = gun.barrelOffset;
            chargeprojectile2.AdditionalScaleMultiplier = 2f;
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_03", "Play_WPN_bsg_shot_01");
            
            //Animate Projectile
            List<string> projectileSpriteNames = new List<string> {$"{SPRITENAME}_charged_projectile_001", $"{SPRITENAME}_charged_projectile_002"};
            int projectileFPS = 10;
            List<IntVector2> projectileSizes = new List<IntVector2> {new IntVector2(30, 30), new IntVector2(30, 30)};
            List<bool> projectileLighteneds = new List<bool> { true, true, true, true, true };
            List<tk2dBaseSprite.Anchor> projectileAnchors = new List<tk2dBaseSprite.Anchor>{tk2dBaseSprite.Anchor.MiddleCenter, tk2dBaseSprite.Anchor.MiddleCenter};
            List<bool> projectileAnchorsChangeColiders = new List<bool> {false, false};
            List<bool> projectilefixesScales = new List<bool> {false, false};
            List<Vector3?> projectileManualOffsets = new List<Vector3?> {Vector2.zero, Vector2.zero};
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?> {null, null};
            List<IntVector2?> projectileOverrideColliderOffsets = new List<IntVector2?> {null, null};
            List<Projectile> projectileOverrideProjectilesToCopyFrom = new List<Projectile> {null, null};
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;

            chargeprojectile2.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);

            ProjectileModule.ChargeProjectile chargeProj1 = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = 0f,
            };
            ProjectileModule.ChargeProjectile chargeProj2 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeprojectile,
                ChargeTime = 1f,
            };
            ProjectileModule.ChargeProjectile chargeProj3 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeprojectile2,
                ChargeTime = 2f,
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> {chargeProj1, chargeProj2, chargeProj3};
            
            gun.quality = PickupObject.ItemQuality.B;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);
            ID = gun.PickupObjectId;
        }
    }
}
