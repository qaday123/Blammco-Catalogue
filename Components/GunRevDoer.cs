using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using BepInEx;


namespace TF2Stuff
{
    public struct RevAudioMessages
    {
        public RevAudioMessages(string start, string end, string rev, string shoot)
        {
            StartAudioMessage = start;
            EndAudioMessage = end;
            RevLoopAudio = rev;
            ShootLoopAudio = shoot;
        }
        public string StartAudioMessage = "";
        public string EndAudioMessage = "";
        public string RevLoopAudio = "";
        public string ShootLoopAudio = "";
    }
    public class GunRevDoer : GunBehaviour
    {
        float _currentSpin = 0f;
        VFXPool _muzzleFlashHolder;
        int _shellsHolder;
        bool doesScreenshake;

        public float RevTime = 0.75f;
        public bool isRevving = false;
        public bool isPostRev = false;
        public int currentAmmo;
        public string StartAudioMessage = "";
        public string EndAudioMessage = "";
        public string RevLoopAudio = "";
        public string ShootLoopAudio = "";
        public int FireLoopStartIndex = 0;

        public float SlowDownMultiplier = 1f;
        public float minusAnimationFPS = 0f;

        public Action OnStartedRev;
        public Action OnEndedRev;
        public void Start()
        {
            currentAmmo = gun.ammo;
            _muzzleFlashHolder = gun.muzzleFlashEffects;
            _shellsHolder = gun.shellsToLaunchOnFire;
            doesScreenshake = gun.doesScreenShake;
            gun.doesScreenShake = false;
            gun.muzzleFlashEffects = CodeShortcuts.Empty;
            gun.shellsToLaunchOnFire = 0;
            if (PlayerOwner)
            {
                PlayerOwner.GunChanged += OnGunChanged;
            }
            OnEndedRev += ResetRev;
            OnStartedRev += StartRev;
        }

        public void Update()
        {
            if (gun && PlayerOwner)
            {
                if (gun.IsFiring && !PlayerOwner.IsDodgeRolling)
                {
                    if (!isRevving && !isPostRev)
                    {
                        OnStartedRev();
                    }
                    else if (_currentSpin < RevTime)
                    {
                        if (currentAmmo != gun.ammo) gun.ammo = currentAmmo;
                        _currentSpin += BraveTime.DeltaTime;
                    }
                    else if (_currentSpin >= RevTime)
                    {
                        if (!isPostRev)
                        {
                            AkSoundEngine.PostEvent(RevLoopAudio + "_stop", gameObject); // only works for pretzy soundbanks
                            AkSoundEngine.PostEvent(ShootLoopAudio, gameObject);
                            gun.muzzleFlashEffects = _muzzleFlashHolder;
                            gun.shellsToLaunchOnFire = _shellsHolder;
                            gun.doesScreenShake = doesScreenshake;
                            gun.spriteAnimator.PlayFromFrame(FireLoopStartIndex);
                            isPostRev = true;
                            isRevving = false;
                        }
                    }
                }
                else if (isPostRev || isRevving)
                {
                    OnEndedRev();
                }
                else
                {
                    currentAmmo = gun.ammo;
                    _currentSpin = Mathf.Max(_currentSpin - BraveTime.DeltaTime, 0f);
                }
            }
        }
        public void StartRev()
        {
            AkSoundEngine.PostEvent(StartAudioMessage, gameObject);
            AkSoundEngine.PostEvent(RevLoopAudio, gameObject);
            isRevving = true;
            if (SlowDownMultiplier != 1f) SlowDown("add");
        }
        public void ResetRev()
        {
            AkSoundEngine.PostEvent(ShootLoopAudio + "_stop", gameObject);
            AkSoundEngine.PostEvent(RevLoopAudio + "_stop", gameObject);
            AkSoundEngine.PostEvent(StartAudioMessage + "_stop", gameObject);
            AkSoundEngine.PostEvent(EndAudioMessage, gameObject);
            isRevving = false;
            isPostRev = false;
            gun.doesScreenShake = false;
            gun.muzzleFlashEffects = CodeShortcuts.Empty;
            gun.shellsToLaunchOnFire = 0;
            if (SlowDownMultiplier != 1f) SlowDown("remove");
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            if (_currentSpin < RevTime)
            {
                gun.GainAmmo(1);
            }
            base.OnPostFired(player, gun);
        }
        public override void PostProcessProjectile(Projectile projectile)
        {
            if (_currentSpin < RevTime)
            {
                projectile.DieInAir(suppressInAirEffects: true);
            }
            else
            {
                base.PostProcessProjectile(projectile);
            }
        }
        public void SetAudioMessages(string start, string end, string revLoop, string shootLoop)
        {
            StartAudioMessage = start;
            EndAudioMessage = end;
            RevLoopAudio = revLoop;
            ShootLoopAudio= shootLoop;
        }
        private void OnGunChanged(Gun previous, Gun current, bool changed)
        {
            if (current != gun && (isPostRev || isRevving))
            {
                OnEndedRev();
            }
        }
        public void SlowDown(string mode)
        {
            mode = mode.ToLower();
            if (mode == "add")
            {
                gun.AddStatToGun(PlayerStats.StatType.MovementSpeed, SlowDownMultiplier, StatModifier.ModifyMethod.MULTIPLICATIVE);
                PlayerOwner.stats.RecalculateStats(PlayerOwner);
                PlayerOwner.spriteAnimator.clipFps -= minusAnimationFPS;
            }
            else if (mode == "remove")
            {
                gun.RemoveStatFromGun(PlayerStats.StatType.MovementSpeed);
                PlayerOwner.stats.RecalculateStats(PlayerOwner);
                PlayerOwner.spriteAnimator.clipFps += minusAnimationFPS;
            }
        }
    }
}
