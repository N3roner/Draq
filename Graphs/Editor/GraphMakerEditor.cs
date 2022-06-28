using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GraphMaker))]
public class GraphMakerEditor : Editor {
   GraphMaker controller;
   int SelectedComponentIndex;

   public override void OnInspectorGUI() {
      EditorGUI.BeginChangeCheck();
      if(controller.ComponentsContainer != null)
         SelectedComponentIndex = EditorGUILayout.Popup("Selected graphable component ", SelectedComponentIndex, controller.ComponentsContainer);
      if(EditorGUI.EndChangeCheck())
         controller.UsedGraphableComponent = controller.IGraphableComponents[SelectedComponentIndex];

      EditorGUI.BeginChangeCheck();
      if(controller.SettingsContainer != null)
         controller.SelectedSettingsIndex = EditorGUILayout.Popup("Selected windows settings ", controller.SelectedSettingsIndex, controller.SettingsContainer);
      if(EditorGUI.EndChangeCheck())
         controller.UsedWindowSettings = controller.Settings[controller.SelectedSettingsIndex];

      EditorGUI.BeginChangeCheck();
      controller.ImportedSettings = (GraphWindowSettings)EditorGUILayout.ObjectField("Import settings", controller.ImportedSettings, typeof(GraphWindowSettings), true);
      if(EditorGUI.EndChangeCheck())
         controller.AddNewSettings();

      if(GUILayout.Button("Open graph window"))
         controller.OpenNewGraphWindow();
      if(GUILayout.Button("Remove selected settings"))
         controller.RemoveSelectedSettings();
   }

   void OnEnable() {
      controller = (GraphMaker)target;
      if(controller != null)
         controller.CheckComponents();
   }
}
