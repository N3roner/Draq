using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Product.Utilities;
using Product.UI;

namespace Product {
   public class GameSpecific {
      public const string NAME = "VIRTUAL_DRAG";
      public const string PRODUCT_NAME = "VirtualDragRaces";
      public const string SHORT_NAME = "vdr";

      public const ProductTypes ProductType = ProductTypes.VDR;
      public static Color32 ColorOverlay = new Color32(104, 102, 88, 255);

      public static void Initialize() {
         Game.OnApplicationFocusRoutine += OnApplicationFocusRoutine;
         
         // TODO: Set proper translation manager domain Id
         TranslationManager.SetDomain("VDR.Visualization", -1);
         
         Game.Init();
      }

      public static bool IsInitialized() {
         return true;
      }

      public static IEnumerator DebugRecordGame(Action<List<DebugFrame>> callback) {
         Debug.Log("Recording session started.");

         var frames = new List<DebugFrame>();

         while(!Game.EndRecordngSession) {
            float currentFps = Mathf.Round(1f / Time.deltaTime);
            double currentTime = Utils.GetEpoch();
            RoundStates currentState = UIRoundManager.State;

            frames.Add(new DebugFrame(currentState, currentTime, currentFps));

            yield return new WaitForSecondsEx(1f);
         }
         callback(frames);

         Debug.Log("Recording session ended.");

         Game.EndRecordngSession = false;
      }

      public static void ExportRecordedSession(List<DebugFrame> frames) {
         new AsyncTask(() => {
            float averageFps = -1f;
            float totalFps = 0f;

            for(int i = 0; i < frames.Count; ++i)
               totalFps += frames[i].Fps;

            averageFps = totalFps / frames.Count;

            var builder = new StringBuilder();

            builder.AppendFormat("Recording session started at: {0}", Utils.EpochToTime((Int64)frames[0].Time, true, false));

            for(int i = 0; i < frames.Count; ++i)
               builder.AppendFormat("\n\n============== Frame {0} ==============\n\t Time: {1} \n\t State: {2} \n\t Fps: {3}", (i + 1), Utils.EpochToTime((Int64)frames[i].Time, true, false), frames[i].State, frames[i].Fps);

            builder.Append("\n\n");

            builder.AppendFormat("Recording session ended at: {0}\n", Utils.EpochToTime((Int64)frames[frames.Count - 1].Time, true, false));

            DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(frames[0].Time);
            DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(frames[frames.Count - 1].Time);

            builder.AppendFormat("\nSession duration: {0} seconds", (endDate - startDate).TotalSeconds);
            builder.AppendFormat("\nAverage fps during recording session: {0}\n", (int)averageFps);

            var states = frames.Select(x => x.State).Distinct();

            RoundStates lowest = RoundStates.NONE;
            RoundStates highest = RoundStates.NONE;

            float lowestFps = Mathf.Infinity;
            float highestFps = Mathf.NegativeInfinity;
            float lowestAvgFps = -1f;
            float highestAvgFps = -1f;

            foreach(var item in states) {
               var selected = frames.Where(frame => frame.State == item);

               int total = 0;

               averageFps = -1f;
               totalFps = 0f;

               foreach(var i in selected) {
                  totalFps += i.Fps;
                  ++total;
               }
               averageFps = totalFps / total;

               builder.AppendFormat("\nAverage fps during {0}: {1}", FormatStateToReadable(item), (int)averageFps);

               if(averageFps < lowestFps) {
                  lowest = item;

                  lowestAvgFps = averageFps;
                  lowestFps = averageFps;
               }

               if(averageFps > highestFps) {
                  highest = item;

                  highestAvgFps = averageFps;
                  highestFps = averageFps;
               }
            }
            builder.AppendFormat("\n\nFps best at {0}: {1}", FormatStateToReadable(highest), (int)highestAvgFps);
            builder.AppendFormat("\nFps lowest at {0}: {1}", FormatStateToReadable(lowest), (int)lowestAvgFps);

            string path = Path.GetFullPath(Game.DataPath + "/RecordingSessions");

            if(!Directory.Exists(path))
               Directory.CreateDirectory(path);

            File.WriteAllText(path + "/" + "Rec_Sesion_" + frames[0].Time + ".txt", builder.ToString());
         });
      }

      static string FormatStateToReadable(RoundStates toFormat) {
         switch(toFormat) {
            case RoundStates.NONE:
               return "None";
            case RoundStates.OUTRIGHT:
               return "Outright";
            case RoundStates.OFFLINE:
               return "Offline";
            case RoundStates.RACE:
               return "Race";
            case RoundStates.RESULTS:
               return "Results";
            case RoundStates.BRACKET:
               return "Bracket";
            case RoundStates.SLOW_MOTION:
               return "Slow Motion";
            case RoundStates.STOPPED:
               return "Stopped";
            case RoundStates.SUPERBONUS:
               return "Superbonus";
            case RoundStates.WEB:
               return "Web";
            case RoundStates.HEAD_TO_HEAD:
               return "HeadToHead";
            default:
               return "None";
         }
      }


      static IEnumerator OnApplicationFocusRoutine(FocusStates focusState) {
         if(Game.FocusState == focusState)
            yield break;

         Game.FocusState = focusState;

         if(Game.FocusState != FocusStates.UNKNOWN)
            Debug.Log("Application is now " + (Game.FocusState == FocusStates.FOCUSED ? "focused." : "not focused."));
         else {
            Debug.Log("Application focus is uknown.");

            yield break;
         }

         while(!Game.Initialized || Game.allSceneCameras.Length < 1)
            yield return null;

         if(Game.FocusState == FocusStates.FOCUSED)
            Unified.CustomShadowsHandler.Begin();
         else
            Unified.CustomShadowsHandler.End();

         yield return new WaitForEndOfFrame();

         for(int i = 0; i < Product.UI.UI.Canvas.childCount; ++i) {
            CanvasGroup canvasGroup = Product.UI.UI.Canvas.GetChild(i).GetComponent<CanvasGroup>();

            if(canvasGroup != null)
               canvasGroup.alpha = Game.FocusState == FocusStates.FOCUSED ? 1f : 0f;
         }

         yield return new WaitForEndOfFrame();

         Transform root = GlobalObjects.Race.transform.Find("Instances");

         var smr = Utils.GetAllComponents<Renderer>(root);

         for(int i = 0; i < smr.Length; ++i)
            smr[i].enabled = Game.FocusState == FocusStates.FOCUSED;

         yield return new WaitForEndOfFrame();

         if(Game.FocusState == FocusStates.NOT_FOCUSED)
            GL.Clear(true, true, Color.black);
      }
   }
}