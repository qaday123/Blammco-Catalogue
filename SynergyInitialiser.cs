using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using static Alexandria.ItemAPI.CustomSynergies;

namespace TF2Stuff
{
    public class SynergyInitialiser
    {
        public static void Initialise()
        {
            // same as CustomSynergies.Add()
            Add("I Love My Balls", new() { Sandman.consoleID }, new() { "scattershot", "helix_bullets", "flak_bullets", }); //"nn:splattershot" }); // need to check if mod enabled
            Add("Home Run All The Time", new() { Sandman.consoleID }, new() { "orbital_bullets", "mr_accretion_jr" });
            Add("The Forbidden Combo", new() { Sandman.consoleID, Cleaver.consoleID });
            Add("Additional Charging Capabilities", new() { CAPPER.consoleID }, new() { "shock_rounds", "shock_rifle" });
            Add("Monkey Mode", new() { BabyFaceBlaster.consoleID, "banana" });


            AddSynergyForm(26, Nailgun.ID, new() { "nail_gun" }, new() { Recon_Pouch.consoleID }, "What Could Have Been", isSelectable: false);

            AddDualWield(ForceANature.ID, ForceANature.consoleID, SodaPopper.ID, SodaPopper.consoleID, "Double-Barrelled Twins");
            AddDualWield(CAPPER.ID, CAPPER.consoleID, 57, "alien_sidearm", "Sidearms Of The Future");
        }
        public static void AddSynergyForm(int baseGun, int newGun, List<string> mandatoryConsoleIDs, List<string> optionalConsoleIDs, string synergy, bool isSelectable)
        {
            (PickupObjectDatabase.GetById(baseGun) as Gun).AddTransformSynergy(newGun, true, synergy, !isSelectable);
            Add(synergy, mandatoryConsoleIDs, optionalConsoleIDs);
        }
        public static void AddDualWield(int gun1, string gun1ConsoleID, int gun2, string gun2ConsoleID, string synergy)
        {
            AdvancedDualWieldSynergyProcessor gun1DUAL = (PickupObjectDatabase.GetById(gun1) as Gun).gameObject.AddComponent<AdvancedDualWieldSynergyProcessor>();
            gun1DUAL.PartnerGunID = gun2;
            gun1DUAL.SynergyNameToCheck = synergy;
            AdvancedDualWieldSynergyProcessor gun2DUAL = (PickupObjectDatabase.GetById(gun2) as Gun).gameObject.AddComponent<AdvancedDualWieldSynergyProcessor>();
            gun2DUAL.PartnerGunID = gun1;
            gun2DUAL.SynergyNameToCheck = synergy;
            Add(synergy, new()
            { gun1ConsoleID, gun2ConsoleID});
        }
    }
}
