using UnityEngine;

public struct CamShakeInfo {
   public AnimationCurve HorizontalShake;
   public AnimationCurve VerticalShake;
   public AnimationCurve ShakeSpeed;
   public float StartTime;
   public float EndTime;
}

[System.Serializable]
public struct Shake {
   public float PreviousX;
   public float PreviousZ;
   public float CurrentX;
   public float CurrentZ;
   public float TargetX;
   public float TargetZ;
}
[System.Serializable]
public struct ShakePrm {
   //public float HorAmplitude;
   //public float HorFrequency;
   //public float VerAmplitude;
   //public float VerFrequency;
   public float ReverseThreshold;
   public float LengthPercentToTarget;
}

public class CameraShake : MonoBehaviour {

   public ShakePrm ShakePrm;
   public Shake Shake;

   public Quaternion GetShake(float deltaTime, float horAmplitude, float verAmplitude) {
      UpdateShake(deltaTime);
      return Quaternion.Euler(new Vector3(Shake.CurrentX * verAmplitude, Shake.CurrentZ * horAmplitude, 0f));
   }

   public void Reset() {
      Shake.CurrentX = 0f;
      Shake.CurrentZ = 0f;
      Shake.TargetX = Random.Range(-1f, 1f);
      Shake.TargetZ = Random.Range(-1f, 1f);
   }

   float AxisShake(float current, float previous, float target, float deltaTime) {
      var targetToCurrent = target - current;
      var previousToCurrent = current - previous;
      float usedLength = 0f;

      if(previous != 0 && Mathf.Abs(targetToCurrent) > Mathf.Abs(previousToCurrent))
         usedLength = previousToCurrent;
      else
         usedLength = targetToCurrent;

      if(current == previous)
         usedLength = targetToCurrent;// * ShakePrm.LengthPercentToTarget / 100;

      return current + (usedLength) * deltaTime;
   }

   void UpdateShake(float deltaTime) {
      Shake.CurrentX = AxisShake(Shake.CurrentX, Shake.PreviousX, Shake.TargetX, deltaTime);

      if(Mathf.Abs(Shake.CurrentX - Shake.TargetX) < ShakePrm.ReverseThreshold) {
         if(Shake.TargetX != 0)
            Shake.PreviousX = Shake.CurrentX;
         Shake.TargetX = Random.Range(-1f, 1f);
      }

      Shake.CurrentZ = AxisShake(Shake.CurrentZ, Shake.PreviousZ, Shake.TargetZ, deltaTime);

      if(Mathf.Abs(Shake.CurrentZ - Shake.TargetZ) < ShakePrm.ReverseThreshold) {
         if(Shake.TargetZ != 0)
            Shake.PreviousZ = Shake.CurrentZ;
         Shake.TargetZ = Random.Range(-1f, 1f);
      }
   }

}
