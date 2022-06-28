using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FixedWidthLabel : IDisposable {
   private readonly ZeroIndent indentReset; //helper class to reset and restore indentation
   public FixedWidthLabel(GUIContent label, Rect passedRect) {
#if UNITY_EDITOR
      //Debug.Log("5");
      EditorGUILayout.BeginHorizontal();
      passedRect.x += GUI.skin.label.CalcSize(label).x;
      passedRect.width = 180;
      EditorGUI.LabelField(passedRect, label);

      EditorGUILayout.LabelField("___)______________________",
         GUILayout.Width( (passedRect.x ) +// actual label width
           9 * UnityEditor.EditorGUI.indentLevel));//indentation from the left side. It's 9 pixels per indent level
      indentReset = new ZeroIndent();
#endif
   }

   public FixedWidthLabel(GUIContent label, float minusIndent) {
#if UNITY_EDITOR
      //Debug.Log("4" + " il " + EditorGUI.indentLevel);
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(label,
         GUILayout.Width(GUI.skin.label.CalcSize(label).x +// actual label width
           9 * UnityEditor.EditorGUI.indentLevel - minusIndent));//indentation from the left side. It's 9 pixels per indent level
      indentReset = new ZeroIndent();
#endif
   }

   public FixedWidthLabel(GUIContent label) {
#if UNITY_EDITOR
      //Debug.Log("4" + " il " + EditorGUI.indentLevel);
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(label,
         GUILayout.Width(GUI.skin.label.CalcSize(label).x +// actual label width
           9 * UnityEditor.EditorGUI.indentLevel));//indentation from the left side. It's 9 pixels per indent level
      indentReset = new ZeroIndent();
#endif
   }

   //public float FixedSlider(float value) {
   //   GUI.skin.horizontalSlider.cal
   //   return value;
   //}

   public FixedWidthLabel(string label, Rect passedInRect)
 : this(new GUIContent(label), passedInRect)//alternative constructor, if we don't want to deal with GUIContents

{
      //Debug.Log("3");
   }

   public FixedWidthLabel(string label)
      : this(new GUIContent(label))//alternative constructor, if we don't want to deal with GUIContents
   {
      //Debug.Log("2");
   }

   public FixedWidthLabel(string label, int minusNesta)
   : this(new GUIContent(label), minusNesta)//alternative constructor, if we don't want to deal with GUIContents
{
      //Debug.Log("2");
   }

   public void Dispose() //restore GUI state
   {
      //Debug.Log("1");
#if UNITY_EDITOR
      indentReset.Dispose();//restore indentation
      EditorGUILayout.EndHorizontal();//finish horizontal group
#endif
   }
}
