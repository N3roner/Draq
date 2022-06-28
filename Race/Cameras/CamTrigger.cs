using UnityEngine;
using System.Collections.Generic;

public class CamTrigger : MonoBehaviour {
   CamController camController;
   CamDefiner containingCam;
   int camIndex;
   List<CamDefiner> definedCams;

   void OnDrawGizmosSelected() {
      if(Application.isPlaying)
         return;

      if(camController == null)
         camController = GetComponentInParent<CamController>();
      camController.InitCams();
      definedCams = camController.GetDefinedCams();

      for(int i = 0; i < definedCams.Count; ++i) {
         if(definedCams[i].NextCamTrigger != null && definedCams[i].NextCamTrigger.name == gameObject.name) {
            containingCam = definedCams[i];
            camIndex = i;
         }
      }

      if(containingCam == null)
         return;

      Gizmos.color = Color.red;
      Gizmos.DrawSphere(transform.position, 1f);
      Gizmos.DrawLine(transform.position, containingCam.transform.position);
      if(camIndex > 0)
         if(camIndex - 1 < definedCams.Count) {
            if(containingCam != null && containingCam.PrevCam != null && containingCam.PrevCam.NextCamTrigger != null) {
               Gizmos.DrawLine(transform.position, containingCam.PrevCam.NextCamTrigger.position);
               Gizmos.DrawLine(containingCam.transform.position, containingCam.PrevCam.NextCamTrigger.position);
               Gizmos.color = Color.green;
               Gizmos.DrawSphere(containingCam.PrevCam.NextCamTrigger.position, 1f);
            }
         }
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(containingCam.transform.position, 1f);
   }
}
