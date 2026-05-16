using UnityEditor;
using UnityEngine;

public class CameraSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Camera for Viewing")]
    public static void SetupCamera()
    {
        Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            var go = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            cam = go.GetComponent<Camera>();
            cam.tag = "MainCamera";
        }

        cam.transform.position = new Vector3(400, 30, 300);
        cam.transform.eulerAngles = new Vector3(40, 45, 0);
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 2000f;

        if (cam.GetComponent<FreeFlyCamera>() == null)
            cam.gameObject.AddComponent<FreeFlyCamera>();

        if (cam.GetComponent<HitCamera>() == null)
            cam.gameObject.AddComponent<HitCamera>();

        if (!cam.orthographic)
            cam.fieldOfView = 50;

        Selection.activeGameObject = cam.gameObject;
        EditorGUIUtility.PingObject(cam);

        Debug.Log($"Camera ready. Hold RMB + WASD/QE to fly. Car hits show closeup in corner.");
    }
}
