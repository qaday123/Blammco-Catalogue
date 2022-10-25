using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.IO;
using static UnityEngine.UI.GridLayoutGroup;

/* NOTESSSS: 
 * Firing animation needs to feel more 'bouncy' (final frame a bit lower could do it) // DONE
 * Better player feedback for when bonus is in play
 * Going into the bonus and coming out of the bonus is delayed and it messes with the feel of the gun so much.
   I don't know an easy fix, since this is likely because of projectile travel speed messing with things
 * Idle animation should have some sort of 'flicker' on the lighter part
 * Bonus damage on fire works now! In exchange for breaking many other things...
*/  
namespace ExampleMod

{
    public class DragunsFury : GunBehaviour
    {
        public static void Add()
        {


            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Dragun's Fury", "dragfury");
            Game.Items.Rename("outdated_gun_mods:dragun's_fury", "qad:dragun's_fury");
            gun.gameObject.AddComponent<DragunsFury>();
            
            //Gun descriptions
            gun.SetShortDescription("Incendiary Cannon");
            gun.SetLongDescription("This unique take on the flamethrower spews out bouts of rage directly harvested from the heart " +
                "of the High Dragun.\n\nHitting an enemy releases the rage, and uses clever gungeon engineering to depressure the " +
                "gun and make it fire faster.");
            
            // Sprite setup
            gun.SetupSprite(null, "dragfury_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            gun.SetAnimationFPS(gun.reloadAnimation, 3);

            // gun setup

            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(125) as Gun, true, false);
            gun.reloadTime = 2f;
            gun.SetBaseMaxAmmo(180);
            gun.gunClass = GunClass.FIRE;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.cooldownTime = 0.9f;
            gun.DefaultModule.numberOfShotsInClip = 180;
            gun.DefaultModule.angleVariance = 8f;

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            projectile.baseData.damage = 8f;
            projectile.baseData.speed *= 1.4f; //speed;
            projectile.baseData.range = 6f;
            projectile.baseData.force = 8f;
            projectile.transform.parent = gun.barrelOffset;
            projectile.AppliesFire = true;
            projectile.FireApplyChance = 100f;
            projectile.AdditionalScaleMultiplier = 2f;
            projectile.fireEffect = (PickupObjectDatabase.GetById(295) as BulletStatusEffectItem).FireModifierEffect;
            PierceProjModifier pierce = projectile.gameObject.GetOrAddComponent<PierceProjModifier>();
            pierce.penetration = 0;

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            gun.encounterTrackable.EncounterGuid = "draguns fury";
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "burning hand";
            gun.barrelOffset.transform.localPosition += new Vector3(1f, 0.625f, 0);
            //gun.transform.position += new Vector3(2f, 0.5f, 0f);
            ID = gun.PickupObjectId;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD

            ETGMod.Databases.Items.Add(gun, false, "ANY");
        }
        public void Pressurisation(Gun gun) // this absolutely does not do what its supposed to do
        {
            //ETGModConsole.Log($"hitenemy:{hitenemy}");
            if (hitenemy)
            {
                gun.DefaultModule.cooldownTime /= 1.4f;
            }
            hitenemy = false;
        }
        public override void PostProcessProjectile(Projectile projectile)
        {
            //projectile.OnHitEnemy += this.OnHitEnemy;
            projectile.specRigidbody.OnPreRigidbodyCollision += this.OnHitEnemy;
            base.PostProcessProjectile(projectile); // i need pressurisation to be below here but I NEED THE GUN OBJECT
            
        }
        private void OnHitEnemy(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            hitenemy = true;
            Pressurisation(myRigidbody.projectile.PossibleSourceGun);
            if (otherRigidbody != null && otherRigidbody.aiActor != null && myRigidbody != null && myRigidbody.projectile && otherRigidbody.aiActor.healthHaver)
            {
                if (otherRigidbody.aiActor.GetEffect("fire") != null)
                {
                    myRigidbody.projectile.baseData.damage *= 3;
                }
            }
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            // Sound setup
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_dragons_fury_shoot", gameObject);
            gun.DefaultModule.cooldownTime = 0.9f;
            //ETGModConsole.Log($"HitEnemy: {hitenemy}");
            //ETGModConsole.Log($"cooldown:{gun.DefaultModule.cooldownTime}");

            /*foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                System.Random randspeed = new System.Random();
                float speed = randspeed.Next(4, 28);
                projectile.baseData.speed = speed;
            }*/
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
                AkSoundEngine.PostEvent("Play_dragons_fury_reload", base.gameObject);
                //gun.SpawnShellCasingAtPosition(new Vector3(0f, 0f, 0f));
            }
        }
        public bool hitenemy;
        public static int ID;
    }
}
