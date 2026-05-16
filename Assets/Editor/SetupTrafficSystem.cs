using UnityEngine;
using UnityEditor;
using HealthbarGames;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class SetupTrafficSystem : EditorWindow
{
    [MenuItem("Tools/Setup Traffic System")]
    public static void Setup()
    {
        RemoveExisting();
        PlaceAtIntersections();
        AddCars();
        Debug.Log("Traffic system fully set up!");
    }

    static void RemoveExisting()
    {
        foreach (var t in FindObjectsOfType<TrafficLightManager>())
            DestroyImmediate(t.gameObject);
        foreach (var t in FindObjectsOfType<TrafficIntersection>())
            DestroyImmediate(t.gameObject);
        foreach (var t in FindObjectsOfType<TrafficLightBase>())
            DestroyImmediate(t.gameObject);
        var existingCars = GameObject.Find("TrafficCars");
        if (existingCars != null) DestroyImmediate(existingCars);
    }

    static void PlaceAtIntersections()
    {
        Vector3[] intersections = FindIntersections();

        var lightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Traffic Lights System/Prefabs/RealTrafficLight.prefab");

        if (lightPrefab == null)
        {
            Debug.LogError("RealTrafficLight.prefab not found at Assets/Traffic Lights System/Prefabs/RealTrafficLight.prefab!");
            return;
        }

        var postPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Traffic Lights System/Prefabs/TL_Post.prefab");

        float poleHeight = 4.24f;

        for (int i = 0; i < intersections.Length; i++)
        {
            Vector3 pos = intersections[i];

            var lightN = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(0, poleHeight, 4.5f), Quaternion.Euler(0, 180, 0), "TL_N_" + i);
            var lightS = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(0, poleHeight, -4.5f), Quaternion.identity, "TL_S_" + i);
            var lightE = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(-4.5f, poleHeight, 0), Quaternion.Euler(0, -90, 0), "TL_E_" + i);
            var lightW = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(4.5f, poleHeight, 0), Quaternion.Euler(0, 90, 0), "TL_W_" + i);

            var tlN = lightN != null ? lightN.GetComponent<RealTrafficLight>() : null;
            var tlS = lightS != null ? lightS.GetComponent<RealTrafficLight>() : null;
            var tlE = lightE != null ? lightE.GetComponent<RealTrafficLight>() : null;
            var tlW = lightW != null ? lightW.GetComponent<RealTrafficLight>() : null;

            var mgrGO = new GameObject("TL_Manager_" + i);
            mgrGO.transform.position = pos;
            var mgr = mgrGO.AddComponent<TrafficLightManager>();

            var field = typeof(TrafficLightManager).GetField("PhaseList",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var phases = new List<TrafficLightPhase>();

                var nsLights = new List<TrafficLightBase>();
                if (tlN != null) nsLights.Add(tlN);
                if (tlS != null) nsLights.Add(tlS);
                if (nsLights.Count > 0)
                {
                    var phaseNS = new TrafficLightPhase();
                    phaseNS.Initialize("NS Go", 2f, 12f, 2f);
                    phaseNS.TrafficLights = nsLights.ToArray();
                    phases.Add(phaseNS);
                }

                var ewLights = new List<TrafficLightBase>();
                if (tlE != null) ewLights.Add(tlE);
                if (tlW != null) ewLights.Add(tlW);
                if (ewLights.Count > 0)
                {
                    var phaseEW = new TrafficLightPhase();
                    phaseEW.Initialize("EW Go", 2f, 12f, 2f);
                    phaseEW.TrafficLights = ewLights.ToArray();
                    phases.Add(phaseEW);
                }

                field.SetValue(mgr, phases);
            }

            var interGO = new GameObject("Intersection_" + i);
            interGO.transform.position = pos;
            var inter = interGO.AddComponent<TrafficIntersection>();
            inter.lightN = tlN;
            inter.lightS = tlS;
            inter.lightE = tlE;
            inter.lightW = tlW;
        }
    }

    static GameObject PlaceLight(GameObject lightPrefab, GameObject postPrefab, Vector3 pos, Quaternion rot, string name)
    {
        if (postPrefab != null)
        {
            var post = (GameObject)PrefabUtility.InstantiatePrefab(postPrefab);
            if (post == null) post = Object.Instantiate(postPrefab);
            if (post != null)
            {
                post.name = name + "_Post";
                post.transform.position = new Vector3(pos.x, 0, pos.z);
                post.transform.rotation = rot;
            }
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(lightPrefab);
        if (go == null) go = Object.Instantiate(lightPrefab);
        if (go == null) return null;
        go.name = name;
        go.transform.position = pos;
        go.transform.rotation = rot;
        return go;
    }

    static Vector3[] FindIntersections()
    {
        return new Vector3[] {
            new Vector3(360, 0, 311),
            new Vector3(360, 0, 654),
            new Vector3(702, 0, 311),
            new Vector3(702, 0, 654)
        };
    }

    static void AddCars()
    {
        var carPaths = FindCarPrefabs();
        if (carPaths.Count == 0)
        {
            Debug.LogError("No car prefabs found!");
            return;
        }

        var carsParent = new GameObject("TrafficCars").transform;

        Vector3[][] routes = {
            new[] { new Vector3(360, 0, 50), new Vector3(360, 0, 250), new Vector3(360, 0, 600), new Vector3(360, 0, 950) },
            new[] { new Vector3(360, 0, 950), new Vector3(360, 0, 600), new Vector3(360, 0, 250), new Vector3(360, 0, 50) },
            new[] { new Vector3(702, 0, 50), new Vector3(702, 0, 260), new Vector3(702, 0, 610), new Vector3(702, 0, 950) },
            new[] { new Vector3(702, 0, 950), new Vector3(702, 0, 610), new Vector3(702, 0, 260), new Vector3(702, 0, 50) },
            new[] { new Vector3(50, 0, 311), new Vector3(200, 0, 311), new Vector3(760, 0, 311), new Vector3(950, 0, 311) },
            new[] { new Vector3(950, 0, 654), new Vector3(760, 0, 654), new Vector3(200, 0, 654), new Vector3(50, 0, 654) }
        };

        System.Random rng = new System.Random(42);

        for (int r = 0; r < routes.Length; r++)
        {
            string carPath = carPaths[rng.Next(carPaths.Count)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(carPath);
            if (prefab == null) continue;

            Vector3 startPos = routes[r][0];
            var car = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (car == null) car = Object.Instantiate(prefab);
            car.transform.position = startPos;
            car.transform.parent = carsParent;
            car.name = "TrafficCar_" + r;

            var follow = car.GetComponent<CarWaypointFollower>();
            if (follow == null) follow = car.AddComponent<CarWaypointFollower>();

            follow.waypoints = new Transform[routes[r].Length];
            for (int w = 0; w < routes[r].Length; w++)
            {
                var wp = new GameObject("WP_R" + r + "_P" + w);
                wp.transform.position = routes[r][w];
                wp.transform.parent = carsParent;
                follow.waypoints[w] = wp.transform;
            }
            follow.speed = UnityEngine.Random.Range(8f, 14f);
            follow.waypointReachDistance = 5f;

            if (car.GetComponent<CarTraffic>() == null)
                car.AddComponent<CarTraffic>();
        }
    }

    static List<string> FindCarPrefabs()
    {
        var paths = new List<string>();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Awb-Free Low Poly Vehicles/Prefabs" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            paths.Add(path);
        }

        guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Cobra Games Studio" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.ToLower().Contains("bus") || path.ToLower().Contains("van"))
                paths.Add(path);
        }
        return paths;
    }
}
