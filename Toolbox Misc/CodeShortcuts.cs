using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static StatModifier;

namespace TF2Stuff
{
    public static class CodeShortcuts
    {
        #region FILE PATHS
        public const int STANDARD_FILENUM_LENGTH = 3;
        public static List<string> GenerateFilePaths(string initialPath, int length)
        {
            if (!initialPath.EndsWith("_"))
            {
                initialPath += '_';
            }

            List<string> paths = new();
            for (int i = 1; i <= length; i++)
            {
                paths.Add(initialPath + NumberEnding(i));
            }
            return paths;
        }
        public static string NumberEnding(int num)
        {
            string s_num = num.ToString();
            string s = "";
            for (int i = 0; i < STANDARD_FILENUM_LENGTH - s_num.Length; i++)
            {
                s += "0";
            }
            s += s_num;
            return s;
        }
        #endregion

        #region VFX
        public static VFXPool Empty => new() { type = VFXPoolType.None, effects = new VFXComplex[0] };
        public static void DoCritEffects(this SpeculativeRigidbody body, bool doTextSquirt = true, params string[] additionalSoundEvents)
        {
            if (body && body.gameObject)
            {
                if (doTextSquirt) VFXToolbox.DoStringSquirt("Critical\nHit!", body.UnitTopCenter, Color.green);
                AkSoundEngine.PostEvent(CodeShortcuts.SelectRandomCritSound(), body.gameObject);
                foreach (string s in additionalSoundEvents)
                {
                    AkSoundEngine.PostEvent(s, body.gameObject);
                }
            }
        }
        public static string SelectRandomCritSound()
        {
            string[] sounds = { "crit_hit1", "crit_hit2", "crit_hit3", "crit_hit4", "crit_hit5" };
            return sounds[UnityEngine.Random.Range(0, sounds.Length)];
        }
        public static string SelectRandomMiniCritSound()
        {
            string[] sounds = { "crit_mini_hit1", "crit_mini_hit2", "crit_mini_hit3", "crit_mini_hit4", "crit_mini_hit5" };
            return sounds[UnityEngine.Random.Range(0, sounds.Length)];
        }
        public static void ApplyHealingAdvanced(this HealthHaver healthHaver, float health, bool doDefaultVFX = true, bool doDefaultSFX = true, List<GameObject> additionalEffects = null, params string[] additionalAudioEvents)
        {
            if (healthHaver && healthHaver.gameObject)
            {
                healthHaver.ApplyHealing(health);
                if (doDefaultSFX) 
                    AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", healthHaver.gameObject);

                foreach(string s in additionalAudioEvents) 
                    AkSoundEngine.PostEvent(s, healthHaver.gameObject);

                if (healthHaver.gameActor != null)
                {
                    if (doDefaultVFX) healthHaver.gameActor.PlayEffectOnActor((PickupObjectDatabase.GetById(73).GetComponent<HealthPickup>().healVFX), Vector3.zero, true, false, false);
                    if (additionalEffects != null)
                    {
                        foreach (GameObject obj in additionalEffects)
                        {
                            if (obj) healthHaver.gameActor.PlayEffectOnActor(obj, Vector3.zero, true, false, false);
                        }
                    }
                }
            }
        }
        #endregion
        public static int DistanceToNearestWall(this SpeculativeRigidbody body, IntVector2 pushDirection, int maxDist = 4)
        {
            if (PhysicsEngine.Instance.OverlapCast(body, null, true, false, null, null, false, null, null))
                return 0; // inside a wall
            Vector2 vector = body.transform.position.XY();
            for (int pixels = 1; pixels <= maxDist; ++pixels)
            {
                body.transform.position = vector + PhysicsEngine.PixelToUnit(pushDirection * pixels);
                body.Reinitialize();
                if (PhysicsEngine.Instance.OverlapCast(body, null, true, false, null, null, false, null, null))
                {
                    --pixels;
                    body.transform.position = vector;
                    body.Reinitialize();
                    return pixels;
                }
            }
            return -1; // too far from a wall to care
        }
        #region Stats
        // Thank pretzel :D
        public static StatModifier NewMult(this StatType s, float a) => new() { statToBoost = s, modifyType = ModifyMethod.MULTIPLICATIVE, amount = a };
        public static StatModifier NewAdd(this StatType s, float a) => new() { statToBoost = s, modifyType = ModifyMethod.ADDITIVE, amount = a };

        #endregion
        #region Guns
        public static void SetGunCooldownBar(this GameUIAmmoController uic, float fillPercentage, bool flipToLeftSide) => uic.SetGunCooldownBar(fillPercentage, flipToLeftSide, new(255, 255, 255, 255));
        public static void SetGunCooldownBar(this GameUIAmmoController uic, float fillPercentage, bool flipToLeftSide, Color32 fillColour)
        {
            if (flipToLeftSide)
                uic.SetGunCooldownBar(fillPercentage, new Vector3(0, -3f), new Vector3(2.75f, 3f), dfSpriteFlip.FlipVertical | dfSpriteFlip.FlipHorizontal, fillColour);
            else
                uic.SetGunCooldownBar(fillPercentage);
        }
        public static void SetGunCooldownBar(this GameUIAmmoController uic, float fillPercentage) => uic.SetGunCooldownBar(fillPercentage, Vector3.zero, new(123f, 3f, 0), dfSpriteFlip.None, new(255, 255, 255, 255));
        public static void SetGunCooldownBar(this GameUIAmmoController uic, float fillPercentage, Vector3 ForegroundOffset, Vector3 FillOffset, dfSpriteFlip ForegroundSpriteFlip, Color32 fillColour)
        {
            uic.GunCooldownForegroundSprite.RelativePosition = uic.GunBoxSprite.RelativePosition + ForegroundOffset; // + babyface.offset1;
            uic.GunCooldownForegroundSprite.flip = ForegroundSpriteFlip;
            uic.GunCooldownFillSprite.RelativePosition = uic.GunBoxSprite.RelativePosition + FillOffset;//+ new Vector3(123f, 3f, 0f);
            uic.GunCooldownFillSprite.Color = fillColour;
            uic.GunCooldownFillSprite.ZOrder = uic.GunBoxSprite.ZOrder + 1;
            uic.GunCooldownForegroundSprite.ZOrder = uic.GunCooldownFillSprite.ZOrder + 1;
            uic.GunCooldownFillSprite.IsVisible = true;
            uic.GunCooldownForegroundSprite.IsVisible = true;
            uic.GunCooldownFillSprite.FillAmount = fillPercentage;
        }

        #endregion
    }
}
