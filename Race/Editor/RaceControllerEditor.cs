using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceController))]
public class RaceControllerEditor : Editor
{
    RaceController controller;

    void OnEnable()
    {
        controller = (RaceController)target;
    }

    Vector2 GetSize(string inputString)
    {
        return GUI.skin.label.CalcSize(new GUIContent(inputString));
    }

    void ContinueWithLabel(ref Rect previousLabelRect, string newLabelContent)
    {
        previousLabelRect.width = GetSize(newLabelContent).x;
        EditorGUI.LabelField(previousLabelRect, newLabelContent);
        previousLabelRect.x += GetSize(newLabelContent).x;
    }

    void ContinueWithInput(ref Rect previousLabelRect, ref float value)
    {
        previousLabelRect.width = GetSize(value.ToString() + "22").x;
        value = EditorGUI.FloatField(previousLabelRect, value);
        previousLabelRect.x += GetSize(value.ToString() + "22").x;
    }
    void ContinueWithInput(ref Rect previousLabelRect, ref string value)
    {
        previousLabelRect.width = GetSize(value.ToString() + "22").x;
        value = EditorGUI.TextField(previousLabelRect, value);
        previousLabelRect.x += GetSize(value.ToString() + "22").x;
    }

    void ContinueWithInput(ref Rect previousLabelRect, ref int value)
    {
        previousLabelRect.width += GetSize(value.ToString()).x;
        value = EditorGUI.IntField(previousLabelRect, value);
    }

    bool ContinueWithButton(ref Rect previousRect, string buttonContentn)
    {
        previousRect.width = GetSize(buttonContentn + "aa").x;
        if (GUI.Button(previousRect, buttonContentn))
            return true;
        previousRect.x += GetSize(buttonContentn + "aa").x;
        return false;
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

    public override void OnInspectorGUI()
    {
        controller.LeftTrackRacer = (Car)EditorGUILayout.ObjectField("Left track car", controller.LeftTrackRacer, typeof(Car), true);
        controller.RightTrackRacer = (Car)EditorGUILayout.ObjectField("Right track car", controller.RightTrackRacer, typeof(Car), true);
        var lastRect = GUILayoutUtility.GetLastRect();
        lastRect.y += lastRect.height + 5;
        EditorGUI.LabelField(lastRect, "Left track racer input ");
        ResetX(ref lastRect);
        ContinueWithLabel(ref lastRect, "Finish time");
        ContinueWithInput(ref lastRect, ref controller.LeftTrackFinishTime);
        ContinueWithLabel(ref lastRect, "ID");
        ContinueWithInput(ref lastRect, ref controller.LeftTrackRacerId);
        NewRow(ref lastRect);
        ContinueWithLabel(ref lastRect, "Right track racer input ");
        ResetX(ref lastRect);
        ContinueWithLabel(ref lastRect, "Finish time");
        ContinueWithInput(ref lastRect, ref controller.RightTrackFinishTime);
        ContinueWithLabel(ref lastRect, "ID");
        ContinueWithInput(ref lastRect, ref controller.RightTrackRacerId);
        NewRow(ref lastRect);
        ResetX(ref lastRect);
        lastRect.height += 3;

        if (ContinueWithButton(ref lastRect, "Record"))
        {
            Product.Utilities.Routines.CreateRoutine(controller.RecordRace(controller.LeftTrackFinishTime, controller.RightTrackFinishTime, controller.LeftTrackRacerId, controller.RightTrackRacerId), Product.Utilities.RoutineTypes.GLOBAL);
            var UiObj = GameObject.Find("_UI");
            if (UiObj != null)
                UiObj.SetActive(false);
        }

        if (ContinueWithButton(ref lastRect, "Play loaded"))
            controller.PlayLoadedReplay();

        ResetX(ref lastRect);
        NewRow(ref lastRect);
        ContinueWithLabel(ref lastRect, "Racers prefab directory");
        ContinueWithInput(ref lastRect, ref controller.RacersPrefabsDirectory);
        NewRow(ref lastRect);
        ContinueWithLabel(ref lastRect, "Racers manifest file name");
        ContinueWithInput(ref lastRect, ref controller.RacersManifestFileName);
        NewRow(ref lastRect);
        EditorGUILayout.LabelField("");
    }
}
