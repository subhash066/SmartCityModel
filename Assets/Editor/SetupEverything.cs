using UnityEditor;
using UnityEngine;

public class SetupEverything : EditorWindow
{
    [MenuItem("Tools/Setup Everything %#S")]
    public static void RunAll()
    {
        Debug.Log("=== Starting full scene setup ===");

        AddCarsToRoads.AddCars();
        AddNPCs.AddNPCManager();
        CameraSetupTool.SetupCamera();

        Debug.Log("=== Setup complete! Press Play to see cars hit NPCs ===");
    }
}
