using UnityEditor;
using UnityEngine;
using System.Linq;

public class AddNPCs : EditorWindow
{
    [MenuItem("Tools/Add NPCs to Scene")]
    public static void AddNPCManager()
    {
        RemoveExisting();

        var guids = AssetDatabase.FindAssets("PlayerArmature t:Prefab", new[] { "Assets/Starter Assets" });
        var prefabGuid = guids.FirstOrDefault();
        if (prefabGuid == null)
        {
            Debug.LogError("Dummy prefab not found in Assets/Lowpoly Dummy Character!");
            return;
        }

        var parent = new GameObject("NPCManager");
        var manager = parent.AddComponent<NPCManager>();
        manager.npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
        manager.npcCount = 30;

        Debug.Log($"Added NPCManager with Dummy character, {manager.npcCount} total NPCs.");
    }

    static void RemoveExisting()
    {
        var existing = GameObject.Find("NPCManager");
        if (existing != null) DestroyImmediate(existing);
    }
}
