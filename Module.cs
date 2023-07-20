using Alexandria.ItemAPI;
using Alexandria.CharacterAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using System.IO;
using UnityEngine;
using System.Collections;

/* GENERAL NOTES:
 * Sprites are not good, don't remake them now (except shortstop and forcanature those look horrific), but keep in mind for future
   if you don't give up by then (equalizer sprite need rework too)
 * TODO: Add IDs and items to subshops if needed // DONE
 * TODO: TAG EVERYTHING
*/
namespace ExampleMod
{

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInDependency("alexandria.etgmod.alexandria")]
    public class Module : BaseUnityPlugin
    {
        public const string NAME = "BlammCo Catalogue";
        public const string VERSION = "1.0.0";
        public const string GUID = "qaday.etg.blammcocatalogue";
        public static readonly string TEXT_COLOR = "#FFFF10";

        public static Module instance;


        public void Start()
        {
            instance = this;
            AudioResourceLoader.LoadFromAssembly("soundtest.bnk", "BlammCo Catalogue");
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager obj)
        {
            ETGMod.Assets.SetupSpritesFromFolder(Path.Combine(this.FolderPath(), "sprites"));

            FakePrefabHooks.Init();
            ItemBuilder.Init();

            // YTBA --> YET TO BE ADDED

            // ----- GUNS -----
            BabyFaceBlaster.Add(); // Player feedback on boost meter // SYNERGIES YTBA:
            //BlackBox.Add(); // FIND OUT HOW TO PROCESS EXPLOSION DAMAGE // SYNERGIES YTBA: "Crutch Weapon"
            Caber.Add();
            Cleaver.Add();
            //DirectHit.Add(); // SYNERGIES YTBA: "Direct Miss"
            DragunsFury.Add();
            ForceANature.Add(); // sprite bad
            InfantryHandgun.Add();
            Panic_Attack.Add();
            PipeLauncher.Add();
            PocketPistol.Add();
            Sasha.Add();
            Scattergun.Add();
            ShortCircuit.Add(); // Ammo cost, sound and charged //DONE: projectile sprite (make better when more skilled)
            Shortstop.Add();
            SodaPopper.Add(); // Actually make hype work // SYNERGIES YTBA: Dual wield with FAN
            StockRocket.Add();
            //SingleGun.Add();
            SyringeGun.Add();
            TheAgent.Add();
            Widowmaker.Add();
            Winger.Add(); // SYNERGIES YTBA: "Who's the clown now?"

            //test.Add();

            // ----- ACTIVES -----
            Buffalo_Steak.Register();
            //InvisWatch.Register(); //fix
            Jarate.Register(); // also fix // FIXEDqqqqq
            //Medigun.Register(); // finish
            Sandman.Register();
            TestActive.Register();
            Quick_Fix.Register();

            // ----- PASSIVES -----
            Bootlegger.Register();
            //Calculus.Register();
            Demoknight_Boots.Register();
            Equalizer.Register();
            ExamplePassive.Register();
            LEM_MkGRAY.Register();
            Patriots_Casket.Register(); // add rocket jumping functionality
            Powerjack.Register();
            Recon_Pouch.Register();
            Ubersaw.Register(); // ADD SLASH VFX
            //VFXTest.Register();

            // ----- GOOPS ----- thanks nn 
            //VFX Setup
            VFXToolbox.InitVFX();
            EasyVFXDatabase.Init(); //Needs to occur before goop definition

            //Status Effect Setup
            StaticStatusEffects.InitCustomEffects();
            JarateEffectSetup.Init();

            //Goop Setup
            GoopUtility.Init();
            CustomGoops.DefineGoops();
            DoGoopEffectHook.Init();
            JarateGoop.Init();

            //Characters
            var data = Loader.BuildCharacter("ExampleMod/Characters/Scout", "qaday.etg.blammcocatalogue",
                    new Vector3(15.8f, 25.3f), // position of char
                    false, // has alt skin
                     new Vector3(28.1f, 43.1f),  // position of alt skin swapper
                     false, // idk what this means
                     false, // Seperate animations w/wo armour
                     false, // Armour health
                     true, //Sprites used by paradox
                     false, //Glows
                     null, //Glow Mat
                     null, //Alt Skin Glow Mat
                     0, //Hegemony Cost
                     false, //HasPast
                     ""); //Past ID String*/

            var data2 = Loader.BuildCharacter("ExampleMod/Characters/Soldier", "qaday.etg.blammcocatalogue",
                    new Vector3(23.2f, 19f), // position of char
                    false, // has alt skin
                     new Vector3(28.1f, 43.1f),  // position of alt skin swapper
                     false, // idk what this means
                     false, // Seperate animations w/wo armour
                     false, // Armour health
                     true, //Sprites used by paradox
                     false, //Glows
                     null, //Glow Mat
                     null, //Alt Skin Glow Mat
                     0, //Hegemony Cost
                     false, //HasPast
                     ""); //Past ID String

            ETGMod.StartGlobalCoroutine(this.DelayedStartHandler());
            Log($"{NAME} v{VERSION} supplies have dropped!", TEXT_COLOR);
        }
        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
        public IEnumerator DelayedStartHandler()
        {
            yield return null;
            this.DelayedInitialisation();
            yield break;
        }
        public void DelayedInitialisation()
        {
            try
            {
                //LibramOfTheChambers.LateInit();

                //CrossmodNPCLootPoolSetup.CheckItems();

                TF2Characters.Scout = ETGModCompatibility.ExtendEnum<PlayableCharacters>(Module.GUID, "Scout");
                //ETGModConsole.Log($"{ETGModCompatibility.ExtendEnum<PlayableCharacters>(Module.GUID, "Scout")}");
                //ETGModConsole.Log("(Also finished DelayedInitialisation)");
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
            }
        }
        public void Awake() { }
    }
}
