using UnityEngine;
using UnityEditor;
using HealthbarGames;
using System.Collections.Generic;
using System.Reflection;

public class PlaceTrafficSystem : EditorWindow
{
    [MenuItem("Tools/Place Traffic System")]
    public static void PlaceSystem()
    {
        RemoveExisting();
        PlaceAtIntersections();
        Debug.Log("Traffic system placed using RealTrafficLight prefabs!");
    }

    static void RemoveExisting()
    {
        foreach (var t in FindObjectsOfType<TrafficLightManager>())
            DestroyImmediate(t.gameObject);
        foreach (var t in FindObjectsOfType<TrafficIntersection>())
            DestroyImmediate(t.gameObject);
        foreach (var t in FindObjectsOfType<TrafficLightBase>())
            DestroyImmediate(t.gameObject);
    }

    static void PlaceAtIntersections()
    {
        Vector3[] intersections = {
            new Vector3(360, 0, 311),
            new Vector3(360, 0, 654),
            new Vector3(702, 0, 311),
            new Vector3(702, 0, 654)
        };

        var lightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Traffic Lights System/Prefabs/RealTrafficLight.prefab");
        var postPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Traffic Lights System/Prefabs/TL_Post.prefab");

        if (lightPrefab == null)
        {
            Debug.LogError("RealTrafficLight.prefab not found!");
            return;
        }
        if (postPrefab == null)
            Debug.LogWarning("TL_Post.prefab not found, using cylinder poles instead.");

        float poleHeight = 4.24f;

        for (int i = 0; i < intersections.Length; i++)
        {
            Vector3 pos = intersections[i];

            var lightN = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(0, poleHeight, -4), Quaternion.identity, "TL_N_" + i);
            var lightS = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(0, poleHeight, 4), Quaternion.Euler(0, 180, 0), "TL_S_" + i);
            var lightE = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(4, poleHeight, 0), Quaternion.Euler(0, 90, 0), "TL_E_" + i);
            var lightW = PlaceLight(lightPrefab, postPrefab, pos + new Vector3(-4, poleHeight, 0), Quaternion.Euler(0, -90, 0), "TL_W_" + i);

            // Manager
            var mgrGO = new GameObject("TL_Manager_" + i);
            mgrGO.transform.position = pos;
            var mgr = mgrGO.AddComponent<TrafficLightManager>();

            var field = typeof(TrafficLightManager).GetField("PhaseList",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var phases = new List<TrafficLightPhase>();

                var phaseNS = new TrafficLightPhase();
                phaseNS.Initialize("NS Go", 2f, 10f, 2f);
                phaseNS.TrafficLights = new TrafficLightBase[] {
                    lightN.GetComponent<RealTrafficLight>(),
                    lightS.GetComponent<RealTrafficLight>()
                };
                phases.Add(phaseNS);

                var phaseEW = new TrafficLightPhase();
                phaseEW.Initialize("EW Go", 2f, 10f, 2f);
                phaseEW.TrafficLights = new TrafficLightBase[] {
                    lightE.GetComponent<RealTrafficLight>(),
                    lightW.GetComponent<RealTrafficLight>()
                };
                phases.Add(phaseEW);

                field.SetValue(mgr, phases);
            }

            // TrafficIntersection for car detection
            var interGO = new GameObject("Intersection_" + i);
            interGO.transform.position = pos;
            var inter = interGO.AddComponent<TrafficIntersection>();
            inter.lightN = lightN.GetComponent<RealTrafficLight>();
            inter.lightS = lightS.GetComponent<RealTrafficLight>();
            inter.lightE = lightE.GetComponent<RealTrafficLight>();
            inter.lightW = lightW.GetComponent<RealTrafficLight>();
        }
    }

    static GameObject PlaceLight(GameObject lightPrefab, GameObject postPrefab, Vector3 pos, Quaternion rot, string name)
    {
        // Place the post at ground level
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

        // Place the light on top of the post
        var go = (GameObject)PrefabUtility.InstantiatePrefab(lightPrefab);
        if (go == null) go = Object.Instantiate(lightPrefab);
        if (go == null) return null;
        go.name = name;
        go.transform.position = pos;
        go.transform.rotation = rot;
        return go;
    }
}
