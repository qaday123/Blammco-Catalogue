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
 * Bleed effect is broken   UPDATE: Bleed is VERY broken since its mostly unused ;-;
 * Extra damage on stun is broken
 * Charge animation is not working like i want it to find out how it works  UPDATE: Done :D
*/
namespace ExampleMod
{
    public class Cleaver : AdvancedGunBehavior
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Flying Guillotine", "cleaver");
            Game.Items.Rename("outdated_gun_mods:flying_guillotine", "qad:flying_guillotine");
            gun.gameObject.AddComponent<Cleaver>();
            
            //Gun descriptions
            gun.SetShortDescription("Cutting Edge");
            gun.SetLongDescription("Why use a tiny throwing knife when you just have a massive cleaver instead?\n\n" +
                "There are chinese symbols engraved on the side of the blade. They roughly translate to 'Dead Meat'.");
            
            // Sprite setup
            gun.SetupSprite(null, "cleaver_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 8);
            gun.SetAnimationFPS(gun.reloadAnimation, 4);
            gun.SetAnimationFPS(gun.chargeAnimation, 12);

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(56) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(656) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.8f;
            gun.DefaultModule.cooldownTime = 0.5f;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.SetBaseMaxAmmo(150);
            gun.AddStatToGun(PlayerStats.StatType.Curse, 1f, StatModifier.ModifyMethod.ADDITIVE);
            gun.gunClass = GunClass.SILLY;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;
            gun.encounterTrackable.EncounterGuid = "Throwing Cleaver";
            gun.AddToSubShop(ItemBuilder.ShopType.Cursula);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 25f;
            projectile.baseData.speed = 26f;
            projectile.baseData.range = 1000f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            /*projectile.bleedEffect = new GameActorBleedEffect
            {
                AffectsEnemies = true,
                AffectsPlayers = false,
                AppliesDeathTint = false,
                AppliesOutlineTint = false,
                AppliesTint = false,
                ChargeAmount = 5f,
                PlaysVFXOnActor = true,
                ChargeDispelFactor = 5f, // ??
                DeathTintColor = Color.red,
                duration = 7.5f,
                effectIdentifier = "bleed", // what debuff it is
                maxStackedDuration = 12f,
                //m_extantReticle = null,  // ????
                //m_isHammerOfDawn = false, // ????
                OutlineTintColor = Color.red,
                OverheadVFX = null, // effect above head
                resistanceType = EffectResistanceType.Poison, // what type for enemyy invicibility
                stackMode = GameActorEffect.EffectStackingMode.Refresh, // how effect should stack
                TintColor = Color.red,
                vfxChargingReticle = null, // ??????
                vfxExplosion = null // ??????  dont put null here cuz it breaks. What should be put here? idfk. thanks dog roll!
            };
            projectile.AppliesBleed = true;
            projectile.BleedApplyChance = 100f;*/

            projectile.AnimateProjectile(new List<string> {
                "cleaverproj_001",
                "cleaverproj_002",
                "cleaverproj_003",
                "cleaverproj_004",
            }, 6, true, new List<IntVector2> {
                new IntVector2(14, 13), //1
                new IntVector2(13, 14), //2            
                new IntVector2(14, 13), //3
                new IntVector2(13, 14), //4
            }, AnimateBullet.ConstructListOfSameValues(false, 4), AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 4),
            AnimateBullet.ConstructListOfSameValues(true, 4),
            AnimateBullet.ConstructListOfSameValues(false, 4),
            AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 4),
            new List<IntVector2?> {
                new IntVector2(12, 12), //1
                new IntVector2(12, 12), //2            
                new IntVector2(12, 12), //3
                new IntVector2(12, 12), //3
            },
            AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4),
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));

            ProjectileModule.ChargeProjectile chargeProj = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = 1f,
                VfxPool = null,
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> { chargeProj };

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 5;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Cleaver",
                "ExampleMod/Resources/CustomGunAmmoTypes/cleaver/cleaver_clipfull",
                "ExampleMod/Resources/CustomGunAmmoTypes/cleaver/cleaver_clipempty");

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "Play_cleaver_throw";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;
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
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += this.OnHitEnemy;
            base.PostProcessProjectile(projectile);
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            PlayerController player = gun.GunPlayerOwner();
            AkSoundEngine.PostEvent("Play_cleaver_hit", base.gameObject);

            if (enemy != null && enemy.aiActor != null && player != null && player.projectile && enemy.aiActor.healthHaver)
            {
                if (enemy.aiActor.GetEffect("stun") != null)
                {
                    player.projectile.baseData.damage *= 1.5f;
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
    }
}
