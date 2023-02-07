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
 * "THE DIRECT MISS" - Scattershot synergy, all rockets fired actively avoid enemies, hitting an enemy causes many homing, 
   bouncing and piercing rockets to fire everywhere
 * 
*/
namespace ExampleMod
{
    public class DirectHit : AdvancedGunBehavior
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Direct Hit", "directhit");
            Game.Items.Rename("outdated_gun_mods:direct_hit", "qad:direct_hit");
            gun.gameObject.AddComponent<DirectHit>();
            
            //Gun descriptions
            gun.SetShortDescription("The Best Melee Weapon");
            gun.SetLongDescription("Diabolical Instant Rocket Ejector Conquers Temples by Hunting Incompetent Targets.\n\n" +
                "Fires a fast rocket with a small explosion radius that does tons of damage. Kills cause up to 2 rockets to fire at the nearest enemies.\n\n" +
                "A powerful weapon praised by the most skilled of gunslingers from where it came, though sheer incompetency led to " +
                "it developing many nicknames such as \"The Direct Miss\" or \"The Best Melee Weapon Ever\". Fortunately, in " +
                "a Gungeon setting, the users are actually good at aiming their guns, making it a formidable force to many.");
            
            // Sprite setup
            gun.SetupSprite(null, "directhit_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 14);
            gun.SetAnimationFPS(gun.reloadAnimation, 1);

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(157) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 2f;
            gun.DefaultModule.cooldownTime = 0.7f;
            gun.DefaultModule.numberOfShotsInClip = 4;
            gun.SetBaseMaxAmmo(120);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD NOISE

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.A;
            gun.encounterTrackable.EncounterGuid = "Direct Hit";
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 45f;
            projectile.baseData.speed = 41f;
            projectile.baseData.range = 50f;
            projectile.baseData.force = 18f;
            projectile.ignoreDamageCaps = true;
            gun.barrelOffset.transform.localPosition += new Vector3(12f/16f,14f/16f);
            gun.carryPixelOffset += new IntVector2(0, 2);
            projectile.transform.parent = gun.barrelOffset;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 22, 6, false, tk2dBaseSprite.Anchor.MiddleLeft, 28, 6);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("stock_rocket",
                "ExampleMod/Resources/CustomGunAmmoTypes/rocket/rocket_clipfull",
                "ExampleMod/Resources/CustomGunAmmoTypes/rocket/rocket_clipempty");
            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = rocketexplosion;
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_directhit_shoot", gameObject);
        }
        private bool HasReloaded;
        
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += OnHitEnemy;
            base.PostProcessProjectile(projectile);
        }

        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            //Projectile projectile = this.projectile;
            //ETGModConsole.Log($"{proj.Direction}, {proj.Direction.ToAngle()}");


            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(this.gun.DefaultModule.projectiles[0]);
            projectile.baseData.damage = 40f;
            projectile.baseData.speed = 41f;
            projectile.baseData.range = 50f;
            projectile.baseData.force = 9f;
            projectile.ignoreDamageCaps = true;
            projectile.baseData.UsesCustomAccelerationCurve = false;
            projectile.SetProjectileSpriteRight("rocket_projectile", 11, 3, false, tk2dBaseSprite.Anchor.MiddleLeft, 12, 2);
            projectile.AdditionalScaleMultiplier = 0.5f;

            //List<PixelCollider> colliders = enemy.specRigidbody.PixelColliders;
            PixelCollider hitbox = enemy.specRigidbody.GetPixelCollider(ColliderType.HitBox);

            //Vector2 projdirection = proj.Direction;
            RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
            //List<AIActor> enemiesInRoom = new List<AIActor>();
            List<float> enemydistance = new List<float>();
            List<Vector2> enemyvectordistance = new List<Vector2>();
            int closest_distance_index = 0;
            int second_closest_distance_index = 1;

            if (absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) != null)
            {
                foreach (AIActor m_Enemy in absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
                {
                    //enemiesInRoom.Add(m_Enemy);
                    if (m_Enemy != enemy.aiActor)
                    {
                        enemydistance.Add(Vector2.Distance(enemy.sprite.WorldCenter, m_Enemy.CenterPosition));
                        enemyvectordistance.Add(m_Enemy.CenterPosition - enemy.sprite.WorldCenter);
                    }
                }

                if (enemydistance.Count > 0)
                {
                    closest_distance_index = enemydistance.IndexOf(enemydistance.Min());
                    float second_closest_distance = 0f;

                    if (enemydistance.Count > 1)
                    {
                        foreach (float distance in enemydistance)
                        {
                            if ((enemydistance.IndexOf(distance) != 0 | distance < second_closest_distance) & distance != enemydistance.Min())
                            {
                                second_closest_distance = distance;
                            }
                        }
                        second_closest_distance_index = enemydistance.IndexOf(second_closest_distance);
                    }
                }
            }

            if (fatal)
            {
                ETGModConsole.Log($"{enemydistance.Count}, {closest_distance_index}, {second_closest_distance_index}");
                if (enemydistance.Count > 0)
                {
                    Vector2 offset = enemyvectordistance[closest_distance_index].ToAngle().DegreeToVector2();
                    //ETGModConsole.Log($"Angle: {enemyvectordistance[closest_distance_index].ToAngle()}, Vector: {hitbox.ManualWidth/16f},{hitbox.ManualHeight/16f} -> {new Vector2(offset[0] * hitbox.ManualWidth / 16f, offset[1] * hitbox.ManualHeight / 16)}");
                    GameObject gameObject1 = SpawnManager.SpawnProjectile(projectile.gameObject, enemy.sprite.WorldCenter + new Vector2(offset[0] * hitbox.ManualWidth / 16f, offset[1] * hitbox.ManualHeight / 16f), Quaternion.Euler(0f, 0f, enemyvectordistance[closest_distance_index].ToAngle()));
                    ETGModConsole.Log("Projectile 1 successfully spawned");
                }
                if (enemydistance.Count > 1 & second_closest_distance_index >= 0)
                {
                    Vector2 offset = enemyvectordistance[second_closest_distance_index].ToAngle().DegreeToVector2();
                    //ETGModConsole.Log($"Angle: {enemyvectordistance[second_closest_distance_index].ToAngle()}, Vector: {hitbox.ManualWidth/16f}, {hitbox.ManualHeight/16f} -> {new Vector2(offset[0] * hitbox.ManualWidth / 16, offset[1] * hitbox.ManualHeight / 16)}");
                    GameObject gameObject2 = SpawnManager.SpawnProjectile(projectile.gameObject, enemy.sprite.WorldCenter + new Vector2(offset[0] * hitbox.ManualWidth / 16f, offset[1] * hitbox.ManualHeight / 16f), Quaternion.Euler(0f, 0f, enemyvectordistance[second_closest_distance_index].ToAngle()));
                    ETGModConsole.Log("Projectile 2 successfully spawned");
                }
                ETGModConsole.Log("Projectiles successfully spawned\n");
            }
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
            doExplosionRing = false,
            damageRadius = 1f,
            pushRadius = 1.2f,
            damage = 10f,
            doDamage = true,
            damageToPlayer = 0,
            secretWallsRadius = 2f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 4f,
            debrisForce = 8f,
            preventPlayerForce = false,
            explosionDelay = 0f,
            usesComprehensiveDelay = false,
            comprehensiveDelay = 0f,
            doScreenShake = false,
            playDefaultSFX = true,
            effect = genericSmallExplosion.effect,
            ignoreList = genericSmallExplosion.ignoreList,
            ss = genericSmallExplosion.ss,
        };
    }
}
