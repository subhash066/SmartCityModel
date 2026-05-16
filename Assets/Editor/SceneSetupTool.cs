using UnityEditor;
using UnityEngine;
using HealthbarGames;

public class SceneSetupTool : EditorWindow
{
    [MenuItem("Tools/Auto Setup Scene (test.unity)")]
    public static void SetupScene()
    {
        SetupNPCs();
        SetupCars();
        SetupTrafficLights();
        SetupStreetLights();
        SetupParkingSpots();
        SetupPlayerResponse();
        SetupTimer();
        SetupMusic();
        Debug.Log("Scene Setup Complete! 30s timer and Music Manager active.");
    }

    static void SetupMusic()
    {
        var musicObj = GameObject.Find("GlobalMusicManager");
        if (musicObj == null)
        {
            musicObj = new GameObject("GlobalMusicManager");
            musicObj.AddComponent<MusicManager>();
            Debug.Log("Initialized Global Music Manager. You can now drag your music into the 'Background Music' slot in the Inspector.");
        }
    }

    static void SetupTimer()
    {
        var timerObj = GameObject.Find("SceneTeleporter");
        if (timerObj == null)
        {
            timerObj = new GameObject("SceneTeleporter");
            timerObj.AddComponent<SceneTimer>();
            Debug.Log("Initialized 15-second Teleport Timer");
        }
    }

    static void SetupPlayerResponse()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.GetComponent<PlayerHitResponse>() == null)
                player.AddComponent<PlayerHitResponse>();
            Debug.Log("Added Hit Response to Player");
        }
    }

    static void SetupNPCs()
    {
        NPCImpact[] impacts = Object.FindObjectsByType<NPCImpact>(FindObjectsSortMode.None);
        foreach (var npc in impacts)
        {
            // Ensure Rigidbody
            var rb = npc.GetComponent<Rigidbody>();
            if (rb == null) rb = npc.gameObject.AddComponent<Rigidbody>();
            rb.mass = 70f;
            rb.isKinematic = true;

            // Ensure CharacterController
            var cc = npc.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = npc.gameObject.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.3f;
                cc.center = new Vector3(0, 0.9f, 0);
            }

            // Ensure NPCMovement
            if (npc.GetComponent<NPCMovement>() == null)
                npc.gameObject.AddComponent<NPCMovement>();
                
            // Disable Animator as requested
            var anim = npc.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;

            npc.gameObject.layer = 3; // NPC layer
        }
    }

    static void SetupCars()
    {
        CarTraffic[] cars = Object.FindObjectsByType<CarTraffic>(FindObjectsSortMode.None);
        foreach (var car in cars)
        {
            var follower = car.GetComponent<CarWaypointFollower>();
            if (follower == null) follower = car.gameObject.AddComponent<CarWaypointFollower>();

            var rb = car.GetComponent<Rigidbody>();
            if (rb == null) rb = car.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; 
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            car.gameObject.layer = 2; // Ignore Raycast layer

            var box = car.GetComponent<BoxCollider>();
            if (box == null) box = car.gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0, 0.75f, 0);
            box.size = new Vector3(2.5f, 1.5f, 5.5f); // Larger collider to prevent "ghosting"
        }
    }

    static void SetupTrafficLights()
    {
        TrafficIntersection[] intersections = Object.FindObjectsByType<TrafficIntersection>(FindObjectsSortMode.None);
        foreach (var intersection in intersections)
        {
            Debug.Log($"Found intersection: {intersection.name}");
        }
    }

    static void SetupStreetLights()
    {
        // Find objects with names like StreetLight or Lamp
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Point || l.type == LightType.Spot)
            {
                if (l.name.ToLower().Contains("street") || l.name.ToLower().Contains("lamp") || l.name.ToLower().Contains("light"))
                {
                    if (l.GetComponentInParent<SmartStreetLight>() == null)
                    {
                        l.gameObject.AddComponent<SmartStreetLight>();
                        Debug.Log($"Upgraded {l.name} to Smart Street Light");
                    }
                }
            }
        }
    }

    static void SetupParkingSpots()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go.name.ToLower().Contains("parking") && !go.name.ToLower().Contains("manager"))
            {
                if (go.GetComponent<SmartParkingSpot>() == null)
                {
                    go.AddComponent<SmartParkingSpot>();
                    Debug.Log($"Initialized Smart Parking Spot: {go.name}");
                }
            }
        }
    }
}
