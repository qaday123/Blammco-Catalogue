using static TF2Stuff.CodeShortcuts;

/* NOTES:
*/

namespace TF2Stuff
{
    public class CAPPER : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("C.A.P.P.E.R", "capper");
            Game.Items.Rename("outdated_gun_mods:capper", "qad:capper");
            gun.gameObject.AddComponent<CAPPER>();
            
            //Gun descriptions
            gun.SetShortDescription("Pew Pew");
            gun.SetLongDescription("A trusty backup weapon that despite having a low initial capacity, " +
                "recharges its ammo over time.\n\nThe Captain's Advanced Pulsetron Particle Electromagnetic Raygun! Just like in the movie, includes the " +
                "completely-safe-not-dubious alien power source to consumer benefit! (Terms and conditions apply.)");
            
            // Sprite setup
            gun.SetupSprite(null, "capper_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 24);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(89) as Gun, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 0.8f;
            gun.DefaultModule.cooldownTime = 0.15f;
            gun.DefaultModule.numberOfShotsInClip = 12;
            gun.DefaultModule.angleVariance = 5f;
            gun.SetBaseMaxAmmo(baseMaxAmmo);
            gun.gunClass = GunClass.PISTOL;
            gun.gunSwitchGroup = "qad_capper";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "capper_shoot");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_pistol_clipin", "Play_WPN_SAA_spin_01");
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 0;
            gun.clipObject = (PickupObjectDatabase.GetById(30) as Gun).clipObject;
            gun.clipsToLaunchOnReload = 1;
            gun.shellsToLaunchOnReload = 0;
            gun.gunScreenShake = new(0.4f, 10f, 0.09f, 0.009f);
            gun.barrelOffset.transform.localPosition += new Vector3(11f / 16f, 11f / 16f, 0);
            gun.muzzleFlashEffects = VFXBuilder.CreateVFXPool("CAPPER muzzleflash VFX", GenerateFilePaths("TF2Items/Resources/MuzzleflashVFX/capper_muzzleflash/capper_muzzleflash_", 5),
                24, new(16, 13), tk2dBaseSprite.Anchor.MiddleLeft, false, 0f, alignment: VFXAlignment.Fixed);
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SMALL_BLASTER;

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;

            //Cloning
            Projectile projectile = gun.DefaultModule.projectiles[0].InstantiateAndFakeprefab();
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 6f;
            projectile.baseData.speed = 32f;
            projectile.baseData.range = 40f;
            projectile.baseData.force = 10f;
            projectile.AdditionalScaleMultiplier = 1f;
            projectile.transform.parent = gun.barrelOffset;
            projectile.hitEffects = (PickupObjectDatabase.GetById(54) as Gun).DefaultModule.projectiles[0].hitEffects;
            projectile.SetProjectileSpriteRight("capper_projectile", 13, 7, true);

            var def = projectile.sprite.CurrentSprite;
            var mats = new List<Material>() { def.material, def.materialInst };
            foreach (var mat in mats)
            {
                if (mat == null)
                    continue;
                mat.SetFloat("_EmissivePower", 1f);
            }

            CombineEvaporateEffect disintegrate = projectile.gameObject.GetOrAddComponent<CombineEvaporateEffect>();
            CombineEvaporateEffect baseEffect = (PickupObjectDatabase.GetById(519) as Gun).alternateVolley.projectiles[0].projectiles[0].GetComponent<CombineEvaporateEffect>();
            disintegrate.ParticleSystemToSpawn = baseEffect.ParticleSystemToSpawn;
            disintegrate.FallbackShader = baseEffect.FallbackShader;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
        public float regenTimePerBullet = 1;
        float currentTime = 0f;
        public static int baseMaxAmmo = 72;

        float reloadBufferTime = 0.4f;
        bool hasMultipliedMaxAmmo;
        public override void OwnedUpdatePlayer(PlayerController owner, GunInventory inventory)
        {
            base.OwnedUpdatePlayer(owner, inventory);
            if (!(gun && owner && !gun.IsFiring))
            {
                return;
            }

            if (owner.PlayerHasActiveSynergy("Sidearms Of The Future") && !hasMultipliedMaxAmmo)
            {
                gun.SetBaseMaxAmmo(baseMaxAmmo * 2);
                gun.CurrentAmmo *= 2;
                hasMultipliedMaxAmmo = true;
            }
            else if (!owner.PlayerHasActiveSynergy("Sidearms Of The Future") && hasMultipliedMaxAmmo)
            { 
                gun.SetBaseMaxAmmo(baseMaxAmmo);
                gun.CurrentAmmo /= 2;
                hasMultipliedMaxAmmo = false;
            }

            if (gun.CurrentAmmo < gun.AdjustedMaxAmmo)
            {
                if (reloadBufferTime <= 0f)
                {
                    if (currentTime < regenTimePerBullet)
                        currentTime += BraveTime.DeltaTime;
                    else
                    {
                        currentTime -= regenTimePerBullet;
                        gun.GainAmmo((owner.PlayerHasActiveSynergy("Additional Charging Capabilities") ? 2 : 1));
                    }
                }
                else
                {
                    reloadBufferTime -= BraveTime.DeltaTime;
                }
            }
        }
        public override void CanCollectAmmoPickup(PlayerController owner, Gun gun, AmmoPickup ammo, ref bool canCollect, ref bool displayAmmoFullMessage)
        {
            canCollect = false;
            displayAmmoFullMessage = true;
            base.CanCollectAmmoPickup(owner, gun, ammo, ref canCollect, ref displayAmmoFullMessage);
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            currentTime = -reloadBufferTime;
            base.OnPostFired(player, gun);
        }
    }
}
