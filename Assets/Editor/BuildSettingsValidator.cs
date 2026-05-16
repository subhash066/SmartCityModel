using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class BuildSettingsValidator : MonoBehaviour
{
    [InitializeOnLoadMethod]
    static void ValidateBuildSettings()
    {
        string[] requiredScenes = { "Assets/test.unity", "Assets/zebra.unity" };
        List<EditorBuildSettingsScene> currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        foreach (string scenePath in requiredScenes)
        {
            bool exists = false;
            foreach (var ebss in currentScenes)
            {
                if (ebss.path == scenePath)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
                {
                    currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    changed = true;
                    Debug.Log($"Added {scenePath} to Build Settings");
                }
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = currentScenes.ToArray();
        }
    }
}
