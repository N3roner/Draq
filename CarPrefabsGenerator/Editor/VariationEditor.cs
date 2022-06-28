using UnityEditor;

[CustomEditor(typeof(Variation))]
public class VariationEditor : Editor {

   Variation controller;

   public override void OnInspectorGUI() {

      EditorGUILayout.LabelField("Variation settings");
      DrawPartInputs(PartToDraw.SPOILER);
      DrawPartInputs(PartToDraw.HOOD);
      DrawPartInputs(PartToDraw.FRONTBUMPER);
      DrawPartInputs(PartToDraw.REARBUMPER);
      DrawPartInputs(PartToDraw.TIRES);
      DrawPartInputs(PartToDraw.RIDECOLOR);
      controller.ApplyChangesToMainEditor = EditorGUILayout.Toggle("Apply to changes to prefab generator", controller.ApplyChangesToMainEditor);
      DrawDefaultInspector();
   }

   void DrawPartInputs(PartToDraw partToDraw) {
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      if(partToDraw == PartToDraw.SPOILER)
         controller.VariationSetup.Spoiler.UsedSpoiler = (SpoilerValues)EditorGUILayout.EnumPopup("Spoiler", controller.VariationSetup.Spoiler.UsedSpoiler);
      if(partToDraw == PartToDraw.HOOD)
         controller.VariationSetup.Hood.UsedPart = (PartValues)EditorGUILayout.EnumPopup("Hood", controller.VariationSetup.Hood.UsedPart);
      if(partToDraw == PartToDraw.FRONTBUMPER)
         controller.VariationSetup.FrontBumper.UsedPart = (PartValues)EditorGUILayout.EnumPopup("Front bumper", controller.VariationSetup.FrontBumper.UsedPart);
      if(partToDraw == PartToDraw.REARBUMPER)
         controller.VariationSetup.RearBumper.UsedPart = (PartValues)EditorGUILayout.EnumPopup("Rear bumper", controller.VariationSetup.RearBumper.UsedPart);
      if(partToDraw == PartToDraw.TIRES)
         controller.VariationSetup.Tires.UsedPart = (PartValues)EditorGUILayout.EnumPopup("Tires", controller.VariationSetup.Tires.UsedPart);
      if(EditorGUI.EndChangeCheck())
         controller.EditCarPart(controller.VariationSetup, false, true);

      EditorGUI.BeginChangeCheck();
      if(partToDraw == PartToDraw.SPOILER)
         controller.VariationSetup.SpoilerColor = EditorGUILayout.ColorField("", controller.VariationSetup.SpoilerColor);
      if(partToDraw == PartToDraw.HOOD)
         controller.VariationSetup.HoodColor = EditorGUILayout.ColorField("", controller.VariationSetup.HoodColor);
      if(partToDraw == PartToDraw.FRONTBUMPER)
         controller.VariationSetup.FrontBumperColor = EditorGUILayout.ColorField("", controller.VariationSetup.FrontBumperColor);
      if(partToDraw == PartToDraw.REARBUMPER)
         controller.VariationSetup.RearBumperColor = EditorGUILayout.ColorField("", controller.VariationSetup.RearBumperColor);
      if(partToDraw == PartToDraw.RIDECOLOR)
         controller.VariationSetup.RideColor = EditorGUILayout.ColorField("Ride color ", controller.VariationSetup.RideColor);
      if(EditorGUI.EndChangeCheck())
         controller.EditCarPart(controller.VariationSetup, true, true);
      EditorGUILayout.EndHorizontal();
   }

   void OnEnable() {
      controller = (Variation)target;
   }
}
