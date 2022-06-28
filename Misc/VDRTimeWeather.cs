using UnityEngine;
using Product.Networking;
using Product.UI;

namespace Product 
{
   class VDRTimeWeather : TimeWeatherController {
      public override Camera GetCamera(bool immediate) {
         var camera = UIRoundManager.CameraController.InGameCamera;

         if(immediate)
            return camera;
         else {
            if(SocketIO.Stage == RoundStages.RESULTS || SocketIO.Stage == RoundStages.NEW)
               return camera;
         }

         return null;
      }
   }
}
