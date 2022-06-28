#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(UIElementsController))]
public class UIElementsControllerEditor : Editor
{
    UIElementsController controller;
    void OnEnable()
    {
        controller = (UIElementsController)target;
        if (controller.UiElements == null)
            controller.UiElements = new System.Collections.Generic.List<CustomUiElement>();

        var nElements = controller.gameObject.GetComponentsInChildren<CustomUiElement>();

        controller.UiElements = new System.Collections.Generic.List<CustomUiElement>();

        for (int i=0; i<nElements.Length; i++)
        {

            controller.UiElements.Add(nElements[i]);
        }

        var done = 0;
    }

    //public override void OnInspectorGUI()
    //{
    //}
}
#endif
