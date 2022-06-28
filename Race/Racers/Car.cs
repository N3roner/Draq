using UnityEngine;
using System.Collections.Generic;

public enum RacerStates
{
    RACING,
    ATSTART,
    INTRO
};

public enum WheelDrive
{
    FRONT,
    REAR
}

[System.Serializable]
public struct SpeedSettings
{
    public float XFrom;
    public float XTo;
    public float TotalIntegralArea;
    public float YAxisShift;
}

/// <summary> Used for storing informations about specific Racer.
/// Contains methods for calculating values needed to accelerate the Car
/// </summary>
public class Car : MonoBehaviour, IGraphable
{
    public int CarID;
    public float FinishTime;
    public int TotalGears;
    public Transform CarBody;
    public Transform FrontWheels;
    public Transform BackWheels;
    public float TireCircumference;
    public float TireDiameter;
    public int currentGear = 0;
    public bool[] LengthUserLocks;
    public bool[] TimeUserLocks;
    public float[] LengthPercents;
    public float[] TimePercents;
    public List<float> MaxSpeedPerGear;
    public List<float> GearsTimings;
    public List<float> GearsDistances;
    public int Speed;
    public int TotalRPM;
    public int RPMSweetSpot;
    public int DisplayedRPM;
    public int RPMAtStart;
    public int RPMDropPercent;
    public bool FinishedRace;
    public RacerStates RacerState
    {
        get { return racerState; }
        set { racerState = value; }
    }
    public WheelDrive WheelDrive;
    public List<GearEvent> GearEvents;
    public List<RaceEvent> IntroEvents;
    public SpeedSettings[] SpeedFunctionSettings;
    RacerStates racerState;
    float burnOutRotationSpeed;
    bool leftTrack;
    int rpmDecrease;
    WindowUpdateHandle changeEvent;
    RaceTrack raceTrack;
    TimeController timeController;
    CustomAnimator animatorComponent;
    IntroSettings introSettings;

    /// <summary> Used for setting initial values of a Car. </summary>
    public void Initialize(IntroSettings passedIntroSettings, GameObject racePrefab, bool passedLeftTrack = false)
    {
        InitializeComponents(racePrefab);
        InitializeCarParts();
        EngineSetup();
        if (RacerState == RacerStates.ATSTART)
            gameObject.transform.position = passedIntroSettings.StartLinePosition;
        else
        {
            gameObject.transform.position = passedIntroSettings.SpawnPosition;
            GearEvents = new List<GearEvent>();
            IntroEvents = new List<RaceEvent>();
            racerState = RacerStates.INTRO;
        }
        FinishedRace = false;
        leftTrack = passedLeftTrack;
        introSettings = new IntroSettings();
        introSettings = passedIntroSettings;
    }

    /// <summary> Updates racer position, speed etc. in recording mode </summary>
    public void OnUpdate()
    {
        Vector3 previousObjectPosition = gameObject.transform.position;
        if (RacerState == RacerStates.INTRO)
        {
            Vector3 objectPosition = gameObject.transform.position;
            if (timeController.ReplayTime <= introSettings.SpawnBurnoutTransitionTime && gameObject.transform.position != introSettings.BurnoutPosition)
            {
                if (animatorComponent && animatorComponent.PlayableAnimations.IntroAnimations)
                    animatorComponent.StartAnimationOfType(AnimationType.START, 1);

                objectPosition.z = introSettings.SpawnPosition.z + SpeedIntegral(timeController.ReplayTime, introSettings.SpawnBurnoutTransitionTime, introSettings.BurnoutPosition.z - introSettings.SpawnPosition.z);
                gameObject.transform.position = objectPosition;

                var deltaPosition = objectPosition.z - previousObjectPosition.z;
                UpdateSpeed(deltaPosition / Time.smoothDeltaTime);
                RotateWheels(deltaPosition);
            }

            if (timeController.ReplayTime > introSettings.SpawnBurnoutTransitionTime && timeController.ReplayTime <= introSettings.SpawnBurnoutTransitionTime + introSettings.BurnoutTime)
            {
                if (IntroEvents.Count == 0)
                {
                    if (leftTrack)
                        IntroEvents.Add(new RaceEvent(timeController.ReplayTime, RaceEvent.LeftRacerBurnout, timeController.ReplayTime + raceTrack.LeftTrackCarIntro.BurnoutTime));
                    else
                        IntroEvents.Add(new RaceEvent(timeController.ReplayTime, RaceEvent.RightRacerBurnout, timeController.ReplayTime + raceTrack.RightTrackCarIntro.BurnoutTime));
                }
                if (animatorComponent && animatorComponent.PlayableAnimations.IntroAnimations)
                    animatorComponent.StartAnimationOfType(AnimationType.BURNOUT, 2);
                burnOutRotationSpeed += 5f;
                var wheelSpinSpeed = burnOutRotationSpeed / TireCircumference * 360 * Time.smoothDeltaTime;
                RotateWheels(wheelSpinSpeed, true, false);
            }

            if (timeController.ReplayTime > introSettings.SpawnBurnoutTransitionTime + introSettings.BurnoutTime && gameObject.transform.position != introSettings.StartLinePosition)
            {
                if (animatorComponent && animatorComponent.PlayableAnimations.IntroAnimations)
                    animatorComponent.StartAnimationOfType(AnimationType.START, 3);

                objectPosition.z = introSettings.BurnoutPosition.z + SpeedIntegral(timeController.ReplayTime - introSettings.SpawnBurnoutTransitionTime - introSettings.BurnoutTime, introSettings.BurnoutStartTransitionTime, introSettings.StartLinePosition.z - introSettings.BurnoutPosition.z);

                if (timeController.ReplayTime + Time.smoothDeltaTime > introSettings.SpawnBurnoutTransitionTime + introSettings.BurnoutTime + introSettings.BurnoutStartTransitionTime)
                {
                    objectPosition.z = 0f;
                    gameObject.transform.eulerAngles = Vector3.zero;
                }
                gameObject.transform.position = objectPosition;

                var deltaPosition = objectPosition.z - previousObjectPosition.z;
                UpdateSpeed(deltaPosition / Time.smoothDeltaTime);
                RotateWheels(deltaPosition);

            }
            if (gameObject.transform.position == introSettings.StartLinePosition && RacerState != RacerStates.ATSTART)
                ChangeState(RacerStates.ATSTART);
        }

        if (RacerState == RacerStates.RACING)
        {
            UpdatePosition();
            var deltaPosition = gameObject.transform.position.z - previousObjectPosition.z;
            RotateWheels(deltaPosition);
            UpdateSpeed(deltaPosition / timeController.DeltaReplayTime);
        }
        if (RacerState == RacerStates.ATSTART && timeController.ReplayTime > raceTrack.TotalIntroDuration)
            ChangeState(RacerStates.RACING);
    }

    public float GetTopSpeed(float finishTime)
    {
        FinishTime = finishTime;
        SetTopSpeeds();
        var maxSpeed = MaxSpeedPerGear[0];
        for (int i = 0; i < MaxSpeedPerGear.Count - 1; i++)
            if (MaxSpeedPerGear[i] > maxSpeed)
                maxSpeed = MaxSpeedPerGear[i];
        return MaxSpeedPerGear[MaxSpeedPerGear.Count - 1];
    }

    /// <summary> Set's top speeds for each gear according to previously set gear timings and distances </summary>
    public void SetTopSpeeds()
    {
        SetDistances();
        SetTimings();
        if (SpeedFunctionSettings == null || SpeedFunctionSettings.Length != TotalGears)
            SpeedFunctionSettings = new SpeedSettings[TotalGears];
        MaxSpeedPerGear = new List<float>();
        float deltaDistance = 0f;
        float deltaTime = 0f;
        float xAxisTotal;
        float yAxis;
        float integralAreaPart;
        float speedTimeRectangle;
        float unshiftedIntegralArea;
        for (int i = 0; i < TotalGears; i++)
        {
            var maxSpeed = 0f;
            if (i == 0)
            {
                deltaDistance = GearsDistances[i];
                deltaTime = GearsTimings[i];

                SpeedFunctionSettings[i].YAxisShift = Mathf.Abs(Mathf.Sin(SpeedFunctionSettings[i].XFrom));
                xAxisTotal = Mathf.Abs(SpeedFunctionSettings[i].XFrom) + SpeedFunctionSettings[i].XTo;
                yAxis = SpeedFunctionSettings[i].YAxisShift + Mathf.Sin(SpeedFunctionSettings[i].XTo);
                unshiftedIntegralArea = Mathf.Abs(SpeedFunctionSettings[i].XFrom) + Mathf.Cos(SpeedFunctionSettings[i].XFrom) + SpeedFunctionSettings[i].XTo - Mathf.Cos(SpeedFunctionSettings[i].XTo);
                SpeedFunctionSettings[i].TotalIntegralArea = unshiftedIntegralArea - ((1 - SpeedFunctionSettings[i].YAxisShift) * xAxisTotal);
                //integralAreaPart => udio povrsine integrala u kvadratu
                integralAreaPart = SpeedFunctionSettings[i].TotalIntegralArea * 100f / (xAxisTotal * yAxis);
                speedTimeRectangle = deltaDistance * 100f / integralAreaPart;
                maxSpeed = speedTimeRectangle / deltaTime;
                SpeedFunctionSettings[i].TotalIntegralArea = SpeedFunctionSettings[i].TotalIntegralArea;
            }
            else
            {
                deltaTime = GearsTimings[i] - GearsTimings[i - 1];
                deltaDistance = GearsDistances[i] - GearsDistances[i - 1] - (MaxSpeedPerGear[i - 1] * (GearsTimings[i] - GearsTimings[i - 1]));

                SpeedFunctionSettings[i].YAxisShift = Mathf.Abs(Mathf.Sin(SpeedFunctionSettings[i].XFrom));
                xAxisTotal = Mathf.Abs(SpeedFunctionSettings[i].XFrom) + SpeedFunctionSettings[i].XTo;
                yAxis = SpeedFunctionSettings[i].YAxisShift + Mathf.Sin(SpeedFunctionSettings[i].XTo);
                unshiftedIntegralArea = Mathf.Abs(SpeedFunctionSettings[i].XFrom) + Mathf.Cos(SpeedFunctionSettings[i].XFrom) + SpeedFunctionSettings[i].XTo - Mathf.Cos(SpeedFunctionSettings[i].XTo);
                SpeedFunctionSettings[i].TotalIntegralArea = unshiftedIntegralArea - ((1 - SpeedFunctionSettings[i].YAxisShift) * xAxisTotal);
                integralAreaPart = SpeedFunctionSettings[i].TotalIntegralArea * 100f / (xAxisTotal * yAxis);
                speedTimeRectangle = deltaDistance * 100f / integralAreaPart;
                maxSpeed = MaxSpeedPerGear[i - 1] + speedTimeRectangle / deltaTime;
                SpeedFunctionSettings[i].YAxisShift = SpeedFunctionSettings[i].YAxisShift;
                SpeedFunctionSettings[i].TotalIntegralArea = SpeedFunctionSettings[i].TotalIntegralArea;
            }
            MaxSpeedPerGear.Add(maxSpeed);
        }
        UpdateAnalyzer();
    }

    public void InitializeCarParts()
    {
        var axles = gameObject.transform.FindChild("Axles");
        for (int i = 0; i < 2; i++)
        {
            if (axles.GetChild(i).name.Contains("axle_front"))
                FrontWheels = axles.GetChild(i).transform.GetChild(0);
            if (axles.GetChild(i).name.Contains("axle_back"))
                BackWheels = axles.GetChild(i).transform.GetChild(0);
        }
        CarBody = gameObject.transform.FindChild("FWD").transform.FindChild("RWD").GetChild(0);
    }
    public void InitArrays()
    {
        if (LengthPercents == null)
            LengthPercents = new float[TotalGears];
        if (TimePercents == null)
            TimePercents = new float[TotalGears];
        if (LengthUserLocks == null || LengthUserLocks.Length != TotalGears)
            LengthUserLocks = new bool[TotalGears];
        if (TimeUserLocks == null || TimeUserLocks.Length != TotalGears)
            TimeUserLocks = new bool[TotalGears];
        if (SpeedFunctionSettings == null || SpeedFunctionSettings.Length != TotalGears)
            SpeedFunctionSettings = new SpeedSettings[TotalGears];
    }

    /// <summary> Returns z position of car. More about integrating sine in notebook no.2 </summary>
    float GetDistance(int currentGear, float from, float to, float t)
    {
        float distance = 0;
        float deltaTime = 0;
        float x;
        float xNormalized;
        float baseIntegral;
        float deltaDistance;
        float areaRatio;

        if (currentGear == 0)
        {
            deltaTime = GearsTimings[currentGear];
            deltaDistance = GearsDistances[currentGear];
            x = (Mathf.Abs(from) + to) * (t / deltaTime) - Mathf.Abs(from);
            xNormalized = (Mathf.Abs(from) + to) * (t / deltaTime);
            baseIntegral = SpeedFunctionSettings[currentGear].YAxisShift * xNormalized - Mathf.Cos(x) + Mathf.Cos(from);
            areaRatio = baseIntegral / SpeedFunctionSettings[currentGear].TotalIntegralArea;
            distance = areaRatio * deltaDistance;
        }
        else
        {
            deltaTime = GearsTimings[currentGear] - GearsTimings[currentGear - 1];
            deltaDistance = GearsDistances[currentGear] - GearsDistances[currentGear - 1] - (MaxSpeedPerGear[currentGear - 1] * deltaTime);
            var normalizedTime = t - GearsTimings[currentGear - 1];
            x = (Mathf.Abs(from) + to) * (normalizedTime / deltaTime) - Mathf.Abs(from);
            xNormalized = (Mathf.Abs(from) + to) * (normalizedTime / deltaTime);
            baseIntegral = SpeedFunctionSettings[currentGear].YAxisShift * xNormalized - Mathf.Cos(x) + Mathf.Cos(from);
            areaRatio = baseIntegral / SpeedFunctionSettings[currentGear].TotalIntegralArea;
            var rectangle = MaxSpeedPerGear[currentGear - 1] * normalizedTime;
            distance = GearsDistances[currentGear - 1] + rectangle + areaRatio * deltaDistance;
        }
        return distance;
    }

    /// <summary> Calculates car speed, distance overtaken, and next transform position </summary>
    void UpdatePosition()
    {
        if (animatorComponent)
        {
            var toAdd = 0;
            if (!animatorComponent.PlayableAnimations.IntroAnimations && animatorComponent.PlayableAnimations.RaceAnimations)
                toAdd = 1;
            if (animatorComponent.PlayableAnimations.RaceAnimations && animatorComponent.PlayableAnimations.IntroAnimations)
                toAdd = 4;
            animatorComponent.StartAnimationOfType(AnimationType.START, toAdd);
        }
        SwitchGears();
        var tempPos = gameObject.transform.position;
        tempPos.z = GetDistance(currentGear, SpeedFunctionSettings[currentGear].XFrom, SpeedFunctionSettings[currentGear].XTo, timeController.ReplayTime - raceTrack.TotalIntroDuration);
        gameObject.transform.position = tempPos;
        RPMUpdate();
        if (timeController.ReplayTime >= (raceTrack.TotalIntroDuration + FinishTime + timeController.SlowMoTimeOffset))
            FinishedRace = true;
    }

    /// <summary> Used for rotating wheels in play mode </summary>
    void RotateWheels(float deltaPosition, bool backWheelsOnly = false, bool calculateSpeed = true)
    {
        if (deltaPosition <= 0f)
            return;
        if (calculateSpeed)
            deltaPosition = deltaPosition / TireCircumference * 360;
        if (!backWheelsOnly)
            FrontWheels.Rotate(Vector3.right, deltaPosition);
        BackWheels.Rotate(Vector3.right, deltaPosition);

        if (backWheelsOnly)
            FrontWheels.eulerAngles = Vector3.zero;
    }

    void EngineSetup()
    {
        DisplayedRPM = RPMAtStart;
        if (RPMSweetSpot == 0)
            RPMSweetSpot = TotalRPM - (TotalRPM * 17) / 100;
        rpmDecrease = RPMSweetSpot - (RPMSweetSpot * RPMDropPercent / 100);
        SetTopSpeeds();
        currentGear = 0;
        TireCircumference = TireDiameter * Mathf.PI;
    }

    void InitializeComponents(GameObject racePrefab)
    {
        if (gameObject.GetComponent<CustomAnimator>())
            animatorComponent = gameObject.GetComponent<CustomAnimator>();
        if (racePrefab.GetComponent<RaceTrack>())
            raceTrack = racePrefab.GetComponent<RaceTrack>();
        if (racePrefab.GetComponent<TimeController>())
            timeController = racePrefab.GetComponent<TimeController>();
    }

    /// <summary> Used in recording mode for changing gears, and changing acceleration that should be used </summary>
    void SwitchGears()
    {
        if ((timeController.ReplayTime - raceTrack.TotalIntroDuration) >= GearsTimings[currentGear])
            if (currentGear < TotalGears - 1)
            {
                currentGear++;
                var trackId = 0;
                if (!leftTrack)
                    trackId = 1;
                GearEvents.Add(new GearEvent((timeController.ReplayTime), currentGear + 1, trackId));
                if (animatorComponent)
                    if (animatorComponent.PlayableAnimations.GearChangingAnimations)
                        animatorComponent.StartAnimationOfType(AnimationType.GEARCHANGE, 0);
            }
    }


    /// <summary> Set's gear distances according to user input </summary>
    void SetDistances()
    {
        GearsDistances = new List<float>();
        for (int i = 0; i < TotalGears; i++)
            if (i == 0)
                GearsDistances.Add(402 * LengthPercents[i] / 100f);
            else
                GearsDistances.Add((402 * LengthPercents[i] / 100f) + GearsDistances[i - 1]);
    }

    /// <summary> Set's gear timings according to user input </summary>
    void SetTimings()
    {
        GearsTimings = new List<float>();
        for (int i = 0; i < TotalGears; i++)
            if (i == 0)
                GearsTimings.Add(FinishTime * TimePercents[i] / 100f);
            else
                GearsTimings.Add((FinishTime * TimePercents[i] / 100f) + GearsTimings[i - 1]);
    }

    /// <summary> Updates RPM to be displayed </summary>
    void RPMUpdate()
    {
        if (currentGear == 0)
            DisplayedRPM = (int)Mathf.Lerp(RPMAtStart, RPMSweetSpot, (timeController.ReplayTime - raceTrack.TotalIntroDuration) / GearsTimings[currentGear]);
        else
        {
            if ((timeController.ReplayTime - raceTrack.TotalIntroDuration - GearsTimings[currentGear - 1]) <= raceTrack.RpmDropTiming)
                DisplayedRPM = (int)Mathf.Lerp(RPMSweetSpot, rpmDecrease, ((timeController.ReplayTime - raceTrack.TotalIntroDuration - GearsTimings[currentGear - 1]) / raceTrack.RpmDropTiming));
            else
                DisplayedRPM = (int)Mathf.Lerp(rpmDecrease, RPMSweetSpot, ((timeController.ReplayTime - raceTrack.TotalIntroDuration - GearsTimings[currentGear - 1] - raceTrack.RpmDropTiming) / (GearsTimings[currentGear] - GearsTimings[currentGear - 1] - raceTrack.RpmDropTiming)));
        }
    }

    /// <summary> Used in play mode for updating car speed, and for rotating wheels </summary>
    void UpdateSpeed(float speed)
    {
        Speed = (int)raceTrack.GetConvertedSpeed(speed);
    }

    Vector2[] IGraphable.GetGraphPoints(WindowUpdateHandle refreshEvent)
    {
        if (raceTrack == null)
            raceTrack = GameObject.Find("_Race").GetComponent<RaceTrack>();
        if (changeEvent == null)
        {
            SetTopSpeeds();
            changeEvent = refreshEvent;
        }
        float[] totalX = new float[TotalGears];
        float timeIncrement = 0.015f;
        float time = 0f;
        float x = 0f;
        int currentGear = 0;
        float deltaTime = 0f;
        int index = 0;

        Vector2[] arrayToReturn = new Vector2[(int)(FinishTime / timeIncrement) + 1];
        for (int i = 0; i < TotalGears; i++)
            totalX[i] = (Mathf.Abs(SpeedFunctionSettings[i].XFrom) + SpeedFunctionSettings[i].XTo);
        if (GearsTimings == null || GearsTimings.Count == 0)
        {
            Debug.LogWarning("Gear timings not initialized");
            return null;
        }
        while (time <= FinishTime)
        {
            time += timeIncrement;
            if (time >= GearsTimings[currentGear])
                if (currentGear < TotalGears - 1)
                    currentGear++;

            var speed = 0f;
            if (currentGear == 0)
            {
                deltaTime = GearsTimings[currentGear];
                x = totalX[currentGear] * (time / deltaTime) - Mathf.Abs(SpeedFunctionSettings[currentGear].XFrom);
                speed = ((Mathf.Sin(x) + SpeedFunctionSettings[currentGear].YAxisShift) / (Mathf.Sin(SpeedFunctionSettings[currentGear].XTo) + SpeedFunctionSettings[currentGear].YAxisShift)) * MaxSpeedPerGear[currentGear];
            }
            else
            {
                deltaTime = GearsTimings[currentGear] - GearsTimings[currentGear - 1];
                x = totalX[currentGear] * ((time - GearsTimings[currentGear - 1]) / deltaTime) - Mathf.Abs(SpeedFunctionSettings[currentGear].XFrom);
                speed = ((Mathf.Sin(x) + SpeedFunctionSettings[currentGear].YAxisShift) / (Mathf.Sin(SpeedFunctionSettings[currentGear].XTo) + SpeedFunctionSettings[currentGear].YAxisShift)) * (MaxSpeedPerGear[currentGear] - MaxSpeedPerGear[currentGear - 1]);
                speed += MaxSpeedPerGear[currentGear - 1];
            }

            if (index < arrayToReturn.Length)
                arrayToReturn[index] = new Vector2(time, raceTrack.GetConvertedSpeed(speed));
            index++;
        }
        arrayToReturn[arrayToReturn.Length - 1] = new Vector2(GearsTimings[TotalGears - 1], raceTrack.GetConvertedSpeed(MaxSpeedPerGear[TotalGears - 1]));
        return arrayToReturn;
    }

    Vector2[] IGraphable.GetMainGraphPoints()
    {
        Vector2[] gearShiftingPoints = new Vector2[TotalGears];
        for (int i = 0; i < TotalGears; i++)
            gearShiftingPoints[i] = new Vector2(GearsTimings[i], raceTrack.GetConvertedSpeed(MaxSpeedPerGear[i]));
        return gearShiftingPoints;
    }

    void UpdateAnalyzer()
    {
        if (changeEvent != null)
            changeEvent.Invoke();
    }

    void ChangeState(RacerStates newState)
    {
        if (RacerState == newState)
            return;
        else
            RacerState = newState;
    }

    float SpeedIntegral(float elapsedTime, float totalTime, float distance)
    {
        return (-Mathf.Cos(elapsedTime / totalTime * Mathf.PI) + 1) / 2 * distance;
    }
}
