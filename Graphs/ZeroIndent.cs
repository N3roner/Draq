#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class ZeroIndent {

   private readonly int originalIndent;//the original indentation value before we change the GUI state
   public ZeroIndent() {
#if UNITY_EDITOR
      originalIndent = EditorGUI.indentLevel;//save original indentation
      EditorGUI.indentLevel = 0;//clear indentation
#endif
   }

   public void Dispose() {
#if UNITY_EDITOR

      //Debug.Log("bla " + EditorGUI.indentLevel);
      EditorGUI.indentLevel = originalIndent;//restore original indentation
#endif
   }
}
