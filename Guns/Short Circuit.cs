﻿using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using static UnityEngine.UI.CanvasScaler;

/* NOTES:
 * Animate charged projectile
 * Find out how to make charged shot cost more ammo
 * Find out how to make charged shot make a different sound
*/
namespace TF2Stuff
{
    public class ShortCircuit : AdvancedGunBehavior
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:short_circuit";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Short Circuit", "circuit");
            Game.Items.Rename("outdated_gun_mods:short_circuit", "qad:short_circuit");
            gun.gameObject.AddComponent<ShortCircuit>();
            
            //Gun descriptions
            gun.SetShortDescription("Discharge This!");
            gun.SetLongDescription("Tap to fire a very short range, but very powerful homing projectile that locks on to enemies' natural conductivity. " +
                "Charge up the gun to discharge a large ball of energy that destroys all incoming projectiles. Costs more ammo and doesn't do much damage.\n\n" +
                "Little gadgets and trinkets like this were a favourite of Engie's toys. After a few touch-ups and it's own power source, the natural lead-based " +
                "foes of the gungeon fear the power this hand has to shut down any of their offensive and defensive capabilities alike.");
            
            // Sprite setup
            gun.SetupSprite(null, "circuit_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.SetAnimationFPS(gun.chargeAnimation, 10);
            gun.TrimGunSprites();
            //gun.SetAnimationFPS(gun.dischargeAnimation, 10);

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 18;
            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(153) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(41) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.2f;
            gun.DefaultModule.numberOfShotsInClip = 800;
            gun.SetBaseMaxAmmo(800);
            gun.gunClass = GunClass.CHARGE;
            gun.gunSwitchGroup = "Banana"; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            gun.carryPixelOffset += new IntVector2(5,0);
            gun.barrelOffset.transform.localPosition += new Vector3(1f, 1f / 16f);

            gun.quality = PickupObject.ItemQuality.A;
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // UNCHARGED PROJECTILE
            projectile.baseData.damage = 40f;
            projectile.baseData.speed = 40f;
            projectile.baseData.range = 0.75f;
            projectile.baseData.force = 5f;
            projectile.transform.parent = gun.barrelOffset;
            projectile.ignoreDamageCaps = true;
            HomingModifier homing = projectile.gameObject.AddComponent<HomingModifier>();
            homing.AngularVelocity = 1600f;
            homing.HomingRadius = 50f;
            //projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);

            // CHARGE PROJECTILE
            //gun.DefaultModule.projectiles.Add(new Projectile());
            Projectile chargeprojectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            chargeprojectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeprojectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeprojectile);
            chargeprojectile.baseData.damage = 2f;
            chargeprojectile.baseData.speed = 6f;
            chargeprojectile.baseData.range = 100f;
            chargeprojectile.baseData.force = 5f;
            chargeprojectile.transform.parent = gun.barrelOffset;
            chargeprojectile.AdditionalScaleMultiplier = 1f;
            //HungryProjectileModifier eat = chargeprojectile.gameObject.AddComponent<HungryProjectileModifier>();
            //eat.HungryRadius = 5f;
            //chargeprojectile.collidesWithProjectiles = true;
            //chargeprojectile.collidesWithEnemies = true; // this causes it to make a hit sound everytime it hits a projectile... 
            PierceProjModifier pierce = chargeprojectile.gameObject.AddComponent<PierceProjModifier>();
            pierce.penetratesBreakables = true;
            pierce.penetration += 10000;
            BlockEnemyProjectilesMod block = chargeprojectile.gameObject.GetOrAddComponent<BlockEnemyProjectilesMod>();
            block.projectileSurvives = true;
            block.RangeExtension = new IntVector2(10,10);
            HomingModifier homing2 = chargeprojectile.gameObject.GetOrAddComponent<HomingModifier>();
            homing2.enabled = false;

            chargeprojectile.AnimateProjectile(new List<string> {
            "energy_ball_001",
            "energy_ball_002",
            "energy_ball_003",
            "energy_ball_004",
            "energy_ball_005",
            "energy_ball_006",
            }, 10, true, AnimateBullet.ConstructListOfSameValues(new IntVector2(32, 32), 6), // sprite sizes
            AnimateBullet.ConstructListOfSameValues(false, 6),  AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 6),
            AnimateBullet.ConstructListOfSameValues(true, 6),
            AnimateBullet.ConstructListOfSameValues(false, 6),
            AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 6),
            new List<IntVector2?> { // hitbox sizes
                new IntVector2(36, 36), //1
                new IntVector2(36, 36), //2            
                new IntVector2(36, 36), //3
                new IntVector2(36, 36), //4
                new IntVector2(36, 36), //5
                new IntVector2(36, 36), //6            
            },
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 6),
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 6)); 
            ProjectileModule.ChargeProjectile chargeProj1 = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = 0f,
                VfxPool = null,
            };
            ProjectileModule.ChargeProjectile chargeProj2 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeprojectile,
                ChargeTime = 1.5f,
                AmmoCost = 25,
                UsedProperties = (ProjectileModule.ChargeProjectileProperties.additionalWwiseEvent | ProjectileModule.ChargeProjectileProperties.ammo), 
                AdditionalWwiseEvent = "Play_shortcircuit_chargeshoot"
                
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> { chargeProj1, chargeProj2 };

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "pulse blue";
            ID = gun.PickupObjectId;

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "Play_shortcircuit_shoot";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;
            PixelCollider test = new PixelCollider();
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            //AkSoundEngine.PostEvent("Play_shortcircuit_shoot", gameObject);
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
                //AkSoundEngine.PostEvent("Play_rocket_reload", base.gameObject);
            }
        }
    }
}
