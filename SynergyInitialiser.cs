using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;

namespace ExampleMod
{
    public class SynergyInitialiser
    {
        public static void Initialise()
        {
            CustomSynergies.Add("What Could Have Been",
                new List<string>() { "qad:recon_pouch", "nail_gun" });

        }
    }
}
