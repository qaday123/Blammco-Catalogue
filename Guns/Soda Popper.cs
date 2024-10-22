using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker.Actions;
using MonoMod.Cil;
using System.Reflection;
using Mono.Cecil.Cil;


/* NOTES:
 * how tf do you do the active charge thingy
*/
namespace TF2Stuff
{
    public class SodaPopper : GunBehaviour
    {
        public static string consoleID;
        public static int ID;
        public static void Add()
        {
            consoleID = $"{MODPREFIX}:soda_popper";

            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Soda Popper", "sodapop");
            Game.Items.Rename("outdated_gun_mods:soda_popper", consoleID);
            gun.gameObject.AddComponent<SodaPopper>();
            
            //Gun descriptions
            gun.SetShortDescription("I'm Not Even Winded!");
            gun.SetLongDescription("Builds hype by dealing damage. Hype is activated when reloading on a full clip, allowing you to dodge roll midair up to 7 times. " +
                "Hype can be activated at any charge level while in combat, or only while full out of combat. Hype does not drain out of combat. " +
                "\n\nIt's Like the Force-A-Nature's less arrogant, more competent sibling. More consistent, less shove-y and reloads faster." +
                "\n\nI wonder if there's still liquid from the can taped to the grip...");
            
            // Sprite setup
            gun.SetupSprite(null, "sodapop_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 20);
            gun.SetAnimationFPS(gun.reloadAnimation, 14);
            gun.TrimGunSprites();

            // gun setup
            gun.reloadTime = 0.8f;
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.SHOTGUN;
            gun.UsesRechargeLikeActiveItem = true;
            gun.ActiveItemStyleRechargeAmount = 0f;

            for (int i = 0; i < 5; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(86) as Gun, true, false);
            }
            foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                //System.Random randspeed = new System.Random();
                //float speed = randspeed.Next(20, 31);
                projectileModule.ammoCost = 1;
                projectileModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                projectileModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                projectileModule.cooldownTime = 0.3125f;
                projectileModule.numberOfShotsInClip = 2;
                projectileModule.angleVariance = 15f;
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(projectileModule.projectiles[0]);
                projectileModule.projectiles[0] = projectile;
                projectile.baseData.damage = 5f;
                projectile.baseData.speed = 26f;
                projectile.baseData.range = 9f;
                projectile.baseData.force = 4f;
                projectile.transform.parent = gun.barrelOffset;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
                if (projectileModule != gun.DefaultModule) {projectileModule.ammoCost = 0;}
            }
            
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SHOTGUN;
            gun.Volley.UsesShotgunStyleVelocityRandomizer = true;
            gun.Volley.IncreaseFinalSpeedPercentMax = 30f;
            gun.Volley.DecreaseFinalSpeedPercentMin = -30f;
            gun.barrelOffset.transform.localPosition = new Vector3(22f/16f, 6f/16f, 0);
            gun.gunSwitchGroup = "qad_doublebarrel"; //(PickupObjectDatabase.GetById(541) as Gun).gunSwitchGroup; // GET RID OF THAT CURSED DEFAULT RELOAD
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "Play_scatter_gun_double_shoot");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_scatter_gun_double_tube_close");
            gun.carryPixelOffset += new IntVector2(4,1);
            gun.shellCasing = (PickupObjectDatabase.GetById(202) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 2;
            gun.reloadShellLaunchFrame = 3;
            gun.gunScreenShake = new(0.4f, 14f, 0.09f, 0.009f);
            gun.gameObject.AddComponent<SodaPopperDisplay>();
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(1) as Gun).muzzleFlashEffects;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public float _hype = MAX_HYPE / 2;
        public const float MAX_HYPE = 100;
        public float rateOfDecay = 15f;
        public bool effectActive = false;
        public float hypeGainMult = 1f;

        List<StatModifier> statModifiers = new()
        {
            StatType.DodgeRollDamage.NewMult(3f),
        };
        public override void OnReloadPressed(PlayerController player, Gun gun, bool manual)
        {
            base.OnReloadPressed(player, gun, manual);
            if (!gun.IsReloading && !effectActive && ((PlayerOwner.IsInCombat && _hype != 0) || (_hype == MAX_HYPE)))
            {
                PlayerOwner.StartCoroutine(DoHypeEffect());
            }
        }
        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += OnEnemyHit;
            base.PostProcessProjectile(projectile);
        }
        public override void Update()
        {
            if (gun && PlayerOwner)
            {
                if (PlayerOwner.PlayerHasActiveSynergy("Kinetic Energy") && !effectActive)
                {
                    AddHype(PlayerOwner.Velocity.magnitude * BraveTime.DeltaTime);
                }
            }
            base.Update();
        }
        public void OnEnemyHit(Projectile projectile, SpeculativeRigidbody enemy, bool fatal) => AddHype(projectile.baseData.damage);
        public void AddHype(float amount)
        {
            if (_hype < MAX_HYPE)
            {
                _hype += amount * hypeGainMult;
                if (_hype >= MAX_HYPE && !effectActive)
                {
                    AkSoundEngine.PostEvent("recharged", base.gameObject);
                    _hype = MAX_HYPE;
                }
            }
        }
        public void OnDodgeRoll(PlayerController player)
        {
            if (PlayerOwner.IsDodgeRolling)
                AkSoundEngine.PostEvent("triple_jump", gameObject);
        }
        public IEnumerator DoHypeEffect()
        {
            Shader old = gun.sprite.renderer.material.shader;
            EnableDisableEffect(true);
            Gun currentGun = gun;
            Gun dualWieldGun = null;
            while (_hype > 0 && (PlayerOwner.IsInCombat || _hype == MAX_HYPE))
            {
                if (PlayerOwner.IsInCombat)
                    _hype -= BraveTime.DeltaTime * rateOfDecay;

                if (PlayerOwner.CurrentGun != currentGun)
                {
                    EnableDisableVFX(false, currentGun);
                    EnableDisableVFX(true, PlayerOwner.CurrentGun);

                    currentGun = PlayerOwner.CurrentGun;
                }
                if (PlayerOwner.inventory.DualWielding)
                {
                    EnableDisableVFX(true, PlayerOwner.CurrentSecondaryGun);
                    dualWieldGun = PlayerOwner.CurrentSecondaryGun;
                }
                else if (!PlayerOwner.inventory.DualWielding && dualWieldGun != null)
                {
                    EnableDisableVFX(false, dualWieldGun);
                    dualWieldGun = null;
                }
                yield return null;
            }
            EnableDisableEffect(false);
            EnableDisableVFX(false, currentGun);
            if (dualWieldGun) EnableDisableVFX(false, dualWieldGun);
            if (_hype < 0) _hype = 0;
            gun.sprite.renderer.material.shader = old;
           
            yield break;
        }
        public void ChangeJumps(ref int jumps) => jumps += 7;
        public void EnableDisableEffect(bool enable)
        {
            EnableDisableVFX(enable, gun);
            if (enable)
            {
                PlayerOwner.OnPreDodgeRoll += OnDodgeRoll;
                PlayerOwner.TF2PlayerExtension().ModifyMaxRollDepth += ChangeJumps;
                PlayerOwner.ownerlessStatModifiers.AddRange(statModifiers);
                PlayerOwner.stats.RecalculateStats(PlayerOwner);
                AkSoundEngine.PostEvent("Play_whip_power_up", gameObject);
                hypeGainMult /= 2f;
            }
            else
            {
                PlayerOwner.OnPreDodgeRoll -= OnDodgeRoll;
                PlayerOwner.TF2PlayerExtension().ModifyMaxRollDepth -= ChangeJumps;
                foreach (StatModifier modifier in statModifiers) 
                    PlayerOwner.ownerlessStatModifiers.Remove(modifier);
                PlayerOwner.stats.RecalculateStats(PlayerOwner);
                if (effectActive) AkSoundEngine.PostEvent("Play_whip_power_down", gameObject);
                hypeGainMult *= 2f;
            }
            effectActive = enable;
        }
        public void EnableDisableVFX(bool enable, Gun gunForVFX)
        {
            float[] glowPower = { 0, 75 };
            Color[] glowColours = { Color.clear, Color.magenta };
            float[] emissiveColorPowers = { 0, 1.55f };
            int Index = Convert.ToInt32(enable);

            gunForVFX.sprite.usesOverrideMaterial = enable;
            gunForVFX.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            gunForVFX.sprite.renderer.material.SetColor("_OverrideColor", glowColours[Index]);//new Color32(255, 25, 232, 255));
            gunForVFX.sprite.renderer.material.SetFloat("_EmissivePower", glowPower[Index]);
            gunForVFX.sprite.renderer.material.SetFloat("_EmissiveColorPower", emissiveColorPowers[Index]);//new Color(255, 25, 232));//255 / 256f, 25 / 256f, 232 / 256f));

        }
        public override void DisableEffectPlayer(PlayerController player)
        {
            EnableDisableEffect(false);
            base.DisableEffectPlayer(player);
        }
        private class SodaPopperDisplay : CustomAmmoDisplay
        {
            Gun _gun;
            SodaPopper _soda;
            PlayerController _owner;
            float half_cycle = 1.2f;
            float _cyclePos = 0f;
            public void Start()
            {
                _gun = base.GetComponent<Gun>();
                _soda = _gun.GetComponent<SodaPopper>();
                _owner = _gun.GunPlayerOwner();
            }
            public override bool DoCustomAmmoDisplay(GameUIAmmoController uic)
            {
                if (!_owner || !_soda || !_gun) return false;

                Color32 FillColour = Color.white;
                float fillPercent = _soda._hype / SodaPopper.MAX_HYPE;
                if (fillPercent == 1)
                {
                    if (_cyclePos < half_cycle) _cyclePos += BraveTime.DeltaTime;
                    if (_cyclePos >= half_cycle) _cyclePos -= 2 * half_cycle; // make it negative so its magnitude decreases :)
                    
                    FillColour = Color32.Lerp(Color.red, new Color32(160, 0, 0, 255), Mathf.Abs(_cyclePos) / half_cycle);
                }
                uic.SetGunCooldownBar(fillPercent, true, FillColour);
                uic.GunAmmoCountLabel.Text = $"[color #ff19e8] Hype[sprite \"ui_down_arrow\"] [/color]{this._owner.VanillaAmmoDisplay()}";
                return true;
            }
        }
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.CheckDodgeRollDepth))]
        private static class DodgeRollDepthPatch
        {
            public static void ModifyDepthValue(ref int current, PlayerController player)
            {
                //if (PassiveItem.IsFlagSetForCharacter(player, typeof(YourItemFlag)))
                //    current++;
                //ETGModConsole.Log("Before: " + current);
                player.TF2PlayerExtension().ModifyMaxRollDepth?.Invoke(ref current);
                //ETGModConsole.Log($"After: {current}");
            }
            [HarmonyILManipulator]
            private static void DodgeRollDepthIL(ILContext IL)
            {
                var cursor = new ILCursor(IL);

                if (cursor.TryGotoNext(MoveType.After,
                      instr => instr.MatchStloc(1)))
                {
                    cursor.Emit(OpCodes.Ldloca_S, (byte)1);
                    cursor.Emit(OpCodes.Ldarg_0); // 1st parameter is player
                    cursor.Emit(OpCodes.Call, typeof(DodgeRollDepthPatch).GetMethod(
                      nameof(DodgeRollDepthPatch.ModifyDepthValue), BindingFlags.Static | BindingFlags.Public)); // mmm
                }
            }
        }
    }
}
