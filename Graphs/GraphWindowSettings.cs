using UnityEngine;

[System.Serializable]
public struct WindowColorSettings {
   public Color AxesColor;
   public Color GridColor;
   public Color GraphLineColor;
   public Color LabelsColor;
   public Color MainGraphPointsColor;
   public Color DragableRectsColor;
}

[System.Serializable]
public class GraphWindowSettings : MonoBehaviour {
   public string SettingsName;
   public float WindowWidth;
   public float WindowHeigth;
   [Range(0, 100)]
   public float TopRightXAxisPercent;
   [Range(0, 100)]
   public float TopRightYAxisPercent;
   [Range(0, 100)]
   public float BottomYAxis;
   [Range(0, 100)]
   public float BottomXAxis;
   [Range(0, 100)]
   public float XAxisDividerLengthPercent;
   [Range(0, 100)]
   public float YAxisDividerLengthPercent;
   public float XAxisDividerWidth;
   public float YAxisDividerWidth;
   public float GraphLineWidth;   public int XAxisDividers;
   public int YAxisDividers;
   public int DecimalPlaces;
   public int SelectedRectIndex;
   public Vector2 MainGraphPointsSize;
   public Vector2 MainGraphPointsContentOffset;
   public Rect[] DragableRects;
   public Vector2 DragableRectsSize;
   public Vector3[] AxesBorders;
   public bool ShowSettings;
   public bool ShowAllSettings;
   public bool DefaultGraphPosition;
   public bool DisplayMainPoints;
   public bool DisplayGrid;
   public bool Instantiated;
   public Rect LastRectDrawn;
   public WindowColorSettings ColorSettings;
   public Vector2 WindowSize;

   public void SetDefault() {
      TopRightXAxisPercent = 94f;
      TopRightYAxisPercent = 2f;
      BottomYAxis = 6f;
      BottomXAxis = 4f;
      XAxisDividers = 20;
      YAxisDividers = 20;
      XAxisDividerLengthPercent = 100;
      YAxisDividerLengthPercent = 100;
      XAxisDividerWidth = 1;
      YAxisDividerWidth = 1;
      GraphLineWidth = 5;
      DecimalPlaces = 2;
      MainGraphPointsSize = new Vector2(5, 5);
      DragableRectsSize = new Vector2(10, 10);
      SelectedRectIndex = -1;
      ShowSettings = false;
      ShowAllSettings = false;
      DefaultGraphPosition = true;
      DisplayMainPoints = true;
      DisplayGrid = true;
      ColorSettings.AxesColor.a = 1;
      ColorSettings.GridColor.a = 1;
      ColorSettings.GraphLineColor.a = 1;
      ColorSettings.LabelsColor.a = 1;
      ColorSettings.MainGraphPointsColor.a = 1;
      ColorSettings.DragableRectsColor.a = 1;
      ColorSettings.AxesColor = Color.black;
      ColorSettings.GridColor = Color.black;
      ColorSettings.GraphLineColor = Color.black;
      ColorSettings.LabelsColor = Color.black;
      ColorSettings.MainGraphPointsColor = Color.black;
      ColorSettings.DragableRectsColor = Color.black;
      AxesBorders = new Vector3[2];
      DragableRects = new Rect[2];
      Instantiated = true;
   }
}
