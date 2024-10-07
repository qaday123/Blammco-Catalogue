using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using Alexandria.VisualAPI;
using Alexandria.BreakableAPI;
using BepInEx;
using System.Collections.Generic;


namespace TF2Stuff
{
    public class BasicGun : AdvancedGunBehavior
    {
        public static string internalName;
        public static int ID;
        public static void Add()
        {
            /* This basic template is a bare bones version of the Template Gun to provide a quick framework for making a gun without all of the clutter. */

            string FULLNAME = "Basic Gun";
            string SPRITENAME = "basicgun";
            internalName = $"{Module.MODPREFIX}:{FULLNAME.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<TemplateGun>();
            gun.SetShortDescription("I made a basic gun!");
            gun.SetLongDescription("A basic framework to build guns on.  Like a gun skeleton to give gun muscles and gun blood before draping in gun skin.");
            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 15);
            gun.SetAnimationFPS(gun.shootAnimation, 15);
            gun.SetAnimationFPS(gun.reloadAnimation, 15);
            tk2dSpriteAnimationClip clip = gun.spriteAnimator.GetClipByName($"{SPRITENAME}_intro");
            gun.SetAnimationFPS(gun.introAnimation, 15);

            /* List of IDs and names: https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects: https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(56) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(56) as Gun).muzzleFlashEffects;

            /* List of default sound files: https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./sound-list
             * Instructions on setting up custom sound files: https://mtgmodders.gitbook.io/etg-modding-guide/misc/using-custom-sounds */
            gun.gunSwitchGroup = $"{Module.MODPREFIX}_{FULLNAME.ToID()}";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "Play_WPN_smileyrevolver_shot_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_WPN_crossbow_reload_01");
            gun.DefaultModule.angleVariance = 5;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.gunClass = GunClass.RIFLE;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 15;
            gun.SetBaseMaxAmmo(150);
            gun.gunHandedness = GunHandedness.AutoDetect;
            gun.carryPixelOffset += new IntVector2(0, 0);
            gun.barrelOffset.transform.localPosition += new Vector3(0.75f, 0.2f);

            /* List of IDs and names: https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects: https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            Projectile projectile = gun.DefaultModule.projectiles[0].InstantiateAndFakeprefab();
            gun.DefaultModule.projectiles[0] = projectile;

            projectile.baseData.damage = 5f;
            projectile.baseData.speed = 25f;
            projectile.baseData.range = 100f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;

            projectile.SetProjectileSpriteRight($"{SPRITENAME}_projectile_001", 20, 8, true, tk2dBaseSprite.Anchor.MiddleCenter, 18, 6);

            gun.quality = PickupObject.ItemQuality.B;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
    }
}