using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using Alexandria.BreakableAPI;
using System.Collections.Generic;
using Alexandria.Misc;
using Alexandria.VisualAPI;

/* NOTES:
 * Bleed effect is broken   UPDATE: Bleed is VERY broken since its mostly unused ;-;
 * Extra damage on stun is broken
 * Charge animation is not working like i want it to find out how it works  UPDATE: Done :D
*/
namespace TF2Stuff
{
    public class Cleaver : GunBehaviour
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:flying_guillotine";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Flying Guillotine", "cleaver");
            Game.Items.Rename("outdated_gun_mods:flying_guillotine", "qad:flying_guillotine");
            gun.gameObject.AddComponent<Cleaver>();
            
            //Gun descriptions
            gun.SetShortDescription("Cutting Edge");
            gun.SetLongDescription("Why use a tiny throwing knife when you just have a massive cleaver instead? Deals critical damage to enemies " +
                "with one or more afflictions.\n\nThere are chinese symbols engraved on the side of the blade. They roughly translate to 'Dead Meat'.");
            
            // Sprite setup
            gun.SetupSprite(null, "cleaver_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 8);
            gun.SetAnimationFPS(gun.reloadAnimation, 4);
            gun.SetAnimationFPS(gun.chargeAnimation, 12);
            gun.TrimGunSprites();

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
            gun.AddToSubShop(ItemBuilder.ShopType.Cursula);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            

            // More projectile setup
            projectile.baseData.damage = 20f;
            projectile.baseData.speed = 36f;
            projectile.baseData.range = 1000f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            

            GameObject debris = BreakableAPIToolbox.GenerateDebrisObject("TF2Items/Resources/HitEffects/cleaver_hit_debris.png", true, AngularVelocity: 720).gameObject;
            debris.GetComponent<tk2dBaseSprite>().CurrentSprite.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
            VFXPool debrisPool = VFXBuilder.CreateBlankVFXPool(debris);
            debrisPool.type = VFXPoolType.All;

            projectile.hitEffects.HasProjectileDeathVFX = true;
            projectile.hitEffects.deathAny = debrisPool;
            projectile.hitEffects.overrideMidairDeathVFX = (PickupObjectDatabase.GetById(417) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
            projectile.hitEffects.alwaysUseMidair = true;
            projectile.hitEffects.midairInheritsFlip = true;

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

            /*projectile.AnimateProjectile(new List<string> {
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
            AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));*/
            projectile.SetProjectileSpriteRight("cleaverproj_001", 14, 13, lightened: false, anchor: tk2dBaseSprite.Anchor.MiddleCenter);

            ProjectileSpin spin = projectile.gameObject.GetOrAddComponent<ProjectileSpin>();
            spin.degreesPerSecond = 1080f;
            spin.directionOfSpinDependsOnVelocity = true;

            ProjectileModule.ChargeProjectile chargeProj = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = 0.8f,
                VfxPool = null,
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> { chargeProj };

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 5;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Cleaver",
                "TF2Items/Resources/CustomGunAmmoTypes/cleaver/cleaver_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/cleaver/cleaver_clipempty");

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "Play_cleaver_throw";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;
        }
        public static int ID;
        bool hasCrit = false;
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += this.OnHitEnemy;
            projectile.specRigidbody.OnPreRigidbodyCollision += PreCollision;
            projectile.specRigidbody.OnTileCollision += HitWorld;
            base.PostProcessProjectile(projectile);
        }
        public void HitWorld(CollisionData tileCollision)
        {
            AkSoundEngine.PostEvent("cleaver_hit_world", tileCollision.MyRigidbody.gameObject);
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            PlayerController player = gun.GunPlayerOwner();
            string[] hitSounds = new[] { "cleaver_hit_01", "cleaver_hit_02", "cleaver_hit_03", "cleaver_hit_04", "cleaver_hit_05" };

            AkSoundEngine.PostEvent(hitSounds[Random.Range(0, hitSounds.Length)], proj.gameObject);

            if (hasCrit)
            {
                enemy.DoCritEffects(); // in CodeShortcuts.cs
                UnityEngine.Object.Instantiate<GameObject>(EasyVFXDatabase.TeleporterPrototypeTelefragVFX, enemy.UnitCenter, Quaternion.identity);
                if (PlayerOwner && PlayerOwner.GetExtComp() && PlayerOwner.PlayerHasActiveSynergy("The Forbidden Combo")) 
                    PlayerOwner.GetExtComp().Enrage(3, true);
            }
        }
        private void PreCollision(SpeculativeRigidbody myBody, PixelCollider myCollider, SpeculativeRigidbody otherBody, PixelCollider otherCollider)
        {
            hasCrit = false;
            if (myBody.projectile != null && otherBody.aiActor != null)
            {
                //ETGModConsole.Log(PlayerOwner.PlayerHasActiveSynergy("The Forbidden Combo") + " " + otherBody.aiActor.behaviorSpeculator.IsStunned);
                if (otherBody.aiActor.m_activeEffects.Count > 0 || otherBody.aiActor.behaviorSpeculator.IsStunned)
                {
                    myBody.projectile.baseData.force *= 2f;
                    myBody.projectile.baseData.damage *= (PlayerOwner.PlayerHasActiveSynergy("The Forbidden Combo")) ? 4f : 3f;
                    hasCrit = true;
                }
            }
        }
    }
}
