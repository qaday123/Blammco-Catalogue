using Alexandria.ItemAPI;
using Alexandria.CharacterAPI;
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
        public const string VERSION = "0.0.0";
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

            // ----- GUNS -----
            BabyFaceBlaster.Add(); // Player feedback on boost meter
            Cleaver.Add();
            DragunsFury.Add();
            ForceANature.Add(); // sprite bad
            PipeLauncher.Add();
            PocketPistol.Add();
            Sasha.Add();
            Scattergun.Add();
            ShortCircuit.Add(); // Ammo cost, sound and charged //DONE: projectile sprite (make better when more skilled)
            Shortstop.Add();
            SodaPopper.Add();
            StockRocket.Add();
            //SingleGun.Add();
            SyringeGun.Add();
            TheAgent.Add();
            Widowmaker.Add();
            Winger.Add();

            // ----- ACTIVES -----
            //InvisWatch.Register(); //fix
            Jarate.Register(); // also fix // FIXED
            Medigun.Register(); // finish
            Sandman.Register();
            TestActive.Register();
            Quick_Fix.Register();

            // ----- PASSIVES -----
            Bootlegger.Register();
            //Calculus.Register();
            Demoknight_Boots.Register();
            Equalizer.Register();
            //ExamplePassive.Register();
            Powerjack.Register();
            Recon_Pouch.Register();
            LEM_MkGRAY.Register();

            // ----- GOOPS ----- thanks nn 
            //VFX Setup
            VFXToolbox.InitVFX();
            EasyVFXDatabase.Init(); //Needs to occur before goop definition

            //Status Effect Setup
            StaticStatusEffects.InitCustomEffects();
            JarateEffectSetup.Init();

            //Goop Setup
            EasyGoopDefinitions.DefineDefaultGoops();
            DoGoopEffectHook.Init();
            JarateGoop.Init();

            //Characters
            var data = Loader.BuildCharacter("ExampleMod/Characters/Scout", "qaday.etg.blammcocatalogue",
                    new Vector3(22.3f, 27.3f),
                    false,
                     new Vector3(28.1f, 43.1f),
                     false,
                     false,
                     false,
                     true, //Sprites used by paradox
                     false, //Glows
                     null, //Glow Mat
                     null, //Alt Skin Glow Mat
                     0, //Hegemony Cost
                     false, //HasPast
                     ""); //Past ID String*/

            ETGMod.StartGlobalCoroutine(this.delayedstarthandler());
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }
        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
        public IEnumerator delayedstarthandler()
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
                ETGModConsole.Log("(Also finished DelayedInitialisation)");
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
