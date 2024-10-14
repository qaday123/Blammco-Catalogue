using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.VisualAPI;
using BepInEx;
using UnityEngine.Analytics;
using Alexandria.BreakableAPI;


// POTENTIAL SYNERGIES: Poison weapons: spawns blob of poison, Something that stuns: Apply stun, Charm weapons/drugs: Apply charm,
// Syringe: Fire Syringes???
// ISSUES TO SOLVE: Fix Syringe Sprite
namespace TF2Stuff
{
    public class SyringeGun : GunBehaviour
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:syringe_gun";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Syringe Gun", "syringe");
            Game.Items.Rename("outdated_gun_mods:syringe_gun", consoleID);
            gun.gameObject.AddComponent<SyringeGun>();
            
            //Gun descriptions
            gun.SetShortDescription("Do No Harm");
            gun.SetLongDescription("Rapid firing mechanism that shoots syringes that have a chance to poison.\n\n" +
                "Despite being in the field of medicine, discovering a way to cheat death alledgedly was too boring, " +
                "and that healing people back to health in seconds wasn't a very good defensive maneuver.\n" +
                "The solution to both? Fire deadly syringes at your enemies of course!");
            
            // Sprite setup
            gun.SetupSprite(null, "syringe_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(56) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(80) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.2f;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 25;
            gun.DefaultModule.angleVariance = 8f;
            gun.SetBaseMaxAmmo(600);
            gun.gunClass = GunClass.SILLY; //when as starting weapon reclass to 'SHITTY'
            gun.barrelOffset.transform.localPosition += new Vector3(0.625f, 0.5f, 0);
            gun.carryPixelOffset = new IntVector2(8, 0);
            //gun.transform.localPosition += new Vector3(1f,0,0);
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.customAmmoType = "ghost_small";
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Syringe",
                "TF2Items/Resources/CustomGunAmmoTypes/syringegun/syringegun_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/syringegun/syringegun_clipempty");

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;
            gun.encounterTrackable.EncounterGuid = "syringe gun";
            gun.AddToSubShop(ItemBuilder.ShopType.Goopton);
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
   
            // More projectile setup
            projectile.baseData.damage = 3f; //when as starting weapon remodify to 3-4 damage, remove poison + others
            projectile.baseData.speed = 28f;
            projectile.baseData.range = 100f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            //projectile.AppliesPoison = true;
            //projectile.PoisonApplyChance = 0.2f;
            projectile.damageTypes |= CoreDamageTypes.Poison;
            ExtremelySimplePoisonBulletBehaviour poisoning = projectile.gameObject.AddComponent<ExtremelySimplePoisonBulletBehaviour>();
            poisoning.procChance = 0.2f;
            poisoning.useSpecialTint = false;
            //projectile.healthEffect = (PickupObjectDatabase.GetById(204) as BulletStatusEffectItem).HealthModifierEffect;
            projectile.SetProjectileSpriteRight("syringe", 17, 3, false, tk2dBaseSprite.Anchor.MiddleCenter); // larger texture
            //projectile.SetProjectileSpriteRight("syringe", 6, 1, false, null); // smaller texture

            verticalSyringe = VFXBuilder.CreateVFXPool("syringe_vertical", 
                CodeShortcuts.GenerateFilePaths("TF2Items/Resources/HitEffects/SyringeHitEffects/syringe_tile_north", 5), 
                12, new IntVector2(12, 9), tk2dBaseSprite.Anchor.MiddleLeft, false, 0f, true, alignment: VFXAlignment.NormalAligned);
            horizontalSyringe = VFXBuilder.CreateVFXPool("syringe_horizontal", 
                CodeShortcuts.GenerateFilePaths("TF2Items/Resources/HitEffects/SyringeHitEffects/syringe_tile_west", 5), 
                12, new IntVector2(12, 9), tk2dBaseSprite.Anchor.MiddleLeft, false, 0f, true);

            projectile.hitEffects.tileMapHorizontal = horizontalSyringe;
            projectile.hitEffects.tileMapVertical = verticalSyringe;
            gun.clipObject = BreakableAPIToolbox.GenerateDebrisObject("TF2Items/Resources/HitEffects/SyringeHitEffects/syringe_clip.png", true, 1, 5, 240, 60, null, 1, null, null, 2).gameObject;
            gun.clipsToLaunchOnReload = 1;
            gun.reloadClipLaunchFrame = 5;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        static VFXPool verticalSyringe;
        static VFXPool horizontalSyringe;
        static GameObject clip;

        /*private void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            projectile.OnHitEnemy += OnHit;
        }
        private void OnHit(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (enemy == null || enemy.aiActor == null)
            {
                return;
            }
            if (enemy != null && enemy.aiActor != null)
            {
                GameActorHealthEffect poisonEffect = Gungeon.Game.Items["irradiated_lead"].GetComponent<BulletStatusEffectItem>().HealthModifierEffect;
                enemy.aiActor.ApplyEffect(poisonEffect);
            }
        }*/
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_syringegun_shoot", gameObject);
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
                AkSoundEngine.PostEvent("Play_syringegun_reload", gameObject);
            }
        }
    }
}
