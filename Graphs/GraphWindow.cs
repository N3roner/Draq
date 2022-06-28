#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


#if UNITY_EDITOR
public class GraphWindow : EditorWindow {
   Vector2[] GraphPoints;
   Vector2[] MainGraphPoints;
   GraphWindowSettings windowSettings;
   bool showColorSettings;
   bool showMainPointsSettings;
   bool showGraphLineSettings;
   bool showAxesSettings;
   bool showGridSettings;

   public void OnGUI() {
      if(windowSettings == null)
         Close();

      if(GUILayout.Button((windowSettings.ShowSettings ? "Hide" : "Show") + " settings"))
         windowSettings.ShowSettings = !windowSettings.ShowSettings;

      if(windowSettings.ShowSettings) {
         if(GUILayout.Button("Load default settings"))
            LoadDefaultSettings();

         EditorGUILayout.BeginHorizontal();
         windowSettings.SettingsName = EditorGUILayout.TextField("Name : ", windowSettings.SettingsName);
         windowSettings.DefaultGraphPosition = EditorGUILayout.Toggle("Keep default graph position ", windowSettings.DefaultGraphPosition);
         windowSettings.ShowAllSettings = EditorGUILayout.Toggle("Show all settings", windowSettings.ShowAllSettings);
         EditorGUILayout.EndHorizontal();

         showColorSettings = EditorGUILayout.Foldout(showColorSettings, "Color settings");
         if(showColorSettings || windowSettings.ShowAllSettings) {
            EditorGUILayout.BeginHorizontal();
            windowSettings.ColorSettings.AxesColor = EditorGUILayout.ColorField(new GUIContent("Axes color :"), windowSettings.ColorSettings.AxesColor);
            windowSettings.ColorSettings.GridColor = EditorGUILayout.ColorField(new GUIContent("Grid color :"), windowSettings.ColorSettings.GridColor);
            windowSettings.ColorSettings.LabelsColor = EditorGUILayout.ColorField(new GUIContent("Labels color :"), windowSettings.ColorSettings.LabelsColor);
            windowSettings.ColorSettings.GraphLineColor = EditorGUILayout.ColorField(new GUIContent("Graph line color :"), windowSettings.ColorSettings.GraphLineColor);
            windowSettings.ColorSettings.MainGraphPointsColor = EditorGUILayout.ColorField("Main graph points color", windowSettings.ColorSettings.MainGraphPointsColor);
            windowSettings.ColorSettings.DragableRectsColor = EditorGUILayout.ColorField("Dragable rects color", windowSettings.ColorSettings.DragableRectsColor);
            EditorGUILayout.EndHorizontal();
         }

         showMainPointsSettings = EditorGUILayout.Foldout(showMainPointsSettings, "Main graph points settings");
         if(showMainPointsSettings || windowSettings.ShowAllSettings) {
            EditorGUILayout.BeginHorizontal();
            windowSettings.DisplayMainPoints = EditorGUILayout.Toggle("Display main graph points ", windowSettings.DisplayMainPoints);
            windowSettings.MainGraphPointsSize = EditorGUILayout.Vector2Field("Main graph points size ", windowSettings.MainGraphPointsSize);
            windowSettings.MainGraphPointsContentOffset = EditorGUILayout.Vector2Field("Main graph points content offset ", windowSettings.MainGraphPointsContentOffset);
            windowSettings.DragableRectsSize = EditorGUILayout.Vector2Field("Dragable size : ", windowSettings.DragableRectsSize);
            EditorGUILayout.EndHorizontal();
         }

         showGraphLineSettings = EditorGUILayout.Foldout(showGraphLineSettings, "Graph line settings ");
         if(showGraphLineSettings || windowSettings.ShowAllSettings) {
            EditorGUILayout.BeginHorizontal();
            windowSettings.GraphLineWidth = EditorGUILayout.FloatField(" graph line width ", windowSettings.GraphLineWidth);
            windowSettings.ColorSettings.GraphLineColor = EditorGUILayout.ColorField(new GUIContent("Graph line color :"), windowSettings.ColorSettings.GraphLineColor);
            EditorGUILayout.EndHorizontal();
         }

         showAxesSettings = EditorGUILayout.Foldout(showAxesSettings, "Axes settings ");
         if(showAxesSettings || windowSettings.ShowAllSettings) {
            EditorGUILayout.BeginHorizontal();
            windowSettings.TopRightXAxisPercent = EditorGUILayout.FloatField("Top X axis perc ", windowSettings.TopRightXAxisPercent);
            windowSettings.TopRightYAxisPercent = EditorGUILayout.FloatField("Top Y axis perc ", windowSettings.TopRightYAxisPercent);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            windowSettings.BottomYAxis = EditorGUILayout.FloatField("Bottom Y Axis ", windowSettings.BottomYAxis);
            windowSettings.BottomXAxis = EditorGUILayout.FloatField("Bottom X Axis ", windowSettings.BottomXAxis);
            EditorGUILayout.EndHorizontal();
         }

         showGridSettings = EditorGUILayout.Foldout(showGridSettings, "Grid settings ");
         if(showGridSettings || windowSettings.ShowAllSettings) {
            EditorGUILayout.BeginHorizontal();
            windowSettings.DisplayGrid = EditorGUILayout.Toggle("Display grid ", windowSettings.DisplayGrid);
            windowSettings.DecimalPlaces = EditorGUILayout.IntField("Content decimals ", windowSettings.DecimalPlaces);
            windowSettings.XAxisDividers = EditorGUILayout.IntField("X axis dividers ", windowSettings.XAxisDividers);
            windowSettings.YAxisDividers = EditorGUILayout.IntField("Y axis dividers ", windowSettings.YAxisDividers);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            windowSettings.XAxisDividerLengthPercent = EditorGUILayout.FloatField("X axis divider length", windowSettings.XAxisDividerLengthPercent);
            windowSettings.YAxisDividerLengthPercent = EditorGUILayout.FloatField("Y axis divider length", windowSettings.YAxisDividerLengthPercent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            windowSettings.XAxisDividerWidth = EditorGUILayout.FloatField("X axis divider width ", windowSettings.XAxisDividerWidth);
            windowSettings.YAxisDividerWidth = EditorGUILayout.FloatField("Y axis divider width ", windowSettings.YAxisDividerWidth);
            EditorGUILayout.EndHorizontal();
         }
      }

      GUI.contentColor = windowSettings.ColorSettings.LabelsColor;
      windowSettings.LastRectDrawn = GUILayoutUtility.GetLastRect();
      if(Event.current.type == EventType.repaint) {
         DrawAxes();
         DrawGrid();
         DrawGraphPoints(windowSettings.ColorSettings.GraphLineColor);
         DrawDragablePoints();
      }
      Event evt = Event.current;
      PickRect(evt);

      if(WindowResized(position) && !windowSettings.DefaultGraphPosition)
         AdjustAxes();
   }

   public void InitializeGraphPoints(Vector2[] graphPoints, Vector2[] mainGraphPoints) {
      GraphPoints = graphPoints;
      MainGraphPoints = mainGraphPoints;
        OnGUI();
   }

   public static GraphWindow GetNewWindow(GraphWindowSettings thisWindowSettings) {
      GraphWindow newGraphWindow = CreateInstance<GraphWindow>();
      EditorUtility.SetDirty(newGraphWindow);
      GetWindow(typeof(GraphWindow));
      newGraphWindow.windowSettings = thisWindowSettings;
      return newGraphWindow;
   }

   void DrawGraphPoints(Color dotsColor) {
      float floatingX;
      float floatingY;

      float maxX = GraphPoints[0].x;
      float maxY = GraphPoints[0].y;

      for(int i = 0; i < GraphPoints.Length; i++) {
         if(GraphPoints[i].x > maxX)
            maxX = GraphPoints[i].x;
         if(GraphPoints[i].y > maxY)
            maxY = GraphPoints[i].y;
      }

      windowSettings.WindowWidth = position.width;
      windowSettings.WindowHeigth = position.height;

      var xAxisLength = windowSettings.AxesBorders[1].x - windowSettings.AxesBorders[0].x;
      var yAxisLength = windowSettings.AxesBorders[0].y - windowSettings.AxesBorders[1].y;

      floatingX = xAxisLength / maxX;
      floatingY = yAxisLength / maxY;

      for(int i = 0; i < GraphPoints.Length; i++) {
         if(i < GraphPoints.Length - 1) {
            Vector3[] points = new Vector3[2];
            points[0] = new Vector3(windowSettings.AxesBorders[0].x + GraphPoints[i].x * floatingX, windowSettings.AxesBorders[0].y - GraphPoints[i].y * floatingY, 0);
            points[1] = new Vector3(windowSettings.AxesBorders[0].x + GraphPoints[i + 1].x * floatingX, windowSettings.AxesBorders[0].y - GraphPoints[i + 1].y * floatingY, 0);
            Handles.color = dotsColor;
            Handles.DrawAAPolyLine(windowSettings.GraphLineWidth, points);
         }
      }

      if(windowSettings.DisplayMainPoints && MainGraphPoints != null) {
         for(int i = 0; i < MainGraphPoints.Length; i++) {
            var labelContent = "s :" + MainGraphPoints[i].y + "\n" + "t :" + MainGraphPoints[i].x;
            var labelSize = GUI.skin.label.CalcSize(new GUIContent(labelContent));

            Vector3 gearShiftingPos = new Vector3();
            gearShiftingPos = new Vector3(windowSettings.AxesBorders[0].x + MainGraphPoints[i].x * floatingX, windowSettings.AxesBorders[0].y - MainGraphPoints[i].y * floatingY, 0f);
            var mainPointRect = new Rect(gearShiftingPos.x - windowSettings.MainGraphPointsSize.x / 2, gearShiftingPos.y - windowSettings.MainGraphPointsSize.y / 2, windowSettings.MainGraphPointsSize.x, windowSettings.MainGraphPointsSize.y);
            Handles.DrawSolidRectangleWithOutline(mainPointRect, windowSettings.ColorSettings.MainGraphPointsColor, windowSettings.ColorSettings.MainGraphPointsColor);
            GUI.Label(new Rect(gearShiftingPos.x + windowSettings.MainGraphPointsContentOffset.x, gearShiftingPos.y + windowSettings.MainGraphPointsContentOffset.y, labelSize.x, labelSize.y), labelContent);
         }
      }
   }

   void AdjustAxes() {
      for(int i = 0; i < windowSettings.DragableRects.Length; i++) {
         var widthRatio = windowSettings.AxesBorders[i].x / windowSettings.WindowSize.x;
         windowSettings.DragableRects[i].x = position.width * widthRatio;

         var heightRatio = windowSettings.AxesBorders[i].y / windowSettings.WindowSize.y;
         windowSettings.DragableRects[i].y = position.height * heightRatio;

         windowSettings.AxesBorders[i] = windowSettings.DragableRects[i].position;
      }
      windowSettings.WindowSize.x = position.width;
      windowSettings.WindowSize.y = position.height;
   }

   bool WindowResized(Rect window) {
      if(windowSettings.WindowSize.x != window.width)
         return true;
      if(windowSettings.WindowSize.y != position.height)
         return true;
      return false;
   }

   void LoadDefaultSettings() {
      windowSettings.SetDefault();
   }

   void PickRect(Event evt) {
      if(windowSettings == null || windowSettings.DragableRects == null)
         return;
      for(int i = 0; i < windowSettings.DragableRects.Length; i++) {
         if(PointInside(evt.mousePosition, windowSettings.DragableRects[i])) {
            if(evt.type == EventType.MouseDown)
               windowSettings.SelectedRectIndex = i;
         }

         if(evt.type == EventType.MouseUp)
            windowSettings.SelectedRectIndex = -1;

         if(windowSettings.SelectedRectIndex >= 0 && evt.type == EventType.MouseDrag) {
            windowSettings.DefaultGraphPosition = false;
            windowSettings.DragableRects[windowSettings.SelectedRectIndex].x = Mathf.Clamp(evt.mousePosition.x, 0, position.width - windowSettings.DragableRects[windowSettings.SelectedRectIndex].width);
            windowSettings.DragableRects[windowSettings.SelectedRectIndex].y = Mathf.Clamp(evt.mousePosition.y, 0, position.height - windowSettings.DragableRects[windowSettings.SelectedRectIndex].height);
            Repaint();
         }
      }
   }

   bool PointInside(Vector3 point, Rect Position) {
      if(point.x < Position.x || point.x > Position.x + Position.width)
         return false;
      if(point.y < Position.y || point.y > Position.y + Position.height)
         return false;
      return true;
   }

   void DrawDragablePoints() {
      Handles.color = windowSettings.ColorSettings.DragableRectsColor;
      for(int i = 0; i < windowSettings.DragableRects.Length; i++) {
         windowSettings.DragableRects[i] = new Rect(windowSettings.AxesBorders[i].x, windowSettings.AxesBorders[i].y, windowSettings.DragableRectsSize.x, windowSettings.DragableRectsSize.y);
         if(i == 0)
            Handles.DrawSolidRectangleWithOutline(windowSettings.DragableRects[i], windowSettings.ColorSettings.DragableRectsColor, windowSettings.ColorSettings.DragableRectsColor);
         else
            Handles.DrawSolidRectangleWithOutline(windowSettings.DragableRects[i], windowSettings.ColorSettings.DragableRectsColor, windowSettings.ColorSettings.DragableRectsColor);
      }
   }

   void SetWindowPoints() {
      windowSettings.AxesBorders[0] = windowSettings.DragableRects[0].position;
      windowSettings.AxesBorders[1] = windowSettings.DragableRects[1].position;
      windowSettings.WindowSize = new Vector2(windowSettings.WindowWidth, windowSettings.WindowHeigth);
   }

   void DrawAxes() {
      windowSettings.WindowWidth = position.width;
      windowSettings.WindowHeigth = position.height;

      var consumedSpace = windowSettings.LastRectDrawn;
      consumedSpace.y += 2 * GUILayoutUtility.GetLastRect().height;
      var maxGraphHeight = windowSettings.WindowHeigth - consumedSpace.y;

      if(windowSettings.DefaultGraphPosition) {
         windowSettings.AxesBorders[0] = new Vector3((windowSettings.WindowWidth * windowSettings.BottomXAxis) / 100, windowSettings.WindowHeigth - (maxGraphHeight * windowSettings.BottomYAxis) / 100, 0f);
         var totalY = windowSettings.WindowHeigth - consumedSpace.y - (windowSettings.WindowHeigth - windowSettings.AxesBorders[0].y);
         windowSettings.AxesBorders[1] = new Vector3((windowSettings.WindowWidth * windowSettings.TopRightXAxisPercent) / 100, consumedSpace.y + (totalY * windowSettings.TopRightYAxisPercent) / 100, 0);

         windowSettings.DragableRects[0].position = windowSettings.AxesBorders[0];
         windowSettings.DragableRects[1].position = windowSettings.AxesBorders[1];
      }
      SetWindowPoints();

      Vector3[] yAxisPoints = new Vector3[2];
      yAxisPoints[0] = new Vector3(windowSettings.AxesBorders[0].x, windowSettings.AxesBorders[0].y, 0f);
      yAxisPoints[1] = new Vector3(windowSettings.AxesBorders[0].x, windowSettings.AxesBorders[1].y, 0f);
      Handles.DrawAAPolyLine(yAxisPoints);

      Vector3[] xAxisPoints = new Vector3[2];
      xAxisPoints[0] = new Vector3(windowSettings.AxesBorders[0].x, windowSettings.AxesBorders[0].y, 0f);
      xAxisPoints[1] = new Vector3(windowSettings.AxesBorders[1].x, windowSettings.AxesBorders[0].y, 0f);
      Handles.DrawAAPolyLine(xAxisPoints);
   }

   void DrawGrid() {
      if(!windowSettings.DisplayGrid)
         return;
      string stringFormat = "0.";
      for(int i = 0; i < windowSettings.DecimalPlaces; i++) {
         if(windowSettings.DecimalPlaces > 0)
            stringFormat += "#";
      }

      var xAxisLength = windowSettings.AxesBorders[1].x - windowSettings.AxesBorders[0].x;
      var yAxisLength = windowSettings.AxesBorders[0].y - windowSettings.AxesBorders[1].y;

      var xDividerMultiplier = Mathf.Abs(xAxisLength / windowSettings.XAxisDividers);
      var xAxisDividersHeight = Mathf.Abs(yAxisLength * windowSettings.XAxisDividerLengthPercent / 100f);
      var xLabelMultiplier = (GraphPoints[GraphPoints.Length - 1].x / windowSettings.XAxisDividers);

      Handles.color = windowSettings.ColorSettings.GridColor;
      Vector3[] xAxisDividers = new Vector3[2];

      for(int i = 1; i <= windowSettings.XAxisDividers; i++) {
         xAxisDividers[0] = new Vector3((windowSettings.AxesBorders[0].x + (xDividerMultiplier * i)), windowSettings.AxesBorders[0].y, 0f);
         xAxisDividers[1] = new Vector3((xAxisDividers[0].x), xAxisDividers[0].y - xAxisDividersHeight, 0f);
         Handles.DrawAAPolyLine(windowSettings.XAxisDividerWidth, xAxisDividers);

         Vector2 xLabelPos = new Vector2(xAxisDividers[0].x, xAxisDividers[0].y);
         var xAxisLabelContent = (xLabelMultiplier * i).ToString(stringFormat);
         var xAxisLabelSize = GUI.skin.label.CalcSize(new GUIContent(xAxisLabelContent));
         GUI.Label(new Rect(xLabelPos.x - xAxisLabelSize.x / 2, xLabelPos.y, xAxisLabelSize.x, xAxisLabelSize.y), xAxisLabelContent);
      }

      var yAxisDividerHeight = Mathf.Abs(xAxisLength * windowSettings.YAxisDividerLengthPercent / 100f);
      var yDividerMultiplier = Mathf.Abs(yAxisLength / windowSettings.YAxisDividers);
      var yLabelMultiplier = (GraphPoints[GraphPoints.Length - 1].y / windowSettings.YAxisDividers);

      Vector3[] yAxisDividerPos = new Vector3[2];
      for(int i = 1; i <= windowSettings.YAxisDividers; i++) {
         yAxisDividerPos[0] = new Vector3(windowSettings.AxesBorders[0].x, windowSettings.AxesBorders[0].y - (yDividerMultiplier * i), 0f);
         yAxisDividerPos[1] = new Vector3(yAxisDividerPos[0].x + yAxisDividerHeight, yAxisDividerPos[0].y, 0f);
         Handles.DrawAAPolyLine(windowSettings.YAxisDividerWidth, yAxisDividerPos);

         Vector2 yLabelPos = new Vector2(yAxisDividerPos[0].x, yAxisDividerPos[0].y);
         var yAxisLabelContent = (yLabelMultiplier * i).ToString(stringFormat);
         var yAxisLabelSize = GUI.skin.label.CalcSize(new GUIContent(yAxisLabelContent));
         GUI.Label(new Rect(yLabelPos.x - yAxisLabelSize.x, yLabelPos.y - yAxisLabelSize.y / 2, yAxisLabelSize.x, yAxisLabelSize.y), yAxisLabelContent);
      }
   }
}
#endif
