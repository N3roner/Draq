using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(CustomAnimatorController))]
public class CustomAnimatorControllerEditor : Editor
{
    CustomAnimatorController controller;

    void OnEnable()
    {
        controller = (CustomAnimatorController)target;
        controller.Init();
    }

    void WriteGroupsColapsingButton(Rect animatorRect, Rect firstRectOriginal)
    {
        var expandColapseText = string.Empty;
        if (controller.ExpandColapseAll)
            expandColapseText = "Expand";
        else
            expandColapseText = "Colapse";

        if (ContinueWithButton(ref animatorRect, expandColapseText + " all", firstRectOriginal.x))
        {
            for (int i = 0; i < controller.AnimationGroups.Count; i++)
                controller.AnimationGroups[i].Expand = controller.ExpandColapseAll;
            controller.ExpandColapseAll = !controller.ExpandColapseAll;
        }
    }

    void WriteSetupsColapsingButtons(int groupIndex, ref Rect passedRect)
    {
        var textToShow = string.Empty;
        if (controller.AnimationGroups[groupIndex].Expand)
            textToShow = "Colapse";
        else
            textToShow = "Expand";
        if (ContinueWithButton(ref passedRect, textToShow))
        {
            controller.AnimationGroups[groupIndex].Expand = !controller.AnimationGroups[groupIndex].Expand;
            if (controller.AnimationGroups[groupIndex].AnimationSetups.Count == 0)
            {
                controller.AnimationGroups[groupIndex].AnimationSetups = new List<AnimationSetup>();
                controller.AnimationGroups[groupIndex].AnimationSetups.Add(controller.GetNewAnimationSetup());
            }
        }
    }

    public override void OnInspectorGUI()
    {
        controller.PlayableAnimations.IntroAnimations = EditorGUILayout.Toggle("Play intro animations", controller.PlayableAnimations.IntroAnimations);
        controller.PlayableAnimations.RaceAnimations = EditorGUILayout.Toggle("Play race animations animations", controller.PlayableAnimations.RaceAnimations);
        controller.PlayableAnimations.GearChangingAnimations = EditorGUILayout.Toggle("Play gear changing animations", controller.PlayableAnimations.GearChangingAnimations);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(" ");

        var animatorRect = GUILayoutUtility.GetLastRect();
        var firstRectOriginal = GUILayoutUtility.GetLastRect();
        ResetX(ref animatorRect);
        WriteGroupsColapsingButton(animatorRect, firstRectOriginal);
        for (int i = 0; i < controller.AnimationGroups.Count; i++)
        {
            EditorGUILayout.Space();
            NewRow(ref animatorRect);
            if (ContinueWithButton(ref animatorRect, "+", firstRectOriginal.x))
                controller.AnimationGroups.Insert(i + 1, controller.GetNewAnimation());
            if (ContinueWithButton(ref animatorRect, "-"))
                controller.AnimationGroups.RemoveAt(i);
            ContinueWithLabel(ref animatorRect, " Animation " + (i+1), 0, animatorRect.xMax);
            controller.AnimationGroups[i].AnimationName = ContinueWithInput(ref animatorRect, controller.AnimationGroups[i].AnimationName, controller.GetLongestAnimationName());
            controller.AnimationGroups[i].AnimationType = (AnimationType)ContinueWithEnum(ref animatorRect, controller.AnimationGroups[i].AnimationType, "BREAKING??");
            WriteSetupsColapsingButtons(i, ref animatorRect);
            ContinueWithLabel(ref animatorRect, "Play this animation", 0, animatorRect.xMax + 20);
            EditorGUI.BeginChangeCheck();
            controller.AnimationGroups[i].PlayAnimation = ContinueWithToggle(ref animatorRect, controller.AnimationGroups[i].PlayAnimation);
            if (EditorGUI.EndChangeCheck())
                for (int j = 0; j < controller.AnimationGroups[i].AnimationSetups.Count; j++)
                    controller.AnimationGroups[i].AnimationSetups[j].AnimationPlaying = controller.AnimationGroups[i].PlayAnimation;

            if (ContinueWithButton(ref animatorRect, "C"))
                controller.CopyAnimationSetups(i);

            if (ContinueWithButton(ref animatorRect, "P"))
                controller.PasteAnimationSetups(i);

            if (controller.AnimationGroups[i].Expand)
            {
                animatorRect.y += 7;
                var tempGroup = controller.AnimationGroups[i];
                for (int j = 0; j < tempGroup.AnimationSetups.Count; j++)
                {
                    EditorGUILayout.Space();
                    NewRow(ref animatorRect);
                    if (ContinueWithButton(ref animatorRect, "+", firstRectOriginal.x + 15, 17))
                        tempGroup.AnimationSetups.Insert(j + 1, controller.GetNewAnimationSetup());

                    if (ContinueWithButton(ref animatorRect, "-", 0, 17))
                        tempGroup.AnimationSetups.RemoveAt(j);

                    ContinueWithLabel(ref animatorRect, "Animation setup " + (j+1));
                    if (tempGroup.AnimationSetups[j].AnimationCurve == null)
                        tempGroup.AnimationSetups[j].AnimationCurve = new AnimationCurve();
                    ContinueWithCurveInput(ref animatorRect, tempGroup.AnimationSetups[j].AnimationCurve);

                    ContinueWithLabel(ref animatorRect, "Played - ", 0, animatorRect.xMax + 15);
                    controller.AnimationGroups[i].AnimationSetups[j].AnimationPlaying = ContinueWithToggle(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].AnimationPlaying, animatorRect.xMax - 2);
                    ContinueWithLabel(ref animatorRect, "Duration");

                    controller.AnimationGroups[i].AnimationSetups[j].AnimationDuration = ContinueWithInput(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].AnimationDuration, controller.GetMaxDuration());
                    controller.AnimationGroups[i].AnimationSetups[j].InfluencedObject = (InfluencedObject)ContinueWithEnum(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].InfluencedObject, "FRONWHEELS???");
                    controller.AnimationGroups[i].AnimationSetups[j].AnimationInfluence = (AnimationInfluenceOptions)ContinueWithEnum(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].AnimationInfluence, "POSITION????");
                    var influenceLabel = firstRectOriginal;
                    influenceLabel.x = animatorRect.x + 10;
                    ContinueWithLabel(ref animatorRect, " X: ", GetSize(" X").x, animatorRect.xMax);
                    controller.AnimationGroups[i].AnimationSetups[j].InfluenceAxis.X = ContinueWithToggle(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].InfluenceAxis.X);
                    ContinueWithLabel(ref animatorRect, "Y: ", GetSize("X").x, animatorRect.xMax);
                    controller.AnimationGroups[i].AnimationSetups[j].InfluenceAxis.Y = ContinueWithToggle(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].InfluenceAxis.Y);
                    ContinueWithLabel(ref animatorRect, "Z: ", GetSize("X").x, animatorRect.xMax);
                    controller.AnimationGroups[i].AnimationSetups[j].InfluenceAxis.Z = ContinueWithToggle(ref animatorRect, controller.AnimationGroups[i].AnimationSetups[j].InfluenceAxis.Z);
                    animatorRect.x += 10;

                    if (ContinueWithButton(ref animatorRect, "C"))
                        controller.CopyAnimationSetup(i, j);

                    if (ContinueWithButton(ref animatorRect, "P"))
                        controller.PasteAnimationSetup(i, j);
                }
            }
            animatorRect.y += 7;
            animatorRect.height = firstRectOriginal.height;
        }

        NewRow(ref animatorRect);
        ResetX(ref animatorRect);
        animatorRect.x = firstRectOriginal.x;

        if (ContinueWithButton(ref animatorRect, "Play"))
            controller.TestPlay();

        ContinueWithLabel(ref animatorRect, "Testing object", 0, animatorRect.xMax);
        animatorRect.x += GetSize("Testing object").x;
        controller.TestingObject = (GameObject)EditorGUI.ObjectField(animatorRect, controller.TestingObject, typeof(UnityEngine.Object), true);

        animatorRect.x += 30;
        ContinueWithLabel(ref animatorRect, "Loop play", 0, animatorRect.xMax);
        controller.LoopAnimation = ContinueWithToggle(ref animatorRect, controller.LoopAnimation);
    }

    Vector2 GetSize(string inputString)
    {
        return GUI.skin.label.CalcSize(new GUIContent(inputString));
    }

    bool ContinueWithToggle(ref Rect usedRect, bool value, float customXStart = 0)
    {
        usedRect.x = usedRect.xMax + 10;
        usedRect.width = 20;

        if (customXStart != 0)
            usedRect.x = customXStart;
        return EditorGUI.ToggleLeft(usedRect, "", value);
    }

    void ContinueWithLabel(ref Rect previousLabelRect, string newLabelContent, float customWidth = 0, float customXStart = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        if (customWidth != 0)
        {
            previousLabelRect.x = previousLabelRect.xMax;
            previousLabelRect.width = customWidth;
        }
        else
            previousLabelRect.width = GetSize(newLabelContent).x;
        if (customXStart != 0)
            previousLabelRect.x = customXStart;

        EditorGUI.LabelField(previousLabelRect, newLabelContent);
    }

    void ContinueWithCurveInput(ref Rect previousLabelRect, AnimationCurve value, float customWidth = 0, float customXStart = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        if (customXStart != 0)
            previousLabelRect.x = customXStart;
        value = EditorGUI.CurveField(previousLabelRect, "", value);
    }

    float ContinueWithInput(ref Rect previousLabelRect, float value, float customWidth = 0, float customXStart = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        if (customXStart != 0)
            previousLabelRect.x += customXStart;
        value = EditorGUI.FloatField(previousLabelRect, "", value);
        return value;
    }

    string ContinueWithInput(ref Rect previousLabelRect, string value, float customWidth = 0, float customXStart = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        if (customXStart != 0)
            previousLabelRect.x += customXStart;
        value = EditorGUI.TextField(previousLabelRect, "", value);
        return value;
    }

    void NewRow(ref Rect usedRect)
    {
        usedRect.y += usedRect.height + 3;
        usedRect.x = GUILayoutUtility.GetLastRect().x;
        EditorGUILayout.LabelField("");
    }

    void ResetX(ref Rect usedRect, int extraWidth = 15)
    {
        usedRect.x = EditorGUIUtility.labelWidth + extraWidth;
    }

    Enum ContinueWithEnum(ref Rect previousRect, System.Enum enumValue, string maxWidth)
    {
        previousRect.x = previousRect.xMax;
        previousRect.width = GetSize(maxWidth).x;
        return EditorGUI.EnumPopup(previousRect, enumValue);
    }

    bool ContinueWithButton(ref Rect previousRect, string buttonContentn, float startX = 0, float customWidth = 0)
    {
        if (startX != 0)
            previousRect.x = startX;
        else
            previousRect.x = previousRect.xMax;

        previousRect.width = GetSize(buttonContentn + "aa").x;

        if (customWidth != 0)
            previousRect.width = customWidth;

        if (GUI.Button(previousRect, buttonContentn))
            return true;
        return false;
    }
}
