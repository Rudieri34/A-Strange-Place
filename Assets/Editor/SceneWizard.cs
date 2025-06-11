using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneWizard : EditorWindow
{
    private string[] scenes;
    private bool[] sceneActivationStatus;
    private int selectedSceneIndex = -1;
    private Vector2 scrollPosition;
    private GUIStyle titleStyle = new GUIStyle();
    private GUIStyle subtitleStyle = new GUIStyle();
    private GUIStyle separatorStyle = new GUIStyle();

    [MenuItem("Tools/Scene Wizard")]
    public static void ShowWindow()
    {
        GetWindow<SceneWizard>("Scene Wizard");
    }

    private void OnEnable()
    {
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.cyan;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        subtitleStyle.fontStyle = FontStyle.Italic;
        subtitleStyle.fontSize = 11;
        subtitleStyle.normal.textColor = Color.gray;
        subtitleStyle.alignment = TextAnchor.MiddleCenter;


        separatorStyle.border = new RectOffset(1, 1, 1, 1);
        separatorStyle.margin = new RectOffset(5, 5, 5, 5);
        separatorStyle.normal.textColor = new Color(0.7f, 0.5f, 0.8f);

        RefreshScenes();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label(" Scene Wizard", titleStyle);
        GUILayout.Space(5);
        GUILayout.Label(" A simple tool to manage scenes!", subtitleStyle);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Select a scene to play:");
        selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, scenes);

        if (GUILayout.Button("Play Scene"))
        {
            if (selectedSceneIndex >= 0 && selectedSceneIndex < scenes.Length)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[selectedSceneIndex].path);
                EditorApplication.isPlaying = true;
            }
            else
            {
                Debug.LogWarning("No scene selected.");
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < scenes.Length; i++)
        {
            GUILayout.BeginHorizontal();
            sceneActivationStatus[i] = GUILayout.Toggle(sceneActivationStatus[i], "", GUILayout.Width(20));
            if (GUILayout.Button(scenes[i]))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path);
                }
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create New Scene", GUILayout.Height(30)))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            RefreshScenes();
        }

        if (GUILayout.Button("Refresh Scenes", GUILayout.Height(30)))
        {
            RefreshScenes();
        }

        if (GUILayout.Button("Save Changes", GUILayout.Height(30)))
        {
            SaveSceneActivationStatus();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(5);
        GUILayout.Label("Version 1.2", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(5);
    }

    private void RefreshScenes()
    {
        scenes = new string[EditorBuildSettings.scenes.Length];
        sceneActivationStatus = new bool[EditorBuildSettings.scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
            sceneActivationStatus[i] = EditorBuildSettings.scenes[i].enabled;
        }

        Repaint();
    }

    private void OnInspectorUpdate() => Repaint();

    private void OnHierarchyChange() => RefreshScenes();

    private void OnProjectChange() => RefreshScenes();

    private void OnFocus() => RefreshScenes();

    private void SaveSceneActivationStatus()
    {
        var buildScenes = new EditorBuildSettingsScene[scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(EditorBuildSettings.scenes[i].path, sceneActivationStatus[i]);
        }

        EditorBuildSettings.scenes = buildScenes;
    }
}
