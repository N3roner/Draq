using UnityEngine;
using System.Collections.Generic;

public struct RacersBodyInformations
{
    public Vector3 Position;
    public Quaternion Rotation;
}

public struct CameraInformations
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float FieldOfView;
    public float NearClip;
    public bool CutFrame;
}

public struct RacerInformations
{
    public Vector3 Position;
    public float Distance;
    public int DisplayedRPMs;
    public int Speed;
    public Quaternion Rotation;
    public RacersBodyInformations BodyInfo;
    public Quaternion FrontWheelsRotation;
    public Quaternion BackWheelsRotation;
}

public struct RaceFrame
{
    public RacerInformations LeftTrackRacer;
    public RacerInformations RightTrackRacer;
    public CameraInformations Cameras;
    public float FrameTime;
}

public enum ReplayStates
{
    LOADING,
    READY,
    UNAVAILABLE,
    FINISHED,
    SAVED
};

public struct BurnoutInfo
{
    public float StartTime;
    public float EndTime;
}

/// <summary> Used for storing informations about Racer while recording </summary>
public class Replay
{
    public ReplayStates ReplayState;
    public string FileLocation;
    public string ReplayName;
    public List<RaceFrame> Frames;
    public List<RaceEvent> IntroRaceEvents;
    public List<GearEvent> GearEvents;
    public List<CamShakeInfo> ShakeQueue;
    public BurnoutInfo[] Burnouts;

    /// <summary> Used for instantiating new replay </summary>
    public Replay()
    {
        Frames = new List<RaceFrame>();
        ReplayState = ReplayStates.LOADING;
        Burnouts = new BurnoutInfo[2];
    }

    /// <summary> Adds a single race frame into Frames array </summary>
    public void AddFrame(RaceFrame raceFrame)
    {
        Frames.Add(raceFrame);
    }

    /// <summary> Returns last keyframe before time passed as parameter </summary>
    public int GetFrameIndex(float time)
    {
        if (Frames.Count <= 0)
            return 0;
        int result = Mathf.FloorToInt(time / Frames[Frames.Count - 1].FrameTime * (Frames.Count - 1));

        if (result >= Frames.Count - 1)
            return -1;

        while (result > 0 && (result >= Frames.Count || Frames[result].FrameTime > time))
            result--;
        while (result < Frames.Count - 1 && (result < 0 || Frames[result + 1].FrameTime < time))
            result++;
        return result;
    }

    public string GetRacerdId()
    {
        var indexOfExtension = ReplayName.IndexOf(".bytes");
        return ReplayName.Split('_')[0].Substring(0, indexOfExtension);
    }

    /// <summary> Retrieves a frame with given index, closest frame if index is out of range or null frame if the replay is empty   </summary>
    public RaceFrame GetFrame(int index)
    {
        if (Frames.Count == 0)
        {
            RaceFrame errorFrame = new RaceFrame();
            errorFrame.FrameTime = -1;
            return errorFrame;
        }
        if (index >= Frames.Count)
            return Frames[Frames.Count - 1];
        return Frames[index];
    }

    public CamShakeInfo GetShake(float time)
    {
        var index = 0;
        while (index < ShakeQueue.Count - 1 && ShakeQueue[index + 1].StartTime < time)
            index++;
        return ShakeQueue[index];
    }

   /// <summary> Returns time  </summary>
   public float GetTimeAtDistance(float distance, bool leftTrackRacer) {
      int frameIndex = 0;

      if(leftTrackRacer) {
         while(frameIndex < Frames.Count - 1 && Frames[frameIndex + 1].LeftTrackRacer.Distance < distance)
            frameIndex++;

         float deltaT = Frames[frameIndex + 1].FrameTime - Frames[frameIndex].FrameTime;
         float deltaD = Frames[frameIndex + 1].LeftTrackRacer.Distance - Frames[frameIndex].LeftTrackRacer.Distance;
         return Frames[frameIndex].FrameTime + deltaT * ((distance - Frames[frameIndex].LeftTrackRacer.Distance) / deltaD);
      } else {
         while(frameIndex < Frames.Count - 1 && Frames[frameIndex + 1].RightTrackRacer.Distance < distance)
            frameIndex++;
         if(frameIndex == Frames.Count - 1)
            frameIndex -= 1;
         float deltaT = Frames[frameIndex + 1].FrameTime - Frames[frameIndex].FrameTime;
         float deltaD = Frames[frameIndex + 1].RightTrackRacer.Distance - Frames[frameIndex].RightTrackRacer.Distance;
         return Frames[frameIndex].FrameTime + deltaT * ((distance - Frames[frameIndex].RightTrackRacer.Distance) / deltaD);
      }
   }
}
