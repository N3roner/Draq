using System;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType
{
    START,
    BURNOUT,
    GEARCHANGE
}

public enum AnimationInfluenceOptions
{
    ROTATION
}

public enum InfluencedObject
{
    CAR,
    OTHER,
    FWD,
    RWD,
    FRONTWHEELS,
    REARWHEELS
}

[Serializable]
public class FullAnimation
{
    public bool PlayAnimation;
    public int QueIndex;
    public bool Expand;
    public string AnimationName;
    public List<AnimationSetup> AnimationSetups;
    public AnimationType AnimationType;
}

[Serializable]
public class PlayableAnimations
{
    public bool IntroAnimations;
    public bool RaceAnimations;
    public bool GearChangingAnimations;
}

[Serializable]
public class CustomBoolVector
{
    public bool X;
    public bool Y;
    public bool Z;
}

[Serializable]
public class AnimationSetup
{
    public AnimationCurve AnimationCurve;
    public float AnimationDuration;
    public bool AnimationPlaying;
    public float AnimationStartTime;
    public AnimationInfluenceOptions AnimationInfluence;
    public InfluencedObject InfluencedObject;
    public CustomBoolVector InfluenceAxis;
}

public class CustomAnimator : MonoBehaviour
{
    public List<FullAnimation> AnimationsToPlay;
    public PlayableAnimations PlayableAnimations;
    public void CustomAnimatorConstructor(List<FullAnimation> animations, PlayableAnimations passedPlayableAnimations)
    {
        AnimationsToPlay = new List<FullAnimation>();
        PlayableAnimations = new PlayableAnimations();
        PlayableAnimations.IntroAnimations = passedPlayableAnimations.IntroAnimations;
        PlayableAnimations.RaceAnimations = passedPlayableAnimations.RaceAnimations;
        PlayableAnimations.GearChangingAnimations = passedPlayableAnimations.GearChangingAnimations;

        for (int i = 0; i < animations.Count; i++)
        {
            var tempAnimation = new FullAnimation();
            tempAnimation.AnimationName = animations[i].AnimationName;
            tempAnimation.AnimationType = animations[i].AnimationType;
            tempAnimation.AnimationSetups = new List<AnimationSetup>();

            for (int j = 0; j < animations[i].AnimationSetups.Count; j++)
            {
                var parsedSetup = animations[i].AnimationSetups[j];
                tempAnimation.AnimationSetups.Add(new AnimationSetup());
                tempAnimation.AnimationSetups[j].AnimationCurve = new AnimationCurve(parsedSetup.AnimationCurve.keys);
                tempAnimation.AnimationSetups[j].AnimationDuration = parsedSetup.AnimationDuration;
                tempAnimation.AnimationSetups[j].AnimationStartTime = 0f;
                tempAnimation.AnimationSetups[j].AnimationInfluence = parsedSetup.AnimationInfluence;
                tempAnimation.AnimationSetups[j].InfluencedObject = parsedSetup.InfluencedObject;
                tempAnimation.AnimationSetups[j].InfluenceAxis = new CustomBoolVector();
                tempAnimation.AnimationSetups[j].InfluenceAxis.X = parsedSetup.InfluenceAxis.X;
                tempAnimation.AnimationSetups[j].InfluenceAxis.Y = parsedSetup.InfluenceAxis.Y;
                tempAnimation.AnimationSetups[j].InfluenceAxis.Z = parsedSetup.InfluenceAxis.Z;
                tempAnimation.AnimationSetups[j].AnimationPlaying = parsedSetup.AnimationPlaying;
            }
            AnimationsToPlay.Add(tempAnimation);
        }
    }

    public void UpdateAnimations(float time)
    {
        for (int i = 0; i < AnimationsToPlay.Count; i++)
            if (AnimationsToPlay[i].PlayAnimation)
                PlayAnimation(time, AnimationsToPlay[i]);
    }

    public void StartAnimationOfType(AnimationType animationType, int inputQueIndex)
    {
        int animationIndex = 0;
        if (animationType == AnimationType.GEARCHANGE && !PlayableAnimations.GearChangingAnimations)
            return;
        if (animationType == AnimationType.GEARCHANGE && PlayableAnimations.GearChangingAnimations)
            inputQueIndex = AnimationsToPlay[AnimationsToPlay.Count - 1].QueIndex + 1;

        for (int i = 0; i < AnimationsToPlay.Count; i++)
            if (AnimationsToPlay[i].AnimationType == animationType)
                animationIndex = i;

        if (AnimationsToPlay[animationIndex].QueIndex != inputQueIndex)
        {
            AnimationsToPlay[animationIndex].QueIndex = inputQueIndex;
            AnimationsToPlay[animationIndex].PlayAnimation = true;

            for (int j = 0; j < AnimationsToPlay[animationIndex].AnimationSetups.Count; j++)
            {
                AnimationsToPlay[animationIndex].AnimationSetups[j].AnimationStartTime = 0f;
                AnimationsToPlay[animationIndex].AnimationSetups[j].AnimationPlaying = true;
            }
        }
    }

    public void StartAnimation(int animationIndex, bool resetAll = false)
    {
        if (resetAll)
            for (int i = 0; i < AnimationsToPlay.Count; i++)
                for (int j = 0; j < AnimationsToPlay[i].AnimationSetups.Count; j++)
                {
                    AnimationsToPlay[i].AnimationSetups[j].AnimationStartTime = 0f;
                    AnimationsToPlay[i].AnimationSetups[j].AnimationPlaying = true;
                }

        AnimationsToPlay[animationIndex].PlayAnimation = true;
    }

    public void PlayAnimation(float time, FullAnimation usedAnimation)
    {
        for (int i = 0; i < usedAnimation.AnimationSetups.Count; i++)
        {
            var tempSetup = usedAnimation.AnimationSetups[i];

            if (usedAnimation.AnimationSetups[i].AnimationPlaying)
            {
                if (tempSetup.AnimationStartTime == 0f)
                    tempSetup.AnimationStartTime = time;

                var animProgress = (time - tempSetup.AnimationStartTime) / tempSetup.AnimationDuration;

                if (animProgress >= 1)
                {
                    usedAnimation.PlayAnimation = false;
                    tempSetup.AnimationPlaying = false;
                }

                List<GameObject> animatedSubject = new List<GameObject>();

                if (tempSetup.InfluencedObject == InfluencedObject.CAR)
                    animatedSubject.Add(gameObject);
                if (tempSetup.InfluencedObject == InfluencedObject.OTHER)
                    animatedSubject.Add(gameObject.transform.FindChild("FWD").transform.FindChild("RWD").GetChild(0).gameObject);
                if (tempSetup.InfluencedObject == InfluencedObject.FWD)
                    animatedSubject.Add(gameObject.transform.FindChild("FWD").gameObject);
                if (tempSetup.InfluencedObject == InfluencedObject.RWD)
                    animatedSubject.Add(gameObject.transform.FindChild("FWD").GetChild(0).gameObject);
                if (tempSetup.InfluencedObject == InfluencedObject.FRONTWHEELS)
                {
                    animatedSubject.Add(gameObject.transform.Find("Axles").GetChild(1).GetChild(0).GetChild(0).gameObject);
                    animatedSubject.Add(gameObject.transform.Find("Axles").GetChild(1).GetChild(0).GetChild(1).gameObject);
                }

                if (tempSetup.InfluencedObject == InfluencedObject.REARWHEELS)
                {
                    animatedSubject.Add(gameObject.transform.Find("Axles").GetChild(0).GetChild(0).GetChild(0).gameObject);
                    animatedSubject.Add(gameObject.transform.Find("Axles").GetChild(0).GetChild(0).GetChild(1).gameObject);
                }

                if (tempSetup.AnimationInfluence == AnimationInfluenceOptions.ROTATION)
                {
                    List<Vector3> rotations = new List<Vector3>();
                    for (int r = 0; r < animatedSubject.Count; r++)
                        rotations.Add(animatedSubject[r].transform.eulerAngles);

                    Vector3 tempVector = new Vector3();
                    tempVector = animatedSubject[0].transform.eulerAngles;

                    if (tempSetup.InfluenceAxis.X)
                    {
                        tempVector.x = tempSetup.AnimationCurve.Evaluate(animProgress);
                        rotations[0] = tempVector;
                    }

                    if (tempSetup.InfluenceAxis.Y)
                    {
                        tempVector.y = tempSetup.AnimationCurve.Evaluate(animProgress);
                        if (tempSetup.InfluencedObject == InfluencedObject.FRONTWHEELS || tempSetup.InfluencedObject == InfluencedObject.REARWHEELS)
                        {
                            var tempLeftRotation = rotations[1];
                            tempLeftRotation.y = tempSetup.AnimationCurve.Evaluate(animProgress);
                            rotations[1] = tempLeftRotation;

                            var tempRightRotation = rotations[0];
                            tempRightRotation.y = 180f + tempSetup.AnimationCurve.Evaluate(animProgress);
                            rotations[0] = tempRightRotation;
                        }
                        else
                            rotations[0] = tempVector;
                    }

                    if (tempSetup.InfluenceAxis.Z)
                    {
                        tempVector.z = tempSetup.AnimationCurve.Evaluate(animProgress);
                        rotations[0] = tempVector;
                    }

                    for (int j = 0; j < animatedSubject.Count; j++)
                        animatedSubject[j].transform.eulerAngles = rotations[j];
                }
            }
        }
    }
}
