using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CamDefiner))]
public class CamDefinerEditor : Editor {
   CamDefiner controller;
   CamController DefinedCams;
   Vector3 currentCamTrigger;
   Vector3 nextCamPosition;
   Vector3 previousCamPosition;
   Vector3 previousCamTrigger;
   Vector3 followTrigger;

   void OnEnable() {
      if(Application.isPlaying)
         return;
      controller = (CamDefiner)target;
      DefinedCams = controller.GetComponentInParent<CamController>();
      DefinedCams.InitCams();
   }

   void OnSceneGUI() {
      if(Application.isPlaying)
         return;
      EditorGUI.BeginChangeCheck();
      
      if(controller.NextCam != null && !Event.current.alt)
         nextCamPosition = Handles.PositionHandle(controller.NextCam.transform.position, controller.NextCam.transform.rotation);

      if(controller.NextCamTrigger != null) {
         Handles.Label(controller.NextCamTrigger.transform.position, "Next cam trigger" + "\n(" + controller.NextCamTrigger.name + ")");
         if(!Event.current.alt)
            currentCamTrigger = Handles.PositionHandle(controller.NextCamTrigger.position, controller.NextCamTrigger.rotation);
      }

      if(controller.StartFollowingTrigger != null) {
         Handles.Label(controller.StartFollowingTrigger.transform.position, "Start following trigger" + "\n(" + controller.StartFollowingTrigger.name + ")");
         if(!Event.current.alt)
            followTrigger = Handles.PositionHandle(controller.StartFollowingTrigger.position, controller.StartFollowingTrigger.rotation);
      }

      if(controller.PrevCam != null && !Event.current.alt) {
         previousCamPosition = Handles.PositionHandle(controller.PrevCam.transform.position, controller.PrevCam.transform.rotation);
         if(controller.PrevCam.NextCamTrigger != null)
            previousCamTrigger = Handles.PositionHandle(controller.PrevCam.NextCamTrigger.transform.position, controller.PrevCam.NextCamTrigger.transform.rotation);
      }
      
      Handles.Label(controller.transform.position + controller.EndPosition, "EndPos");
      controller.EndPosition = Handles.PositionHandle(controller.transform.position + controller.EndPosition, Quaternion.identity) - controller.transform.position;

      if(EditorGUI.EndChangeCheck()) {
         if(controller.NextCam != null)
            controller.NextCam.transform.position = nextCamPosition;

         if(controller.NextCamTrigger != null)
            controller.NextCamTrigger.position = currentCamTrigger;

         if(controller.PrevCam != null) {
            controller.PrevCam.transform.position = previousCamPosition;
            if(controller.PrevCam.NextCamTrigger != null)
               controller.PrevCam.NextCamTrigger.transform.position = previousCamTrigger;
         }

         if(controller.StartFollowingTrigger != null)
            controller.StartFollowingTrigger.transform.position = followTrigger;
      }

      Handles.Label(controller.transform.position, "Selected cam" + "\n(" + controller.name + ")");

      if(controller.NextCam != null)
         Handles.Label(controller.NextCam.transform.position, "Next cam" + "\n(" + controller.NextCam.name + ")");

      if(controller.PrevCam != null) {
         Handles.Label(controller.PrevCam.transform.position, "Previous cam" + "\n(" + controller.PrevCam.name + ")");
         if(controller.PrevCam.NextCamTrigger != null)
            Handles.Label(controller.PrevCam.NextCamTrigger.transform.position, "Selected cam trigger" + "\n(" + controller.PrevCam.NextCamTrigger.name + ")");
      }
   }
}
