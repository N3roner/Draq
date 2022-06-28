using System.Collections.Generic;
using UnityEngine;

/// <summary> Race recorder states </summary>
public enum RecorderStates {
   RECORDINGINTRO,
   RECORDING,
   PLAYING,
   IDLE
};

/// <summary> Used for recording and playing replays </summary>
public class RaceRecorder : MonoBehaviour {
   public Replay RaceReplay;
   public RecorderStates RecorderState;
   public bool RenderRecording;
   RaceFrame currentFrame, nextFrame;
   TimeController timeController;
   CamController camController;
   Car LeftTrackCar;
   Car RightTrackCar;

   /// <summary> Initializes replays, and set's states needed for recording mode </summary>
   public void InitRecording(Car leftRacer, Car rightRacer) {
      RaceReplay = new Replay();
      LeftTrackCar = leftRacer;
      RightTrackCar = rightRacer;
      RaceReplay.ReplayName = leftRacer.FinishTime.ToString() + "_" + rightRacer.FinishTime.ToString();
      InitializeComponents();
      timeController.Reset();
      RecorderState = RecorderStates.RECORDINGINTRO;
   }

   void InitializeComponents() {
      if(gameObject.GetComponent<TimeController>())
         timeController = gameObject.GetComponent<TimeController>();
      if(gameObject.GetComponentInChildren<CamController>())
         camController = gameObject.GetComponentInChildren<CamController>();
   }

   /// <summary> Updates time and controlls playing a replay </summary>
   public void PlayingUpdate() {
      timeController.OnUpdate();
      PlayFrame();
   }

   /// <summary> Updates time and controlls when should SaveFrame method be called </summary>
   public void RecordingUpdate() {
      if(RecorderState == RecorderStates.RECORDINGINTRO || RecorderState == RecorderStates.RECORDING)
         SaveFrame(camController.CamsUpdate());
      timeController.OnUpdate();
   }

   public void FinishRecording(Car leftRacer, Car rightRacer) {
      if(RaceReplay.ReplayState != ReplayStates.SAVED) {
         SaveFrame();
         RaceReplay.GearEvents = new List<GearEvent>();
         foreach(var item in leftRacer.GearEvents)
            RaceReplay.GearEvents.Add(item);
         foreach(var item in rightRacer.GearEvents)
            RaceReplay.GearEvents.Add(item);

         RaceReplay.IntroRaceEvents = new List<RaceEvent>();
         foreach(var item in leftRacer.IntroEvents)
            RaceReplay.IntroRaceEvents.Add(item);
         foreach(var item in rightRacer.IntroEvents)
            RaceReplay.IntroRaceEvents.Add(item);

         timeController.GearEvents = RaceReplay.GearEvents;
         timeController.LinearSlowMotion(RaceReplay);

         RaceReplay.ShakeQueue = camController.ShakeQueue;
         RaceReplay.ReplayState = ReplayStates.SAVED;
         RemoveAnimators();
      }
   }

   void RemoveAnimators() {
      if(LeftTrackCar.GetComponent<CustomAnimator>())
         DestroyImmediate(LeftTrackCar.GetComponent<CustomAnimator>());
      if(RightTrackCar.GetComponent<CustomAnimator>())
         DestroyImmediate(RightTrackCar.GetComponent<CustomAnimator>());
   }

   /// <summary> Set's recorder and timecontroller states for playing mode </summary>
   public void PlayLoadedReplay() {
      if(RaceReplay == null || LeftTrackCar == null || RightTrackCar == null) {
         Debug.LogWarning("Replays not loaded");
         return;
      }
      camController.SetReplay();
      timeController.Reset();
      RecorderState = RecorderStates.PLAYING;
      timeController.TimeControllerState = TimeControllerStates.PLAYING;
   }

   /// <summary> Saves information about racer into raceframe and adds it to replay </summary>
   void SaveFrame(bool frameToDelete = false) {
      RaceFrame raceFrame = new RaceFrame();
      raceFrame.Cameras = new CameraInformations();
      raceFrame.LeftTrackRacer = new RacerInformations();
      raceFrame.RightTrackRacer = new RacerInformations();

      raceFrame.LeftTrackRacer.Position = LeftTrackCar.transform.position;
      raceFrame.LeftTrackRacer.Rotation = LeftTrackCar.transform.rotation;

      raceFrame.LeftTrackRacer.BodyInfo.Position = LeftTrackCar.CarBody.position;
      raceFrame.LeftTrackRacer.BodyInfo.Rotation = LeftTrackCar.CarBody.rotation;

      raceFrame.LeftTrackRacer.FrontWheelsRotation = LeftTrackCar.FrontWheels.rotation;
      raceFrame.LeftTrackRacer.BackWheelsRotation = LeftTrackCar.BackWheels.rotation;

      raceFrame.LeftTrackRacer.Distance = LeftTrackCar.transform.position.z;
      raceFrame.LeftTrackRacer.DisplayedRPMs = LeftTrackCar.DisplayedRPM;
      raceFrame.LeftTrackRacer.Speed = LeftTrackCar.Speed;

      raceFrame.RightTrackRacer.Position = RightTrackCar.transform.position;
      raceFrame.RightTrackRacer.Rotation = RightTrackCar.transform.rotation;

      raceFrame.RightTrackRacer.BodyInfo.Position = RightTrackCar.CarBody.position;
      raceFrame.RightTrackRacer.BodyInfo.Rotation = RightTrackCar.CarBody.rotation;

      raceFrame.RightTrackRacer.FrontWheelsRotation = RightTrackCar.FrontWheels.rotation;
      raceFrame.RightTrackRacer.BackWheelsRotation = RightTrackCar.BackWheels.rotation;

      raceFrame.RightTrackRacer.Distance = RightTrackCar.transform.position.z;
      raceFrame.RightTrackRacer.DisplayedRPMs = RightTrackCar.DisplayedRPM;
      raceFrame.RightTrackRacer.Speed = RightTrackCar.Speed;

      raceFrame.Cameras.Position = camController.ActiveCam.transform.position;
      raceFrame.Cameras.Rotation = camController.ActiveCam.transform.rotation;
      raceFrame.Cameras.FieldOfView = camController.ActiveCam.Camera.fieldOfView;
      raceFrame.Cameras.NearClip = camController.ActiveCam.Camera.nearClipPlane;
      raceFrame.FrameTime = timeController.ReplayTime;

      if(frameToDelete)
         raceFrame.Cameras.CutFrame = true;

      if(RaceReplay.Frames.Count > 2) {
         if(RaceReplay.Frames[RaceReplay.Frames.Count - 1].Cameras.CutFrame) {
            var tempFrame = RaceReplay.Frames[RaceReplay.Frames.Count - 1];
            tempFrame.FrameTime = RaceReplay.Frames[RaceReplay.Frames.Count - 2].FrameTime + 0.001f;
            RaceReplay.Frames[RaceReplay.Frames.Count - 1] = tempFrame;
         }
      }
      RaceReplay.AddFrame(raceFrame);
   }

   /// <summary> Updates racers position according to current and next frame </summary>
   void PlayFrame() {
      if(RaceReplay == null)
         return;
      if(RaceReplay.ReplayState != ReplayStates.FINISHED) {
         int currentIndex = RaceReplay.GetFrameIndex(timeController.RaceTime);
         if(currentIndex < 0) {
            RaceReplay.ReplayState = ReplayStates.FINISHED;
            return;
         }

         if(timeController.RaceTime > 0f) {
            currentFrame = RaceReplay.GetFrame(currentIndex);
            nextFrame = RaceReplay.GetFrame(currentIndex + 1);
         } else {
            currentFrame = RaceReplay.GetFrame(currentIndex + 1);
            nextFrame = RaceReplay.GetFrame(currentIndex);
         }

         if(currentFrame.FrameTime >= 0f && nextFrame.FrameTime >= 0) {
            float interPolationPoint = (timeController.RaceTime - currentFrame.FrameTime) / (nextFrame.FrameTime - currentFrame.FrameTime);

            LeftTrackCar.transform.position = Vector3.Lerp(currentFrame.LeftTrackRacer.Position, nextFrame.LeftTrackRacer.Position, interPolationPoint);
            LeftTrackCar.transform.rotation = Quaternion.Lerp(currentFrame.LeftTrackRacer.Rotation, nextFrame.LeftTrackRacer.Rotation, interPolationPoint);

            RightTrackCar.transform.position = Vector3.Lerp(currentFrame.RightTrackRacer.Position, nextFrame.RightTrackRacer.Position, interPolationPoint);
            RightTrackCar.transform.rotation = Quaternion.Lerp(currentFrame.RightTrackRacer.Rotation, nextFrame.RightTrackRacer.Rotation, interPolationPoint);

            LeftTrackCar.CarBody.position = Vector3.Lerp(currentFrame.LeftTrackRacer.BodyInfo.Position, nextFrame.LeftTrackRacer.BodyInfo.Position, interPolationPoint);
            LeftTrackCar.CarBody.rotation = Quaternion.Lerp(currentFrame.LeftTrackRacer.BodyInfo.Rotation, nextFrame.LeftTrackRacer.BodyInfo.Rotation, interPolationPoint);

            RightTrackCar.CarBody.position = Vector3.Lerp(currentFrame.RightTrackRacer.BodyInfo.Position, nextFrame.RightTrackRacer.BodyInfo.Position, interPolationPoint);
            RightTrackCar.CarBody.rotation = Quaternion.Lerp(currentFrame.RightTrackRacer.BodyInfo.Rotation, nextFrame.RightTrackRacer.BodyInfo.Rotation, interPolationPoint);

            LeftTrackCar.FrontWheels.rotation = Quaternion.Lerp(currentFrame.LeftTrackRacer.FrontWheelsRotation, nextFrame.LeftTrackRacer.FrontWheelsRotation, interPolationPoint);
            LeftTrackCar.BackWheels.rotation = Quaternion.Lerp(currentFrame.LeftTrackRacer.BackWheelsRotation, nextFrame.LeftTrackRacer.BackWheelsRotation, interPolationPoint);

            RightTrackCar.FrontWheels.rotation = Quaternion.Lerp(currentFrame.RightTrackRacer.FrontWheelsRotation, nextFrame.RightTrackRacer.FrontWheelsRotation, interPolationPoint);
            RightTrackCar.BackWheels.rotation = Quaternion.Lerp(currentFrame.RightTrackRacer.BackWheelsRotation, nextFrame.RightTrackRacer.BackWheelsRotation, interPolationPoint);

            LeftTrackCar.DisplayedRPM = currentFrame.LeftTrackRacer.DisplayedRPMs;
            LeftTrackCar.Speed = currentFrame.LeftTrackRacer.Speed;

            RightTrackCar.DisplayedRPM = currentFrame.RightTrackRacer.DisplayedRPMs;
            RightTrackCar.Speed = currentFrame.RightTrackRacer.Speed;

            if(!camController.SlowmoActive) {
               camController.InGameCamera.transform.position = Vector3.Lerp(currentFrame.Cameras.Position, nextFrame.Cameras.Position, interPolationPoint);
               camController.InGameCamera.transform.rotation = Quaternion.Lerp(currentFrame.Cameras.Rotation, nextFrame.Cameras.Rotation, interPolationPoint);
            } else {
               camController.InGameCamera.transform.position = camController.FinishCam.transform.position;
               camController.InGameCamera.transform.rotation = camController.FinishCam.transform.rotation;
            }
            var shake = RaceReplay.GetShake(timeController.ReplayTime);
            var shakeInterpolation = (timeController.ReplayTime - shake.StartTime) / (shake.EndTime - shake.StartTime);
            var horShake = shake.HorizontalShake.Evaluate(shakeInterpolation);
            var verShake = shake.VerticalShake.Evaluate(shakeInterpolation);
            var shakeSpeed = shake.ShakeSpeed.Evaluate(shakeInterpolation);
            camController.InGameCamera.transform.rotation *= camController.GetShake(horShake, verShake, shakeSpeed);
            //if(currentFrame.Cameras.ShakeSpeed > Mathf.Epsilon && nextFrame.Cameras.ShakeSpeed > Mathf.Epsilon) {
            /*var horAmplitude = Mathf.Lerp(currentFrame.Cameras.HorShakeAmplitude, nextFrame.Cameras.HorShakeAmplitude, interPolationPoint);
            var verAmplitude = Mathf.Lerp(currentFrame.Cameras.VerShakeAmplitude, nextFrame.Cameras.VerShakeAmplitude, interPolationPoint);
            var speed = Mathf.Lerp(currentFrame.Cameras.ShakeSpeed, nextFrame.Cameras.ShakeSpeed, interPolationPoint);
            var cut = currentFrame.Cameras.CutFrame && timeController.DeltaRaceTime > timeController.RaceTime - currentFrame.FrameTime;
            camController.InGameCamera.transform.rotation *= camController.GetShake(horAmplitude, verAmplitude, speed, cut);*/
            //}
            camController.InGameCamera.fieldOfView = Mathf.Lerp(currentFrame.Cameras.FieldOfView, nextFrame.Cameras.FieldOfView, interPolationPoint);
            camController.InGameCamera.nearClipPlane = Mathf.Lerp(currentFrame.Cameras.NearClip, nextFrame.Cameras.NearClip, interPolationPoint);
         }
      }
   }
}

//public void GenerateSlowMotion()
//{
//    if (timeController.LinearSlowMo)
//        timeController.LinearSlowMotion(RaceReplay);
//    else
//        timeController.CalculateSlowmo(RaceReplay);

//    timeController.GearEvents = new List<GearEvent>();

//    foreach (var item in RaceReplay.GearEvents)
//        timeController.GearEvents.Add(item);

//    foreach (var item in RaceReplay.IntroRaceEvents)
//        timeController.RaceEvents.Add(item);
//    //foreach(var item in RightTrackReplay.GearEvents)
//    //   timeController.GearEvents.Add(item);

//    timeController.GearEvents.Sort((a, b) => a.Time.CompareTo(b.Time));
//}

//void OnLeftBurnoutChange(bool flag)
//{
//    if (flag)
//        RaceReplay.Burnouts[0].StartTime = timeController.ReplayTime;
//    else
//        RaceReplay.Burnouts[0].EndTime = timeController.ReplayTime;
//}

//void OnRightBurnoutChange(bool flag)
//{
//    if (flag)
//        RaceReplay.Burnouts[1].StartTime = timeController.ReplayTime;
//    else
//        RaceReplay.Burnouts[1].EndTime = timeController.ReplayTime;
//}
