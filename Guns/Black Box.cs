using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using BepInEx;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Dungeonator;
using System.Linq;
using Brave.BulletScript;

/* NOTES:
 * "CRUTCH WEAPON" - Synergy with Crutch, makes all rockets homing and heals player after dealing a higher amount of damage than it's
   normal function requires.
 * HOW TO ADD EXPLOSION DAMAGE TO DAMAGE POOL
*/
namespace TF2Stuff
{
    public class BlackBox : AdvancedGunBehavior
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:black_box";

            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Black Box", "blackbox");
            Game.Items.Rename("outdated_gun_mods:black_box", consoleID);
            gun.gameObject.AddComponent<BlackBox>();
            
            //Gun descriptions
            gun.SetShortDescription("Tryhard!");
            gun.SetLongDescription("Dealing enough damage calls a random pickup item to be dropped.\n\n" +
                "Notorious for being a \"crutch\" to it's wielders, being able to last a much longer time without actually investing " +
                "time in actually improving their gunplay skills. Fortunately (or unfortunately, as some may see it), this function " +
                "as mostly been removed and replaced with something much more insignificant.");
            
            // Sprite setup
            gun.SetupSprite(null, "blackbox_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 15);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 2f;
            gun.DefaultModule.cooldownTime = 0.9f;
            gun.DefaultModule.numberOfShotsInClip = 3;
            gun.SetBaseMaxAmmo(240);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.encounterTrackable.EncounterGuid = "The Black Box";
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 25f;
            projectile.baseData.speed = 30f;
            projectile.baseData.range = 50f;
            projectile.baseData.force = 10f;
            gun.barrelOffset.transform.localPosition += new Vector3(12f/16f,14f/16f);
            gun.carryPixelOffset += new IntVector2(0, 2);
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("stock_rocket",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipfull",
                "TF2Items/Resources/CustomGunAmmoTypes/rocket/rocket_clipempty");
            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = rocketexplosion;
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public static string consoleID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_directhit_shoot", gameObject);
        }
        private bool HasReloaded;
        
        public override void PostProcessProjectile(Projectile projectile)
        {
            //projectile.OnHitEnemy += OnHitEnemy;
            
            projectile.specRigidbody.OnPreRigidbodyCollision += OnHitEnemy;
            base.PostProcessProjectile(projectile);
        }

        private void OnHitEnemy(SpeculativeRigidbody proj, PixelCollider hitbox, SpeculativeRigidbody enemy, PixelCollider otherPixelCollider)
        {
            float damage = proj.projectile.baseData.damage;

            if (enemy.healthHaver != null)
            {
                if (proj.projectile.baseData.damage <= enemy.healthHaver.currentHealth) { TotalDamage += damage; }
                else { TotalDamage += enemy.healthHaver.currentHealth; }
                //HOW TO ADD EXPLOSION DAMAGE TO DAMAGE POOL
                //ETGModConsole.Log($"Damage: {TotalDamage}");
            }

            /*RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
            List<AIActor> enemiesInRoom = new List<AIActor>();

            if (absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) != null)
            {
                foreach (AIActor m_Enemy in absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
                {
                    if ((m_Enemy != enemy.aiActor) & (Vector2.Distance(m_Enemy.CenterPosition, proj.projectile.specRigidbody.UnitCenter) <= 4f))
                    {
                        if (proj.projectile.baseData.damage <= m_Enemy.healthHaver.currentHealth) { TotalDamage += damage; }
                        else { TotalDamage += m_Enemy.healthHaver.currentHealth; }
                    }
                }
            }*/
            ETGModConsole.Log($"Damage: {TotalDamage}");

            if (TotalDamage >= 500f)
            {
                PickupIndex = UnityEngine.Random.Range(0, 8);
                SupplyDropDoer.SpawnSupplyDrop(enemy.specRigidbody.UnitCenter, PickupIDs[PickupIndex]);
                //LootEngine.SpawnItem(PickupObjectDatabase.GetById(PickupIDs[PickupIndex]).gameObject, enemy.specRigidbody.UnitCenter, Vector2.zero, 1f, false, true, false);
                //ETGModConsole.Log($"Pickup spawned.");
                TotalDamage -= 500f;
            }
            //LootEngine.SpawnItem(PickupObjectDatabase.GetById(120).gameObject, enemy.specRigidbody.UnitCenter, Vector2.zero, 1f, false, true, false);
            // ^ FIGURE OUT HOW THIS WORKS

            
        }
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
                AkSoundEngine.PostEvent("Play_rocket_reload", base.gameObject);
            }
        }
        public static ExplosionData rocketexplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 4f,
            pushRadius = 4.5f,
            damage = 15f,
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
            doScreenShake = false,
            playDefaultSFX = true,
            effect = genericLargeExplosion.effect,
            ignoreList = genericLargeExplosion.ignoreList,
            ss = genericLargeExplosion.ss,
        };
        public float TotalDamage;
        public int PickupIndex;
        public static List<int> PickupIDs = new List<int>
        {
            78, //Ammo
            600, //Spread Ammo
            565, //Glass Guon Stone
            73, //Half Heart
            85, //Heart
            120, //Armor
            224, //Blank
            67, //Key
        };
    }
}
