using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddXRDeviceSimulator
{
    [MenuItem("Tools/Bank Vault/Add XR Device Simulator")]
    public static void Add()
    {
        // Locate the prefab anywhere in the project (handles version-specific sample paths).
        string prefabPath = null;
        foreach (var guid in AssetDatabase.FindAssets("\"XR Device Simulator\" t:Prefab"))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.EndsWith("/XR Device Simulator.prefab"))
            {
                prefabPath = p;
                break;
            }
        }
        // Newer XRI versions renamed to "XR Interaction Simulator".
        if (prefabPath == null)
        {
            foreach (var guid in AssetDatabase.FindAssets("\"XR Interaction Simulator\" t:Prefab"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith("/XR Interaction Simulator.prefab"))
                {
                    prefabPath = p;
                    break;
                }
            }
        }
        if (prefabPath == null)
        {
            EditorUtility.DisplayDialog("Add XR Device Simulator",
                "Could not find the XR Device Simulator prefab. " +
                "Open Window → Package Manager → XR Interaction Toolkit → Samples and import 'XR Device Simulator'.",
                "OK");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("[BankVault] Failed to load simulator prefab at " + prefabPath);
            return;
        }

        // Remove any existing instance for idempotency.
        var existing = GameObject.Find(prefab.name);
        if (existing != null) Object.DestroyImmediate(existing);

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(inst, "Add XR Device Simulator");
        Selection.activeGameObject = inst;

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[BankVault] " + prefab.name + " added to scene from " + prefabPath +
                  ". Press Play and use WASD + mouse to drive the rig.");
    }
}
