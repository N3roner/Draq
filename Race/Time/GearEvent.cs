using System;

public class GearEvent : IComparable<GearEvent>
{
    public float Time;
    public int Gear;
    public int TrackID;
    public bool Fired;

    public GearEvent(float time, int gear, int trackID)
    {
        Time = time;
        Gear = gear;
        TrackID = trackID;
        Fired = false;
    }

    public int CompareTo(GearEvent other)
    {
        return Time.CompareTo(other.Time);
    }
}
