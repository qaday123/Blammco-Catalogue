using Alexandria.CharacterAPI;

/* GENERAL NOTES:
 * Sprites are not good, don't remake them now (except shortstop and forcanature those look horrific), but keep in mind for future
   if you don't give up by then (equalizer sprite need rework too)
 * TODO: Add IDs and items to subshops if needed // DONE
 * TODO: TAG EVERYTHING
*/
namespace TF2Stuff
{

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInDependency("alexandria.etgmod.alexandria")]
    [HarmonyPatch]
    public class Module : BaseUnityPlugin
    {
        public const string NAME = "BlammCo Catalogue";
        public const string VERSION = "2.1.0";
        public const string GUID = "qaday.etg.blammcocatalogue";
        public static readonly string TEXT_COLOR = "#FFFF10";

        public static List<string> NameShortsRemoved = new List<string>();

        public static Module instance;


        public void Start()
        {
            /*foreach (string file in Directory.GetFiles(Paths.PluginPath, "*-ccremove.spapi", SearchOption.AllDirectories))
            {
                NameShortsRemoved.AddRange(File.ReadAllLines(file).Select(x => $"player{x.ToLowerInvariant()}"));
            }
            new Harmony(GUID).PatchAll();//*/
            instance = this;
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager obj)
        {
            new Harmony(GUID).PatchAll();
            // BOTH SOUNDBANKS ARE USED (first one used wwise, 2nd used Pretzel's tool), i haven't overwritten because audio editing was done.
            AudioResourceLoader.LoadFromAssembly("soundtest.bnk", "BlammCo Catalogue");
            AudioResourceLoader.LoadFromAssembly("tf2_sounds.bnk", "BlammCo Catalogue");
            ETGMod.Assets.SetupSpritesFromFolder(Path.Combine(this.FolderPath(), "sprites"));

            FakePrefabHooks.Init();
            ItemBuilder.Init();

            // YTBA --> YET TO BE ADDED

            // ----- GUNS -----
            BabyFaceBlaster.Add(); // Player feedback on boost meter // SYNERGIES YTBA:
            BlackBox.Add(); // FIND OUT HOW TO PROCESS EXPLOSION DAMAGE // SYNERGIES YTBA: "Crutch Weapon"
            //Caber.Add(); //rework
            CAPPER.Add();
            Cleaver.Add(); //rework // pretty good now :D
            DirectHit.Add(); // SYNERGIES YTBA: "Direct Miss"
            DragunsFury.Add();
            ForceANature.Add(); // sprite bad // NOT ANYMORE
            InfantryHandgun.Add();
            LibertyLauncher.Add();
            Nailgun.Add(); // Sound broken :( // :)
            Panic_Attack.Add();
            PipeLauncher.Add();
            PocketPistol.Add();
            RocketJumper.Add();
            Sasha.Add(); //add rev time // DONE
            Scattergun.Add();
            ShortCircuit.Add(); // Ammo cost, sound and charged //DONE: projectile sprite (make better when more skilled)
            Shortstop.Add();
            SodaPopper.Add(); // Actually make hype work // SYNERGIES YTBA: Dual wield with FAN // DONE :D
            StockRocket.Add();
            //SingleGun.Add();
            SyringeGun.Add();
            TheAgent.Add();
            Widowmaker.Add();
            Winger.Add(); // SYNERGIES YTBA: "Who's the clown now?"

            //test.Add();

            // ----- ACTIVES -----
            Bonk_Soda.Register();
            Buffalo_Steak.Register();
            Disciplinary_Action.Register();
            //InvisWatch.Register(); //fix
            Jarate.Register(); // also fix // FIXEDqqqqq
            //Medigun.Register(); // finish
            Sandman.Register();
            Sandvich.Register();
            //TestActive.Register();
            Quick_Fix.Register();

            // ----- PASSIVES -----
            //Bootlegger.Register(); //rework
            //Calculus.Register();
            Candy_Cane.Register();
            Demoknight_Boots.Register();
            Equalizer.Register();
            //ExamplePassive.Register();
            Gunboats.Register();
            //LEM_MkGRAY.Register(); //rework
            Patriots_Casket.Register(); // add rocket jumping functionality // done
            Powerjack.Register();
            Recon_Pouch.Register();
            Ubersaw.Register(); // ADD SLASH VFX
            //VFXTest.Register();

            // UNFINISHED - COMMENT OUT NEXT UPDATE
            //Airstrike.Add();
            Dispenser.Register();
            

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
            var scout = Loader.BuildCharacter("TF2Items/Characters/Scout", "qaday.etg.blammcocatalogue",
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

            var doer = scout.idleDoer;
            doer.phases = new CharacterSelectIdlePhase[]
            {
                new CharacterSelectIdlePhase() { outAnimation= "crouch"},
                new CharacterSelectIdlePhase() { outAnimation = "thumbsup"},
                new CharacterSelectIdlePhase() { outAnimation = "poser"}
            }; // breach idles no workey
    
            var soldier = Loader.BuildCharacter("TF2Items/Characters/Soldier", "qaday.etg.blammcocatalogue",
                    new Vector3(23.2f, 19f), // position of char
                    false, // has alt skin
                     new Vector3(28.1f, 43.1f),  // position of alt skin swapper
                     true, // probably to get rid of certain breach features of characters?
                     false, // Seperate animations w/wo armour
                     false, // Armour health
                     true, //Sprites used by paradox
                     false, //Glows
                     null, //Glow Mat
                     null, //Alt Skin Glow Mat
                     0, //Hegemony Cost
                     false, //HasPast
                     ""); //Past ID String

            // SYNERGY SETUP
            SynergyInitialiser.Initialise();

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
        
        /*[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Start))]
        [HarmonyPostfix]
        public static void RemoveMarineHelmet(PlayerController __instance)
        {
            if (NameShortsRemoved.Contains(__instance.name.ToLowerInvariant().Replace("(clone)", "")))
            {
                __instance.lostAllArmorVFX = __instance.lostAllArmorAltVfx = null;
            }
        }*/
        public void DelayedInitialisation()
        {
            try
            {
                //LibramOfTheChambers.LateInit();

                //CrossmodNPCLootPoolSetup.CheckItems();

                TF2Characters.Scout = ETGModCompatibility.ExtendEnum<PlayableCharacters>(Module.GUID, "Scout");
                TF2Characters.Soldier = ETGModCompatibility.ExtendEnum<PlayableCharacters>(Module.GUID, "Soldier");
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
