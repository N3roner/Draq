using UnityEngine;
//using UnityEditor;
public class FinishCam : MonoBehaviour {

   public Camera Camera;
   public float MinFoV;
   public float MaxFoV;
   CamDefiner finishCamDefiner;
   public Object PrefabToReset;
   public GameObject objToReset;

   public void ResetPref() {
      Debug.Log("reseting");
      //PrefabUtility.ResetToPrefabState(Selection.activeGameObject);

      //if(PrefabUtility.ResetToPrefabState(Selection.activeGameObject))
      //   Debug.Log("Reseted to prefab state successfully!");
      //else
      //   Debug.LogError("Couldnt reset to a prefab state " + Selection.activeGameObject.name);

      //gameObject.SetActive(false);
      //gameObject.SetActive(true);
#if UNITY_EDITOR
      UnityEditor.PrefabUtility.ResetToPrefabState(gameObject);
      UnityEditor.PrefabUtility.RevertPrefabInstance(gameObject);

      UnityEditor.PrefabUtility.ResetToPrefabState(PrefabToReset);
      UnityEditor.PrefabUtility.RevertPrefabInstance(objToReset);
#endif

      //EditorUtility.ResetToPrefabState(PrefabToReset);
      //PrefabUtility.RevertPrefabInstance(objToReset);
   }

   public void Init() {
      if(finishCamDefiner != null)
         return;
      finishCamDefiner = gameObject.GetComponent<CamDefiner>();
   }

   public void CamUpdate(GameObject camFocus, GameObject firstRacer, GameObject secondRacer) {

      if(camFocus.transform.position.z >= 402f && finishCamDefiner.FocusOptions != FocusDefinition.FOCUSLOCKED) {
         finishCamDefiner.RotationOptions = RotationOptions.LOCKED;
         finishCamDefiner.FieldOfViewOptions = FieldOfViewOptions.LOCKED;

      }

      //if((firstRacer.transform.position.z >= 402f || secondRacer.transform.position.z >= 402f) && finishCamDefiner.FocusOptions != FocusDefinition.FOCUSLOCKED) {
      //   finishCamDefiner.RotationOptions = RotationOptions.LOCKED;
      //   finishCamDefiner.FieldOfViewOptions = FieldOfViewOptions.LOCKED;

      //}

      //if(finishCamDefiner.FocusOptions == FocusDefinition.FOCUSMIDDLE && finishCamDefiner.RotationOptions != RotationOptions.LOCKED) {
      //   if(finishCamDefiner.TriggerOptions == TriggerOptions.MIDPOINT) {
      //      if(camFocus.transform.position.z >= 402f && finishCamDefiner.FocusOptions != FocusDefinition.FOCUSLOCKED) {
      //         finishCamDefiner.RotationOptions = RotationOptions.LOCKED;
      //         finishCamDefiner.FieldOfViewOptions = FieldOfViewOptions.LOCKED;
      //      }
      //   }
      //}
   }
}
