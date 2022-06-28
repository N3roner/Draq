using UnityEngine;

/// <summary>
/// SlowmoSlice
///
/// A segment of slowmotion with constant acceleration;
/// x values are race time and y values are replay speed
/// </summary>
public struct SlowmoSlice {
   public Vector2 Start;
   public Vector2 End;

   /** constructor assures the fields are assigned */
   public SlowmoSlice(Vector2 start, Vector2 end) {
      Start = start;
      End = end;
   }
}

/// <summary>
/// TimeSlice
///
/// Maps a segment of slowmotion with constant acceleration to replay time
/// </summary>
public class TimeSlice {
   public float RaceTimeStart { get; private set; }
   public float RaceTimeEnd { get; private set; }
   public float ReplayTimeStart { get; private set; }
   public float ReplayTimeEnd { get; private set; }
   public float StartSpeed { get; private set; }
   public float EndSpeed { get; private set; }

   /** create TimeSlice from SlowmoSlice, offset can be used to override 1:1 mapping to slowmo speeds */
   public TimeSlice(float start, SlowmoSlice slowmo, float offset = 0f) {
      RaceTimeStart = slowmo.Start.x;
      RaceTimeEnd = slowmo.End.x;
      ReplayTimeStart = start;
      StartSpeed = slowmo.Start.y;
      EndSpeed = slowmo.End.y;
      float minY = slowmo.Start.y < slowmo.End.y ? slowmo.Start.y : slowmo.End.y;
      float deltaY = Mathf.Abs(slowmo.Start.y - slowmo.End.y);
      if(Mathf.Abs(minY + deltaY / 2f) < Mathf.Epsilon)
         ReplayTimeEnd = ReplayTimeStart + offset;
      else
         ReplayTimeEnd = ReplayTimeStart + (slowmo.End.x - slowmo.Start.x) / (minY + deltaY / 2f) + offset;
   }

   /** create TimeSlice with explicitly defined start and end for race and replay time */
   public TimeSlice(float repStart, float repEnd, float raceStart, float raceEnd, float startSpeed, bool endSpeed) {
      ReplayTimeStart = repStart;
      ReplayTimeEnd = repEnd;
      RaceTimeStart = raceStart;
      RaceTimeEnd = raceEnd;
      if(Mathf.Abs(RaceTimeEnd - RaceTimeStart) < Mathf.Epsilon) {
         StartSpeed = 0f;
         EndSpeed = 0f;
      } else {
         StartSpeed = startSpeed;
         if(!endSpeed) {
            EndSpeed = StartSpeed;
            return;
         }

         var avgSpeed = (RaceTimeEnd - RaceTimeStart) / (ReplayTimeEnd - ReplayTimeStart);
         EndSpeed = 2f * avgSpeed - startSpeed;
         if(EndSpeed < 0f) {
            Debug.LogWarning("TimeSlice calculated EndSpeed < 0, please check inputs");
            EndSpeed = 0f;
            StartSpeed = 2f * (RaceTimeEnd - RaceTimeStart) / (RaceTimeEnd - RaceTimeStart);
         }

      }
   }

   /** returns race time corresponding to given replay time, or closest race time in the slice if replay time is out of range */
   public float GetRaceTime(float replayTime) {
      if(replayTime < ReplayTimeStart || replayTime > ReplayTimeEnd) {
         Debug.LogWarning("Warning: requested replay time (" + replayTime + ") out of range of time slice (" + ReplayTimeStart + " - " + ReplayTimeEnd + "), returning closest");
         //RaceInterface.Log("Warning: requested replay time (" + replayTime + ") out of range of time slice (" + ReplayTimeStart + " - " + ReplayTimeEnd + "), returning closest");
         if(replayTime < ReplayTimeStart)
            return RaceTimeStart;
         if(replayTime > ReplayTimeEnd)
            return RaceTimeEnd;
      }
      var currentSpeed = GetRaceSpeed(replayTime);
      var minSpeed = StartSpeed < currentSpeed ? StartSpeed : currentSpeed;

      return Mathf.Clamp(RaceTimeStart + (replayTime - ReplayTimeStart) * (minSpeed + Mathf.Abs(StartSpeed - currentSpeed) / 2f), RaceTimeStart, RaceTimeEnd);
   }

   /** get race time / replay time ratio at given replay time, or start/end speed if replay time is out of range */
   public float GetRaceSpeed(float replayTime) {
      if(replayTime < ReplayTimeStart || replayTime > ReplayTimeEnd)
         Debug.LogWarning("Warning: requested replay time(" + replayTime + ") out of range of time slice(" + ReplayTimeStart + " - " + ReplayTimeEnd + "), returning closest");
      //RaceInterface.Log("Warning: requested replay time (" + replayTime + ") out of range of time slice (" + ReplayTimeStart + " - " + ReplayTimeEnd + "), returning closest");
      if(replayTime < ReplayTimeStart)
         return StartSpeed;
      if(replayTime > ReplayTimeEnd)
         return EndSpeed;
      var interpolationPoint = (replayTime - ReplayTimeStart) / (ReplayTimeEnd - ReplayTimeStart);
      return Mathf.Lerp(StartSpeed, EndSpeed, interpolationPoint);
   }

   /** get replay time corresponding to given race time, or start/end replay time if race time is out of range (NOTE: accuracy of this method has not been verified) */
   public float GetReplayTime(float raceTime) {
      if(raceTime < RaceTimeStart || RaceTimeStart > RaceTimeEnd) {
         Debug.LogWarning("Warning: requested race time (" + raceTime + ") out of range of time slice (" + ReplayTimeStart + " - " + ReplayTimeEnd + "), returning closest");
         //RaceInterface.Log("Warning: requested race time (" + raceTime + ") out of range of time slice (" + ReplayTimeStart + " - " + ReplayTimeEnd + "), returning closest");
         if(raceTime < ReplayTimeStart)
            return RaceTimeStart;
         if(raceTime > RaceTimeEnd)
            return ReplayTimeEnd;
      }

      var raceTimeNormalized = (raceTime - RaceTimeStart) / (RaceTimeEnd - RaceTimeStart);
      var replTime = (ReplayTimeEnd - ReplayTimeStart);
      var ret = raceTimeNormalized * replTime + ReplayTimeStart;
      return ret;

      //var deltaV = Mathf.Abs(EndSpeed - StartSpeed);
      //var deltaR = ReplayTimeEnd - ReplayTimeStart;
      //var a = 2f * deltaV / deltaR;
      //var b = StartSpeed < EndSpeed ? StartSpeed : EndSpeed;
      //var c = -(raceTime - RaceTimeStart);
      //return (-b + Mathf.Sqrt(b * b - 4f * a * c)) / (2f * a);
   }
}
