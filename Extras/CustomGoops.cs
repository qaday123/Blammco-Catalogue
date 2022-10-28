using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.Misc;

namespace ExampleMod
{
    public class CustomGoops
    {
        public static GoopDefinition JarateGoop;
        public static void DefineGoops()
        {
            // JARATE GOOP - oh god what am i doing please help me
            JarateGoop = new GoopDefinition
            {
                damagesEnemies = false,
                damagesPlayers = false,
                baseColor32 = MoreColours.jarateyellow,
                goopTexture = GoopUtility.WaterDef.goopTexture,
                lifespan = 10,
                usesLifespan = true,
                //CanBeElectrified = true,
            };
        }
    }
}
