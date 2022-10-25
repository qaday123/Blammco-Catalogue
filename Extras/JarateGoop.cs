using Alexandria.ItemAPI;
using Alexandria.Misc;
using ExampleMod;
using UnityEngine;

public class JarateGoop : SpecialGoopBehaviourDoer
{
    public static void Init()
    {
        EasyGoopDefinitions.JarateGoop = ScriptableObject.CreateInstance<GoopDefinition>();
        EasyGoopDefinitions.JarateGoop.CanBeIgnited = false;
        EasyGoopDefinitions.JarateGoop.damagesEnemies = false;
        EasyGoopDefinitions.JarateGoop.damagesPlayers = false;
        EasyGoopDefinitions.JarateGoop.baseColor32 = MoreColours.jarateyellow;
        EasyGoopDefinitions.JarateGoop.goopTexture = EasyGoopDefinitions.WaterGoop.goopTexture;
        //EasyGoopDefinitions.JarateGoop.CanBeElectrified = true;
        EasyGoopDefinitions.JarateGoop.usesLifespan = true;
        EasyGoopDefinitions.JarateGoop.lifespan = 30f;
        EasyGoopDefinitions.JarateGoop.name = "piss";
        GoopUtility.RegisterComponentToGoopDefinition(EasyGoopDefinitions.JarateGoop, typeof(JarateGoop));
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