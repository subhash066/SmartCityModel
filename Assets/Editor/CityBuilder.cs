using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class CityBuilder : EditorWindow
{
    [MenuItem("Tools/Build Smart City")]
    public static void BuildCity()
    {
        if (!EditorUtility.DisplayDialog("Build Smart City", "This will clear existing scene objects and build a complete Smart City. Continue?", "Yes", "No"))
            return;

        ClearScene();
        BuildGround();

        var roads = FindPrefabs("LowPolyCityAssetPack/Prefabs/Roads");
        var buildings = new List<string>();
        buildings.AddRange(FindPrefabs("POLYGON city pack/Prefabs/Buildings"));
        buildings.AddRange(FindPrefabs("Simple city plain/Prefabs"));
        buildings.AddRange(FindPrefabs("Studio Horizon/Simple Building Generic Free/Prefabs"));
        buildings.AddRange(FindPrefabs("LowPolyCityAssetPack/Prefabs/Buildings"));
        buildings.AddRange(FindPrefabs("LowPolyCityAssetPack/Prefabs/Houses"));
        var trees = FindPrefabs("Palmov Island");
        var lamps = FindPrefabs("POLYGON city pack/Prefabs/Lamps");
        var cars = FindPrefabs("Awb-Free Low Poly Vehicles/Prefabs");
        var tls = FindPrefabs("POLYGON city pack/Prefabs/Props", "traffic light");

        BuildRoadGrid(roads);
        BuildCityBlocks(buildings);
        BuildPark(trees);
        PlaceProps(lamps);
        PlaceTrafficLights(tls);
        SpawnCars(cars);
        SetupPlayer();
        SetupLighting();

        Debug.Log("Smart City built successfully!");
    }

    static void ClearScene()
    {
        var all = new List<GameObject>(FindObjectsOfType<GameObject>(true));
        foreach (var go in all)
        {
            if (go.hideFlags != HideFlags.None) continue;
            if (go.transform.parent != null) continue;
            if (go.scene.name == null || go.scene.path == null) continue;
            DestroyImmediate(go);
        }
    }

    static List<string> FindPrefabs(string contains, string nameContains = null)
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/" + contains });
        var paths = new List<string>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (nameContains == null || path.ToLower().Contains(nameContains.ToLower()))
                paths.Add(path);
        }
        return paths;
    }

    static GameObject LoadPrefab(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            Debug.LogWarning("Prefab not found: " + path);
        return prefab;
    }

    static GameObject Place(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (prefab == null) return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (go == null)
            go = Object.Instantiate(prefab);
        go.transform.SetPositionAndRotation(pos, rot);
        if (parent) go.transform.parent = parent;
        return go;
    }

    static GameObject Place(GameObject prefab, Vector3 pos, Transform parent = null)
    {
        return Place(prefab, pos, Quaternion.identity, parent);
    }

    static void BuildGround()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Ground";
        go.transform.localScale = new Vector3(100, 1, 100);
        go.transform.position = new Vector3(0, -0.5f, 0);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.15f, 0.2f, 0.1f);
        go.GetComponent<Renderer>().material = mat;
    }

    static void BuildRoadGrid(List<string> roadPaths)
    {
        var straight = roadPaths.FirstOrDefault(p => PathGet(p).StartsWith("straightroad") && !PathGet(p).Contains("type"));
        var quad = roadPaths.FirstOrDefault(p => PathGet(p) == "quadroad" || PathGet(p) == "quadroadtype_2");
        var straightPrefab = LoadPrefab(straight);
        var quadPrefab = LoadPrefab(quad);

        var parent = new GameObject("Roads").transform;
        float size = 20f;
        int n = 5;

        for (int x = 0; x < n; x++)
        {
            for (int z = 0; z < n; z++)
            {
                Vector3 pos = new Vector3(x * size + size / 2, 0, z * size + size / 2);
                var road = Place(x == 0 && z == 0 && quadPrefab != null ? quadPrefab : straightPrefab, pos, parent);
                if (x == 0 && z == 0 && road != null)
                {
                    var st = road.GetComponent<SimpleTrafficLight>();
                    if (st == null) st = road.AddComponent<SimpleTrafficLight>();
                }
            }
        }
    }

    static string PathGet(string p) => System.IO.Path.GetFileNameWithoutExtension(p).ToLower();

    static void BuildCityBlocks(List<string> buildingPaths, System.Random rng = null)
    {
        if (rng == null) rng = new System.Random(42);
        var parent = new GameObject("Buildings").transform;
        float size = 20f;
        int n = 5;

        for (int x = 0; x < n; x++)
        {
            for (int z = 0; z < n; z++)
            {
                int count = rng.Next(2, 5);
                for (int i = 0; i < count; i++)
                {
                    if (buildingPaths.Count == 0) break;
                    string path = buildingPaths[rng.Next(buildingPaths.Count)];
                    var prefab = LoadPrefab(path);
                    if (prefab == null) continue;

                    float bx = x * size + 2 + (float)rng.NextDouble() * (size - 5);
                    float bz = z * size + 2 + (float)rng.NextDouble() * (size - 5);
                    Place(prefab, new Vector3(bx, 0, bz), Quaternion.Euler(0, rng.Next(4) * 90, 0), parent);
                }
            }
        }
    }

    static void BuildPark(List<string> treePaths)
    {
        var parent = new GameObject("Trees").transform;
        var rng = new System.Random(42);
        for (int i = 0; i < 40; i++)
        {
            if (treePaths.Count == 0) break;
            string path = treePaths[rng.Next(treePaths.Count)];
            var prefab = LoadPrefab(path);
            if (prefab == null) continue;

            float x = 15 + (float)rng.NextDouble() * 70;
            float z = 15 + (float)rng.NextDouble() * 70;
            var go = Place(prefab, new Vector3(x, 0, z), Quaternion.Euler(0, (float)rng.NextDouble() * 360, 0), parent);
            if (go != null)
                go.transform.localScale *= UnityEngine.Random.Range(0.8f, 1.5f);
        }
    }

    static void PlaceProps(List<string> lampPaths, System.Random rng = null)
    {
        if (rng == null) rng = new System.Random(42);
        var parent = new GameObject("Props").transform;
        float size = 20f;
        int n = 5;

        for (int x = 0; x < n; x++)
        {
            for (int z = 0; z < n; z++)
            {
                if (rng.Next(3) != 0 || lampPaths.Count == 0) continue;
                string path = lampPaths[rng.Next(lampPaths.Count)];
                var prefab = LoadPrefab(path);
                if (prefab == null) continue;
                Place(prefab, new Vector3(x * size + size / 2, 0, z * size + size / 2),
                    Quaternion.Euler(0, rng.Next(4) * 90, 0), parent);
            }
        }
    }

    static void PlaceTrafficLights(List<string> tlPaths)
    {
        var parent = new GameObject("TrafficLights").transform;
        float size = 20f;
        int n = 5;

        for (int x = 1; x < n; x++)
        {
            for (int z = 1; z < n; z++)
            {
                if (tlPaths.Count == 0) return;
                var prefab = LoadPrefab(tlPaths[0]);
                if (prefab == null) continue;
                var tl = Place(prefab, new Vector3(x * size, 0, z * size), parent);
                if (tl != null && tl.GetComponent<SimpleTrafficLight>() == null)
                {
                    var st = tl.AddComponent<SimpleTrafficLight>();
                    if (st != null)
                    {
                        st.redLight = tl.GetComponent<Light>();
                        st.yellowLight = tl.GetComponentsInChildren<Light>().Skip(1).FirstOrDefault();
                        st.greenLight = tl.GetComponentsInChildren<Light>().Skip(2).FirstOrDefault();
                    }
                }
            }
        }
    }

    static void SpawnCars(List<string> carPaths)
    {
        var parent = new GameObject("Cars").transform;
        var rng = new System.Random(42);
        float size = 20f;
        int n = 5;

        for (int i = 0; i < 6; i++)
        {
            if (carPaths.Count == 0) break;
            string path = carPaths[rng.Next(carPaths.Count)];
            var prefab = LoadPrefab(path);
            if (prefab == null) continue;

            float x = (float)rng.NextDouble() * n * size;
            float z = (float)rng.NextDouble() * n * size;
            var car = Place(prefab, new Vector3(x, 1f, z), Quaternion.Euler(0, rng.Next(4) * 90, 0), parent);

            var follow = car.GetComponent<CarWaypointFollower>();
            if (follow == null) follow = car.AddComponent<CarWaypointFollower>();

            int wpCount = rng.Next(3, 6);
            follow.waypoints = new Transform[wpCount];
            for (int w = 0; w < wpCount; w++)
            {
                var wp = new GameObject("WP_" + i + "_" + w);
                wp.transform.position = new Vector3(
                    (float)rng.NextDouble() * n * size, 0.5f,
                    (float)rng.NextDouble() * n * size);
                wp.transform.parent = parent;
                follow.waypoints[w] = wp.transform;
            }
            follow.speed = UnityEngine.Random.Range(5f, 12f);
        }
    }

    static void SetupPlayer()
    {
        var prefabPath = FindPrefabs("Starter Assets/Runtime/FirstPersonController/Prefabs", "Playe")
            .FirstOrDefault(p => p.EndsWith("PlayerCapsule.prefab"));

        if (prefabPath != null)
        {
            var prefab = LoadPrefab(prefabPath);
            Place(prefab, new Vector3(10, 2, 10), Quaternion.identity);
        }
        else
        {
            var go = new GameObject("Player");
            var cam = go.AddComponent<Camera>();
            cam.tag = "MainCamera";
            go.AddComponent<AudioListener>();
            go.AddComponent<CharacterController>();
            go.transform.position = new Vector3(10, 2, 10);
        }

        var camPath = FindPrefabs("Starter Assets/Runtime/FirstPersonController/Prefabs", "Follow")
            .FirstOrDefault(p => p.EndsWith("PlayerFollowCamera.prefab"));
        if (camPath != null)
        {
            var prefab = LoadPrefab(camPath);
            Place(prefab, Vector3.zero, Quaternion.identity);
        }
    }

    static void SetupLighting()
    {
        var go = new GameObject("Directional Light");
        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        go.transform.rotation = Quaternion.Euler(50, -30, 0);
        go.tag = "Light";
    }
}
