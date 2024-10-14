using Gungeon;
using Alexandria.ItemAPI;
using UnityEngine;

namespace TF2Stuff
{
    public class PipeLauncher : GunBehaviour
    {
        public static ExplosionData genericSmallExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData;
        public static ExplosionData genericLargeExplosion = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData;
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:pipe_launcher";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Pipe Launcher", "pipe");
            Game.Items.Rename("outdated_gun_mods:pipe_launcher", consoleID);
            gun.gameObject.AddComponent<PipeLauncher>();

            //Gun descriptions
            gun.SetShortDescription("Incorrect Clip Sizes");
            gun.SetLongDescription("This 6-barreled grenade launcher will launch 4 grenades in quick succession. Deals more damage on direct hit.\n\n" +
                "It seems as though it is impossible to load the correct number of grenades into the barrel. People have tried. " +
                "All have failed.");

            // Sprite setup
            gun.SetupSprite(null, "pipe_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.TrimGunSprites();

            // Gun setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(56) as Gun, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.8f;
            gun.DefaultModule.cooldownTime = 0.5f;
            gun.DefaultModule.numberOfShotsInClip = 4;
            gun.DefaultModule.angleVariance = 4f;
            gun.barrelOffset.transform.localPosition += new Vector3(0, -0.125f, 0);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.SetBaseMaxAmmo(40);
            gun.AddToSubShop(ItemBuilder.ShopType.Trorc);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.encounterTrackable.EncounterGuid = "pipelauncher";

            //Cloning
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // Projectile setup
            projectile.baseData.damage = 15f;
            projectile.baseData.speed = 20f;
            projectile.baseData.force = 5f;
            projectile.baseData.range = 12f;
            projectile.transform.parent = gun.barrelOffset;
            gun.barrelOffset.transform.localPosition += new Vector3(0.5f, 0, 0);
            projectile.SetProjectileSpriteRight("pipeprojectile", 14, 8, false, tk2dBaseSprite.Anchor.MiddleCenter);
            ETGMod.Databases.Items.Add(gun, false, "ANY");

            // Projectile Modifiers
            //gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.GRENADE;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Pipe Launcher", 
                "TF2Items/Resources/CustomGunAmmoTypes/pipelauncher/pipelauncher_clipfull", 
                "TF2Items/Resources/CustomGunAmmoTypes/pipelauncher/pipelauncher_clipempty");
            BounceProjModifier bounce = projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
            bounce.enabled = true;
            bounce.ExplodeOnEnemyBounce = true;
            bounce.numberOfBounces += 1;
            ID = gun.PickupObjectId;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD

            // IT WORKS :DDDDDDDDDDDDDDDDDDDDDDDDDDD
            ExplosiveModifier explode = projectile.gameObject.GetOrAddComponent<ExplosiveModifier>();
            explode.explosionData = pipeexplosion;
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_Grenade_launcher_shoot", gameObject);
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
                AkSoundEngine.PostEvent("Play_Grenade_launcher_drum_close", base.gameObject);
            }
        }

        public static ExplosionData pipeexplosion = new ExplosionData
        {
            useDefaultExplosion = false,
            doExplosionRing = true,
            damageRadius = 4f,
            pushRadius = 4.5f,
            damage = 20f,
            doDamage = true,
            damageToPlayer = 0,
            secretWallsRadius = 6f,
            forcePreventSecretWallDamage = false,
            doDestroyProjectiles = true,
            doForce = true,
            force = 5f,
            debrisForce = 10f,
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
        public static int ID;
    }
}
