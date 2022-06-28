using UnityEngine;

/// <summary>
/// Class for tire burnout simulation via ParticleSystem. This scrpit controls a two part system.
/// You need two prefabs with two distinct particle systems for this to work as intended.
/// </summary>
public class BurnoutController : MonoBehaviour
{

    [Header("Prefab Containers÷÷÷÷÷", order = 0)]
    [Space(-15, order = 1)]
    [Header("~~~~~~~~~~~~~~~~~~", order = 2)]
    [Space(10, order = 3)]
    [Tooltip("Just drag the prefab with the particle system here. The particle system should render a big smoke cloud.")]
    /// <summary>
    /// Class for prefab instancing.
    /// </summary>
    public ParticleSystem AtmosphericSmoke;

    [Tooltip("Just drag the prefab with the particle system here. The particle system should render a smoke ring around the tire.")]
    /// <summary>
    /// Class for prefab instancing.
    /// </summary>
    public ParticleSystem TireSmoke;

    [Header("Smoke Emission Rates÷÷", order = 0)]
    [Space(-15, order = 1)]
    [Header("~~~~~~~~~~~~~~~~~~", order = 2)]
    [Space(10, order = 3)]
    [Range(0, 100)]
    [Tooltip("Emission rate for the left car atmospheric smoke burnout.")]
    /// <summary>
    /// Left car atmospheric smoke emission rate.
    /// </summary>
    public float LeftCarAtmospheric = 40;

    [Range(0, 1000)]
    [Tooltip("Emission rate for the left car tire smoke burnout.")]
    /// <summary>
    /// Left car tire smoke emissio rate.
    /// </summary>
    public float LeftCarTire = 300;

    [Range(0, 1)]
    [Tooltip("Time delay in seconds from when the wheels start spinig to when the smoke starts emiting for the left car.")]
    /// <summary>
    /// Left car tire burnot emision delay.
    /// </summary>
    public float LeftCarBurnoutDelay = 0;

    [Space(10)]
    [Range(0, 100)]
    [Tooltip("Emission rate for the right car atmospheric smoke burnout.")]
    /// <summary>
    /// Right car atmospheric smoke emission rate.
    /// </summary>
    public float RightCarAtmospheric = 30;

    [Range(0, 1000)]
    [Tooltip("Emission rate for the right car tire smoke burnout.")]
    /// <summary>
    /// Right car tire smoke emissio rate.
    /// </summary>
    public float RightCarTire = 500;

    [Range(0, 1)]
    [Tooltip("Time delay in seconds from when the wheels start spinig to when the smoke starts emiting for the right car.")]
    /// <summary>
    /// Right car tire burnot emision delay.
    /// </summary>
    public float RightCarBurnoutDelay = 0;

    [Header("Smoke outro veloceties÷", order = 0)]
    [Space(-15, order = 1)]
    [Header("~~~~~~~~~~~~~~~~~~", order = 2)]
    [Space(10, order = 3)]
    [Range(0.01f, 5)]
    [Tooltip("The velocity of the smoke that folows the left car after the burnout is finished.")]
    /// <summary>
    /// Left car burnout outro velocity.
    /// </summary>
    public float LeftCarOutroVelocity = 1;

    [Range(0.01f, 5)]
    [Tooltip("The velocity of the smoke that folows the right car after the burnout is finished.")]
    /// <summary>
    /// Right car burnout outro velocity.
    /// </summary>
    public float RightCarOutroVelocity = 1;
    RaceController raceController;
    TimeController timeController;
    Replay replay;
    Car leftTrackCar;
    Car rightTrackCar;
    ParticleSystem[] tireSystems;
    bool isInit = false;
    bool leftIsInBurnout = false;
    bool rightIsInBurnout = false;
    ParticleSystem.MinMaxCurve initialZVelocityCurve;
    AnimationCurve velocityCurve;

    public void OnUpdate()
    {
        if (!leftTrackCar || !rightTrackCar)
            isInit = false;

        if (!isInit)
        {
            raceController = gameObject.GetComponent<RaceController>();
            timeController = gameObject.GetComponent<TimeController>();
            leftTrackCar = raceController.LeftTrackRacer;
            rightTrackCar = raceController.RightTrackRacer;
            if (leftTrackCar != null && rightTrackCar != null)
            {
                if (leftTrackCar.BackWheels != null && rightTrackCar.BackWheels != null)
                {
                    InitTrackCarBurnoutSystem(leftTrackCar);
                    InitTrackCarBurnoutSystem(rightTrackCar);
                    SetAtmosphericSmokeEmissionToZero(leftTrackCar);
                    SetAtmosphericSmokeEmissionToZero(rightTrackCar);
                    replay = raceController.RaceRecorder.RaceReplay;
                    initialZVelocityCurve = AtmosphericSmoke.velocityOverLifetime.z;
                    isInit = true;
                }
            }
        }

        if (isInit && raceController.RaceRecorder.RecorderState == RecorderStates.PLAYING)
        {
            if (timeController.RaceTime >= (replay.Burnouts[0].StartTime + LeftCarBurnoutDelay) && timeController.RaceTime < (replay.Burnouts[0].EndTime) && !leftIsInBurnout)
            {
                ModifyCarParticleEmission(leftTrackCar, LeftCarAtmospheric, LeftCarTire);
                leftIsInBurnout = true;
            }
            if (timeController.RaceTime >= (replay.Burnouts[0].EndTime) && timeController.RaceTime < (replay.Burnouts[0].EndTime))
            {
                SetTireSmokeEmisionToZero(leftTrackCar);
                if (leftTrackCar.Speed > 2.5f)
                {
                    ModifyCarParticleVelocity(leftTrackCar, LeftCarOutroVelocity);
                }
            }
            if (timeController.RaceTime >= (replay.Burnouts[0].EndTime))
            {
                SetAtmosphericSmokeEmissionToZero(leftTrackCar);
                SetParticleVelocityToDefault(leftTrackCar);
                leftIsInBurnout = false;
            }

            if (timeController.RaceTime >= (replay.Burnouts[1].StartTime + RightCarBurnoutDelay) && timeController.RaceTime < (replay.Burnouts[1].EndTime) && !rightIsInBurnout)
            {
                ModifyCarParticleEmission(rightTrackCar, RightCarAtmospheric, RightCarTire);
                rightIsInBurnout = true;
            }
            if (timeController.RaceTime >= (replay.Burnouts[1].EndTime) && timeController.RaceTime < (replay.Burnouts[1].EndTime))
            {
                SetTireSmokeEmisionToZero(rightTrackCar);
                if (rightTrackCar.Speed > 3.5f)
                {
                    ModifyCarParticleVelocity(rightTrackCar, RightCarOutroVelocity);
                }
            }
            if (timeController.RaceTime >= (replay.Burnouts[1].EndTime))
            {
                SetAtmosphericSmokeEmissionToZero(rightTrackCar);
                SetParticleVelocityToDefault(rightTrackCar);
                rightIsInBurnout = false;
            }
        }
    }

    /// <summary>
    /// Add prefab instances with the particle systems to the wheels.
    /// </summary>
    private void InitTrackCarBurnoutSystem(Car car)
    {
        if (car.BackWheels.childCount == 2)
        {
            for (int i = 0; i < car.BackWheels.childCount; i++)
            {
                Instantiate(AtmosphericSmoke, car.BackWheels.GetChild(i).transform, false);
                Instantiate(TireSmoke, car.BackWheels.GetChild(i).transform, false);
            }
        }
    }

    /// <summary>
    /// Modify particle system emission for given car dependeing on car state.
    /// </summary>
    private void ModifyCarParticleEmission(Car car, float atmEmission, float tireEmission)
    {
        for (int i = 0; i < car.BackWheels.childCount; i++)
        {
            tireSystems = car.BackWheels.GetChild(i).GetComponentsInChildren<ParticleSystem>();
            var atmEm = tireSystems[0].emission;
            atmEm.rate = atmEmission;
            var tireEm = tireSystems[1].emission;
            tireEm.rate = tireEmission;
        }
    }

    /// <summary>
    /// Set atmospheric smoke particle system emission for given car to zero.
    /// </summary>
    private void SetAtmosphericSmokeEmissionToZero(Car car)
    {
        for (int i = 0; i < car.BackWheels.childCount; i++)
        {
            tireSystems = car.BackWheels.GetChild(i).GetComponentsInChildren<ParticleSystem>();
            var atmEm = tireSystems[0].emission;
            atmEm.rate = 0;
        }
    }

    /// <summary>
    /// Set tire smoke particle system emission for given car to zero.
    /// </summary>
    private void SetTireSmokeEmisionToZero(Car car)
    {
        for (int i = 0; i < car.BackWheels.childCount; i++)
        {
            tireSystems = car.BackWheels.GetChild(i).GetComponentsInChildren<ParticleSystem>();
            var tireEm = tireSystems[1].emission;
            tireEm.rate = 0;
        }
    }

    /// <summary>
    /// Set particle system velocity for given car.
    /// </summary>
    private void ModifyCarParticleVelocity(Car car, float velocity)
    {
        for (int i = 0; i < car.BackWheels.childCount; i++)
        {
            tireSystems = car.BackWheels.GetChild(i).GetComponentsInChildren<ParticleSystem>();
            var atmEm = tireSystems[0].velocityOverLifetime;
            velocityCurve = new AnimationCurve();
            velocityCurve.AddKey(0.0f, 0.1f);
            velocityCurve.AddKey(0.25f, 0.15f);
            velocityCurve.AddKey(0.5f, 0.5f);
            velocityCurve.AddKey(0.75f, 0.85f);
            velocityCurve.AddKey(1.0f, 1.0f);
            atmEm.z = new ParticleSystem.MinMaxCurve(velocity, velocityCurve);
        }
    }

    /// <summary>
    /// Set particle system velocity for given car to default.
    /// </summary>
    private void SetParticleVelocityToDefault(Car car)
    {
        for (int i = 0; i < car.BackWheels.childCount; i++)
        {
            tireSystems = car.BackWheels.GetChild(i).GetComponentsInChildren<ParticleSystem>();
            var atmEm = tireSystems[0].velocityOverLifetime;
            atmEm.z = initialZVelocityCurve;
        }
    }
}
