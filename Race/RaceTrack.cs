using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpeedUnit {
   KMPH,
   MPH
}

[Serializable]
public struct IntroSettings {
   public float SpawnBurnoutTransitionTime;
   public float BurnoutTime;
   public float BurnoutStartTransitionTime;
   public float SpawnBurnoutTransitionPercent;
   public float BurnoutTimePercent;
   public float BurnoutStartTransitionPercent;
   public Vector3 SpawnPosition;
   public Vector3 BurnoutPosition;
   public Vector3 StartLinePosition;
   public float XOffsetMin;
   public float XOffsetMax;
   public float XOffset;
   public List<UiSlider> TimeSliders;
   public bool[] SliderLocks;
   public float[] TimePercents;
   public float[] TimePercentsCopy;
}

public class RaceTrack : MonoBehaviour {
   public float RaceTrackLength;
   public float TotalIntroDuration;
   public SpeedUnit SpeedUnitUsed;
   public float RpmDropTiming;
   public string[] TimePercentFields;
   [SerializeField]
   public float SpawnZMin, SpawnZMax, BurnoutZMin, BurnoutZMax;
   [SerializeField]
   public IntroSettings LeftTrackCarIntro, RightTrackCarIntro;
   public GameObject ChristmasTree;
   public float StageTimeOffset;
   public float ReadyTimeOffset;
   public float TreeTurnOffOfFset;
   Material Lights_Go;
   Material Lights_Prestage;
   Material Lights_Ready;
   Material Lights_Stage;
   float prestageTime;
   float readyTime;
   float stageTime;
   float goTime;

   public void SetChristmasTreeTimings(float passedPrestageTime, float passedGoTime) {
      var totalTreeTime = passedGoTime - passedPrestageTime;
      prestageTime = passedPrestageTime;
      stageTime = prestageTime + (totalTreeTime * StageTimeOffset) / 100;
      readyTime = stageTime + (totalTreeTime * ReadyTimeOffset) / 100;
      goTime = passedGoTime;
      InitializeChristmassTree();
   }

   void InitializeChristmassTree() {
      if(ChristmasTree != null && Lights_Go == null) {
         var tree = ChristmasTree.transform;
         Lights_Go = tree.FindChild("Lights_Go").GetComponent<Renderer>().material;
         Lights_Prestage = tree.FindChild("Lights_Prestage").GetComponent<Renderer>().material;
         Lights_Ready = tree.FindChild("Lights_Ready").GetComponent<Renderer>().material;
         Lights_Stage = tree.FindChild("Lights_Stage").GetComponent<Renderer>().material;
      }
   }

   public void UpdateChristmasTree(float currentTime) {
      if(!ChristmasTree)
         return;

      if(currentTime >= prestageTime && currentTime <= stageTime && Lights_Prestage.mainTextureOffset == Vector2.zero)
         Lights_Prestage.mainTextureOffset = new Vector2(0.2f, 0);

      if(currentTime >= stageTime && currentTime <= readyTime && Lights_Stage.mainTextureOffset == Vector2.zero)
         Lights_Stage.mainTextureOffset = new Vector2(0.4f, 0);

      if(currentTime >= readyTime && currentTime <= goTime && Lights_Ready.mainTextureOffset == Vector2.zero)
         Lights_Ready.mainTextureOffset = new Vector2(0.6f, 0);

      if(currentTime >= goTime && currentTime <= goTime + TreeTurnOffOfFset && Lights_Go.mainTextureOffset == Vector2.zero) {
         Lights_Go.mainTextureOffset = new Vector2(0.8f, 0);
         Lights_Ready.mainTextureOffset = Vector2.zero;

      }
      if(currentTime >= goTime + TreeTurnOffOfFset && Lights_Go.mainTextureOffset == new Vector2(0.8f, 0)) {
         Lights_Prestage.mainTextureOffset = Vector2.zero;
         Lights_Stage.mainTextureOffset = Vector2.zero;
         Lights_Go.mainTextureOffset = Vector2.zero;
      }
   }

   public void SetPositions() {
      LeftTrackCarIntro.XOffset = UnityEngine.Random.Range(LeftTrackCarIntro.XOffsetMin, LeftTrackCarIntro.XOffsetMax);
      RightTrackCarIntro.XOffset = UnityEngine.Random.Range(RightTrackCarIntro.XOffsetMin, RightTrackCarIntro.XOffsetMax);

      LeftTrackCarIntro.SpawnPosition = new Vector3(LeftTrackCarIntro.XOffset, 0, UnityEngine.Random.Range(SpawnZMin, SpawnZMax));
      RightTrackCarIntro.SpawnPosition = new Vector3(RightTrackCarIntro.XOffset, 0, UnityEngine.Random.Range(SpawnZMin, SpawnZMax));

      LeftTrackCarIntro.BurnoutPosition = new Vector3(LeftTrackCarIntro.XOffset, 0, UnityEngine.Random.Range(BurnoutZMin, BurnoutZMax));
      RightTrackCarIntro.BurnoutPosition = new Vector3(RightTrackCarIntro.XOffset, 0, UnityEngine.Random.Range(BurnoutZMin, BurnoutZMax));

      LeftTrackCarIntro.StartLinePosition = new Vector3(LeftTrackCarIntro.XOffset, 0, 0);
      RightTrackCarIntro.StartLinePosition = new Vector3(RightTrackCarIntro.XOffset, 0, 0);
   }

   public IntroSettings InitializeArrays(IntroSettings original) {
      const int totalFields = 3;

      if(original.TimePercents == null || original.TimePercents.Length != totalFields) {
         original.TimePercents = new float[totalFields];
         original.TimePercents[0] = original.SpawnBurnoutTransitionPercent;
         original.TimePercents[1] = original.BurnoutTimePercent;
         original.TimePercents[2] = original.BurnoutStartTransitionPercent;
      }

      if(TimePercentFields == null || TimePercentFields.Length != totalFields) {
         TimePercentFields = new string[totalFields];
         TimePercentFields[0] = "Spawn - burnout transition time : ";
         TimePercentFields[1] = "Burnout time : ";
         TimePercentFields[2] = "Burnout - start line transition time : ";
      }

      if(original.TimePercentsCopy == null || original.TimePercentsCopy.Length != totalFields) {
         original.TimePercentsCopy = new float[TimePercentFields.Length];
         for(int i = 0; i < original.TimePercents.Length; i++)
            original.TimePercentsCopy[i] = original.TimePercents[i];
      }

      if(original.SliderLocks == null || original.SliderLocks.Length != totalFields)
         original.SliderLocks = new bool[TimePercentFields.Length];

      return original;
   }

   public IntroSettings UpdateTimings(float[] percentsArray, IntroSettings passedIn) {
      passedIn.SpawnBurnoutTransitionTime = percentsArray[0] * TotalIntroDuration / 100f;
      passedIn.BurnoutTime = percentsArray[1] * TotalIntroDuration / 100f;
      passedIn.BurnoutStartTransitionTime = percentsArray[2] * TotalIntroDuration / 100f;

      passedIn.SpawnBurnoutTransitionPercent = percentsArray[0];
      passedIn.BurnoutTimePercent = percentsArray[1];
      passedIn.BurnoutStartTransitionPercent = percentsArray[2];
      return passedIn;
   }

   public float GetConvertedSpeed(float speedInMetersPerSecond) {
      var speedToReturn = 0f;
      if(SpeedUnitUsed == SpeedUnit.KMPH)
         speedToReturn = speedInMetersPerSecond * 3.6f;
      if(SpeedUnitUsed == SpeedUnit.MPH)
         speedToReturn = speedInMetersPerSecond * 2.23694f;
      return speedToReturn;
   }
}
