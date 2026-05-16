using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddCarsToRoads : EditorWindow
{
    [MenuItem("Tools/Add Cars to Roads")]
    public static void AddCars()
    {
        var carPaths = FindCarPrefabs();
        if (carPaths.Count == 0)
        {
            Debug.LogError("No car prefabs found!");
            return;
        }

        RemoveExistingCars();

        var carsParent = new GameObject("TrafficCars").transform;

        Vector3[][] routes = {
            new[] { new Vector3(360, 1, 50), new Vector3(360, 1, 250), new Vector3(360, 1, 600), new Vector3(360, 1, 950) },
            new[] { new Vector3(360, 1, 950), new Vector3(360, 1, 600), new Vector3(360, 1, 250), new Vector3(360, 1, 50) },
            new[] { new Vector3(702, 1, 50), new Vector3(702, 1, 260), new Vector3(702, 1, 610), new Vector3(702, 1, 950) },
            new[] { new Vector3(702, 1, 950), new Vector3(702, 1, 610), new Vector3(702, 1, 260), new Vector3(702, 1, 50) },
            new[] { new Vector3(50, 1, 311), new Vector3(200, 1, 311), new Vector3(760, 1, 311), new Vector3(950, 1, 311) },
            new[] { new Vector3(950, 1, 654), new Vector3(760, 1, 654), new Vector3(200, 1, 654), new Vector3(50, 1, 654) }
        };

        float[] startOffsets = { 0, 18, 36, 54, 72, 90 };
        int totalCars = 0;

        System.Random rng = new System.Random(42);

        for (int r = 0; r < routes.Length; r++)
        {
            foreach (float offset in startOffsets)
            {
                string carPath = carPaths[rng.Next(carPaths.Count)];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(carPath);
                if (prefab == null) continue;

                Vector3 startPos = routes[r][0];
                Vector3 dir = (routes[r][1] - routes[r][0]).normalized;
                startPos += dir * offset;

                var car = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (car == null) car = Object.Instantiate(prefab);
                car.transform.position = startPos;
                car.transform.parent = carsParent;
                car.name = "TrafficCar_" + totalCars;

                var rb = car.GetComponent<Rigidbody>();
                if (rb == null) rb = car.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.mass = 500f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                var follow = car.GetComponent<CarWaypointFollower>();
                if (follow == null) follow = car.AddComponent<CarWaypointFollower>();

                follow.waypoints = new Transform[routes[r].Length];
                for (int w = 0; w < routes[r].Length; w++)
                {
                    var wp = new GameObject("WP_R" + r + "_C" + totalCars + "_P" + w);
                    wp.transform.position = routes[r][w];
                    wp.transform.parent = carsParent;
                    follow.waypoints[w] = wp.transform;
                }
                follow.speed = Random.Range(8f, 14f);
                follow.waypointReachDistance = 5f;

                if (car.GetComponent<CarTraffic>() == null)
                    car.AddComponent<CarTraffic>();

                totalCars++;
            }
        }

        Debug.Log("Added " + totalCars + " cars on roads with traffic light detection!");
    }

    static void RemoveExistingCars()
    {
        var existing = GameObject.Find("TrafficCars");
        if (existing != null) DestroyImmediate(existing);
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
