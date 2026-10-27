using UnityEditor;

/// <summary>
/// Unity Editor utility forwarding menu commands to EscapeRoomSceneBuilder.
/// </summary>
public static class EscapeRoomEditorMenu
{
    [MenuItem("VR Escape Room/Generate & Save Escape Room Scene", false, 10)]
    public static void GenerateAndSaveScene()
    {
        EscapeRoomSceneBuilder.BuildAndSaveScene(!UnityEngine.Application.isBatchMode);
    }

    [MenuItem("VR Escape Room/Add EscapeRoom Scene to Build Settings", false, 11)]
    public static void AddSceneToBuildSettings()
    {
        EscapeRoomSceneBuilder.AddSceneToBuildSettings();
    }

    [MenuItem("VR Escape Room/Open EscapeRoom Scene", false, 12)]
    public static void OpenEscapeRoomScene()
    {
        EscapeRoomSceneBuilder.OpenScene();
    }
}
