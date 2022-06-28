using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP.SocketIO;
using Product.UI;
using Product.Utilities;
using Product.Networking;

namespace Product {
   public class Global {
      public static GameObject InitializationRoot { get; private set; }
      public static Text InitializationText { get; private set; }
      public static Transform InitializationCircle { get; private set; }
      public static LoadingScreen LoadingCircle { get; private set; }
      public static Routine InitializationCircleRoutine { get; set; }
      public static Routine InitializationRoutine { get; private set; }

      public static float StartTime;
      public static bool ResetStarted;
      
      public static void OnAwake() {
         SocketIOBase.LogMessages = true;
         Unified.Weather.Enabled = false;

         Config.InstanceConfig = typeof(GameConfig);

         RestartController.Init(Game.GetRestartTypes());
         GameSpecific.Initialize();
         CmdArguments.Init();
         Config.Init();
         Quality.Init();
      }
      
      /** Global initialization. */
      public static Routine Init() {
         return InitializationRoutine = Routines.CreateFrameDependentRoutine(InitRoutine(), RoutineTypes.GLOBAL, true, true);
      }
      
      /** Called every frame in GlobalController. */
      public static void Update() {
         UpdateKeys();

         if(!Game.Initialized)
            return;

         UI.UI.OnUpdate();
         Game.OnUpdate();
      }

      public static IEnumerator QuitWithDelay(string text) {
         InitializationText.text = text.ToUpper();
         
         yield return new WaitForSecondsEx(5f);
         
         Application.Quit();
      }

      static void InvokeGarbageCollector() {
         GlobalObjects.Global.GetComponent<GlobalController>().GarbageCollector();
      }

      static void SetInitializationText(string text) {
         InitializationText.text = TranslationManager.GetValue("uiInitialization." + text, Casing.UPPER);
      }

      static void WaitForConfig() {
         if(Time.time - StartTime > 60f) {
            if (!ResetStarted) {
               Game.ResetWithDelay(TranslationManager.GetValue("uiInitialization.CONNECTION_TIMEOUT_MESSAGE", Casing.UPPER) + "\n\n" + TranslationManager.GetValue("uiInitialization.RESTART_MESSAGE", Casing.UPPER), 5f);

               ResetStarted = true;
            }
         }
      }

      static void UpdateKeys() {
         if(Game.Initialized) {
            // TODO: Provjeriti kako rjesit
            /*if(CmdArguments.RaceId > 0) {
               var mouseScroll = Input.GetAxis("Mouse ScrollWheel");

               float raceProgress = UIRoundManager.TimeController.ReplayTime += mouseScroll;

               if(raceProgress > 0f && raceProgress <= UIRoundManager.RaceRecorder.Replay.TotalDuration)
                  UIRoundManager.TimeController.ReplayTime = raceProgress;
            }*/
         }
         if(!Game.PlatformBuild && !Game.WebBuild) {
            // TODO: Provjeriti
            /*if(Input.GetKeyDown(KeyCode.P)) 
               UIRoundManager.RaceRecorder.Play(GlobalObjects.Race.GetComponent<RaceRecorder>().GetReplay());

            if(Input.GetKeyDown(KeyCode.L))
               UIRoundManager.RaceRecorder.PlayRandomFromBundle();*/

            if(Input.GetKeyDown(KeyCode.T))
               TranslationManager.IncrementLanguage();

            if(Input.GetKeyDown(KeyCode.O))
               UIManager.IncrementOdds();
         }
#if UNITY_STANDALONE
         if(!Game.PlatformBuild && !Game.WebBuild) {
            if(Input.GetKeyDown(KeyCode.Escape))
               Application.Quit();
         }
#endif
      }
      
      /** Initialization Routine. */
      static IEnumerator InitRoutine() {
         InitializationRoot = CustomTag.GetSingletonWithTag("Canvas/Misc/Initialization", "Initialization_Root").gameObject;
         InitializationText = CustomTag.GetSingletonWithTag("Canvas/Misc/Initialization", "Initialization_Text").GetComponent<Text>();
         InitializationCircle = CustomTag.GetSingletonWithTag("Canvas/Misc/Initialization", "Initialization_Loading");

         Routine tcInit = TranslationManager.Init();

         while(tcInit.Active)
            yield return null;

         SetInitializationText("CONNECTING_MESSAGE");

         LoadingCircle = new LoadingScreen();
         InitializationCircleRoutine = new Routine();

         if(!InitializationCircleRoutine.Active)
            InitializationCircleRoutine = Routines.CreateFrameDependentRoutine(LoadingCircle.RotateCircle(InitializationCircle, -1f), RoutineTypes.GLOBAL);

         GlobalObjects.Init();

         Unified.Weather.Initialize();

         StartTime = Time.time;
         ResetStarted = false;

         if(Game.PlatformBuild || Game.WebBuild || CmdArguments.Env != CmdArguments.Environments.NONE) {
            while(string.IsNullOrEmpty(Config.CurrentConfig.SocketIOData.Connection.Url)) {
               WaitForConfig();

               yield return null;
            }
         }

         Debug.Log("Connecting to " + Config.CurrentConfig.SocketIOData.Connection.Url);

         if(!Game.TestBuild)
            SocketIO.Init();

         if(Game.PlatformBuild || Game.WebBuild || CmdArguments.Env != CmdArguments.Environments.NONE) {
            StartTime = Time.time;

            while(SocketIOBase.Client.Manager.State != SocketManager.States.Open) {
               WaitForConfig();

               yield return null;
            }
         }
         Unified.Weather.Update();

         SetInitializationText("INITIALIZATION_MESSAGE");

         yield return GlobalObjects.Race.GetComponent<RaceController>().Init().WaitForFinish();

         // yield return GlobalObjects.Advertisement.GetComponent<AdsController>().Init().WaitForFinish();

         Routine uiInitRountine = Product.UI.UI.Init();
         
         yield return uiInitRountine.WaitForFinish();

         AssetManager.Init();

         if(!Game.TestBuild) {
            if(string.IsNullOrEmpty(Config.CurrentConfig.SocketIOData.Connection.Url)) {
               if(CmdArguments.RaceId > 0)
                  InitializationText.text = "TESTING RACE WITH ID: " + CmdArguments.RaceId;
               else {
                  SetInitializationText("START_LOCAL_MODE");

                  yield return new WaitForSecondsEx(1.5f);

                  UIRoundManager.HideInitialization();
               }
            } else {
               if(!Game.Started)
                  SetInitializationText("STARTING_MESSAGE");
            }
         }
         // TranslationManager.UpdateAllSceneStrings();
         // GameQuality.Init();

         if(CmdArguments.RaceId > 0) {
            yield return new WaitForSecondsEx(5f);

            //UIRoundManager.RaceRecorder.LoadFromFileId(CmdArguments.RaceId.ToString());

            // TODO: Ovo provjerit
            /*while(UIRoundManager.RaceRecorder.Replay.State != ReplayStates.READY)
               yield return null;

            UIRoundManager.RaceRecorder.PlayLoaded();*/

            UIRoundManager.HideInitialization();
         }

         Unified.CustomShadowsHandler.Init();
         Unified.CustomShadowsHandler.RunAsync();

         Game.Initialized = true;

         // Cleanup
         InvokeGarbageCollector();

         yield return null;
      }
   }
}
