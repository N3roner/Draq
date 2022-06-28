#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(CustomUiElement))]
public class CustomUiElementEditor : Editor
{
    CustomUiElement controller;
    void OnEnable()
    {
        controller = (CustomUiElement)target;
        if(!controller.gameObject.GetComponent<Image>())
        controller.gameObject.AddComponent<Image>();
        //if (controller.animationSetup == null)
            controller.animationSetup = new CustomUiElement.UiAnimationSetup();
        controller.animationSetup.InfluenceAxis = new CustomBoolVector();
        if(controller.animationSetup.AnimationCurve == null)
        controller.animationSetup.AnimationCurve = new AnimationCurve();
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("hahaha");
        var lengthSettingsRect = GUILayoutUtility.GetLastRect();
        NewRow(ref lengthSettingsRect);
        ContinueWithLabel(ref lengthSettingsRect, "Starting position");
        if (controller.StartingPosition == null)
            controller.StartingPosition = new CustomVector2();
        if (controller.EndingPosition == null)
            controller.EndingPosition = new CustomVector2();

        controller.StartingPosition.X = ContinueWithInput(ref lengthSettingsRect, controller.StartingPosition.X, 40,0,false);
        ContinueWithLabel(ref lengthSettingsRect, "                Y ");
        controller.StartingPosition.Y = ContinueWithInput(ref lengthSettingsRect, controller.StartingPosition.Y, 40, 0, false);
        NewRow(ref lengthSettingsRect);
        ContinueWithLabel(ref lengthSettingsRect, "Ending position");
        controller.EndingPosition.X = ContinueWithInput(ref lengthSettingsRect, controller.EndingPosition.X, 40,0,false);
        ContinueWithLabel(ref lengthSettingsRect, "                Y ");
        controller.EndingPosition.Y = ContinueWithInput(ref lengthSettingsRect, controller.EndingPosition.Y, 40, 0, false);
        NewRow(ref lengthSettingsRect);
        ContinueWithLabel(ref lengthSettingsRect, "Animation setup ");

        ContinueWithLabel(ref lengthSettingsRect, " X : ", GetSize(" X ").x, lengthSettingsRect.xMax);
        controller.animationSetup.InfluenceAxis.X = ContinueWithToggle(ref lengthSettingsRect, controller.animationSetup.InfluenceAxis.X);
        ContinueWithLabel(ref lengthSettingsRect, " Y : ", GetSize(" X ").x, lengthSettingsRect.xMax);
        controller.animationSetup.InfluenceAxis.Y = ContinueWithToggle(ref lengthSettingsRect, controller.animationSetup.InfluenceAxis.Y);
        ContinueWithLabel(ref lengthSettingsRect, " Z : ", GetSize(" X ").x, lengthSettingsRect.xMax);
        controller.animationSetup.InfluenceAxis.Z = ContinueWithToggle(ref lengthSettingsRect, controller.animationSetup.InfluenceAxis.Z);
        NewRow(ref lengthSettingsRect);
        ContinueWithCurveInput(ref lengthSettingsRect, controller.animationSetup.AnimationCurve);
    }

    void ContinueWithCurveInput(ref Rect previousLabelRect, AnimationCurve value, float customWidth = 0, float customXStart = 0)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        if (customXStart != 0)
            previousLabelRect.x = customXStart;
        value = EditorGUI.CurveField(previousLabelRect, "", value);
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

    float ContinueWithInput(ref Rect previousLabelRect, float value, float customWidth = 0, float customXStart = 0, bool bezze = false)
    {
        previousLabelRect.x = previousLabelRect.xMax;
        previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
        if (customXStart != 0)
            previousLabelRect.x += customXStart;
        value = EditorGUI.FloatField(previousLabelRect, "", value);
        return value;
    }

    //void ContinueWithInput(ref Rect previousLabelRect, float value, float customWidth = 0, float customXStart = 0)
    //{
    //    previousLabelRect.x = previousLabelRect.xMax;
    //    previousLabelRect.width = customWidth == 0 ? GetSize(value.ToString()).x : customWidth;
    //    if (customXStart != 0)
    //        previousLabelRect.x += customXStart;
    //    value = EditorGUI.FloatField(previousLabelRect, "", value);
    //    //ContinueWithLabel(ref previousLabelRect, " Y ");
    //    //value.Y = EditorGUI.FloatField(previousLabelRect, "", value.Y);
    //}

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

    bool ContinueWidthButton(ref Rect previousRect, string buttonContentn, float startX = 0)
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

}
#endif
