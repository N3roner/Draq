#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PrefabGeneratorWindow : EditorWindow
{
    CarPrefabsGenerator controller;
    Vector2 scrollPos;
    float globalLineCounter;
    bool displayListOfTires;
    bool displayListOfCars;
    bool displayScreenShotSection;
    bool displayOtherSettings;
    int lineHMultiplier;
    float colorPickerWidth = 45;
    float sLine;
    float lineH;
    float inputEnumsWidth;
    float lblWidth;
    int indentPixels = 20;
    bool displayListOfReferencedSettings;

    public void SetController(CarPrefabsGenerator passedController)
    {
        controller = passedController;
    }

    public static PrefabGeneratorWindow GetNewWindow()
    {
        PrefabGeneratorWindow newGraphWindow = CreateInstance<PrefabGeneratorWindow>();
        EditorUtility.SetDirty(newGraphWindow);
        GetWindow(typeof(PrefabGeneratorWindow));
        return newGraphWindow;
    }

    void OnGUI()
    {
        if (controller == null)
            return;

        Event e = Event.current;
        if (e.keyCode == KeyCode.Escape)
            controller.CloseCurrentWindow();

        lineHMultiplier = 9;
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, new Rect(0, 0, 1500, globalLineCounter));
        BeginWindows();
        DrawHeaderButtons();
        DrawListOfCars();
        DrawListOfReferencedAccSettings();
        DrawListOfTires();
        DrawScreenShootingSection();
        DrawOtherSettings();
        SetHeightsWidths();
        DrawSetupInputs();
        EndWindows();
        GUI.EndScrollView();
        globalLineCounter = lineH * lineHMultiplier;
        controller.MouseListener();
    }

    float GetTextWidth(string text)
    {
        return GUI.skin.label.CalcSize(new GUIContent(text + "1234567")).x;
    }

    void DrawHeaderButtons()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("One click", GUILayout.Width(GetTextWidth("One click"))))
            controller.OneClick();

        if (GUILayout.Button("Clear all ", GUILayout.Width(GetTextWidth("Clear all"))))
            controller.ClearAll();

        if (GUILayout.Button("Import all ", GUILayout.Width(GetTextWidth("Import all"))))
            controller.ImportAll();

        if (GUILayout.Button("Create prefabs manifest file", GUILayout.Width(GetTextWidth("Create prefabs manifest file"))))
            controller.CreatePrefabsManifestFile(controller.PrefabsManifestFileName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate variations and materials", GUILayout.Width(GetTextWidth("Generate variations and materials"))))
            controller.GenerateVariations();

        if (GUILayout.Button("Generate prefabs and take screenshot of each variation", GUILayout.Width(GetTextWidth("Generate prefabs and take screenshot of each variation"))))
            controller.GeneratePrefabs();
        GUILayout.EndHorizontal();
    }

    void DrawScreenShootingSection()
    {
        displayScreenShotSection = EditorGUILayout.Foldout(displayScreenShotSection, "Screen shooting settings");
        if (displayScreenShotSection)
        {
            GUILayout.Label("Save Path", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.TextField(controller.ScreenShotsPath, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                controller.ScreenShotsPath = EditorUtility.SaveFolderPanel("Path to Save Images", controller.ScreenShotsPath, Application.dataPath);
            controller.SSTaker.ResWidth = EditorGUILayout.IntField("Resolution width", controller.SSTaker.ResWidth);
            controller.SSTaker.ResHeight = EditorGUILayout.IntField("Resolution height", controller.SSTaker.ResHeight);
            EditorGUILayout.EndHorizontal();
            controller.ScreenShotExtension = EditorGUILayout.TextField("Screen shot file extension ", controller.ScreenShotExtension);
            controller.SSTaker.UsedCam = (Camera)EditorGUILayout.ObjectField("Cam used for screen shooting", controller.SSTaker.UsedCam, typeof(Camera), true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("- List of postures");
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < controller.SSTaker.SSObjects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                controller.SSTaker.SSObjects[i] = (GameObject)EditorGUILayout.ObjectField("Posture " + (1 + i), controller.SSTaker.SSObjects[i], typeof(GameObject), true);
                GUILayout.EndHorizontal();
            }
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            controller.SSObj = (GameObject)EditorGUILayout.ObjectField("Import example posture ", controller.SSObj, typeof(GameObject), true);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                controller.SSTaker.SSObjects.Add(controller.SSObj);
                controller.SSObj = null;
            }

            if (GUILayout.Button("Clear postures", GUILayout.Width(GetTextWidth("Clear postures"))))
                controller.SSTaker.SSObjects = new List<GameObject>();
            lineHMultiplier += (7 + controller.SSTaker.SSObjects.Count);
        }
    }

    void DrawListOfCars()
    {
        displayListOfCars = EditorGUILayout.Foldout(displayListOfCars, "Display list of cars");
        if (displayListOfCars)
        {
            for (int i = 0; i < controller.Cars.Count; i++)
            {
                var removeCar = DrawIndentedField(controller.Cars[i], "Car " + (i + 1), indentPixels, true);
                if (removeCar)
                    controller.Cars.RemoveAt(i);
            }

            EditorGUI.BeginChangeCheck();
            controller.ImportedCar = (GameObject)EditorGUILayout.ObjectField("Add new car : ", controller.ImportedCar, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
                controller.ImportNewCar();
            EditorGUILayout.Space();
            if (GUILayout.Button("Import all cars from : " + controller.AvailableCarPrefabsPath, GUILayout.Width(GetTextWidth("Import all cars from :" + controller.AvailableCarPrefabsPath))))
                controller.ImportAllCars();

            if (GUILayout.Button("Clear list of cars", GUILayout.Width(GetTextWidth("Clear list of cars"))))
                controller.Cars = new List<GameObject>();
            lineHMultiplier += controller.Cars.Count + 4;
            EditorGUILayout.Space();
        }
    }

    void DrawListOfReferencedAccSettings()
    {
        displayListOfReferencedSettings = EditorGUILayout.Foldout(displayListOfReferencedSettings, "Display list of referenced acceleration settings");
        if (displayListOfReferencedSettings)
        {
            for (int i = 0; i < controller.ReferencedAccelerationSettings.Count; i++)
            {
                var removeRefCar = DrawIndentedField(controller.ReferencedAccelerationSettings[i], "Referenced car " + (i + 1), indentPixels);
                if (removeRefCar)
                    controller.ReferencedAccelerationSettings.RemoveAt(i);
            }
            EditorGUI.BeginChangeCheck();
            controller.ReferenceCar = (Car)EditorGUILayout.ObjectField("Add new settings : ", controller.ReferenceCar, typeof(Car), true);
            if (EditorGUI.EndChangeCheck())
                controller.ImportNewReferenceCar();

            EditorGUILayout.Space();
            var btnContentIARC = "Import all reference cars from : " + controller.AvailableReferenceCarsPath;
            if (GUILayout.Button(btnContentIARC, GUILayout.Width(GetTextWidth(btnContentIARC))))
                controller.ImportAllReferenceCars();

            var btnContentCRF = "Clear list of referenced cars";
            if (GUILayout.Button(btnContentCRF, GUILayout.Width(GetTextWidth(btnContentCRF))))
                controller.ReferencedAccelerationSettings = new List<Car>();
            lineHMultiplier += controller.ReferencedAccelerationSettings.Count + 4;
            EditorGUILayout.Space();
        }
    }

    void DrawListOfTires()
    {
        displayListOfTires = EditorGUILayout.Foldout(displayListOfTires, "Display list of tires");
        if (displayListOfTires)
        {
            for (int i = 0; i < controller.Tires.Count; i++)
            {
                var tireRemove = DrawIndentedField(controller.Tires[i], ("Tire : " + (i + 1)), indentPixels, true);
                if (tireRemove)
                    controller.Tires.RemoveAt(i);
            }
            EditorGUI.BeginChangeCheck();
            controller.ImportedTire = (GameObject)EditorGUILayout.ObjectField("Import tire : ", controller.ImportedTire, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
                controller.Tires.Add(controller.ImportedTire);
            controller.ImportedTire = null;
            EditorGUILayout.Space();
            string btnContenIAT = "Import all tires from : " + controller.AvailableTires;
            if (GUILayout.Button(btnContenIAT, GUILayout.Width(GetTextWidth(btnContenIAT))))
                controller.ImportAllTires();

            var btnContentCLT = "Clear list of tires";
            if (GUILayout.Button(btnContentCLT, GUILayout.Width(GetTextWidth(btnContentCLT))))
                controller.Tires = new List<GameObject>();

            lineHMultiplier += controller.Tires.Count + 4;
            EditorGUILayout.Space();
        }
    }

    void DrawIndentedField(ref float value, string labelContent, int indentPixels)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentPixels);
        value = EditorGUILayout.FloatField(labelContent, value);
        GUILayout.EndHorizontal();
    }

    void DrawIndentedField(ref string value, string labelContent, int indentPixels)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentPixels);
        value = EditorGUILayout.TextField(labelContent, value);
        GUILayout.EndHorizontal();
    }
    bool DrawIndentedField(Car passedCar, string labelContent, int indentPixels)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentPixels);
        if (passedCar)
            passedCar.name = EditorGUILayout.TextField(labelContent, passedCar.name);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.ToggleLeft("-", false);
        if (EditorGUI.EndChangeCheck())
        {
            GUILayout.EndHorizontal();
            return true;
        }
        GUILayout.EndHorizontal();
        return false;
    }

    bool DrawIndentedField(GameObject value, string labelContent, int indentPixels, bool btnToo = false)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentPixels);
        value = (GameObject)EditorGUILayout.ObjectField(labelContent, value, typeof(GameObject), true);
        if (btnToo)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.ToggleLeft("-", false);
            if (EditorGUI.EndChangeCheck())
            {
                GUILayout.EndHorizontal();
                return true;
            }
        }
        GUILayout.EndHorizontal();
        return false;
    }

    void DrawIndentedField(ref int value, string labelContent, int indentPixels)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentPixels);
        value = EditorGUILayout.IntField(labelContent, value);
        GUILayout.EndHorizontal();
    }

    void DrawOtherSettings()
    {
        displayOtherSettings = EditorGUILayout.Foldout(displayOtherSettings, "Other settings");
        if (displayOtherSettings)
        {
            EditorGUILayout.LabelField("- Paths ");
            DrawIndentedField(ref controller.AvailableMaterialsPath, "Available materials ", indentPixels);
            DrawIndentedField(ref controller.AvailableCarPrefabsPath, "Available prefabs ", indentPixels);
            DrawIndentedField(ref controller.PrefabsDirectory, "Prefabs and materials folder ", indentPixels);
            DrawIndentedField(ref controller.TireMaterialsPath, "Tire materials folder ", indentPixels);
            DrawIndentedField(ref controller.PrefabsManifestFileName, "Prefabs manifest file name ", indentPixels);
            EditorGUILayout.LabelField("- UI elements width");
            DrawIndentedField(ref controller.WidthEditor, "Dropdown buttons width ", indentPixels);
            DrawIndentedField(ref controller.ColorPickersWidth, "Color pickers width ", indentPixels);
            DrawIndentedField(ref controller.MaterialPickerWidth, "Material pickers width ", indentPixels);
            DrawIndentedField(ref indentPixels, "Indent pixels", indentPixels);
            EditorGUILayout.LabelField("- Variation position");
            EditorGUI.BeginChangeCheck();
            DrawIndentedField(ref controller.XAxesOffset, "X axes offset", indentPixels);
            DrawIndentedField(ref controller.ZAxesOffset, "Z axes offset", indentPixels);
            if (EditorGUI.EndChangeCheck())
                controller.EditVariationsPosition();
            lineHMultiplier += 13;
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        controller.NumberOfClasses = EditorGUILayout.IntField("Number of classes", controller.NumberOfClasses);
        if (EditorGUI.EndChangeCheck())
        {
            if (controller.NumberOfClasses <= 0)
                controller.NumberOfClasses = 1;
        }
    }

    void DrawHeaderLabels(int currentIndex)
    {
        EditorGUI.LabelField(GetHeadersRect(1), "Spoiler");
        EditorGUI.LabelField(GetHeadersRect(2), "Hood");
        EditorGUI.LabelField(GetHeadersRect(3), "Front bumper");
        EditorGUI.LabelField(GetHeadersRect(4), "Rear bumper");
        EditorGUI.LabelField(GetHeadersRect(5), "Tires");
        var rideColorRect = GetHeadersRect(6);
        rideColorRect.x = GetPickerRect(6).x;
        EditorGUI.LabelField(rideColorRect, "Ride color");
        var decalsRect = GetHeadersRect(7);
        decalsRect.x = GetPickerRect(6, "Ride color __").x;
        EditorGUI.LabelField(decalsRect, "Decal");
        if (currentIndex == 0)
        {
            decalsRect.x += 40;
            EditorGUI.BeginChangeCheck();
            EditorGUI.Toggle(decalsRect, false);
            if (EditorGUI.EndChangeCheck())
                ShuffleDecals();
            decalsRect.x += 10;
            EditorGUI.LabelField(decalsRect, " - Shuffle");
        }
        lineHMultiplier++;
    }

    void ShuffleDecals()
    {
        for (int i = 0; i < controller.NumberOfClasses; i++)
            for (int j = 0; j < controller.Classes[i].NumberOfVariations; j++)
            {
                var index = Random.Range(0, 4);
                controller.Classes[i].Setups[j].UsedDecal = (Decals)index;
            }
    }

    Rect GetHeadersRect(int columnIndex)
    {
        return new Rect(columnIndex * inputEnumsWidth, lineH * lineHMultiplier, inputEnumsWidth, sLine);
    }

    Rect GetEnumRect(int columnIndex)
    {
        return new Rect(columnIndex * inputEnumsWidth, lineH * lineHMultiplier, inputEnumsWidth / 2, sLine);
    }

    Rect GetPickerRect(int columnIndex)
    {
        return new Rect(columnIndex * inputEnumsWidth + inputEnumsWidth / 2 + 10, lineH * lineHMultiplier, colorPickerWidth, sLine);
    }

    Rect GetPickerRect(int columnIndex, string txtXMover)
    {
        return new Rect((columnIndex * inputEnumsWidth + inputEnumsWidth / 2 + 10) + GUI.skin.label.CalcSize(new GUIContent(txtXMover)).x, lineH * lineHMultiplier, colorPickerWidth, sLine);
    }

    void DrawInputEnums(int classIndex, int variationIndex)
    {
        EditorGUI.BeginChangeCheck();
        var tempSetup = controller.Classes[classIndex].Setups[variationIndex];
        if (tempSetup.Spoiler == null)
            tempSetup.Initializer();

        tempSetup.Spoiler.UsedSpoiler = (SpoilerValues)EditorGUI.EnumPopup(GetEnumRect(1), tempSetup.Spoiler.UsedSpoiler);
        tempSetup.Hood.UsedPart = (PartValues)EditorGUI.EnumPopup(GetEnumRect(2), tempSetup.Hood.UsedPart);
        tempSetup.FrontBumper.UsedPart = (PartValues)EditorGUI.EnumPopup(GetEnumRect(3), tempSetup.FrontBumper.UsedPart);
        tempSetup.RearBumper.UsedPart = (PartValues)EditorGUI.EnumPopup(GetEnumRect(4), tempSetup.RearBumper.UsedPart);
        tempSetup.Tires.UsedPart = (PartValues)EditorGUI.EnumPopup(GetEnumRect(5), tempSetup.Tires.UsedPart);
        tempSetup.UsedDecal = (Decals)EditorGUI.EnumPopup(GetPickerRect(6, "Ride color __"), tempSetup.UsedDecal);
        if (EditorGUI.EndChangeCheck())
            controller.EditVariation(classIndex, variationIndex);
    }

    void DrawColorPickers(int classIndex, int variationIndex)
    {
        EditorGUI.BeginChangeCheck();
        var tempSetup = controller.Classes[classIndex].Setups[variationIndex];
        tempSetup.SpoilerColor = EditorGUI.ColorField(GetPickerRect(1), tempSetup.SpoilerColor);
        tempSetup.HoodColor = EditorGUI.ColorField(GetPickerRect(2), tempSetup.HoodColor);
        tempSetup.FrontBumperColor = EditorGUI.ColorField(GetPickerRect(3), tempSetup.FrontBumperColor);
        tempSetup.RearBumperColor = EditorGUI.ColorField(GetPickerRect(4), tempSetup.RearBumperColor);
        tempSetup.TiresColor = EditorGUI.ColorField(GetPickerRect(5), tempSetup.TiresColor);
        tempSetup.RideColor = EditorGUI.ColorField(GetPickerRect(6), tempSetup.RideColor);
        if (EditorGUI.EndChangeCheck())
            controller.EditVariation(classIndex, variationIndex, true);
    }

    void DrawSetupInputs()
    {
        if (controller.Classes.Count != controller.NumberOfClasses)
            controller.EditTimeClassesList();
        else
        {
            for (int i = 0; i < controller.NumberOfClasses; i++)
            {
                controller.Classes[i].ClassTime = EditorGUI.IntField(new Rect(GUILayoutUtility.GetLastRect().x, lineHMultiplier * lineH, lblWidth * 2, sLine), "Class " + (i + 1) + " time ", controller.Classes[i].ClassTime);
                lineHMultiplier++;
                EditorGUI.BeginChangeCheck();
                controller.Classes[i].NumberOfVariations = EditorGUI.IntField(new Rect(GUILayoutUtility.GetLastRect().x, lineHMultiplier * lineH, lblWidth * 2, sLine), "Number of variations ", controller.Classes[i].NumberOfVariations);
                if (EditorGUI.EndChangeCheck())
                    controller.EditNumberOfVariations(i);

                lineHMultiplier++;
                controller.EditTimeClassSetups(i);
                DrawHeaderLabels(i);
                for (int j = 0; j < controller.Classes[i].NumberOfVariations; j++)
                {
                    EditorGUI.LabelField(new Rect(2, lineH * lineHMultiplier, inputEnumsWidth, 40), "Variation " + (j + 1));
                    DrawInputEnums(i, j);
                    DrawColorPickers(i, j);
                    lineHMultiplier++;
                }
                lineHMultiplier++;
            }
        }
    }

    void SetHeightsWidths()
    {
        sLine = EditorGUIUtility.singleLineHeight;
        lineH = EditorGUIUtility.singleLineHeight + 3f;
        lblWidth = EditorGUIUtility.labelWidth;
        inputEnumsWidth = EditorGUIUtility.labelWidth + controller.WidthEditor;
        if (controller.ColorPickersWidth != 0)
            colorPickerWidth = controller.ColorPickersWidth;
        else
            colorPickerWidth = 45;
    }
}
#endif
