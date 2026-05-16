using UnityEditor;
using UnityEngine;
using System.Linq;

public class NPCSpawnUtility : EditorWindow
{
    [MenuItem("Tools/Spawn 2 Stationary NPCs")]
    public static void SpawnStationaryNPCs()
    {
        var guids = AssetDatabase.FindAssets("PlayerArmature t:Prefab", new[] { "Assets/Starter Assets" });
        var prefabGuid = guids.FirstOrDefault();
        if (prefabGuid == null)
        {
            Debug.LogError("PlayerArmature prefab not found in Assets/Starter Assets!");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
        
        // Find a good place to put them. Near the origin or near an existing object.
        Vector3 basePos = Vector3.zero;
        var existingManager = GameObject.Find("NPCManager");
        if (existingManager != null)
        {
            NPCManager manager = existingManager.GetComponent<NPCManager>();
            basePos = new Vector3((manager.spawnAreaMin.x + manager.spawnAreaMax.x) / 2f, 0, (manager.spawnAreaMin.y + manager.spawnAreaMax.y) / 2f);
        }

        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = basePos + new Vector3(i * 2, 0, 0);
            GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            npc.transform.position = pos;
            npc.transform.rotation = Quaternion.identity;
            npc.name = $"Stationary_NPC_{i + 1}";

            // Add Rigidbody for physics impact
            var rb = npc.GetComponent<Rigidbody>();
            if (rb == null) rb = npc.AddComponent<Rigidbody>();
            rb.mass = 70f;
            rb.isKinematic = true;

            // Setup components
            if (npc.GetComponent<CharacterController>() == null)
            {
                var cc = npc.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.3f;
                cc.center = new Vector3(0, 0.9f, 0);
            }

            // Disable movement
            var movement = npc.GetComponent<NPCMovement>();
            if (movement != null)
            {
                movement.enabled = false;
            }
            else
            {
                movement = npc.AddComponent<NPCMovement>();
                movement.enabled = false;
            }

            // Ensure impact script is present
            if (npc.GetComponent<NPCImpact>() == null)
            {
                npc.AddComponent<NPCImpact>();
            }

            // Cleanup player components if using PlayerArmature
            var inputs = npc.GetComponent("StarterAssetsInputs");
            if (inputs != null) DestroyImmediate(inputs);
            var tpController = npc.GetComponent("ThirdPersonController");
            if (tpController != null) DestroyImmediate(tpController);
            var pInput = npc.GetComponent("PlayerInput");
            if (pInput != null) DestroyImmediate(pInput);

            Undo.RegisterCreatedObjectUndo(npc, "Spawn Stationary NPC");
            Debug.Log($"Spawned stationary NPC at {pos}");
        }
    }
}
