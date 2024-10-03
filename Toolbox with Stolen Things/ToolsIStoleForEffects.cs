
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;
using System.Collections;
using System.Diagnostics;

namespace TF2Stuff
{

    //thank you nevernamed ----------- STATUS EFFECT STUFF ----------
    class StatusEffectHelper
    {
        public static GameActorCheeseEffect GenerateCheese(float length = 10f, float intensity = 50f)
        {
            GameActorCheeseEffect customCheese = new GameActorCheeseEffect
            {
                duration = length,
                TintColor = StaticStatusEffects.elimentalerCheeseEffect.TintColor,
                DeathTintColor = StaticStatusEffects.elimentalerCheeseEffect.DeathTintColor,
                effectIdentifier = "Cheese",
                AppliesTint = true,
                AppliesDeathTint = true,
                resistanceType = EffectResistanceType.None,
                CheeseAmount = intensity,

                //Eh
                OverheadVFX = StaticStatusEffects.elimentalerCheeseEffect.OverheadVFX,
                AffectsPlayers = StaticStatusEffects.elimentalerCheeseEffect.AffectsPlayers,
                AppliesOutlineTint = StaticStatusEffects.elimentalerCheeseEffect.AppliesOutlineTint,
                OutlineTintColor = StaticStatusEffects.elimentalerCheeseEffect.OutlineTintColor,
                PlaysVFXOnActor = StaticStatusEffects.elimentalerCheeseEffect.PlaysVFXOnActor,
                AffectsEnemies = StaticStatusEffects.elimentalerCheeseEffect.AffectsEnemies,
                debrisAngleVariance = StaticStatusEffects.elimentalerCheeseEffect.debrisAngleVariance,
                debrisMaxForce = StaticStatusEffects.elimentalerCheeseEffect.debrisMaxForce,
                debrisMinForce = StaticStatusEffects.elimentalerCheeseEffect.debrisMinForce,
                CheeseCrystals = StaticStatusEffects.elimentalerCheeseEffect.CheeseCrystals,
                CheeseGoop = StaticStatusEffects.elimentalerCheeseEffect.CheeseGoop,
                CheeseGoopRadius = StaticStatusEffects.elimentalerCheeseEffect.CheeseGoopRadius,
                crystalNum = StaticStatusEffects.elimentalerCheeseEffect.crystalNum,
                crystalRot = StaticStatusEffects.elimentalerCheeseEffect.crystalRot,
                crystalVariation = StaticStatusEffects.elimentalerCheeseEffect.crystalVariation,
                maxStackedDuration = StaticStatusEffects.elimentalerCheeseEffect.maxStackedDuration,
                stackMode = StaticStatusEffects.elimentalerCheeseEffect.stackMode,
                vfxExplosion = StaticStatusEffects.elimentalerCheeseEffect.vfxExplosion,
            };
            return customCheese;
        }
        public static GameActorHealthEffect GeneratePoison(float dps = 3, bool damagesEnemies = true, float duration = 4, bool affectsPlayers = true)
        {
            GameActorHealthEffect customPoison = new GameActorHealthEffect
            {
                duration = duration,
                TintColor = StaticStatusEffects.irradiatedLeadEffect.TintColor,
                DeathTintColor = StaticStatusEffects.irradiatedLeadEffect.DeathTintColor,
                effectIdentifier = "Poison",
                AppliesTint = true,
                AppliesDeathTint = true,
                resistanceType = EffectResistanceType.Poison,
                DamagePerSecondToEnemies = dps,
                ignitesGoops = false,

                //Eh
                OverheadVFX = StaticStatusEffects.irradiatedLeadEffect.OverheadVFX,
                AffectsEnemies = damagesEnemies,
                AffectsPlayers = StaticStatusEffects.irradiatedLeadEffect.AffectsPlayers,
                AppliesOutlineTint = StaticStatusEffects.irradiatedLeadEffect.AppliesOutlineTint,
                OutlineTintColor = StaticStatusEffects.irradiatedLeadEffect.OutlineTintColor,
                PlaysVFXOnActor = StaticStatusEffects.irradiatedLeadEffect.PlaysVFXOnActor,
            };
            return customPoison;
        }
        public static GameActorFireEffect GenerateFireEffect(float dps = 3, bool damagesEnemies = true, float duration = 4)
        {
            GameActorFireEffect customFire = new GameActorFireEffect
            {
                duration = duration,
                TintColor = StaticStatusEffects.hotLeadEffect.TintColor,
                DeathTintColor = StaticStatusEffects.hotLeadEffect.DeathTintColor,
                effectIdentifier = StaticStatusEffects.hotLeadEffect.effectIdentifier,
                AppliesTint = true,
                AppliesDeathTint = true,
                resistanceType = EffectResistanceType.Fire,
                DamagePerSecondToEnemies = dps,
                ignitesGoops = true,

                //Eh
                OverheadVFX = StaticStatusEffects.hotLeadEffect.OverheadVFX,
                AffectsEnemies = damagesEnemies,
                AffectsPlayers = StaticStatusEffects.hotLeadEffect.AffectsPlayers,
                AppliesOutlineTint = StaticStatusEffects.hotLeadEffect.AppliesOutlineTint,
                OutlineTintColor = StaticStatusEffects.hotLeadEffect.OutlineTintColor,
                PlaysVFXOnActor = StaticStatusEffects.hotLeadEffect.PlaysVFXOnActor,

                FlameVfx = StaticStatusEffects.hotLeadEffect.FlameVfx,
                flameBuffer = StaticStatusEffects.hotLeadEffect.flameBuffer,
                flameFpsVariation = StaticStatusEffects.hotLeadEffect.flameFpsVariation,
                flameMoveChance = StaticStatusEffects.hotLeadEffect.flameMoveChance,
                flameNumPerSquareUnit = StaticStatusEffects.hotLeadEffect.flameNumPerSquareUnit,
                maxStackedDuration = StaticStatusEffects.hotLeadEffect.maxStackedDuration,
                stackMode = StaticStatusEffects.hotLeadEffect.stackMode,
                IsGreenFire = StaticStatusEffects.hotLeadEffect.IsGreenFire,
            };
            return customFire;
        }
        public static GameActorCharmEffect GenerateCharmEffect(float duration)
        {
            GameActorCharmEffect charmEffect = new GameActorCharmEffect
            {
                duration = duration,
                TintColor = StaticStatusEffects.charmingRoundsEffect.TintColor,
                AppliesDeathTint = StaticStatusEffects.charmingRoundsEffect.AppliesDeathTint,
                AppliesTint = StaticStatusEffects.charmingRoundsEffect.AppliesTint,
                effectIdentifier = StaticStatusEffects.charmingRoundsEffect.effectIdentifier,
                DeathTintColor = StaticStatusEffects.charmingRoundsEffect.DeathTintColor,
                OverheadVFX = StaticStatusEffects.charmingRoundsEffect.OverheadVFX,
                AffectsEnemies = StaticStatusEffects.charmingRoundsEffect.AffectsEnemies,
                AppliesOutlineTint = StaticStatusEffects.charmingRoundsEffect.AppliesOutlineTint,
                AffectsPlayers = StaticStatusEffects.charmingRoundsEffect.AffectsPlayers,
                maxStackedDuration = StaticStatusEffects.charmingRoundsEffect.maxStackedDuration,
                OutlineTintColor = StaticStatusEffects.charmingRoundsEffect.OutlineTintColor,
                PlaysVFXOnActor = StaticStatusEffects.charmingRoundsEffect.PlaysVFXOnActor,
                resistanceType = StaticStatusEffects.charmingRoundsEffect.resistanceType,
                stackMode = StaticStatusEffects.charmingRoundsEffect.stackMode,
            };
            return charmEffect;
        }
        public static JarateStatusEffect GenerateJarateEffect(float duration, float dps, bool tintEnemy, Color bodyTint, bool tintCorpse, Color corpseTint)
        {
            JarateStatusEffect commonJarate = new JarateStatusEffect
            {
                duration = 15,
                effectIdentifier = "Jarate",
                resistanceType = EffectResistanceType.None,
                OverheadVFX = JarateEffectSetup.jarateVFXObject,
                AffectsEnemies = true,
                AffectsPlayers = false,
                AppliesOutlineTint = false,
                PlaysVFXOnActor = false,
                AppliesTint = tintEnemy,
                AppliesDeathTint = tintCorpse,
                TintColor = bodyTint,
                DeathTintColor = corpseTint,
            };
            return commonJarate;
        }
    }
    public class StatusEffectBulletMod : MonoBehaviour
    {
        public StatusEffectBulletMod()
        {
            pickRandom = false;
        }
        private void Start()
        {
            self = base.GetComponent<Projectile>();
            if (pickRandom)
            {
                List<StatusData> validStatuses = new List<StatusData>();
                foreach (StatusData data in datasToApply)
                {
                    if (UnityEngine.Random.value <= data.applyChance)
                    {
                        validStatuses.Add(data);
                    }
                }
                if (validStatuses.Count() > 0)
                {
                    StatusData selectedStatus = BraveUtility.RandomElement(validStatuses);
                    if (selectedStatus.applyTint) self.AdjustPlayerProjectileTint(selectedStatus.effectTint, 1);
                    self.statusEffectsToApply.Add(selectedStatus.effect);
                }
            }
            else
            {
                foreach (StatusData data in datasToApply)
                {
                    if (UnityEngine.Random.value <= data.applyChance)
                    {
                        if (data.applyTint) self.AdjustPlayerProjectileTint(data.effectTint, 1);
                        self.statusEffectsToApply.Add(data.effect);
                    }
                }
            }
        }

        private Projectile self;
        public List<StatusData> datasToApply = new List<StatusData>();
        public bool pickRandom;
        public class StatusData
        {
            public GameActorEffect effect;
            public float applyChance;
            public Color effectTint;
            public bool applyTint = false;
        }
    }
    public class StaticStatusEffects
    { // Thank you nevernamed
        //---------------------------------------BASEGAME STATUS EFFECTS
        //Fires
        public static GameActorFireEffect hotLeadEffect = PickupObjectDatabase.GetById(295).GetComponent<BulletStatusEffectItem>().FireModifierEffect;
        public static GameActorFireEffect greenFireEffect = PickupObjectDatabase.GetById(706).GetComponent<Gun>().DefaultModule.projectiles[0].fireEffect;
        public static GameActorFireEffect SunlightBurn = PickupObjectDatabase.GetById(748).GetComponent<Gun>().DefaultModule.chargeProjectiles[0].Projectile.fireEffect;


        //Freezes
        public static GameActorFreezeEffect frostBulletsEffect = PickupObjectDatabase.GetById(278).GetComponent<BulletStatusEffectItem>().FreezeModifierEffect;
        public static GameActorFreezeEffect chaosBulletsFreeze = PickupObjectDatabase.GetById(569).GetComponent<ChaosBulletsItem>().FreezeModifierEffect;

        //Poisons
        public static GameActorHealthEffect irradiatedLeadEffect = PickupObjectDatabase.GetById(204).GetComponent<BulletStatusEffectItem>().HealthModifierEffect;

        //Charms
        public static GameActorCharmEffect charmingRoundsEffect = PickupObjectDatabase.GetById(527).GetComponent<BulletStatusEffectItem>().CharmModifierEffect;

        //Cheeses
        public static GameActorCheeseEffect elimentalerCheeseEffect = (PickupObjectDatabase.GetById(626) as Gun).DefaultModule.projectiles[0].cheeseEffect;
        public static GameActorCheeseEffect instantCheese = StatusEffectHelper.GenerateCheese(10, 150);

        //Speed Changes
        public static GameActorSpeedEffect tripleCrossbowSlowEffect = (PickupObjectDatabase.GetById(381) as Gun).DefaultModule.projectiles[0].speedEffect;

        public static JarateStatusEffect StandardJarateEffect;
        //----------------------------------------CUSTOM STATUS EFFECTS
        public static void InitCustomEffects()
        {
            
        }
    }
    public class ExtremelySimpleStatusEffectBulletBehaviour : MonoBehaviour
    {
        public ExtremelySimpleStatusEffectBulletBehaviour()
        {
            this.tintColour = Color.red;
            this.useSpecialTint = false;
            this.onFiredProcChance = 1;
            this.onHitProcChance = 1;
            this.fireEffect = StaticStatusEffects.hotLeadEffect;
            this.usesFireEffect = false;
            this.usesCharmEffect = false;
            this.usesPoisonEffect = false;
            this.usesSpeedEffect = false;
            this.speedEffect = StaticStatusEffects.tripleCrossbowSlowEffect;
            this.poisonEffect = StaticStatusEffects.irradiatedLeadEffect;
            this.charmEffect = StaticStatusEffects.charmingRoundsEffect;
        }
        public GameActorFireEffect fireEffect;
        public bool usesFireEffect;
        public bool usesCharmEffect;
        public GameActorCharmEffect charmEffect;
        public GameActorHealthEffect poisonEffect;
        public bool usesPoisonEffect;
        public bool usesSpeedEffect;
        public GameActorSpeedEffect speedEffect;
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();
            if (UnityEngine.Random.value <= onFiredProcChance)
            {
                if (useSpecialTint)
                {
                    m_projectile.AdjustPlayerProjectileTint(tintColour, 2);
                }
                m_projectile.OnHitEnemy += this.OnHitEnemy;
            }
        }
        private void OnHitEnemy(Projectile bullet, SpeculativeRigidbody enemy, bool fatal)
        {
            if (enemy != null && enemy.gameActor != null && enemy.healthHaver != null && enemy.healthHaver.IsAlive)
            {
                if (UnityEngine.Random.value <= onHitProcChance)
                {
                    if (usesFireEffect) { enemy.gameActor.ApplyEffect(this.fireEffect, 1f, null); }
                    if (usesPoisonEffect) { enemy.gameActor.ApplyEffect(this.poisonEffect, 1f, null); }
                    if (usesCharmEffect) { enemy.gameActor.ApplyEffect(this.charmEffect, 1f, null); }
                    if (usesSpeedEffect) { enemy.gameActor.ApplyEffect(this.speedEffect, 1f, null); }
                }
            }
        }
        private Projectile m_projectile;
        public Color tintColour;
        public bool useSpecialTint;
        public float onFiredProcChance;
        public float onHitProcChance;
    }
    public class ExtremelySimplePoisonBulletBehaviour : MonoBehaviour
    {
        public ExtremelySimplePoisonBulletBehaviour()
        {
            this.tintColour = Color.green;
            this.useSpecialTint = true;
            this.procChance = 1;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();
            if (useSpecialTint)
            {
                m_projectile.AdjustPlayerProjectileTint(tintColour, 2);
            }
            m_projectile.OnHitEnemy += this.OnHitEnemy;
        }
        private void OnHitEnemy(Projectile bullet, SpeculativeRigidbody enemy, bool fatal)
        {
            if (enemy && enemy.gameActor && enemy.healthHaver)
            {
                if (UnityEngine.Random.value <= procChance)
                {
                    GameActorHealthEffect irradiatedLeadEffect = Gungeon.Game.Items["irradiated_lead"].GetComponent<BulletStatusEffectItem>().HealthModifierEffect;
                    GameActorHealthEffect poisonToApply = new GameActorHealthEffect
                    {
                        duration = irradiatedLeadEffect.duration,
                        DamagePerSecondToEnemies = irradiatedLeadEffect.DamagePerSecondToEnemies,
                        TintColor = tintColour,
                        DeathTintColor = tintColour,
                        effectIdentifier = irradiatedLeadEffect.effectIdentifier,
                        AppliesTint = true,
                        AppliesDeathTint = true,
                        resistanceType = EffectResistanceType.Poison,

                        //Eh
                        OverheadVFX = irradiatedLeadEffect.OverheadVFX,
                        AffectsEnemies = true,
                        AffectsPlayers = false,
                        AppliesOutlineTint = false,
                        ignitesGoops = false,
                        OutlineTintColor = tintColour,
                        PlaysVFXOnActor = false,
                    };
                    if (duration > 0) poisonToApply.duration = duration;
                    enemy.gameActor.ApplyEffect(poisonToApply, 1f, null);
                    //ETGModConsole.Log("Applied poison");
                }
            }
            else
            {
                ETGModConsole.Log("Target could not be poisoned");
            }
        }

        private Projectile m_projectile;
        public Color tintColour;
        public bool useSpecialTint;
        public float procChance;
        public int duration;
    }
}