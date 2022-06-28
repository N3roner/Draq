using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(Car))]
public class CarEditor : Editor
{
    Car controller;
    bool showTimeSettings;
    bool showGearSettings;
    bool showSpeedFunctionSettings;
    bool showIntroSection;
    bool showBodyDeformationSection;
    bool showRPMSettings;
    int numberOfgears;
    float[] lengthPercentsCopy;
    float[] timePercentsCopy;
    List<UiSlider> lengthSliders;
    List<UiSlider> timeSliders;
    void OnEnable()
    {
        controller = (Car)target;
        if (controller == null)
        {
            Debug.Log("null");
            return;
        }
        if (controller.FinishTime <= 0)
            controller.FinishTime = 1;
        controller.InitArrays();
        numberOfgears = controller.TotalGears;
        lengthPercentsCopy = new float[controller.TotalGears];
        timePercentsCopy = new float[controller.TotalGears];
        for (int i = 0; i < controller.TotalGears; i++)
        {
            lengthPercentsCopy[i] = controller.LengthPercents[i];
            timePercentsCopy[i] = controller.TimePercents[i];
        }
        lengthSliders = new List<UiSlider>();
        timeSliders = new List<UiSlider>();
        controller.InitializeCarParts();
    }

    public override void OnInspectorGUI()
    {
        controller.CarID = EditorGUILayout.IntField("Car ID ", controller.CarID);
        EditorGUI.BeginChangeCheck();
        controller.FinishTime = EditorGUILayout.FloatField("Wanted finish time ", controller.FinishTime);
        if (EditorGUI.EndChangeCheck())
            controller.SetTopSpeeds();
        controller.CarBody = EditorGUILayout.ObjectField("Car body", controller.CarBody, typeof(Transform), true) as Transform;
        controller.FrontWheels = EditorGUILayout.ObjectField("Front wheels ", controller.FrontWheels, typeof(Transform), true) as Transform;
        controller.BackWheels = EditorGUILayout.ObjectField("Back wheel ", controller.BackWheels, typeof(Transform), true) as Transform;
        controller.Speed = EditorGUILayout.IntField("Car speed", controller.Speed);
        controller.DisplayedRPM = EditorGUILayout.IntField("Displayed RPM ", controller.DisplayedRPM);
        controller.currentGear = EditorGUILayout.IntField("Current gear : ", controller.currentGear);
        controller.TireDiameter = EditorGUILayout.FloatField("Tire diameter", controller.TireDiameter);
        //controller.WheelDrive = (WheelDrive)EditorGUILayout.EnumPopup("Wheel drive", controller.WheelDrive
        if (controller.TotalGears != numberOfgears)
        {
            ResetArrays();
            controller.SetTopSpeeds();
        }
        showRPMSettings = EditorGUILayout.Foldout(showRPMSettings, "   Revolutions per minute settings");
        if (showRPMSettings)
        {
            controller.TotalRPM = EditorGUILayout.IntField("Total RPM ", controller.TotalRPM);
            controller.RPMSweetSpot = EditorGUILayout.IntField("RPM sweet spot", controller.RPMSweetSpot);
            controller.RPMAtStart = EditorGUILayout.IntField("RPM at start", controller.RPMAtStart);
            controller.RPMDropPercent = EditorGUILayout.IntField("RPM drop percent", controller.RPMDropPercent);
        }
        showGearSettings = EditorGUILayout.Foldout(showGearSettings, "   Gear settings");
        var labelWidth = EditorGUIUtility.labelWidth;

        if (showGearSettings)
        {
            controller.TotalGears = EditorGUILayout.IntField(" - Number of gears : ", controller.TotalGears);
            if (controller.TotalGears <= 0)
                controller.TotalGears = 1;
            var metersLabel = " Meters ";
            var slidersWidth = EditorGUIUtility.currentViewWidth - labelWidth - GetSize(metersLabel).x - GetSize("20.000").x - GetSize("20.000").x - 40;
            var lengthSettingsRect = GUILayoutUtility.GetLastRect();

            if (controller.TotalGears != numberOfgears)
                return;

            lengthSettingsRect.y += lengthSettingsRect.height + 3;
            if (slidersWidth < 15)
                slidersWidth = 15 + 5;
            for (int i = 0; i < controller.TotalGears; i++)
            {
                ContinueWithLabel(ref lengthSettingsRect, "Gear " + (i + 1) + " length settings");
                ResetX(ref lengthSettingsRect);
                controller.LengthUserLocks[i] = ContinueWithToggle(ref lengthSettingsRect, controller.LengthUserLocks[i]);
                EditorGUI.BeginDisabledGroup(controller.LengthUserLocks[i]);
                EditorGUI.BeginChangeCheck();
                controller.LengthPercents[i] = ContinueWithSlider(ref lengthSettingsRect, controller.LengthPercents[i], 0, 100, slidersWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    SliderUtilities.UpdateSliderValues(controller.LengthPercents, lengthPercentsCopy, controller.LengthUserLocks, lengthSliders, i);
                    SliderUtilities.SaveValuesToCatched(controller.LengthPercents, lengthPercentsCopy);
                    controller.SetTopSpeeds();
                }
                var meters = i == 0 ? controller.GearsDistances[i] : (controller.GearsDistances[i] - controller.GearsDistances[i - 1]);
                ContinueWithInput(ref lengthSettingsRect, controller.LengthPercents[i], GetSize("20.000").x);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(true);
                ContinueWithLabel(ref lengthSettingsRect, metersLabel, 0, lengthSettingsRect.xMax);
                ContinueWithInput(ref lengthSettingsRect, meters, GetSize("20.000").x);
                EditorGUI.EndDisabledGroup();
                NewRow(ref lengthSettingsRect);
            }
            ResetX(ref lengthSettingsRect);
            var resetBtnPos = EditorGUIUtility.currentViewWidth - labelWidth - GetSize("Reset lengths").x + EditorGUIUtility.labelWidth - 20;
            ContinueWithLabel(ref lengthSettingsRect, "Total lengths % " + SliderUtilities.GetSum(controller.LengthPercents, controller.LengthUserLocks, false));
            if (ContinueWithButton(ref lengthSettingsRect, "Reset lengths", resetBtnPos))
                SliderUtilities.ResetSliders(controller.LengthPercents, controller.LengthUserLocks);
            NewRow(ref lengthSettingsRect);
            EditorGUILayout.Space();
        }

        showTimeSettings = EditorGUILayout.Foldout(showTimeSettings, "   Gears timings");

        if (showTimeSettings)
        {
            var timeLabel = " Seconds ";
            var slidersWidth = EditorGUIUtility.currentViewWidth - labelWidth - GetSize(timeLabel).x - GetSize("20.000").x - GetSize("20.000").x - 40;
            var timeSettingsRect = GUILayoutUtility.GetLastRect();

            if (controller.TotalGears != numberOfgears)
                return;

            timeSettingsRect.y += timeSettingsRect.height + 3;
            if (slidersWidth < 15)
                slidersWidth = 15 + 5;
            for (int i = 0; i < controller.TotalGears; i++)
            {
                ContinueWithLabel(ref timeSettingsRect, "Gear " + (i + 1) + " time settings");
                ResetX(ref timeSettingsRect);
                controller.TimeUserLocks[i] = ContinueWithToggle(ref timeSettingsRect, controller.TimeUserLocks[i]);
                EditorGUI.BeginDisabledGroup(controller.TimeUserLocks[i]);
                EditorGUI.BeginChangeCheck();
                controller.TimePercents[i] = ContinueWithSlider(ref timeSettingsRect, controller.TimePercents[i], 0, 100, slidersWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    SliderUtilities.UpdateSliderValues(controller.TimePercents, timePercentsCopy, controller.TimeUserLocks, timeSliders, i);
                    SliderUtilities.SaveValuesToCatched(controller.TimePercents, timePercentsCopy);
                    controller.SetTopSpeeds();
                }
                var time = i == 0 ? controller.GearsTimings[i] : (controller.GearsTimings[i]);
                ContinueWithInput(ref timeSettingsRect, controller.TimePercents[i], GetSize("20.000").x);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(true);
                ContinueWithLabel(ref timeSettingsRect, " Seconds ", 0, timeSettingsRect.xMax);
                ContinueWithInput(ref timeSettingsRect, time, GetSize("20.000").x);
                EditorGUI.EndDisabledGroup();
                NewRow(ref timeSettingsRect);
            }
            ResetX(ref timeSettingsRect);
            var resetBtnPos = EditorGUIUtility.currentViewWidth - labelWidth - GetSize("Reset timings").x + EditorGUIUtility.labelWidth - 20;
            ContinueWithLabel(ref timeSettingsRect, "Total time % " + SliderUtilities.GetSum(controller.TimePercents, controller.TimeUserLocks, false));
            if (ContinueWithButton(ref timeSettingsRect, "Reset timings", resetBtnPos))
                SliderUtilities.ResetSliders(controller.TimePercents, controller.TimeUserLocks);
            NewRow(ref timeSettingsRect);
            EditorGUILayout.Space();
            showTimeSettings = true;
        }

        showSpeedFunctionSettings = EditorGUILayout.Foldout(showSpeedFunctionSettings, "   Speed function settings and top speeds");

        if (showSpeedFunctionSettings)
        {
            var xFromLbl = "X from ";
            var xToLbl = " X to ";
            var topSpeedLbl = " Top speed ";
            var totalLeftWidth2 = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - EditorGUIUtility.fieldWidth;
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y += lastRect.height + 3;
            var totLeft = totalLeftWidth2 - GetSize(xFromLbl).x - GetSize(xToLbl).x - GetSize(topSpeedLbl).x - GetSize("500.000").x - 2 * GetSize("1.577").x;
            var slidersWidth = totLeft / 2;
            if (slidersWidth < 15)
                slidersWidth = 15 + 5;

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < numberOfgears; i++)
            {
                ContinueWithLabel(ref lastRect, "Gear " + (i + 1) + " settings");
                ContinueWithLabel(ref lastRect, xFromLbl, 0, EditorGUIUtility.labelWidth + 15);
                controller.SpeedFunctionSettings[i].XFrom = ContinueWithSlider(ref lastRect, controller.SpeedFunctionSettings[i].XFrom, -1.57f, 0, slidersWidth);
                ContinueWithInput(ref lastRect, controller.SpeedFunctionSettings[i].XFrom, GetSize("-1.570").x, 5);
                ContinueWithLabel(ref lastRect, xToLbl, GetSize(xToLbl).x);
                controller.SpeedFunctionSettings[i].XTo = ContinueWithSlider(ref lastRect, controller.SpeedFunctionSettings[i].XTo, 0, 1.57f, slidersWidth);
                ContinueWithInput(ref lastRect, controller.SpeedFunctionSettings[i].XTo, GetSize("-1.570").x, 5);
                EditorGUI.BeginDisabledGroup(true);
                ContinueWithLabel(ref lastRect, topSpeedLbl, GetSize(topSpeedLbl).x);
                ContinueWithInput(ref lastRect, controller.MaxSpeedPerGear[i], GetSize("5000001").x, 5);
                EditorGUI.EndDisabledGroup();
                NewRow(ref lastRect);
            }
            if (EditorGUI.EndChangeCheck())
                controller.SetTopSpeeds();
            EditorGUILayout.Space();
        }
    }

    Vector2 GetSize(string inputString)
    {
        return GUI.skin.label.CalcSize(new GUIContent(inputString));
    }

    bool ContinueWithToggle(ref Rect usedRect, bool value)
    {
        usedRect.width = 20;
        return EditorGUI.ToggleLeft(usedRect, "", value);
    }

    float ContinueWithSlider(ref Rect previousLabelRect, float value, float leftValue, float rightValue, float customWidth = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        return GUI.HorizontalSlider(previousLabelRect, value, leftValue, rightValue);
    }

    void ContinueWithLabel(ref Rect previousLabelRect, string newLabelContent, float customWidth = 0, float customXStart = 0)
    {
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

    void ContinueWithInput(ref Rect previousLabelRect, float value, float customWidth = 0, float customXStart = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        if (customXStart != 0)
            previousLabelRect.x += customXStart;
        value = EditorGUI.FloatField(previousLabelRect, "", value);
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

    bool ContinueWithButton(ref Rect previousRect, string buttonContentn, float startX = 0)
    {
        if (startX != 0)
            previousRect.x = startX;
        else
            previousRect.x = previousRect.xMax;

        previousRect.width = GetSize(buttonContentn + "aa").x;
        if (GUI.Button(previousRect, buttonContentn))
            return true;
        return false;
    }

    void ResetArrays()
    {
        Array.Resize(ref controller.LengthPercents, controller.TotalGears);
        Array.Resize(ref controller.TimePercents, controller.TotalGears);
        Array.Resize(ref controller.LengthUserLocks, controller.TotalGears);
        Array.Resize(ref controller.TimeUserLocks, controller.TotalGears);
        Array.Resize(ref controller.SpeedFunctionSettings, controller.TotalGears);
        SliderUtilities.ResetSliders(controller.LengthPercents, controller.LengthUserLocks);
        SliderUtilities.ResetSliders(controller.TimePercents, controller.TimeUserLocks);
        lengthPercentsCopy = new float[controller.TotalGears];
        timePercentsCopy = new float[controller.TotalGears];
        for (int i = 0; i < controller.TotalGears; i++)
        {
            lengthPercentsCopy[i] = controller.LengthPercents[i];
            timePercentsCopy[i] = controller.TimePercents[i];
        }
        numberOfgears = controller.TotalGears;
    }
}
