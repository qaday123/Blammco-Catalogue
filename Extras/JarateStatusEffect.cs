using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    public class JarateEffectSetup
    {
        public static List<string> JarateFilePath = new List<string>()
        {
            "ExampleMod/Resources/StatusEffectVFX/jarate_effect_icon",
        };
        public static GameObject jarateVFXObject;
        public static void Init()
        {
            JarateStatusEffect StandJarate = StatusEffectHelper.GenerateJarateEffect(100, 2, true, MoreColours.jarateyellow, true, MoreColours.jarateyellow);
            StaticStatusEffects.StandardJarateEffect = StandJarate;
            jarateVFXObject = SpriteBuilder.SpriteFromResource("ExampleMod/Resources/StatusEffectVFX/jarate_effect_icon", new GameObject("JarateIcon"));
            jarateVFXObject.SetActive(false);
            tk2dBaseSprite vfxSprite = jarateVFXObject.GetComponent<tk2dBaseSprite>();
            vfxSprite.GetCurrentSpriteDef().ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerCenter, vfxSprite.GetCurrentSpriteDef().position3);
            FakePrefab.MarkAsFakePrefab(jarateVFXObject);
            UnityEngine.Object.DontDestroyOnLoad(jarateVFXObject);
        }
    }
    public class JarateStatusEffect : GameActorEffect // ok i stole most of this from nn (he's even doing the jarate item rn lmao)
    {
        public static List<string> JarateFilePath = new List<string>()
        {
            "ExampleMod/Resources/StatusEffectVFX/jarate_effect_icon",
        };
        public static GameObject jarateVFXObject;
        public JarateStatusEffect()
        {
            this.TintColor = MoreColours.jarateyellow;
            this.DeathTintColor = MoreColours.jarateyellow;
            this.AppliesTint = false;
            this.AppliesDeathTint = false;
        }

        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            base.OnEffectApplied(actor, effectData, partialAmount);
            if (actor.aiActor)
            {
                actor.healthHaver.AllDamageMultiplier += 0.75f;
            }
        }
        public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            base.OnEffectRemoved(actor, effectData);
            if (actor.aiActor)
            {
                actor.healthHaver.AllDamageMultiplier -= 0.75f;
            }
        }
    }
}