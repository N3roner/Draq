using UnityEngine;

public enum LerpingOptions {
   DEFAULT,
   COSINE,
   SINE,
   SMOOTHSTEP
}

public class IntroCam : MonoBehaviour {

   public Vector3 StartingPosition;
   public Quaternion StartingRotation;
   public Vector3 EndingRotation;
   public float EndingRotationTime;
   //public Vector3 StartingRotation;
   public float RotationSpeed;
   public GameObject Parent;
   public Vector3 RotationDirection;
   public AnimationCurve AnimationCurve;
#pragma warning disable 414
   TimeController timeController;
#pragma warning disable 414
   Vector3 goPos;

   public float MinCamHeight;
   public float MaxCamHeight;
   public LerpingOptions LerpingFunction;
   Camera introCam;

   public void SetParent(GameObject newParent) {
      Parent = newParent;
      goPos = gameObject.transform.position;
   }

   public void ChangeCamsY(float normalizedTime) {
      goPos = gameObject.transform.position;

      var goP = Mathf.Lerp(MinCamHeight, MaxCamHeight, AnimationCurve.Evaluate(normalizedTime));

      goPos.y = goP;

      gameObject.transform.position = goPos;
   }

   public void RotateArroundParent(Vector3 parent) {

      //var udaljenost = transform.position.z - parent.z;
      //if(udaljenost <= 30f) {
      //   var temp = transform.position;
      //   temp.z += 0.12f;
      //   transform.position = temp;
      //}


      transform.RotateAround(parent, RotationDirection, RotationSpeed * Time.deltaTime);

      var nesta = gameObject.GetComponent<Transform>().localEulerAngles;

      nesta.z = StartingRotation.z;
      gameObject.GetComponent<Transform>().localEulerAngles = nesta;

      //var nesta2 = gameObject.GetComponent<Transform>().localRotation;


      //var nekaRot = gameObject.transform.rotation;
      //nekaRot.z = StartingRotation.z;
      //gameObject.transform.rotation = nekaRot;

      //var introCamRot = introCam.transform.localRotation;
      //introCamRot.z += 1f;
      //introCam.transform.localRotation = introCamRot;

      //var currentRott = transform.rotation;
      //currentRott.x = StartingRotation.x;
      //transform.rotation = currentRott;

      //var currentRott = transform.eulerAngles;
      //currentRott.z = StartingRotation.z;
      //transform.eulerAngles = currentRott;

      //if(timeController.ReplayTime <= 2f)
      //   return;

      //var rotProgress = (timeController.ReplayTime) / EndingRotationTime;
      //if(rotProgress <= 1f) {
      //   var tempRotation = transform.eulerAngles;
      //   tempRotation.z = Mathf.Lerp(StartingRotation.z, EndingRotation.z, rotProgress);
      //   transform.eulerAngles = tempRotation;
      //   //transform.eulerAngles = Vector3.Lerp(StartingRotation, EndingRotation, rotProgress);
      //}

      //if(timeController.ReplayTime <= EndingRotationTime)
      //   return;

      //var currentRott2 = transform.eulerAngles;
      //currentRott2.z = EndingRotation.z;
      //transform.eulerAngles = currentRott2;

   }

   public void Init() {
      transform.position = StartingPosition;
      transform.rotation = StartingRotation;
      //transform.eulerAngles = StartingRotation;
      timeController = gameObject.GetComponentInParent<TimeController>();
      introCam = gameObject.GetComponent<CamDefiner>().Camera;
   }

   public void ZoomOut() {

   }

   //void Update() {
   //   var nesta = AnimationCurve.keys;

   //   Debug.Log("anim : " + AnimationCurve.Evaluate(timeController.ReplayTime) + " ll : ");

   //}
}
