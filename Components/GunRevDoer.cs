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
    public class GunRevDoer : MonoBehaviour
    {
        public Gun self;
        PlayerController gunOwner;
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

        public Action OnStartedRev;
        public Action OnEndedRev;
        public void Start()
        {
            self = base.GetComponent<Gun>();
            currentAmmo = self.ammo;
            gunOwner = self.GunPlayerOwner();
            _muzzleFlashHolder = self.muzzleFlashEffects;
            _shellsHolder = self.shellsToLaunchOnFire;
            doesScreenshake = self.doesScreenShake;
            self.doesScreenShake = false;
            self.muzzleFlashEffects = CodeShortcuts.Empty;
            self.shellsToLaunchOnFire = 0;
            self.PostProcessProjectile += OnFire;
            if (gunOwner)
            {
                gunOwner.GunChanged += OnGunChanged;
            }
            OnEndedRev += ResetRev;
            OnStartedRev += StartRev;
        }
        
        public void Update()
        {
            if (self && gunOwner)
            {
                if (self.IsFiring && !gunOwner.IsDodgeRolling)
                {
                    if (!isRevving && !isPostRev)
                    {
                        OnStartedRev();
                    }
                    else if (_currentSpin < RevTime)
                    {
                        if (currentAmmo != self.ammo) self.ammo = currentAmmo;
                        _currentSpin += BraveTime.DeltaTime;
                    }
                    else if (_currentSpin >= RevTime)
                    {
                        if (!isPostRev)
                        {
                            AkSoundEngine.PostEvent(RevLoopAudio + "_stop", gameObject); // only works for pretzy soundbanks
                            AkSoundEngine.PostEvent(ShootLoopAudio, gameObject);
                            self.muzzleFlashEffects = _muzzleFlashHolder;
                            self.shellsToLaunchOnFire = _shellsHolder;
                            self.doesScreenShake = doesScreenshake;
                            self.spriteAnimator.PlayFromFrame(FireLoopStartIndex);
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
                    currentAmmo = self.ammo;
                    _currentSpin = Mathf.Max(_currentSpin - BraveTime.DeltaTime, 0f);
                }
            }
        }
        public void StartRev()
        {
            AkSoundEngine.PostEvent(StartAudioMessage, gameObject);
            AkSoundEngine.PostEvent(RevLoopAudio, gameObject);
            isRevving = true;
        }
        public void ResetRev()
        {
            AkSoundEngine.PostEvent(ShootLoopAudio + "_stop", gameObject);
            AkSoundEngine.PostEvent(RevLoopAudio + "_stop", gameObject);
            AkSoundEngine.PostEvent(StartAudioMessage + "_stop", gameObject);
            AkSoundEngine.PostEvent(EndAudioMessage, gameObject);
            isRevving = false;
            isPostRev = false;
            self.doesScreenShake = false;
            self.muzzleFlashEffects = CodeShortcuts.Empty;
            self.shellsToLaunchOnFire = 0;
        }
        public void OnFire(Projectile projectile)
        {
            if (_currentSpin < RevTime)
            {
                projectile.DieInAir(suppressInAirEffects: true);
            }
        }
        private void OnGunChanged(Gun previous, Gun current, bool changed)
        {
            if (current != self && (isPostRev || isRevving))
            {
                OnEndedRev();
            }
        }

    }
}
