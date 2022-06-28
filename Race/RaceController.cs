using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Product.Utilities;
using Newtonsoft.Json;

public struct TopSpeeds {
   public float LeftTrackRacer;
   public float RightTrackRacer;
}

/// <summary> Used for controlling recording and playing processes </summary>
public class RaceController : MonoBehaviour {
   public Car LeftTrackRacer;
   public Car RightTrackRacer;
   public int LeftTrackRacerId;
   public int RightTrackRacerId;
   public float LeftTrackFinishTime;
   public float RightTrackFinishTime;
   public event TimeController.RacerFinishHandler RacerFinish;
   public event TimeController.TimeChangeHandler TimeChange;
   public event TimeController.GearChangeHandler GearChange;
   public List<PrefabInformation> AvailablePrefabs;
   public RaceRecorder RaceRecorder;
   public string RacersPrefabsDirectory;
   public string RacersManifestFileName;
   bool initialized = false;
   CustomAnimatorController animatorController;
   BurnoutController burnoutController;
   TimeController timeController;
   CamController camController;
   RaceTrack raceTrack;

   /// <summary> Returns top speeds for parsed racers and parsed timings </summary>
   public TopSpeeds GetTopSpeeds(int leftTrackRacerId, int rightTrackRacerId, float leftTrackFinishTime, float rightTrackFinishTime, SpeedUnit speedUnitUsed) {
      TopSpeeds topSpeeds = new TopSpeeds();
      if(AvailablePrefabs == null)
         AvailablePrefabs = GetAvailablePrefabs(RacersManifestFileName);
      if(leftTrackRacerId >= AvailablePrefabs.Count)
         leftTrackRacerId = 0;
      if(rightTrackRacerId >= AvailablePrefabs.Count)
         rightTrackRacerId = 0;
      var tempLeft = GetPrefab(AvailablePrefabs[leftTrackRacerId].PrefabPath).GetComponent<Car>();
      var tempRight = GetPrefab(AvailablePrefabs[rightTrackRacerId].PrefabPath).GetComponent<Car>();

      topSpeeds.LeftTrackRacer = tempLeft.GetTopSpeed(leftTrackFinishTime);
      topSpeeds.RightTrackRacer = tempRight.GetTopSpeed(rightTrackFinishTime);
      if(speedUnitUsed == SpeedUnit.KMPH) {
         topSpeeds.LeftTrackRacer *= 3.6f;
         topSpeeds.RightTrackRacer *= 3.6f;
      }
      if(speedUnitUsed == SpeedUnit.MPH) {
         topSpeeds.LeftTrackRacer *= 2.23694f;
         topSpeeds.RightTrackRacer *= 2.23694f;
      }

      DestroyImmediate(tempLeft.gameObject);
      DestroyImmediate(tempRight.gameObject);
      return topSpeeds;
   }

   /// <summary> Initializes components needed for proper functioning of the system </summary>
   void InitializeComponents() {
      if(!timeController)
         timeController = gameObject.GetComponent<TimeController>();
      if(!RaceRecorder)
         RaceRecorder = gameObject.GetComponent<RaceRecorder>();
      if(!camController)
         camController = gameObject.GetComponentInChildren<CamController>();
      if(!raceTrack)
         raceTrack = gameObject.GetComponentInChildren<RaceTrack>();
      if(raceTrack.RpmDropTiming == 0)
         raceTrack.RpmDropTiming = 0.245f;
      if(gameObject.GetComponent<CustomAnimatorController>())
         animatorController = gameObject.GetComponent<CustomAnimatorController>();
      if(gameObject.GetComponent<BurnoutController>())
         burnoutController = gameObject.GetComponent<BurnoutController>();
   }

   /// <summary> Updates everything needed for recording and playing replays, and updates racers animators </summary>
   void Update() {
      if(!initialized)
         return;
      RecordingUpdate();
      PlayingUpdate();
      AnimationsTestingUpdate();
   }

   /// <summary> Used for updating positions and rotations of racers and cameras </summary>
   void PlayingUpdate() {
      if(RaceRecorder.RecorderState == RecorderStates.PLAYING) {
         RaceRecorder.PlayingUpdate();
         burnoutController.OnUpdate();
         raceTrack.UpdateChristmasTree(timeController.ReplayTime);
      }
   }

   /// <summary> Used while recording race </summary>
   void RecordingUpdate() {
      if(RaceRecorder.RecorderState == RecorderStates.RECORDINGINTRO || RaceRecorder.RecorderState == RecorderStates.RECORDING) {
         UpdateAnimator();
         RaceRecorder.RecordingUpdate();
         LeftTrackRacer.OnUpdate();
         RightTrackRacer.OnUpdate();

         if(LeftTrackRacer.RacerState == RacerStates.ATSTART && RightTrackRacer.RacerState == RacerStates.ATSTART) {
            LeftTrackRacer.Initialize(raceTrack.LeftTrackCarIntro, gameObject, true);
            RightTrackRacer.Initialize(raceTrack.RightTrackCarIntro, gameObject);
            RaceRecorder.RecorderState = RecorderStates.RECORDING;
            camController.CamControllerState = CamStates.ATSTART;
         }

         if(LeftTrackRacer.FinishedRace && RightTrackRacer.FinishedRace && RaceRecorder.RaceReplay.ReplayState != ReplayStates.SAVED) {
            RaceRecorder.FinishRecording(LeftTrackRacer, RightTrackRacer);
            ResetStates();
         }
      }
   }

   /// <summary> Used only in edit mode for testing animations </summary>
   void AnimationsTestingUpdate() {
#if UNITY_EDITOR
      if(animatorController.TestingObject) {
         timeController.OnUpdate();
         animatorController.OnUpdate();
      }
#endif
   }

   /// <summary> Used for updating racers animations </summary>
   void UpdateAnimator() {
      if(!animatorController)
         return;
      if(RaceRecorder.RecorderState == RecorderStates.RECORDINGINTRO && !animatorController.PlayableAnimations.IntroAnimations)
         return;
      if(RaceRecorder.RecorderState == RecorderStates.RECORDING && !animatorController.PlayableAnimations.RaceAnimations)
         return;
      animatorController.OnUpdate();
   }

   /// <summary> Used for initializing burnout controller settings and timings. Sets starting and ending timings off left and right racers burnouts </summary>
   void SetupBurnouts() {
      for(int i = 0; i < RaceRecorder.RaceReplay.IntroRaceEvents.Count; i++) {
         var tempEvent = RaceRecorder.RaceReplay.IntroRaceEvents[i];
         if(RaceRecorder.RaceReplay.IntroRaceEvents[i].Message == RaceEvent.LeftRacerBurnout) {
            RaceRecorder.RaceReplay.Burnouts[0].StartTime = tempEvent.Time;
            RaceRecorder.RaceReplay.Burnouts[0].EndTime = tempEvent.RaceTime;
         }

         if(RaceRecorder.RaceReplay.IntroRaceEvents[i].Message == RaceEvent.RightRacerBurnout) {
            RaceRecorder.RaceReplay.Burnouts[1].StartTime = tempEvent.Time;
            RaceRecorder.RaceReplay.Burnouts[1].EndTime = tempEvent.RaceTime;
         }
      }
   }

   GameObject GetPrefab(string prefabPath) {
      return Instantiate(Resources.Load<GameObject>(RemoveCloneSufix(prefabPath)));
   }

   List<PrefabInformation> GetAvailablePrefabs(string fileName, string fullPath = null) {
      var filePath = fullPath == null ? Application.dataPath + RacersPrefabsDirectory + fileName : fullPath + fileName;
      var fileReader = new StreamReader(filePath);
      List<PrefabInformation> prefabsInfo = new List<PrefabInformation>();
      string tempLine;
      while((tempLine = fileReader.ReadLine()) != null)
         prefabsInfo.Add(JsonConvert.DeserializeObject<PrefabInformation>(tempLine));
      fileReader.Close();
      return prefabsInfo;
   }

   public void GetRacers(int leftRacerId, int rightRacerId) {
      DestroyRacers();
      if(AvailablePrefabs == null)
         AvailablePrefabs = GetAvailablePrefabs(RacersManifestFileName);
      if(leftRacerId > AvailablePrefabs.Count)
         leftRacerId = 1;
      if(rightRacerId > AvailablePrefabs.Count)
         rightRacerId = 1;

      LeftTrackRacer = GetPrefab(AvailablePrefabs[leftRacerId - 1].PrefabPath).GetComponent<Car>();
      RightTrackRacer = GetPrefab(AvailablePrefabs[rightRacerId - 1].PrefabPath).GetComponent<Car>();
      LeftTrackRacer.name = RemoveCloneSufix(LeftTrackRacer.name);
      RightTrackRacer.name = RemoveCloneSufix(RightTrackRacer.name);
   }

   string RemoveCloneSufix(string currentName) {
      return currentName.Substring(0, currentName.Length - 7);
   }

   public void SpawnCars() {
      raceTrack.SetPositions();
      LeftTrackRacer.Initialize(raceTrack.LeftTrackCarIntro, gameObject, true);
      RightTrackRacer.Initialize(raceTrack.RightTrackCarIntro, gameObject);
   }

   public IEnumerator RecordRace(float leftRacerTiming, float rightRacerTiming, int leftRacerId, int rightRacerId) {
      if(RaceRecorder.RecorderState == RecorderStates.RECORDING) {
         Debug.LogWarning("Already recording !");
         yield return null;
      }
      GetRacers(leftRacerId, rightRacerId);
      CustomAnimatorsInit();
      LeftTrackRacerId = leftRacerId;
      RightTrackRacerId = rightRacerId;
      LeftTrackRacer.FinishTime = leftRacerTiming;
      RightTrackRacer.FinishTime = rightRacerTiming;
      SpawnCars();
      RaceRecorder.InitRecording(LeftTrackRacer, RightTrackRacer);
      timeController.totalIntroWithDelays = raceTrack.TotalIntroDuration + timeController.RaceInitDelay + timeController.RaceIntroDelay;
      camController.Init(LeftTrackRacer, RightTrackRacer);
      if(RaceRecorder.RenderRecording)
         timeController.TimeControllerState = TimeControllerStates.PLAYING;
      else
         timeController.TimeControllerState = TimeControllerStates.RECORDING;
      if(!RaceRecorder.RenderRecording)
         while(RaceRecorder.RecorderState != RecorderStates.IDLE)
            Update();
      yield return null;
   }

   public void PlayLoadedReplay() {
      if(RaceRecorder.RaceReplay == null || LeftTrackRacer == null || RightTrackRacer == null)
         return;
      RaceRecorder.PlayLoadedReplay();
      camController.PlayModeSettings();
      LeftTrackRacer.currentGear = 1;
      RightTrackRacer.currentGear = 1;
      SetupBurnouts();
      var christmasTreePrestageTime = raceTrack.TotalIntroDuration + timeController.RaceIntroDelay;
      var christmasTreeGoTime = christmasTreePrestageTime + timeController.RaceInitDelay;
      raceTrack.SetChristmasTreeTimings(christmasTreePrestageTime, christmasTreeGoTime);
   }

   /// <summary> Initializes everything needed for proper functioning </summary>
   public Routine Init() {
      return Routines.CreateRoutine(InitRoutine(), RoutineTypes.GLOBAL);
   }

   IEnumerator InitRoutine() {
      InitializeComponents();
      timeController.RacerFinish += OnRacerFinish;
      timeController.TimeChange += OnTimeChange;
      timeController.GearChange += OnGearChange;
      timeController.Reset();
      initialized = true;
      RaceRecorder.RecorderState = RecorderStates.IDLE;
      yield return null;
   }

   void DestroyRacers() {
      if(LeftTrackRacer != null)
         DestroyImmediate(LeftTrackRacer.gameObject);
      if(RightTrackRacer != null)
         DestroyImmediate(RightTrackRacer.gameObject);
   }

   void OnGearChange(GearEvent gearChangeEvent) {
      if(GearChange != null)
         GearChange(gearChangeEvent);
      if(gearChangeEvent.TrackID == 0)
         LeftTrackRacer.currentGear = gearChangeEvent.Gear;
      else
         RightTrackRacer.currentGear = gearChangeEvent.Gear;
   }

   /// <summary> Set's timecontroller, racerecorder and car states to IDLE </summary>
   void ResetStates() {
      RaceRecorder.RecorderState = RecorderStates.IDLE;
      timeController.Reset();
   }

   string GenerateRandomIds(int totalPrefabs) {
      int firstId = Random.Range(1, totalPrefabs);
      int secondId = Random.Range(1, totalPrefabs);
      if(firstId == secondId)
         GenerateRandomIds(totalPrefabs);
      return firstId + "_" + secondId;
   }

   /// <summary> Set's racerecorder, timecontroller and replays states according to state received from event </summary>
   void OnTimeChange(string state) {
      if(state == RaceEvent.RepEnd && RaceRecorder.RecorderState != RecorderStates.RECORDING) {
         RaceRecorder.RecorderState = RecorderStates.IDLE;
         timeController.TimeControllerState = TimeControllerStates.IDLE;
      }
      if(state == RaceEvent.SlowStart)
         RaceRecorder.RaceReplay.ReplayState = ReplayStates.READY;
      if(TimeChange != null)
         TimeChange(state);
   }

   /// <summary> Triggered when racer passes finish line in slow motion </summary>
   void OnRacerFinish(int racerId) {
      if(RacerFinish != null)
         RacerFinish(racerId);
   }

   void CustomAnimatorsInit() {
      if(!animatorController) {
         Debug.LogWarning("No CustomAnimatorController component found");
         return;
      }
      if(animatorController.PlayableAnimations.IntroAnimations || animatorController.PlayableAnimations.RaceAnimations) {
         List<List<FullAnimation>> RacersAnimations = animatorController.GetRacersAnimations();
         animatorController.ControlledAnimators = new List<CustomAnimator>();
         animatorController.ControlledAnimators.Add(animatorController.AnimatorConstructor(LeftTrackRacer.gameObject, RacersAnimations[0], timeController));
         animatorController.ControlledAnimators.Add(animatorController.AnimatorConstructor(RightTrackRacer.gameObject, RacersAnimations[1], timeController));
      }
   }
}

//void RecordAndPlayRandom()
//{
//    if (RaceRecorder.RecorderState == RecorderStates.IDLE && timeController.TimeControllerState == TimeControllerStates.IDLE && !playNow)
//    {
//        var timings = GenerateRandomTimings();
//        var winnerTiming = int.Parse(timings.Split('_')[0]) / 1000f;
//        var loserTiming = int.Parse(timings.Split('_')[1]) / 1000f;
//        var ids = GenerateRandomIds(GetNumberAvailablePrefabs(RacersManifestFileName));
//        Debug.Log("IDS : " + ids);
//        var winnerId = int.Parse(ids.Split('_')[0]);
//        var loserId = int.Parse(ids.Split('_')[1]);
//        LoadAfterRecording = true;
//        RecordRace(winnerTiming, loserTiming, winnerId, loserId);
//    }

//    if (playNow && RaceRecorder.RecorderState != RecorderStates.PLAYING)
//        PlayLoadedReplays();
//}

//int GetNumberAvailablePrefabs(string fileName, string fullPath = null)
//{
//    var filePath = fullPath == null ? Application.dataPath + RacersPrefabsDirectory + fileName : fullPath + fileName;
//    return File.ReadAllLines(filePath).Length;
//}

//string GenerateRandomTimings()
//{
//    float classTime = Random.Range(11, 17);
//    float winnerDecimals = Random.Range(100, 999);
//    float loserDecimals = Random.Range(100, 999);

//    var winnerTime = (classTime.ToString() + winnerDecimals.ToString());
//    var loserTime = (classTime.ToString() + loserDecimals.ToString());

//    if (winnerTime.Last<char>() == '0')
//        winnerTime = winnerTime.Substring(0, winnerTime.Length - 1) + '1';
//    if (loserTime.Last<char>() == '0')
//        loserTime = loserTime.Substring(0, loserTime.Length - 1) + '1';
//    return winnerTime + "_" + loserTime;
//}
