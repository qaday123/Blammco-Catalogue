using Alexandria.ItemAPI;
using Alexandria.Misc;
using TF2Stuff;
using UnityEngine;

public class JarateGoop : SpecialGoopBehaviourDoer
{
    public static void Init()
    {
        CustomGoops.JarateGoop = ScriptableObject.CreateInstance<GoopDefinition>();
        CustomGoops.JarateGoop.CanBeIgnited = false;
        CustomGoops.JarateGoop.damagesEnemies = false;
        CustomGoops.JarateGoop.damagesPlayers = false;
        CustomGoops.JarateGoop.baseColor32 = MoreColours.jarateyellow;
        CustomGoops.JarateGoop.goopTexture = GoopUtility.WaterDef.goopTexture;
        //EasyGoopDefinitions.JarateGoop.CanBeElectrified = true;
        CustomGoops.JarateGoop.usesLifespan = true;
        CustomGoops.JarateGoop.lifespan = 30f;
        CustomGoops.JarateGoop.name = "piss";
        GoopUtility.RegisterComponentToGoopDefinition(CustomGoops.JarateGoop, typeof(JarateGoop));
    }
    public override void DoGoopEffectUpdate(DeadlyDeadlyGoopManager goop, GameActor actor, IntVector2 position)
    {
        if (actor && !(actor.aiAnimator.IsPlaying("spawn") && !actor.aiAnimator.IsPlaying("awaken")) && actor.healthHaver && !actor.healthHaver.IsBoss)
        {
            actor.ApplyEffect(StaticStatusEffects.StandardJarateEffect);
        }
        base.DoGoopEffectUpdate(goop, actor, position);
    }
}