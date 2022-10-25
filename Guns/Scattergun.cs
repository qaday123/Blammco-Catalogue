using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.IO;
using JetBrains.Annotations;
using HutongGames.PlayMaker;

/* NOTES: 
 * Shooting anim looks weird. fix
*/
namespace ExampleMod
{
    public class Scattergun : GunBehaviour
    {
        public static void Add()
        {
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Scattergun", "scatgun");
            Game.Items.Rename("outdated_gun_mods:scattergun", "qad:scattergun");
            gun.gameObject.AddComponent<Scattergun>();
            
            //Gun descriptions
            gun.SetShortDescription("Meatshot!");
            gun.SetLongDescription("Hitting all pellets on an enemy will deal extra damage.\n\n" +
                "A peculiar double-barrelled lever-action sawed-off shotgun that is very effective close up, or when all the " +
                "bullets hit the enemy in some other fashion.");
            
            // Sprite setup
            gun.SetupSprite(null, "scatgun_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 18);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);

            // gun setup
            gun.reloadTime = 1.3f;
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.SHOTGUN;

            for (int i = 0; i < 5; i++)
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
                projectileModule.angleVariance = 12f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 4f;
                projectile.baseData.speed *= 1f; //speed;
                projectile.baseData.range = 9f;
                projectile.baseData.force = 8f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;
            gun.encounterTrackable.EncounterGuid = "scattergun";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.DecreaseFinalSpeedPercentMin = -45f;
            gun.Volley.IncreaseFinalSpeedPercentMax = 20f;
            gun.barrelOffset.transform.localPosition += new Vector3(0, 0.375f, 0);
            ID = gun.PickupObjectId;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            //gun.transform.position += new Vector3(2f, 0.5f, 0f);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
        }
        //if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += this.OnHitEnemy;
            base.PostProcessProjectile(projectile);
            //ETGModConsole.Log("Proj hit");
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON) { floormult = floormults[0]; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.SEWERGEON) { floormult = floormults[1]; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON) { floormult = floormults[2]; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATHEDRALGEON) { floormult = floormults[3]; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON) { floormult = floormults[4]; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.RATGEON || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON) { floormult = floormults[5]; }
            else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON) { floormult = floormults[6]; }
            else { floormult = 1.5f; } // this is real inefficient but idk how to make it check every floor cuz idk how to activate the
                                       // OnEnteredNewFloor event

            bullethits++;
            //ETGModConsole.Log($"bullethits: {bullethits}");
            //gun.GainAmmo(1);
            if (bullethits == 5)
            {
                if (enemy && enemy.healthHaver && !fatal)
                {
                    //enemy.projectile.baseData.damage *= 4*floormult; //this doesnt work
                    enemy.healthHaver.ApplyDamage(10*floormult,Vector2.zero, "oh shit scattergun did something", CoreDamageTypes.None, DamageCategory.Normal, true, null, false);
                    //ETGModConsole.Log("Damage applied successfully");
                }
            }
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_scatter_gun_shoot", gameObject);

            /*foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                System.Random randspeed = new System.Random();
                float speed = randspeed.Next(4, 28);
                projectile.baseData.speed = speed;
            }*/
            bullethits = 0;
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
                AkSoundEngine.PostEvent("Play_scatter_gun_reload", base.gameObject);
                //gun.SpawnShellCasingAtPosition(new Vector3(0f, 0f, 0f));
            }
        }

        public int bullethits;
        public float curdamage;
        public float floormult;
        public float[] floormults = new float[]
        {
            1f, //keep
            1.3f, // oub
            1.25f, // gungeon
            1.6f, // abbey
            1.5f, // mine
            1.7f, // hollow, R&G, rat
            1.8f, // forge and hell
        };
        public string oldfloor;
        public string curfloor;
        public static int ID;
    }
}
