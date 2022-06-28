using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceTrack))]
public class RaceTrackEditor : Editor
{
    bool introSettings;
    RaceTrack controller;

    void OnEnable()
    {
        controller = (RaceTrack)target;
        controller.LeftTrackCarIntro = controller.InitializeArrays(controller.LeftTrackCarIntro);
        controller.RightTrackCarIntro = controller.InitializeArrays(controller.RightTrackCarIntro);
    }

    public override void OnInspectorGUI()
    {
        controller.ChristmasTree = (GameObject)EditorGUILayout.ObjectField("Christmass tree", controller.ChristmasTree, typeof(GameObject),true);
        controller.StageTimeOffset = EditorGUILayout.FloatField("Tree stage time percent offset", controller.StageTimeOffset);
        controller.ReadyTimeOffset = EditorGUILayout.FloatField("Tree ready time percent offset", controller.ReadyTimeOffset);
        controller.TreeTurnOffOfFset = EditorGUILayout.FloatField("Tree turn off time offset", controller.TreeTurnOffOfFset);
        controller.RaceTrackLength = EditorGUILayout.FloatField("Race track length", controller.RaceTrackLength);
        controller.TotalIntroDuration = EditorGUILayout.FloatField("Total intro duration", controller.TotalIntroDuration);
        controller.SpeedUnitUsed = (SpeedUnit)EditorGUILayout.EnumPopup("Speed units", controller.SpeedUnitUsed);
        controller.RpmDropTiming = EditorGUILayout.FloatField("RPM droping time", controller.RpmDropTiming);
        introSettings = EditorGUILayout.Foldout(introSettings, "Race intro settings");

        if (introSettings)
        {
            DisplayOffset(ref controller.LeftTrackCarIntro.XOffsetMin, ref controller.LeftTrackCarIntro.XOffsetMax, "Left track X offset");
            DisplayOffset(ref controller.RightTrackCarIntro.XOffsetMin, ref controller.RightTrackCarIntro.XOffsetMax, "Right track X offset");
            DisplayOffset(ref controller.SpawnZMin, ref controller.SpawnZMax, "Spawn position Z offset");
            DisplayOffset(ref controller.BurnoutZMin, ref controller.BurnoutZMax, "Burnout position Z offset");

            GUILayout.Label("-- Left track car settings");
            controller.LeftTrackCarIntro = DisplayCarIntroSettings(controller.LeftTrackCarIntro);

            GUILayout.Label("-- Right track car settings");
            controller.RightTrackCarIntro = DisplayCarIntroSettings(controller.RightTrackCarIntro);
        }
    }

    public void DisplayOffset(ref float min, ref float max, string labelContent)
    {
        var labelWidth = EditorGUIUtility.labelWidth;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(labelContent, GUILayout.Width(labelWidth - (labelWidth * 2f / 100f)));
        EditorGUILayout.LabelField("Min", GUILayout.Width(30));
        min = EditorGUILayout.FloatField(min);
        EditorGUILayout.LabelField("Max", GUILayout.Width(30));
        max = EditorGUILayout.FloatField(max);
        EditorGUILayout.EndHorizontal();
    }

    public IntroSettings DisplayCarIntroSettings(IntroSettings passedIntroSettings)
    {
        var labelWidth = EditorGUIUtility.labelWidth;
        var remainingWidth = EditorGUIUtility.currentViewWidth - labelWidth;

        for (int i = 0; i < controller.TimePercentFields.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(controller.TimePercentFields[i], GUILayout.Width(labelWidth - (labelWidth * 2f / 100f)));
            passedIntroSettings.SliderLocks[i] = EditorGUILayout.Toggle(passedIntroSettings.SliderLocks[i], GUILayout.Width(remainingWidth * 5 / 100));
            EditorGUI.BeginDisabledGroup(passedIntroSettings.SliderLocks[i]);
            EditorGUI.BeginChangeCheck();
            passedIntroSettings.TimePercents[i] = EditorGUILayout.Slider("", passedIntroSettings.TimePercents[i], 0f, 100f, GUILayout.Width(remainingWidth * 60 / 100));
            if (EditorGUI.EndChangeCheck())
            {
                SliderUtilities.UpdateSliderValues(passedIntroSettings.TimePercents, passedIntroSettings.TimePercentsCopy, passedIntroSettings.SliderLocks, passedIntroSettings.TimeSliders, i);
                SliderUtilities.SaveValuesToCatched(passedIntroSettings.TimePercents, passedIntroSettings.TimePercentsCopy);
                passedIntroSettings = controller.UpdateTimings(passedIntroSettings.TimePercents, passedIntroSettings);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Label("Time : " + (passedIntroSettings.TimePercents[i] * controller.TotalIntroDuration / 100f), GUILayout.Width(remainingWidth * 20 / 100));
            EditorGUILayout.EndHorizontal();
        }
        return passedIntroSettings;
    }
}
