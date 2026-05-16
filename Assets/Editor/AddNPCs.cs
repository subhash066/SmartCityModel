using UnityEditor;
using UnityEngine;
using System.Linq;

public class AddNPCs : EditorWindow
{
    [MenuItem("Tools/Add NPCs to Scene")]
    public static void AddNPCManager()
    {
        RemoveExisting();

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Lowpoly Dummy Character" });
        var prefabGuid = guids.FirstOrDefault();
        if (prefabGuid == null)
        {
            Debug.LogError("Dummy prefab not found in Assets/Lowpoly Dummy Character!");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
        if (prefab == null)
        {
            Debug.LogError("Failed to load Dummy prefab!");
            return;
        }

        var parent = new GameObject("NPCs").transform;

        Vector3[] positions = {
            new Vector3(360, 0, 450),
            new Vector3(700, 0, 300)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            var npc = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (npc == null) npc = Object.Instantiate(prefab);
            npc.transform.position = positions[i];
            npc.transform.parent = parent;
            npc.name = "Dummy_NPC_" + i;

            var anim = npc.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;

            var cc = npc.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = npc.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.3f;
                cc.center = new Vector3(0, 0.9f, 0);
            }

            var impact = npc.GetComponent<NPCImpact>();
            if (impact == null) npc.AddComponent<NPCImpact>();

            var movement = npc.GetComponent<NPCMovement>();
            if (movement != null) Object.DestroyImmediate(movement);
        }

        Debug.Log("Placed 2 static NPCs on road. Console logs on car hit.");
    }

    static void RemoveExisting()
    {
        var existing = GameObject.Find("NPCManager");
        if (existing != null) Object.DestroyImmediate(existing);
        var existing2 = GameObject.Find("NPCs");
        if (existing2 != null) Object.DestroyImmediate(existing2);
    }
}
