using UnityEngine;

public delegate void WindowUpdateHandle();

public interface IGraphable
{
    Vector2[] GetGraphPoints(WindowUpdateHandle windowUpdate);
    Vector2[] GetMainGraphPoints();
}

public class GraphMaker : MonoBehaviour
{
    public IGraphable UsedGraphableComponent;
    public GraphWindowSettings UsedWindowSettings;
    public IGraphable[] IGraphableComponents;
    public string[] ComponentsContainer;
    public GraphWindowSettings ImportedSettings;
    public GraphWindowSettings[] Settings;
    public int SelectedSettingsIndex;
    public string[] SettingsContainer;
    static event WindowUpdateHandle windowUpdater;
#if UNITY_EDITOR
    GraphWindow graphWindow;
#endif

    public void OpenNewGraphWindow()
    {
#if UNITY_EDITOR
        if (!CheckComponents())
            return;
        if (graphWindow == null)
        {
            if (UsedWindowSettings == null)
            {
                UsedWindowSettings = gameObject.AddComponent<GraphWindowSettings>();
                UsedWindowSettings.SetDefault();
            }
            if (!UsedWindowSettings.Instantiated)
                UsedWindowSettings.SetDefault();
            windowUpdater += UpdateWindowContent;
            if (CheckGraphPoints(UsedGraphableComponent.GetGraphPoints(windowUpdater), UsedGraphableComponent.GetMainGraphPoints()))
            {
                graphWindow = GraphWindow.GetNewWindow(UsedWindowSettings);
                graphWindow.InitializeGraphPoints(UsedGraphableComponent.GetGraphPoints(windowUpdater), UsedGraphableComponent.GetMainGraphPoints());
                graphWindow.OnGUI();
                //graphWindow.Repaint();
            }
            //UpdateWindowContent();
        }
#endif
    }

    public bool CheckComponents()
    {
        IGraphableComponents = GetComponents<IGraphable>();
        Settings = GetComponents<GraphWindowSettings>();

        if (IGraphableComponents.Length == 0)
        {
            Debug.LogWarning("IGraphable interface not implemented in any of the gameobjects components");
            UsedGraphableComponent = null;
            ComponentsContainer = null;
            return false;
        }
        else
        {
            if (UsedGraphableComponent == null)
                UsedGraphableComponent = IGraphableComponents[0];
        }

        ComponentsContainer = new string[IGraphableComponents.Length];

        for (int i = 0; i < IGraphableComponents.Length; i++)
            ComponentsContainer[i] = IGraphableComponents[i].ToString() + i;

        if (Settings.Length > 0 && UsedWindowSettings == null)
            UsedWindowSettings = Settings[0];

        SettingsContainer = new string[Settings.Length];

        int unNamed = 0;
        int duplicate = 0;

        for (int i = 0; i < Settings.Length; i++)
        {
            if (Settings[i].SettingsName == string.Empty || Settings[i].SettingsName == null)
            {
                SettingsContainer[i] = "unnamed_" + unNamed;
                //Settings[i].SettingsName = SettingsContainer[i];
                unNamed++;
            }
            else
                SettingsContainer[i] = Settings[i].SettingsName;

            for (int j = 0; j < i; j++)
            {
                if (SettingsContainer[i] == SettingsContainer[j])
                {
                    SettingsContainer[i] += "_" + duplicate;
                    duplicate++;
                }
            }
        }
        return true;
    }

    public void AddNewSettings()
    {
        System.Type type = ImportedSettings.GetType();
        Component importedSettings = gameObject.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
            field.SetValue(importedSettings, field.GetValue(ImportedSettings));
        ImportedSettings = null;
    }

    public void RemoveSelectedSettings()
    {
        if (Settings.Length == 0)
            return;
        DestroyImmediate(Settings[SelectedSettingsIndex]);
        GUIUtility.ExitGUI();
        CheckComponents();
    }

    bool CheckGraphPoints(Vector2[] graphPoints, Vector2[] mainGraphPoints)
    {
        if (graphPoints == null || graphPoints.Length == 0)
        {
            Debug.LogWarning("Graph points not initialized");
            return false;
        }
        if (mainGraphPoints == null || mainGraphPoints.Length == 0)
        {
            Debug.LogWarning("Main graph points not initialized");
            return false;
        }
        return true;
    }

    void UpdateWindowContent()
    {
#if UNITY_EDITOR
        if (graphWindow == null)
            return;
        graphWindow.InitializeGraphPoints(UsedGraphableComponent.GetGraphPoints(windowUpdater), UsedGraphableComponent.GetMainGraphPoints());
        graphWindow.Repaint();
#endif
    }
}
