using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CarPrefabsGenerator))]
public class CarPrefabsGeneratorEditor : Editor {
   CarPrefabsGenerator controller;

   public override void OnInspectorGUI() {
      if(GUILayout.Button("Open generator"))
         controller.GetNewGeneratorWindow();
      DrawDefaultInspector();
   }

   void OnEnable() {
      controller = (CarPrefabsGenerator)target;
      controller.Init();
   }
}
