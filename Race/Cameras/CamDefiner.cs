using UnityEngine;

public enum FocusDefinition {
   FOCUSMIDDLE,
   FOCUSFIRST,
   FOCUSSECOND,
   FOCUSLOCKED
}

public enum TriggerOptions {
   FIRSTRACER,
   SECONDRACER,
   MIDPOINT
}

public enum FieldOfViewOptions {
   AUTOADJUSTED,
   LOCKED
}

public enum RotationOptions {
   UNLOCKED,
   LOCKED
}

public enum CamType {
   FIXED,
   FOLLOW,
   FIRSTPERSON
}

public enum LerpOptions {
   DISTANCE,
   TIME,
   NONE
}

public class CamDefiner : MonoBehaviour {
   public Camera Camera;
   //public int CameraID;
   public CamDefiner PrevCam;
   public CamDefiner NextCam;
   public Transform NextCamTrigger;
   public Transform StartFollowingTrigger;
   public FocusDefinition FocusOptions;
   [Tooltip("Next cam will be triggered by ")]
   public TriggerOptions TriggerOptions;
   public FieldOfViewOptions FieldOfViewOptions;
   public RotationOptions RotationOptions;
   public CamType CameraType;
   [Range(0f, 1f)]
   public float CameraLag;
   FocusDefinition defaultFocusOption;
   TriggerOptions defaultTriggerOption;
   FieldOfViewOptions defaultFoVOption;
   public float MinFov;
   public float MaxFov;
   public bool CapZoomSpeed;
   public float ZoomAcceleration;
   public LerpOptions CamProgression;
   public float Duration;
   public Vector3 StartPosition { get; private set; }
   public Vector3 EndPosition;
   public AnimationCurve PosProgression;
   public Vector3 Rotation;
   public AnimationCurve RotProgression;
   public float Roll;
   public AnimationCurve HorizontalShake;
   public AnimationCurve VerticalShake;
   public AnimationCurve ShakeSpeed;
   RotationOptions defaultRotationOption;
   CamType defaultCamType;
   Vector3 defaultCamPosition;
   Transform defaultParent;

   public float GetEndDistance() {
      if(NextCamTrigger != null)
         return NextCamTrigger.position.z;
      return float.PositiveInfinity;
   }

   void SaveDefaultSettings() {
      defaultFocusOption = FocusOptions;
      defaultTriggerOption = TriggerOptions;
      defaultFoVOption = FieldOfViewOptions;
      defaultRotationOption = RotationOptions;
      defaultCamType = CameraType;
      defaultCamPosition = transform.position;
      defaultParent = gameObject.transform.parent;
   }

   public void LoadDefaultSettings() {
      FocusOptions = defaultFocusOption;
      TriggerOptions = defaultTriggerOption;
      FieldOfViewOptions = defaultFoVOption;
      RotationOptions = defaultRotationOption;
      CameraType = defaultCamType;
      transform.position = defaultCamPosition;
      if(CameraType == CamType.FIRSTPERSON) {
         gameObject.transform.SetParent(defaultParent);
      }
   }

   void Awake() {
      SaveDefaultSettings();
   }

   void OnEnable() {
      LoadDefaultSettings();
      StartPosition = transform.position;
   }

   void OnDrawGizmosSelected() {
      if(Application.isPlaying)
         return;

      if(!Application.isEditor || NextCamTrigger == null)
         return;

      var definedCams = GetComponentInParent<CamController>();
      definedCams.InitCams();

      Gizmos.color = Color.green;

      //if(CameraID >= definedCams.GetDefinedCams().Count)
      //   return;

      //if(definedCams.GetCam(CameraID-1).NextCamTrigger == null)
      //   Debug.Log("cam id : " + CameraID);

      if(PrevCam != null && PrevCam.NextCamTrigger != null)
         Gizmos.DrawSphere(PrevCam.NextCamTrigger.position, 1f);

      Gizmos.color = Color.red;
      //Gizmos.DrawSphere(transform.position, 1f);
      Gizmos.DrawSphere(NextCamTrigger.position, 1f);

      if(PrevCam != null && PrevCam.NextCamTrigger != null) {
         Gizmos.DrawLine(PrevCam.NextCamTrigger.position, NextCamTrigger.transform.position);
         Gizmos.DrawLine(PrevCam.NextCamTrigger.position, transform.position);
         Gizmos.DrawLine(NextCamTrigger.position, transform.position);
      }
      if(PrevCam == null && NextCamTrigger != null)
         Gizmos.DrawLine(transform.position, NextCamTrigger.transform.position);

      Gizmos.color = Color.blue;
      Gizmos.DrawSphere(Camera.transform.position, 1f);
   }
}
