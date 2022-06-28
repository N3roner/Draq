using System.Collections.Generic;
using Unified.Logging;
using UnityEngine;

namespace Product {
   class VDRCustomShadows : Unified.CustomShadows {
      [HideInInspector]
      private RaceController RaceController;

      public override void OnInitialize() {
         if(GlobalObjects.Race != null)
            RaceController = GlobalObjects.Race.GetComponent<RaceController>();

         if(RaceController == null)
            Log.e("CustomShadows", "RaceController not found");
      }

      public bool HasRacers() {
         return (RaceController != null) && (RaceController.LeftTrackRacer != null) && (RaceController.RightTrackRacer != null);
      }

      public override bool CanDropShadows() {
         bool weatherShadows = true;

         if(Unified.Weather.Controller != null)
            weatherShadows = Unified.Weather.Controller.TimeWeather.Weather != Weather.WeatherType.RAIN;

         return HasRacers() && (Networking.SocketIO.Stage == Networking.RoundStages.RACE) && weatherShadows;
      }

      public override Vector3 GetAverage() {
         if(HasRacers())
            return (RaceController.LeftTrackRacer.gameObject.transform.position + RaceController.RightTrackRacer.gameObject.transform.position) * 0.5f;

         return Vector3.zero;
      }

      public override void Combine() {
         if(RaceController == null)
            return;

         var leftRenderers = RaceController.LeftTrackRacer.GetComponentsInChildren<MeshRenderer>();
         var rightRenderers = RaceController.RightTrackRacer.GetComponentsInChildren<MeshRenderer>();

         var renderersList = new List<MeshRenderer>();
         renderersList.AddRange(leftRenderers);
         renderersList.AddRange(rightRenderers);

         var renderers = renderersList.ToArray();
         var shadows = new List<MeshRenderer>();

         for(int i = 0; i < renderers.Length; ++i) {
            if(renderers[i].name.Contains("shadow")) {
               Debug.LogWarning(renderers[i].name + " added to shadows");
               shadows.Add(renderers[i]);
            }
         }

         ShadowableMeshes(shadows);
      }
   }
}
