using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Time controller states </summary>
public enum TimeControllerStates
{
    PLAYING,
    RECORDING,
    IDLE
};

/// <summary> Stores race times used for calculating slow motion speed </summary>
public struct SlowmoInfo
{
    public int RacerID;
    public float InitTime;
    public float EntryTime;
    public float FinishTime;
    public float ExitTime;
}

/// <summary> Maps race time to replay time when recording and stores it in replay.
/// Controls time progress in play mode
/// </summary>
public class TimeController : MonoBehaviour
{
    public float ReplayTime
    {
        get
        {
            return replayTime;
        }
        set
        {
            replayTime = value;
            if (TimeControllerState == TimeControllerStates.PLAYING)
                UpdateRaceTime();
        }
    }
    public TimeControllerStates TimeControllerState;
    public float WinnerTime { get; private set; }
    public float ReplaySpeed;
    public float RecordingTimestep;
    public delegate void TimeStateChange(string state);
    public List<GearEvent> GearEvents;
    public List<RaceEvent> RaceEvents;
    public List<TimeSlice> TimeSlices;
    public float SlowmoEntryTransition;
    public float SlowmoExitTransition;
    public float FreezeDuration;
    public float SlowmoSpeed;
    public float RaceIntroDelay;
    public float RaceEndingDelay;
    public float RaceInitDelay;
    public float RaceTime;
    public float RaceSpeed;
    public float DeltaReplayTime;
    public float DeltaRaceTime;
    public float SliderEndTime;
    public float LastEventOffset { get; private set; }
    public bool LinearSlowMo;
    public float TotalRaceDuration;
    public float SlowMoRaceSpeed;
    public float SlowMoTimeOffset;
    public float SlowMoInitDelay;
    public delegate void RacerFinishHandler(int racerId);
    public event RacerFinishHandler RacerFinish;

    public delegate void TimeChangeHandler(string newState);
    public event TimeChangeHandler TimeChange;
    public delegate void GearChangeHandler(GearEvent gearChangeEvent);
    public event GearChangeHandler GearChange;
    public float totalIntroWithDelays;
    float replayTime;
    RaceTrack raceTrack;

    /// <summary> Set's replay time to 0, and replay speed to 1 </summary>
    public void Reset()
    {
        ReplayTime = 0f;
        ReplaySpeed = 1f;
        TimeControllerState = TimeControllerStates.IDLE;
    }

    /// <summary> Updates replaytime, race time and delta times according to state of timecontroller </summary>
    public void OnUpdate()
    {
        if (TimeControllerState == TimeControllerStates.PLAYING)
        {
            DeltaReplayTime = Time.smoothDeltaTime * ReplaySpeed;
            ReplayTime += DeltaReplayTime;
            DeltaRaceTime = DeltaReplayTime * RaceSpeed;
        }

        if (TimeControllerState == TimeControllerStates.RECORDING)
        {
            ReplayTime += RecordingTimestep;
            RaceTime = ReplayTime;
            DeltaReplayTime = RecordingTimestep;
            DeltaRaceTime = DeltaReplayTime;
        }
    }

    TimeSlice PrevSlice()
    {
        return TimeSlices[TimeSlices.Count - 1];
    }

    public void LinearSlowMotion(Replay passedReplay)
    {
        if (raceTrack == null)
            raceTrack = gameObject.GetComponent<RaceTrack>();

        var leftRacerFinish = passedReplay.GetTimeAtDistance(402f, true);
        var rightRacerFinish = passedReplay.GetTimeAtDistance(402f, false);

        var winner = leftRacerFinish < rightRacerFinish ? leftRacerFinish : rightRacerFinish;
        var loser = leftRacerFinish > rightRacerFinish ? leftRacerFinish : rightRacerFinish;

        var winnerBeforeFinish = winner - SlowMoTimeOffset;
        var loserAfterFinish = loser + SlowMoTimeOffset;
        var winnerSlowStartToFinishLine = (winner - winnerBeforeFinish) / SlowMoRaceSpeed;

        RaceEvents = new List<RaceEvent>();
        foreach (var item in passedReplay.IntroRaceEvents)
            RaceEvents.Add(item);

        TimeSlices = new List<TimeSlice>();
        RaceEvents.Add(new RaceEvent(0f, RaceEvent.RepInit));
        RaceEvents.Add(new RaceEvent(RaceIntroDelay, RaceEvent.IntroStart));
        TimeSlices.Add(new TimeSlice(0f, RaceIntroDelay, 0f, 0f, 0f, false));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + raceTrack.TotalIntroDuration, PrevSlice().RaceTimeEnd, raceTrack.TotalIntroDuration, 1, false));
        RaceEvents.Add(new RaceEvent(PrevSlice().ReplayTimeEnd, RaceEvent.IntroEnd));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + RaceInitDelay, PrevSlice().RaceTimeEnd, PrevSlice().RaceTimeEnd, 0, false));
        var loserAfterRaceTime = loser - raceTrack.TotalIntroDuration + SlowMoTimeOffset;
        RaceEvents.Add(new RaceEvent(PrevSlice().ReplayTimeEnd, RaceEvent.RaceStart));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + loserAfterRaceTime, PrevSlice().RaceTimeEnd, PrevSlice().RaceTimeEnd + loserAfterRaceTime, 1, false));
        RaceEvents.Add(new RaceEvent(PrevSlice().GetReplayTime(winner), RaceEvent.WinnerFinish, winner - raceTrack.TotalIntroDuration));
        RaceEvents.Add(new RaceEvent(PrevSlice().GetReplayTime(loser), RaceEvent.LoserFinish, loser - raceTrack.TotalIntroDuration));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + SlowMoInitDelay, PrevSlice().RaceTimeEnd, PrevSlice().RaceTimeEnd, 0, false));
        RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 1].ReplayTimeStart, RaceEvent.SlowInit));
        RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 1].ReplayTimeEnd, RaceEvent.SlowStart));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + winnerSlowStartToFinishLine, winnerBeforeFinish, winner, SlowMoRaceSpeed, false));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + FreezeDuration, winner, winner, 0, false));
        var loserSlowMo = (loser - winner) / SlowMoRaceSpeed;
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + loserSlowMo, winner, loser, SlowMoRaceSpeed, false));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + FreezeDuration, loser, loser, 0f, false));
        var loserAfterFinishSlowMo = (loserAfterFinish - loser) / SlowMoRaceSpeed;
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + loserAfterFinishSlowMo, loser, loserAfterFinish, SlowMoRaceSpeed, false));
        TimeSlices.Add(new TimeSlice(PrevSlice().ReplayTimeEnd, PrevSlice().ReplayTimeEnd + RaceEndingDelay + Time.smoothDeltaTime, PrevSlice().RaceTimeEnd, PrevSlice().RaceTimeEnd, 0, false));
        RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 2].ReplayTimeEnd, RaceEvent.SlowEnd));
        RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 1].ReplayTimeEnd - Time.smoothDeltaTime, RaceEvent.RepEnd));
        var slowMoTotalTime = (loserAfterFinish - winnerBeforeFinish) / SlowMoRaceSpeed + (FreezeDuration * 2) + RaceInitDelay + SlowMoInitDelay;
        TotalRaceDuration = RaceIntroDelay + loser + SlowMoTimeOffset + slowMoTotalTime + RaceEndingDelay;
        WinnerTime = winner - raceTrack.TotalIntroDuration;
        //Debug.Log("1 : " + TotalRaceDuration + " second : " + GetTotalRaceTime((winnerAtFinish - raceTrack.TotalIntroDuration), (loserAtFinish - raceTrack.TotalIntroDuration)));
    }

    ///<summary> Generates time slices and events for given replay(play mode) </summary>
    public void CalculateSlowmo(Replay replay)
    {
        Debug.Log("exp slowmo");
        var slowmoTimes = GetAllSlowmos(replay);
        var finishTimes = new float[2];
        for (int i = 0; i < finishTimes.Length; ++i)
            finishTimes[i] = slowmoTimes[i].FinishTime;

        Array.Sort(slowmoTimes, (a, b) => a.FinishTime.CompareTo(b.FinishTime));
        var lastInit = slowmoTimes[slowmoTimes.Length - 1].InitTime;

        RaceEvents = new List<RaceEvent>();
        RaceEvents.Add(new RaceEvent(0f, RaceEvent.RepInit));
        RaceEvents.Add(new RaceEvent(RaceInitDelay, RaceEvent.RaceStart));
        RaceEvents.Add(new RaceEvent(slowmoTimes[slowmoTimes.Length - 1].InitTime + RaceInitDelay, RaceEvent.SlowInit));
        RaceEvents.Add(new RaceEvent(slowmoTimes[slowmoTimes.Length - 1].InitTime + RaceInitDelay + SlowMoInitDelay, RaceEvent.SlowStart));
        var slowSlices = GetSlowmoSlices(slowmoTimes);
        TimeSlices = new List<TimeSlice>();
        TimeSlices.Add(new TimeSlice(0f, new SlowmoSlice(new Vector2(0f, 1f), new Vector2(0f, 1f)), RaceInitDelay));
        TimeSlices.Add(new TimeSlice(RaceInitDelay, new SlowmoSlice(new Vector2(0f, 1f), new Vector2(lastInit + SlowMoInitDelay, 1f))));

        for (int i = 0; i < slowSlices.Count; ++i)
        {
            TimeSlices.Add(new TimeSlice(TimeSlices[TimeSlices.Count - 1].ReplayTimeEnd, slowSlices[i]));
            for (int j = 0; j < slowmoTimes.Length; ++j)
            {
                if (Mathf.Abs(TimeSlices[TimeSlices.Count - 1].RaceTimeEnd - slowmoTimes[j].FinishTime) < Mathf.Epsilon)
                {
                    var prev = TimeSlices[TimeSlices.Count - 1];
                    TimeSlices.Add(new TimeSlice(prev.ReplayTimeEnd, prev.ReplayTimeEnd + FreezeDuration, prev.RaceTimeEnd, prev.RaceTimeEnd, 0f, false));
                    RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 1].ReplayTimeStart, "finish " + slowmoTimes[j].RacerID));
                }
            }
        }
        RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 1].ReplayTimeEnd - RaceEndingDelay - Time.smoothDeltaTime, RaceEvent.SlowEnd));
        RaceEvents.Add(new RaceEvent(TimeSlices[TimeSlices.Count - 1].ReplayTimeEnd - Time.smoothDeltaTime, RaceEvent.RepEnd));
    }

    /// <summary> Sets race time based on replay time and time mapping in TimeSlices (playing mode) </summary>
    void UpdateRaceTime()
    {
        if (TimeSlices == null)
            return;
        if (TimeControllerState != TimeControllerStates.PLAYING && TimeSlices == null || TimeSlices.Count < 1)
        {
            RaceTime = 0f;
            return;
        }
        int j = 0;
        while (TimeSlices[j].ReplayTimeEnd < ReplayTime && j < TimeSlices.Count - 1)
            j++;
        RaceTime = TimeSlices[j].GetRaceTime(ReplayTime);
        RaceSpeed = TimeSlices[j].GetRaceSpeed(ReplayTime);

        PlayEvents();
        PlayGearEvents();
    }

    ///<summary> Generates slowmotion info for all racers</summary>
    SlowmoInfo[] GetAllSlowmos(Replay replays)
    {
        var result = new SlowmoInfo[2];
        //for(int i = 0; i < result.Length; ++i) {
        //   result[i].InitTime = replays[i].GetTimeAtDistance(402f) + SlowMoTimeOffset;
        //   result[i].EntryTime = replays[i].GetTimeAtDistance(402f - SlowmoEntryTransition);
        //   result[i].FinishTime = replays[i].GetTimeAtDistance(402f);
        //   result[i].ExitTime = replays[i].GetTimeAtDistance(402f + SlowmoExitTransition);
        //   result[i].RacerID = i + 1;
        //}
        return result;
    }

    /// <summary> Generate replay speed graphs for individual racers</summary>
    List<SlowmoSlice> GetSlowmoSlices(SlowmoInfo[] slowmoTimes)
    {
        var segments = new SlowmoSlice[slowmoTimes.Length * 2];
        for (int i = 0; i < slowmoTimes.Length; ++i)
        {
            segments[i * 2].Start = new Vector2(slowmoTimes[i].EntryTime, 1f);
            segments[i * 2].End = new Vector2(slowmoTimes[i].FinishTime, SlowmoSpeed);
            segments[i * 2 + 1].Start = segments[i * 2].End;
            segments[i * 2 + 1].End = new Vector2(slowmoTimes[i].ExitTime, 1f);
        }
        var points = new List<Vector2>();
        for (int i = 0; i < slowmoTimes.Length; ++i)
        {
            points.Add(new Vector2(slowmoTimes[i].EntryTime, 1f));
            points.Add(new Vector2(slowmoTimes[i].FinishTime, SlowmoSpeed));
            points.Add(new Vector2(slowmoTimes[i].ExitTime, 1f));
        }
        for (int i = 0; i < segments.Length - 2; i += 2)
        {
            for (int j = i + 2; j < segments.Length; j += 2)
            {
                var leftLeft = GetIntersection(segments[i].Start, segments[i].End, segments[j].Start, segments[j].End);
                if (leftLeft != Vector2.zero)
                    points.Add(leftLeft);
                var leftRight = GetIntersection(segments[i].Start, segments[i].End, segments[j + 1].Start, segments[j + 1].End);
                if (leftRight != Vector2.zero)
                    points.Add(leftRight);
                var rightLeft = GetIntersection(segments[i + 1].Start, segments[i + 1].End, segments[j].Start, segments[j].End);
                if (rightLeft != Vector2.zero)
                    points.Add(rightLeft);
                var rightRight = GetIntersection(segments[i + 1].Start, segments[i + 1].End, segments[j + 1].Start, segments[j + 1].End);
                if (rightRight != Vector2.zero)
                    points.Add(rightRight);
            }
        }
        var validPoints = new List<Vector2>();
        for (int i = 0; i < points.Count; ++i)
        {
            bool valid = true;
            for (int j = 0; j < segments.Length; ++j)
            {
                if (PointAboveLine(points[i], segments[j].Start, segments[j].End))
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
                validPoints.Add(points[i]);
        }
        validPoints.Sort((a, b) => a.x.CompareTo(b.x));
        var slowSlices = new List<SlowmoSlice>();
        for (int i = 0; i < validPoints.Count - 2; ++i)
            slowSlices.Add(new SlowmoSlice(validPoints[i], validPoints[i + 1]));
        slowSlices.Add(new SlowmoSlice(slowSlices[slowSlices.Count - 1].End, new Vector2(slowmoTimes[1].InitTime, 1f)));
        return slowSlices;
    }

    /// <summary> Concatenates individual replay speed graphs using lowest speed  </summary>
    Vector2 GetIntersection(Vector2 firstLineA, Vector2 firstLineB, Vector2 secondLineA, Vector2 secondLineB)
    {
        if (firstLineA.x > firstLineB.x)
        {
            var temp = firstLineA;
            firstLineA = firstLineB;
            firstLineB = temp;
        }
        if (secondLineA.x > secondLineB.x)
        {
            var temp = secondLineA;
            secondLineA = secondLineB;
            secondLineB = temp;
        }
        var firstSlope = (firstLineB.y - firstLineA.y) / (firstLineB.x - firstLineA.x);
        var firstIntercept = firstLineA.y - firstSlope * firstLineA.x;
        var secondSlope = (secondLineB.y - secondLineA.y) / (secondLineB.x - secondLineA.x);
        var secondIntercept = secondLineA.y - secondSlope * secondLineA.x;
        if (Mathf.Abs(firstSlope - secondSlope) < Mathf.Epsilon)
            return Vector2.zero;
        Vector2 intersection;
        intersection.x = (secondIntercept - firstIntercept) / (firstSlope - secondSlope);
        if (intersection.x < firstLineA.x || intersection.x > firstLineB.x || intersection.x < secondLineA.x || intersection.x > secondLineB.x)
            return Vector2.zero;
        intersection.y = firstSlope * intersection.x + firstIntercept;
        if (firstLineA.y > firstLineB.y)
        {
            var temp = firstLineA;
            firstLineA = firstLineB;
            firstLineB = temp;
        }
        if (secondLineA.y > secondLineB.y)
        {
            var temp = secondLineA;
            secondLineA = secondLineB;
            secondLineB = temp;
        }
        if (intersection.y < firstLineA.y || intersection.y > firstLineB.y || intersection.y < secondLineA.y || intersection.y > secondLineB.y)
            return Vector2.zero;
        return intersection;
    }

    /// <summary> Whether the given point is directly above the line segment defined by line start and end </summary>
    bool PointAboveLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        var leftX = lineStart.x < lineEnd.x ? lineStart.x : lineEnd.x;
        var rightX = lineStart.x > lineEnd.x ? lineStart.x : lineEnd.x;
        if (point.x < leftX || point.x > rightX)
            return false;
        var startToPoint = point - lineStart;
        var endToPoint = point - lineEnd;
        var cross = Vector3.Cross(startToPoint, endToPoint);
        return cross.z - 0.00001f > 0f;
    }

    /// <summary> Sends events at appropriate replay time (playing mode) </summary>
    void PlayEvents()
    {
        if (RaceEvents == null)
            return;
        for (int i = 0; i < RaceEvents.Count; ++i)
        {
            if (RaceEvents[i].Fired && RaceEvents[i].Time > ReplayTime)
                RaceEvents[i].Fired = false;
            if (!RaceEvents[i].Fired && RaceEvents[i].Time <= ReplayTime)
            {
                RaceEvents[i].Fired = true;
                //Debug.Log("race event : " + RaceEvents[i].Message + " time : " + RaceEvents[i].Time + " rt " + RaceEvents[i].RaceTime);
                LastEventOffset = ReplayTime - RaceEvents[i].Time;
                var splitMessage = RaceEvents[i].Message.Split(' ');
                if (RacerFinish != null && splitMessage.Length > 1 && splitMessage[0] == "finish")
                    RacerFinish(int.Parse(splitMessage[1]));
                else
                {
                    if (TimeChange != null && RaceEvent.MessageToInt(RaceEvents[i].Message) >= 0)
                        TimeChange(RaceEvents[i].Message);
                    else
                        Debug.Log("Invalid event message: " + RaceEvents[i].Message);
                }
            }
        }
    }

    void PlayGearEvents()
    {
        for (int i = 0; i < GearEvents.Count; i++)
        {
            if (GearEvents[i].Fired && GearEvents[i].Time > RaceTime)
                GearEvents[i].Fired = false;
            if (!GearEvents[i].Fired && GearEvents[i].Time <= RaceTime)
            {
                GearEvents[i].Fired = true;
                if (GearChange != null)
                    GearChange(GearEvents[i]);
                else
                    Debug.LogWarning("No listener");
            }
        }
    }

    float GetTotalRaceTime(float winnerTime, float loserTime)
    {
        var OtherDelays = RaceIntroDelay + RaceInitDelay + SlowMoInitDelay + RaceEndingDelay + FreezeDuration * 2 + raceTrack.TotalIntroDuration;
        var TotalRaceTime = (loserTime - winnerTime + 2 * SlowMoTimeOffset) / SlowMoRaceSpeed + OtherDelays + loserTime + SlowMoTimeOffset;
        return TotalRaceTime;
    }
}
