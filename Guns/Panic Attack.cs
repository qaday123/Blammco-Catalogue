using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.IO;
using JetBrains.Annotations;
using HutongGames.PlayMaker;
using static UnityEngine.UI.GridLayoutGroup;
using Brave.BulletScript;
using System.Collections;

/* NOTES: 
 * Something wrong with panic attack's passive benefit - make sequence a single coroutine instead of splitting it // after 6 hours finally fixed
*/
namespace TF2Stuff
{
    public class Panic_Attack : AdvancedGunBehavior
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:panic_attack";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Panic Attack", "panic_attack");
            Game.Items.Rename("outdated_gun_mods:panic_attack", consoleID);
            gun.gameObject.AddComponent<Panic_Attack>();

            //Gun descriptions
            gun.SetShortDescription("Don't Breath, Just Shoot.");
            gun.SetLongDescription("Entering a room will increase this gun's shooting and reload speed significiantly, decaying " +
                "within a short period of time. This effect is extended to all guns, but slightly less powerful.\n\n" +
                "Blamm.Co is not responsible for any real heart attacks this weapon may or may not actually cause. Blamm.Co is also " +
                "not responsible for affecting other weapons, causing them to exhibit symptoms of extreme anxiety. Blamm.Co is not " +
                "responsible if you don't understand where the bullets in this gun goes.");

            // Sprite setup
            gun.SetupSprite(null, "panic_attack_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 20);
            gun.SetAnimationFPS(gun.reloadAnimation, 16);
            gun.TrimGunSprites();

            // gun setup
            gun.reloadTime = 1.5f;
            gun.SetBaseMaxAmmo(180);
            gun.gunClass = GunClass.SHOTGUN;

            for (int i = 0; i < 8; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            }
            foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                //System.Random randspeed = new System.Random();
                //float speed = randspeed.Next(4, 28);
                projectileModule.ammoCost = 1;
                projectileModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                projectileModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                projectileModule.cooldownTime = 0.6f;
                projectileModule.numberOfShotsInClip = 6;
                projectileModule.angleVariance = 11f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 3f;
                projectile.baseData.speed *= 1.5f; //speed;
                projectile.baseData.range = 15f;
                projectile.baseData.force = 5f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) { projectileModule.ammoCost = 0; }
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "panic_attack";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = false;
            gun.barrelOffset.transform.localPosition += new Vector3(8f / 16f, 16f / 16f, 0);
            gun.carryPixelOffset = new IntVector2(8, 0);
            //ID = gun.PickupObjectId;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            //gun.transform.position += new Vector3(2f, 0.5f, 0f);
            gun.shellCasing = (PickupObjectDatabase.GetById(202) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 1;
            gun.shellsToLaunchOnReload = 0;
            gun.gunScreenShake = new(0.4f, 9f, 0.12f, 0.012f);

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].eventAudio = "Play_shotgun_shoot_full";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).frames[0].triggerEvent = true;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[0].eventAudio = "Play_shotgun_reload";
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.reloadAnimation).frames[0].triggerEvent = true;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        //if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
        }
        private bool HasReloaded;
        protected override void Update()
        {
            base.Update();
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
            }
        }

        private void OnEnteredCombat()
        {
            if (gun && gun.GunPlayerOwner() && gun.GunPlayerOwner().CurrentGun)
            { 
                GameManager.Instance.StartCoroutine(HandleCombatBuff(gun.GunPlayerOwner()));
            }
        }
        private IEnumerator HandleCombatBuff(PlayerController player)
        {
            StatModifier reloadspeed;
            StatModifier rateofFire;
            float timetowait;
            for (int i = 0; i < 41; i++)
            {
                Gun current_gun = player.CurrentGun;
                if (current_gun.PickupObjectId == gun.PickupObjectId)
                {
                    reloadspeed = StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, max_reload_buff + i*((1 - max_reload_buff) / 40f));
                    rateofFire = StatModifier.Create(PlayerStats.StatType.RateOfFire, StatModifier.ModifyMethod.MULTIPLICATIVE, max_shotspeed_buff - i*((max_shotspeed_buff - 1) / 40f));
                }
                else
                {
                    reloadspeed = StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, max_reload_buff_other + i*((1 - max_reload_buff_other) / 40f));
                    rateofFire = StatModifier.Create(PlayerStats.StatType.RateOfFire, StatModifier.ModifyMethod.MULTIPLICATIVE, max_shotspeed_buff_other - i*((max_shotspeed_buff_other - 1) / 40f));
                }
                player.ownerlessStatModifiers.Add(reloadspeed);
                player.ownerlessStatModifiers.Add(rateofFire);
                player.stats.RecalculateStats(player, true, false);
                if (i == 0) { timetowait = 1f; } else { timetowait = 0.1f; }
                yield return new WaitForSeconds(timetowait);
                player.ownerlessStatModifiers.Remove(reloadspeed);
                player.ownerlessStatModifiers.Remove(rateofFire);
                player.stats.RecalculateStats(player, true, false);
            }
            yield break;
        }

        public static int ID;
        protected override void OnPickup(GameActor owner)
        {
            base.OnPickup(owner);
            PlayerController player_owner = (owner as PlayerController);
            if (player_owner != null)
            {
                isprimary = true;
            }
            else
            {
                isprimary = false;
            }
            (owner as PlayerController).OnEnteredCombat += this.OnEnteredCombat;
        }
        protected override void OnPostDrop(GameActor owner)
        {
            base.OnPostDrop(owner);
            (owner as PlayerController).OnEnteredCombat -= this.OnEnteredCombat;
        }
        public override void OnDestroy()
        {
            PlayerController player;
            if (isprimary)
            {
                player = GameManager.Instance.PrimaryPlayer;
            }
            else
            {
                player = GameManager.Instance.SecondaryPlayer;
            }
            player.OnEnteredCombat -= this.OnEnteredCombat;
            base.OnDestroy();
        }
        public bool isprimary;
        public float max_reload_buff = 0.5f;
        public float max_shotspeed_buff = 2f;
        public float max_reload_buff_other = 0.7f;
        public float max_shotspeed_buff_other = 1.5f;
    }
}
