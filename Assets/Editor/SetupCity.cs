using UnityEngine;
using UnityEditor;

public class SetupCity : EditorWindow
{
    [MenuItem("Tools/Setup Everything _%#s")]
    public static void Setup()
    {
        // Step 1: Ground
        EnsureGround();

        // Step 2: Cars on roads
        AddCarsToRoads.AddCars();

        // Step 3: Traffic lights with stands
        PlaceTrafficSystem.PlaceSystem();

        Debug.Log("=== City Setup Complete! Hit Play ===");
    }

    static void EnsureGround()
    {
        if (GameObject.Find("Ground") != null) return;

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(200, 1, 200);
        ground.transform.position = new Vector3(500, -0.5f, 500);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.12f, 0.18f, 0.08f);
        ground.GetComponent<Renderer>().material = mat;

        // Also add a road-colored strip under main roads
        AddRoadStrip(new Vector3(360, -0.45f, 500), new Vector3(0.5f, 0.01f, 1000));
        AddRoadStrip(new Vector3(702, -0.45f, 500), new Vector3(0.5f, 0.01f, 1000));
        AddRoadStrip(new Vector3(500, -0.45f, 311), new Vector3(1000, 0.01f, 0.5f));
        AddRoadStrip(new Vector3(500, -0.45f, 654), new Vector3(1000, 0.01f, 0.5f));

        Debug.Log("Ground + road strips created");
    }

    static void AddRoadStrip(Vector3 pos, Vector3 scale)
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "RoadStrip";
        strip.transform.position = pos;
        strip.transform.localScale = scale;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.25f, 0.25f, 0.25f);
        strip.GetComponent<Renderer>().material = mat;
        DestroyImmediate(strip.GetComponent<Collider>());
    }
}
