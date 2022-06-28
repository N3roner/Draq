using System;

/// <summary>
/// RaceEvent
///
/// Stores timestamped messages for use by Race system's events
/// </summary>
public class RaceEvent : IComparable<RaceEvent>
{

    public float Time;
    public float RaceTime;
    public string Message;
    public bool Fired;

    public const string IntroStart = "start introduction";
    public const string IntroEnd = "end intro";
    public const string RaceStart = "start race";
    public const string SlowInit = "init slowmo";
    public const string SlowStart = "start slowmo";
    public const string SlowEnd = "end slowmo";
    public const string RepInit = "init replay";
    //public const string RepStart = "start race";
    public const string RepEnd = "end race";
    public const string WinnerFinish = "winner at finish";
    public const string LoserFinish = "loser at finish";
    public const string LeftRacerBurnout = "left racer burnout";
    public const string RightRacerBurnout = "right racer burnout";

    /** numerical representations of event messages */
    enum EventCodes
    {
        REPLAY_INIT,
        INTRO_START,
        INTRO_END,
        RACE_START,
        SLOWMO_INIT,
        SLOWMO_START,
        SLOWMO_END,
        REPLAY_END,
        RACER_FINISH,
        WINNER_FINISH,
        LOSER_FINISH,
        LEFT_BURNOUT,
        RIGHT_BURNOUT
    };

    /** constructors assures that all the fields are assigned */
    public RaceEvent(float time, string message, float raceTime = 0f)
    {
        Time = time;
        Message = message;
        Fired = false;
        if (raceTime != 0f)
            RaceTime = raceTime;
    }

    /** allows sorting events by timestamp */
    public int CompareTo(RaceEvent otherEvent)
    {
        return Time.CompareTo(otherEvent.Time);
    }

    /** converts event message to integer code for easier storage in replay file */
    public static int MessageToInt(string message)
    {
        if (message == IntroStart)
            return (int)EventCodes.INTRO_START;
        if (message == IntroEnd)
            return (int)EventCodes.INTRO_END;
        if (message == RepInit)
            return (int)EventCodes.REPLAY_INIT;
        if (message == RaceStart)
            return (int)EventCodes.RACE_START;
        if (message == SlowInit)
            return (int)EventCodes.SLOWMO_INIT;
        if (message == SlowStart)
            return (int)EventCodes.SLOWMO_START;
        if (message == SlowEnd)
            return (int)EventCodes.SLOWMO_END;
        if (message == RepEnd)
            return (int)EventCodes.REPLAY_END;
        if (message == WinnerFinish)
            return (int)EventCodes.WINNER_FINISH;
        if (message == LoserFinish)
            return (int)EventCodes.LOSER_FINISH;
        if (message == LeftRacerBurnout)
            return (int)EventCodes.LEFT_BURNOUT;
        if (message == RightRacerBurnout)
            return (int)EventCodes.RIGHT_BURNOUT;
        var splitMessage = message.Split(' ');
        if (splitMessage.Length < 2 || splitMessage[0] != "finish")
            return -1;
        return int.Parse(splitMessage[1]) + (int)EventCodes.RACER_FINISH;
    }

    /** converts integer code back to event message so it can be used by the Race system */
    public static string IntToMesage(int messageCode)
    {
        if (messageCode == (int)EventCodes.REPLAY_INIT)
            return RepInit;
        if (messageCode == (int)EventCodes.INTRO_END)
            return IntroEnd;
        if (messageCode == (int)EventCodes.RACE_START)
            return RaceStart;
        if (messageCode == (int)EventCodes.SLOWMO_INIT)
            return SlowInit;
        if (messageCode == (int)EventCodes.SLOWMO_START)
            return SlowStart;
        if (messageCode == (int)EventCodes.SLOWMO_END)
            return SlowEnd;
        if (messageCode == (int)EventCodes.REPLAY_END)
            return RepEnd;
        if (messageCode == (int)EventCodes.WINNER_FINISH)
            return WinnerFinish;
        if (messageCode == (int)EventCodes.LOSER_FINISH)
            return LoserFinish;
        if (messageCode <= (int)EventCodes.RACER_FINISH)
            return "Error: invalid event message";
        if (messageCode == (int)EventCodes.LEFT_BURNOUT)
            return LeftRacerBurnout;
        if (messageCode == (int)EventCodes.RIGHT_BURNOUT)
            return RightRacerBurnout;
        return "finish " + (messageCode - (int)EventCodes.RACER_FINISH);
    }
}
