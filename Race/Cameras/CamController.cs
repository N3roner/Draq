using UnityEngine;
using System.Collections.Generic;

public enum CamStates
{
    INTRO,
    ATSTART,
    RACE
}
/*[System.Serializable]
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
   public float HorAmplitude;
   public float HorFrequency;
   public float VerAmplitude;
   public float VerFrequency;
   public float ReverseThreshold;
   public float LengthPercentToTarget;
}*/

public class CamController : MonoBehaviour
{
    public CamStates CamControllerState;
    Car leftRacer;
    Car rightRacer;
    TimeController timeController;
    public Transform CamFocus;
    public GameObject FixedCams;
    public GameObject FinishCam;
    public CamDefiner ActiveCam;
    public float MarginRadius;
    GameObject activeCamGround;
    public List<CamDefiner> DefinedCams { get; private set; }
    public List<CamShakeInfo> ShakeQueue { get; private set; }
    public float LeftCarPivotOffset;
    public float RightCarPivotOffset;
    Vector3 leftCarCenter;
    Vector3 rightCarCenter;
    List<Vector3> screenDots;
    public Vector3 HoodCamPosition;
    public CameraShake Shake;
    public Camera InGameCamera;
    public RaceTrack Track;
    public bool SlowmoActive { get; private set; }
    Vector3 activeCamStart;
    Vector3 activeCamEnd;
    int activeCamIndex;
    float activeCamStartTime;
    Vector3 followCamDelta;
    Vector3 fixedCamPos;
    float zoomSpeed;

    /*public void Reset() {
       ActiveCam = null;
       ShakeQueue = new List<CamShakeInfo>();
    }*/

    public void SetReplay() {
      for(int i = 0; i < DefinedCams.Count; ++i)
         DefinedCams[i].gameObject.SetActive(false);
      InGameCamera.gameObject.SetActive(true);
   }

    public bool CamsUpdate()
    {
        leftCarCenter = leftRacer.transform.position;
        leftCarCenter.z -= LeftCarPivotOffset;
        rightCarCenter = rightRacer.transform.position;
        rightCarCenter.z -= RightCarPivotOffset;
        UpdateCamPosition();
        UpdateFocusAndFoV(leftCarCenter, rightCarCenter);
        return SetCurrentCam();
    }

    public List<CamDefiner> GetDefinedCams()
    {
        return DefinedCams;
    }

    public void InitCams()
    {
        if (DefinedCams == null || !Application.isPlaying)
        {
            DefinedCams = new List<CamDefiner>();
            foreach (var defined in FixedCams.GetComponentsInChildren<CamDefiner>())
            {
                DefinedCams.Add(defined);
                if (defined.Camera == null)
                    defined.Camera = defined.GetComponent<Camera>();
            }
        }

        if (!Application.isPlaying)
            return;

        if (DefinedCams.Count > 0)
        {
            DefinedCams.Sort((a, b) => a.GetEndDistance().CompareTo(b.GetEndDistance()));
            for (int i = 0; i < DefinedCams.Count; ++i)
            {
                if (i > 0)
                {
                    DefinedCams[i].PrevCam = DefinedCams[i - 1];
                    DefinedCams[i - 1].NextCam = DefinedCams[i];
                }
                DefinedCams[i].gameObject.SetActive(false);
            }
            DefinedCams[0].gameObject.SetActive(true);
        }
    }

    void InitOtherComponents()
    {
        if (timeController == null || activeCamGround == null)
        {
            timeController = GetComponentInParent<TimeController>();
            timeController.TimeChange += OnTimeChange;
            timeController.RacerFinish += OnRacerFinish;
            activeCamGround = new GameObject();
            activeCamGround.transform.SetParent(transform);
            activeCamGround.name = "Active cam ground projection";
        }
        timeController.Reset();
        timeController.TimeControllerState = TimeControllerStates.RECORDING;
    }

    public void Init(Car leftTrackRacer, Car rightTrackRacer)
    {
        InitOtherComponents();
        InitCams();
        ActiveCam = null;
        ShakeQueue = new List<CamShakeInfo>();
        leftRacer = leftTrackRacer;
        rightRacer = rightTrackRacer;
        CamControllerState = CamStates.INTRO;
        ReplaceCams(0);
    }

    private void OnRacerFinish(int racerId)
    {
        if (ActiveCam.FocusOptions == FocusDefinition.FOCUSFIRST && ActiveCam.TriggerOptions == TriggerOptions.FIRSTRACER)
        {
            ActiveCam.FieldOfViewOptions = FieldOfViewOptions.LOCKED;
            ActiveCam.RotationOptions = RotationOptions.LOCKED;
        }
    }

    private void OnTimeChange(string newState)
    {

        if (newState == RaceEvent.IntroEnd)
            CamControllerState = CamStates.ATSTART;
      //if (newState == RaceEvent.RaceStart)
      //    CamControllerState = CamStates.RACE;

      if(newState == RaceEvent.SlowStart) {
         SlowmoActive = true;
      } else if(newState != RaceEvent.SlowEnd)
         SlowmoActive = false;
    }

    void FindRelevantDots(Vector3 leftCar, Vector3 rightCar)
    {
        screenDots = new List<Vector3>();
        Vector3 activeCamZero = ActiveCam.transform.position;
        activeCamZero.y = 0;
        activeCamGround.transform.position = activeCamZero;
        Vector3 lrld = new Vector3();
        Vector3 lrrd = new Vector3();
        Vector3 rrld = new Vector3();
        Vector3 rrrd = new Vector3();
        if (ActiveCam.CameraType == CamType.FIRSTPERSON)
        {
            if (leftCar.z > rightCar.z)
            {
                lrld = -activeCamGround.transform.right * MarginRadius + leftCar;
                lrrd = activeCamGround.transform.right * MarginRadius + leftCar;
                screenDots.Add(lrld);
                screenDots.Add(lrrd);
            }
            else
            {
                rrld = -activeCamGround.transform.right * MarginRadius + rightCar;
                rrrd = activeCamGround.transform.right * MarginRadius + rightCar;
                screenDots.Add(rrld);
                screenDots.Add(rrrd);
            }
        }
        else
        {
            lrld = -activeCamGround.transform.right * MarginRadius + leftCar;
            lrrd = activeCamGround.transform.right * MarginRadius + leftCar;
            screenDots.Add(lrld);
            screenDots.Add(lrrd);

            rrld = -activeCamGround.transform.right * MarginRadius + rightCar;
            rrrd = activeCamGround.transform.right * MarginRadius + rightCar;
            screenDots.Add(rrld);
            screenDots.Add(rrrd);
        }
        screenDots.Sort((d1, d2) => (activeCamGround.transform.InverseTransformPoint(d1).x / activeCamGround.transform.InverseTransformPoint(d1).z).
        CompareTo(activeCamGround.transform.InverseTransformPoint(d2).x / activeCamGround.transform.InverseTransformPoint(d2).z));
    }

    void UpdateFocusAndFoV(Vector3 leftCar, Vector3 rightCar, bool smoothZoom = true)
    {
        FindRelevantDots(leftCar, rightCar);
        UpdateFocus(leftCar, rightCar);
        UpdateFoV(smoothZoom);

        if (ActiveCam.RotationOptions != RotationOptions.LOCKED)
            ActiveCam.Camera.transform.LookAt(GetCamFocus());

        var progress = GetProgress();
        ActiveCam.transform.RotateAround(ActiveCam.transform.position, ActiveCam.transform.forward, Mathf.Lerp(0f, ActiveCam.Roll, progress) * timeController.DeltaReplayTime);

        /*if(ActiveCam.CameraType == CamType.FIRSTPERSON) {
           ActiveCam.transform.rotation *= GetShake();
        }*/
    }

    void UpdateFocus(Vector3 leftCar, Vector3 rightCar)
    {
        activeCamGround.transform.LookAt(screenDots[0]);

        var AF = screenDots[0] - activeCamGround.transform.position;
        var BF = screenDots[screenDots.Count - 1] - activeCamGround.transform.position;
        var AFBangle = Vector3.Angle(AF, BF);
        var angle = Quaternion.Euler(0, AFBangle / 2, 0);

        activeCamGround.transform.rotation *= angle;

        Vector3 leftRacer;
        Vector3 rightRacer;
        float midPoint;

        if (ActiveCam.CameraType == CamType.FIRSTPERSON)
        {
            leftRacer = activeCamGround.transform.InverseTransformPoint(screenDots[0]);
            rightRacer = activeCamGround.transform.InverseTransformPoint(screenDots[screenDots.Count - 1]);
            midPoint = (leftRacer.z + rightRacer.z) / 2;
        }
        else
        {
            leftRacer = activeCamGround.transform.InverseTransformPoint(leftCar);
            rightRacer = activeCamGround.transform.InverseTransformPoint(rightCar);
            midPoint = (leftRacer.z + rightRacer.z) / 2;
        }

        var cFPos = activeCamGround.transform.position + activeCamGround.transform.forward * midPoint;

        if (ActiveCam.CameraType != CamType.FIRSTPERSON)
        {
            cFPos.x = 0;
            cFPos.y = 0;
        }
        else
            cFPos.y = 1f;

        //var diff = ActiveCam.transform.position.z - cFPos.z;

        if (ActiveCam.FocusOptions != FocusDefinition.FOCUSLOCKED)
            CamFocus.transform.position = cFPos;

        /*if(ActiveCam.CameraType == CamType.FOLLOW) {
           var camPosition = ActiveCam.transform.position;
           camPosition.z = CamFocus.transform.position.z + followCamDelta.z;
           ActiveCam.transform.position = camPosition;
        }*/

        if (ActiveCam.StartFollowingTrigger != null)
        {
            if (CamFocus.transform.position.z >= ActiveCam.StartFollowingTrigger.transform.position.z && ActiveCam.CameraType != CamType.FOLLOW)
            {
                ActiveCam.CameraType = CamType.FOLLOW;
                followCamDelta = (ActiveCam.transform.position - cFPos);
            }
        }
    }

    void UpdateCamPosition()
    {
        if (ActiveCam == null || ActiveCam.CamProgression == LerpOptions.NONE || ActiveCam.CameraType == CamType.FIRSTPERSON)
            return;
        var progress = GetProgress();
        var pos = fixedCamPos;
        if (ActiveCam.CameraType == CamType.FIXED)
        {
            if ((ActiveCam.StartPosition - ActiveCam.EndPosition).magnitude > Mathf.Epsilon)
                pos = Vector3.Lerp(ActiveCam.StartPosition, ActiveCam.StartPosition + ActiveCam.EndPosition, ActiveCam.PosProgression.Evaluate(progress));
        }
        if (ActiveCam.CameraType == CamType.FOLLOW)
            pos = CamFocus.position + followCamDelta;
        var rotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, ActiveCam.Rotation, ActiveCam.RotProgression.Evaluate(progress)));
        pos -= CamFocus.position;
        pos = rotation * pos;
        pos += CamFocus.position;
        ActiveCam.transform.position = pos;
    }

    void UpdateFoV(bool smoothed = true)
    {
        float horizontalFOV = Vector3.Angle(activeCamGround.transform.position - screenDots[0], activeCamGround.transform.position - screenDots[screenDots.Count - 1]);
        var nextFOV = GetVerticalFov(horizontalFOV, ActiveCam.Camera.aspect);

        if (ActiveCam.CapZoomSpeed && smoothed)
        {
            var deltaFov = nextFOV - ActiveCam.Camera.fieldOfView;
            var stoppingDistance = zoomSpeed * Mathf.Abs(zoomSpeed) / (2f * ActiveCam.ZoomAcceleration);
            var zoomAcceleration = deltaFov > 0f ? ActiveCam.ZoomAcceleration : -ActiveCam.ZoomAcceleration;
            if (deltaFov > 0f && deltaFov < stoppingDistance)
                zoomAcceleration = -ActiveCam.ZoomAcceleration;
            if (deltaFov < 0f && deltaFov > stoppingDistance)
                zoomAcceleration = ActiveCam.ZoomAcceleration;
            if (deltaFov < 0f && deltaFov > stoppingDistance)
                zoomSpeed += zoomAcceleration * timeController.DeltaReplayTime;
            if (Mathf.Abs(zoomSpeed * timeController.DeltaReplayTime) > Mathf.Abs(deltaFov))
            {
                if (zoomSpeed < 0f && deltaFov < 0f || zoomSpeed > 0f && deltaFov > 0f)
                    zoomSpeed = deltaFov;
            }
            nextFOV = ActiveCam.Camera.fieldOfView + zoomSpeed * timeController.DeltaReplayTime;
        }

        if (!(ActiveCam.FieldOfViewOptions == FieldOfViewOptions.LOCKED /*|| ActiveCam.CameraType == CamType.FIRSTPERSON*/))
            ActiveCam.Camera.fieldOfView = nextFOV;
        if (ActiveCam.MinFov > Mathf.Epsilon && ActiveCam.MaxFov > ActiveCam.MinFov)
            ActiveCam.Camera.fieldOfView = Mathf.Clamp(ActiveCam.Camera.fieldOfView, ActiveCam.MinFov, ActiveCam.MaxFov);
    }

    public Vector3 GetCamFocus()
    {
        var pos = CamFocus.transform.position;
        if (ActiveCam.FocusOptions == FocusDefinition.FOCUSFIRST)
        {
            pos = GetFirstOrSecond();
            pos.x = 0;
        }
        if (ActiveCam.FocusOptions == FocusDefinition.FOCUSSECOND)
        {
            pos = GetFirstOrSecond(false);
            pos.x = 0;
        }
        if(pos.z > Track.RaceTrackLength)
           pos = new Vector3(pos.x, pos.y, Track.RaceTrackLength);
        if (ActiveCam.CameraLag < Mathf.Epsilon)
            return pos;
        var camProgress = (CamFocus.transform.position.z - activeCamStart.z) / (ActiveCam.NextCamTrigger.position.z - activeCamStart.z);
        var result = Vector3.Lerp(activeCamStart, activeCamEnd, camProgress);
        return result.z < ActiveCam.NextCamTrigger.position.z ? result : ActiveCam.NextCamTrigger.position;
    }

    public void PlayModeSettings()
    {
        for (int i = 0; i < DefinedCams.Count; i++)
            DefinedCams[i].Camera.depth = 0;
        InGameCamera.depth = 1;
    }

    Vector3 GetFirstOrSecond(bool first = true)
    {
        if (first)
            return leftRacer.transform.position.z > rightRacer.transform.position.z ? leftCarCenter : rightCarCenter;
        else
            return leftRacer.transform.position.z < rightRacer.transform.position.z ? leftCarCenter : rightCarCenter;
    }

    Transform GetFirstOrSecond(int racer)
    {
        if (racer == 1)
            return leftRacer.transform.position.z > rightRacer.transform.position.z ? leftRacer.transform : rightRacer.transform;
        else
            return leftRacer.transform.position.z < rightRacer.transform.position.z ? leftRacer.transform : rightRacer.transform;
    }

    public Vector3 GetTriggerPosition(CamDefiner definedCam)
    {
        if (definedCam.TriggerOptions == TriggerOptions.FIRSTRACER)
            return GetFirstOrSecond();
        if (definedCam.TriggerOptions == TriggerOptions.SECONDRACER)
            return GetFirstOrSecond(false);
        return (leftRacer.transform.position + rightRacer.transform.position) / 2;
    }

    bool SetCurrentCam()
    {
        if (activeCamIndex == DefinedCams.Count - 1)
            return false;

        int k = 0;

        if (ActiveCam.CamProgression != LerpOptions.TIME)
        {
            while (GetTriggerPosition(DefinedCams[k]).z > DefinedCams[k].NextCamTrigger.position.z && k < DefinedCams.Count - 1 || DefinedCams[k].CamProgression == LerpOptions.TIME)
            {
                k++;
                if (DefinedCams[k].NextCamTrigger == null)
                    break;
            }
        }
        else
            k = timeController.ReplayTime > activeCamStartTime + ActiveCam.Duration ? activeCamIndex + 1 : activeCamIndex;
        return ReplaceCams(k);
    }

    float Sine(float elapsedTime, float totalTime)
    {
        return Mathf.Sin(elapsedTime / totalTime * Mathf.PI / 2);
    }

    float Cosine(float elapsedTime, float totalTime)
    {
        return Mathf.Cos(elapsedTime / totalTime * Mathf.PI);
    }

    public float SmoothStep(float elapsedTime, float totalLerpingTime)
    {
        var t = elapsedTime / totalLerpingTime;
        return t * t * (3f - 2f * t);
    }

    public Vector3 V3Lerp(float elapsedTime, float totalLerpingTime, Vector3 StartingVector, Vector3 EndingVector, LerpingOptions lerpingOption)
    {
        if (lerpingOption == LerpingOptions.SMOOTHSTEP)
            return Vector3.Lerp(StartingVector, EndingVector, SmoothStep(elapsedTime, totalLerpingTime));

        if (lerpingOption == LerpingOptions.COSINE)
            return Vector3.Lerp(StartingVector, EndingVector, Cosine(elapsedTime, totalLerpingTime));
        if (lerpingOption == LerpingOptions.SINE)
            return Vector3.Lerp(StartingVector, EndingVector, Sine(elapsedTime, totalLerpingTime));
        return Vector3.Lerp(StartingVector, EndingVector, elapsedTime / totalLerpingTime);
    }

    public float ValueLerp(float elapsedTime, float totalLerpingTime, float StartingValue, float EndingValue, LerpingOptions lerpingOption)
    {
        if (lerpingOption == LerpingOptions.SMOOTHSTEP)
            return Mathf.Lerp(StartingValue, EndingValue, SmoothStep(elapsedTime, totalLerpingTime));
        if (lerpingOption == LerpingOptions.SINE)
            return Mathf.Lerp(StartingValue, EndingValue, Sine(elapsedTime, totalLerpingTime));
        if (lerpingOption == LerpingOptions.COSINE)
            return Mathf.Lerp(StartingValue, EndingValue, Cosine(elapsedTime, totalLerpingTime));
        return Mathf.Lerp(StartingValue, EndingValue, elapsedTime / totalLerpingTime);
    }

    bool ReplaceCams(int camNumber)
    {
        if (camNumber >= DefinedCams.Count)
            Debug.Log("stop");
        if (ActiveCam == DefinedCams[camNumber])
            return false;

        //FinishCam.SetActive(false);
        if (ActiveCam != null)
        {
            SaveShake();
            activeCamStart = camNumber == 0 ? new Vector3(0f, 0f, Track.SpawnZMax) : ActiveCam.NextCamTrigger.position;
            activeCamEnd = DefinedCams[camNumber].NextCamTrigger != null ? DefinedCams[camNumber].NextCamTrigger.position : new Vector3(0f, 0f, Track.RaceTrackLength);
            if ((activeCamStart - activeCamEnd).magnitude < Mathf.Epsilon)
            {
                activeCamStart = ActiveCam.transform.position;
                activeCamEnd = activeCamStart;
            }
            var halfway = (activeCamStart + activeCamEnd) / 2f;
            activeCamStart = Vector3.Lerp(activeCamStart, halfway, DefinedCams[camNumber].CameraLag);
            activeCamEnd = Vector3.Lerp(activeCamEnd, halfway, DefinedCams[camNumber].CameraLag);

            ActiveCam.gameObject.SetActive(false);
            if (ActiveCam.CameraType == CamType.FIRSTPERSON)
                ActiveCam.LoadDefaultSettings();
        }
        zoomSpeed = 0f;

        ActiveCam = DefinedCams[camNumber];
        activeCamIndex = camNumber;
        activeCamStartTime = timeController.ReplayTime;
        fixedCamPos = ActiveCam.transform.position;
        //var left = activeCamGround.transform.InverseTransformPoint(leftRacer.transform.position);
        //var right = activeCamGround.transform.InverseTransformPoint(leftRacer.transform.position);
        //var midPoint = (left.z + right.z) / 2;
        ActiveCam.Camera.depth = 1;
        ActiveCam.gameObject.SetActive(true);

        if(leftRacer && rightRacer)
        UpdateFocusAndFoV(leftRacer.transform.position, rightRacer.transform.position, false);

        followCamDelta = (ActiveCam.transform.position - CamFocus.position/*(activeCamGround.transform.position + activeCamGround.transform.forward * midPoint)*/);

        if (ActiveCam.CameraType == CamType.FIRSTPERSON)
        {
            ActiveCam.Camera.nearClipPlane = 0.01f;
            ActiveCam.transform.SetParent(GetFirstOrSecond(2));
            ActiveCam.transform.localPosition = HoodCamPosition;
            ActiveCam.transform.localRotation = new Quaternion(0, 0, 0, 0);
        }
        return true;
    }

    void SaveShake()
    {
        var addition = new CamShakeInfo();
        addition.StartTime = ShakeQueue.Count > 0 ? ShakeQueue[ShakeQueue.Count - 1].EndTime : 0f;
        addition.EndTime = timeController.ReplayTime + timeController.RaceIntroDelay;
        addition.HorizontalShake = ActiveCam.HorizontalShake;
        addition.VerticalShake = ActiveCam.VerticalShake;
        addition.ShakeSpeed = ActiveCam.ShakeSpeed;
        ShakeQueue.Add(addition);
    }

    float GetProgress()
    {
        var progress = 0f;
        if (ActiveCam.CamProgression == LerpOptions.DISTANCE)
            progress = (CamFocus.position.z - activeCamStart.z) / (activeCamEnd.z - activeCamStart.z);
        if (ActiveCam.CamProgression == LerpOptions.TIME)
            progress = (timeController.ReplayTime - activeCamStartTime) / ActiveCam.Duration;
        return progress;
    }

    public float GetVerticalFov(float horizontalFov, float aspect)
    {
        horizontalFov *= Mathf.Deg2Rad;
        return Mathf.Abs(2f * Mathf.Atan(Mathf.Tan(horizontalFov / 2f) / aspect) * Mathf.Rad2Deg);
    }

    public Quaternion GetShake(float horAmplitude, float verAmplitude, float speed, bool reset = false)
    {
        if (Shake == null)
            return Quaternion.identity;
        if (reset)
            Shake.Reset();
        return Shake.GetShake(timeController.DeltaRaceTime * speed, horAmplitude, verAmplitude);
    }

    public float GetVerticalShake()
    {
        if (ActiveCam.HorizontalShake.length < 2)
            return 0f;
        var progress = GetProgress();
        return ActiveCam.VerticalShake.Evaluate(progress);
    }

    public float GetHorizontalShake()
    {
        if (ActiveCam.VerticalShake.length < 2)
            return 0f;
        var progress = GetProgress();
        return ActiveCam.HorizontalShake.Evaluate(progress);
    }

    public float GetShakeSpeed()
    {
        if (ActiveCam.ShakeSpeed.length < 2)
            return 0f;
        var progress = GetProgress();
        return ActiveCam.ShakeSpeed.Evaluate(progress);
    }
}
