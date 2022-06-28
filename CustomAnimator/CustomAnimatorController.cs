using UnityEngine;
using System.Collections.Generic;

public class CustomAnimatorController : MonoBehaviour
{
    public List<FullAnimation> AnimationGroups;
    public GameObject TestingObject;
    public PlayableAnimations PlayableAnimations;
    public AnimationSetup CopiedAnimationSetup;
    public List<AnimationSetup> CopiedAnimationSetups;
    public List<CustomAnimator> ControlledAnimators;
    public bool LoopAnimation;
    public bool ExpandColapseAll;
    TimeController timeController;

    public void Init()
    {
        if (AnimationGroups == null)
            AnimationGroups = new List<FullAnimation>();
        if (AnimationGroups.Count == 0)
            AnimationGroups.Add(GetNewAnimation());
        if (PlayableAnimations == null)
            PlayableAnimations = new PlayableAnimations();
        CopiedAnimationSetup = null;
        CopiedAnimationSetups = null;
    }

    public void OnUpdate()
    {
        for (int i = 0; i < ControlledAnimators.Count; i++)
        {
            ControlledAnimators[i].UpdateAnimations(timeController.ReplayTime);
#if UNITY_EDITOR
            for (int j = 0; j < AnimationGroups.Count; j++)
                if (LoopAnimation && AnimationGroups[j].PlayAnimation && !ControlledAnimators[i].AnimationsToPlay[j].PlayAnimation)
                    ControlledAnimators[i].StartAnimation(j, true);
#endif
        }
    }

    public List<List<FullAnimation>> GetRacersAnimations()
    {
        var numberOfStartAn = GetNumberOfAnimations(AnimationGroups, AnimationType.START);
        var numberOfBurnoutAnimations = GetNumberOfAnimations(AnimationGroups, AnimationType.BURNOUT);
        var numberOfGearChangingAnimations = GetNumberOfAnimations(AnimationGroups, AnimationType.GEARCHANGE);

        var leftStartAnimationIndex = Random.Range(0, numberOfStartAn);
        var leftBurnOutAnimationIndex = Random.Range(0, numberOfBurnoutAnimations);
        var leftGearchangingAnimationIndex = Random.Range(0, numberOfGearChangingAnimations);

        var righStartAnimationIndex = GetAnimationIndexForRightRacer(leftStartAnimationIndex, numberOfStartAn);
        var rightBurnOutAnimationIndex = GetAnimationIndexForRightRacer(leftBurnOutAnimationIndex, numberOfBurnoutAnimations);
        var rightGearchangingAnimationIndex = GetAnimationIndexForRightRacer(leftGearchangingAnimationIndex, numberOfGearChangingAnimations);

        var startAnimationCounter = -1;
        var burnoutAnimationCounter = -1;
        var gearchangingAnimationCounter = -1;

        List<List<FullAnimation>> RacersAnimations = new List<List<FullAnimation>>();
        List<FullAnimation> LeftRacerAnimations = new List<FullAnimation>();
        List<FullAnimation> RightRacerAnimations = new List<FullAnimation>();

        for (int i = 0; i < AnimationGroups.Count; i++)
        {
            if (AnimationGroups[i].AnimationType == AnimationType.START)
            {
                startAnimationCounter++;
                if (startAnimationCounter == leftStartAnimationIndex)
                    LeftRacerAnimations.Add(AnimationGroups[i]);
                if (startAnimationCounter == righStartAnimationIndex)
                    RightRacerAnimations.Add(AnimationGroups[i]);
            }

            if (AnimationGroups[i].AnimationType == AnimationType.BURNOUT)
            {
                burnoutAnimationCounter++;
                if (burnoutAnimationCounter == leftBurnOutAnimationIndex)
                    LeftRacerAnimations.Add(AnimationGroups[i]);
                if (burnoutAnimationCounter == rightBurnOutAnimationIndex)
                    RightRacerAnimations.Add(AnimationGroups[i]);
            }
            if (AnimationGroups[i].AnimationType == AnimationType.GEARCHANGE)
            {
                gearchangingAnimationCounter++;
                if (gearchangingAnimationCounter == leftGearchangingAnimationIndex)
                    LeftRacerAnimations.Add(AnimationGroups[i]);
                if (gearchangingAnimationCounter == rightGearchangingAnimationIndex)
                    RightRacerAnimations.Add(AnimationGroups[i]);
            }
        }
        RacersAnimations.Add(LeftRacerAnimations);
        RacersAnimations.Add(RightRacerAnimations);
        return RacersAnimations;
    }

    int GetAnimationIndexForRightRacer(int leftRacerAnimationIndex, int maxNumberOfAnimations)
    {
        var rightRacerAnimationIndex = 0;
        if (maxNumberOfAnimations == 1)
            rightRacerAnimationIndex = 0;

        if (maxNumberOfAnimations > 1)
        {
            if (leftRacerAnimationIndex == 0)
                rightRacerAnimationIndex = 1;

            if (leftRacerAnimationIndex + 1 > maxNumberOfAnimations)
                rightRacerAnimationIndex = maxNumberOfAnimations;

            if (leftRacerAnimationIndex - 1 > 0 && leftRacerAnimationIndex != 0)
                rightRacerAnimationIndex = leftRacerAnimationIndex - 1;
        }
        return rightRacerAnimationIndex;
    }

    public CustomAnimator AnimatorConstructor(GameObject animatorHost, List<FullAnimation> animations, TimeController passedTimeController)
    {
        timeController = passedTimeController;
        if (animatorHost.GetComponent<CustomAnimator>())
            DestroyImmediate(animatorHost.GetComponent<CustomAnimator>());
        animatorHost.AddComponent<CustomAnimator>().CustomAnimatorConstructor(animations, PlayableAnimations);
        return animatorHost.GetComponent<CustomAnimator>();
    }

    public void TestPlay()
    {
        if (!TestingObject)
        {
            Debug.LogWarning("No testing object");
            return;
        }
        ControlledAnimators = new List<CustomAnimator>();
        ControlledAnimators.Add(AnimatorConstructor(TestingObject, AnimationGroups, gameObject.GetComponent<TimeController>()));
        for (int i = 0; i < AnimationGroups.Count; i++)
            if (AnimationGroups[i].PlayAnimation)
                for (int j = 0; j < AnimationGroups[i].AnimationSetups.Count; j++)
                    if (AnimationGroups[i].AnimationSetups[j].AnimationPlaying)
                        TestingObject.GetComponent<CustomAnimator>().StartAnimation(i, true);

        timeController.ReplayTime = 0f;
        timeController.TimeControllerState = TimeControllerStates.PLAYING;
    }

    public float GetMaxDuration()
    {
        var maxLength = GetSize(AnimationGroups[0].AnimationSetups[0].AnimationDuration.ToString()).x;
        for (int i = 0; i < AnimationGroups.Count; i++)
            for (int j = 0; j < AnimationGroups[i].AnimationSetups.Count; j++)
                if (GetSize(AnimationGroups[i].AnimationSetups[j].AnimationDuration.ToString()).x > maxLength)
                    maxLength = GetSize(AnimationGroups[i].AnimationSetups[j].AnimationDuration.ToString()).x;
        return maxLength + 4;
    }

    public float GetLongestAnimationName()
    {
        var maxLength = GetSize(AnimationGroups[0].AnimationName).x;
        for (int i = 0; i < AnimationGroups.Count; i++)
            if (GetSize(AnimationGroups[i].AnimationName).x > maxLength)
                maxLength = GetSize(AnimationGroups[i].AnimationName).x;
        return maxLength += GetSize("?").x;
    }

    public AnimationSetup GetNewAnimationSetup()
    {
        AnimationSetup newSetup = new AnimationSetup();
        newSetup.AnimationCurve = new AnimationCurve();
        newSetup.AnimationInfluence = new AnimationInfluenceOptions();
        newSetup.InfluencedObject = new InfluencedObject();
        newSetup.InfluenceAxis = new CustomBoolVector();
        return newSetup;
    }

    public AnimationSetup CopyAnimationSetup(AnimationSetup originalSetup)
    {
        var setupCopy = new AnimationSetup();
        var tempSetup = originalSetup;
        setupCopy.AnimationCurve = new AnimationCurve(originalSetup.AnimationCurve.keys);
        setupCopy.AnimationDuration = tempSetup.AnimationDuration;
        setupCopy.AnimationInfluence = tempSetup.AnimationInfluence;
        setupCopy.InfluenceAxis = new CustomBoolVector();
        setupCopy.InfluenceAxis.X = tempSetup.InfluenceAxis.X;
        setupCopy.InfluenceAxis.Y = tempSetup.InfluenceAxis.Y;
        setupCopy.InfluenceAxis.Z = tempSetup.InfluenceAxis.Z;
        setupCopy.InfluencedObject = tempSetup.InfluencedObject;
        return setupCopy;
    }

    public void PasteAnimationSetup(int groupIndex, int setupIndex)
    {
        if (CopiedAnimationSetup == null)
        {
            Debug.LogWarning("No animation setup copied ");
            return;
        }
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].AnimationCurve = new AnimationCurve(CopiedAnimationSetup.AnimationCurve.keys);
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].AnimationDuration = CopiedAnimationSetup.AnimationDuration;
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].AnimationInfluence = CopiedAnimationSetup.AnimationInfluence;
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].AnimationPlaying = CopiedAnimationSetup.AnimationPlaying;
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].InfluenceAxis = new CustomBoolVector();
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].InfluenceAxis.X = CopiedAnimationSetup.InfluenceAxis.X;
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].InfluenceAxis.Y = CopiedAnimationSetup.InfluenceAxis.Y;
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].InfluenceAxis.Z = CopiedAnimationSetup.InfluenceAxis.Z;
        AnimationGroups[groupIndex].AnimationSetups[setupIndex].InfluencedObject = CopiedAnimationSetup.InfluencedObject;
    }

    public void CopyAnimationSetup(int groupIndex, int setupIndex)
    {
        CopiedAnimationSetup = new AnimationSetup();
        var tempSetup = AnimationGroups[groupIndex].AnimationSetups[setupIndex];
        CopiedAnimationSetup.AnimationCurve = new AnimationCurve(tempSetup.AnimationCurve.keys);
        CopiedAnimationSetup.AnimationDuration = tempSetup.AnimationDuration;
        CopiedAnimationSetup.AnimationInfluence = tempSetup.AnimationInfluence;
        CopiedAnimationSetup.AnimationPlaying = tempSetup.AnimationPlaying;
        CopiedAnimationSetup.InfluenceAxis = new CustomBoolVector();
        CopiedAnimationSetup.InfluenceAxis.X = tempSetup.InfluenceAxis.X;
        CopiedAnimationSetup.InfluenceAxis.Y = tempSetup.InfluenceAxis.Y;
        CopiedAnimationSetup.InfluenceAxis.Z = tempSetup.InfluenceAxis.Z;
        CopiedAnimationSetup.InfluencedObject = tempSetup.InfluencedObject;
    }

    public FullAnimation GetNewAnimation()
    {
        var newAnimation = new FullAnimation();
        newAnimation.AnimationSetups = new List<AnimationSetup>();
        newAnimation.AnimationName = "Animation name";
        newAnimation.Expand = false;
        return newAnimation;
    }

    public void CopyAnimationSetups(int groupIndex)
    {
        CopiedAnimationSetups = new List<AnimationSetup>();
        for (int i = 0; i < AnimationGroups[groupIndex].AnimationSetups.Count; i++)
            CopiedAnimationSetups.Add(CopyAnimationSetup(AnimationGroups[groupIndex].AnimationSetups[i]));
    }

    public void PasteAnimationSetups(int groupIndex)
    {
        if (CopiedAnimationSetups == null || CopiedAnimationSetups.Count == 0)
        {
            Debug.LogWarning("No animation setups copied");
            return;
        }
        for (int i = 0; i < CopiedAnimationSetups.Count; i++)
            AnimationGroups[groupIndex].AnimationSetups.Add(CopiedAnimationSetups[i]);
    }

    int GetNumberOfAnimations(List<FullAnimation> animations, AnimationType animationType)
    {
        var animationTypeCounter = 0;
        for (int i = 0; i < animations.Count; i++)
            if (animations[i].AnimationType == animationType)
                animationTypeCounter++;
        return animationTypeCounter;
    }

    Vector2 GetSize(string inputString)
    {
        return GUI.skin.label.CalcSize(new GUIContent(inputString));
    }
}
