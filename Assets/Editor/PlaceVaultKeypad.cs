using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PlaceVaultKeypad
{
    const string PrefabPath = "Assets/Keypad/Prefabs/Keypad.prefab";

    // Right wall inner face is at x = +2.0 (wall center 2.05, thickness 0.1).
    const float InnerWallX = 2.0f;
    const float WallGap = 0.01f;     // tiny gap so it does not z-fight the wall
    const float MountY = 1.4f;       // center height of the keypad
    const float MountZ = 10.7f;      // same spot as the old procedural keypad, after the lasers
    const float TargetHeight = 0.28f; // real-world height of the keypad panel

    [MenuItem("Tools/Bank Vault/Place Keypad (Right Wall)")]
    public static void Place()
    {
        var root = GameObject.Find("BankVault");
        if (root == null)
        {
            EditorUtility.DisplayDialog("Place Keypad",
                "No 'BankVault' object found in the active scene.", "OK");
            return;
        }

        // Remove the old procedural keypad and any previously placed asset keypad.
        var oldProcedural = root.transform.Find("Keypad");
        if (oldProcedural != null) Object.DestroyImmediate(oldProcedural.gameObject);
        var oldAsset = root.transform.Find("VaultKeypad");
        if (oldAsset != null) Object.DestroyImmediate(oldAsset.gameObject);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Place Keypad",
                "Keypad prefab not found at:\n" + PrefabPath, "OK");
            return;
        }

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.name = "VaultKeypad";
        Undo.RegisterCreatedObjectUndo(inst, "Place Vault Keypad");
        inst.transform.SetParent(root.transform, false);

        // Face -X: panel front (+Z) points into the hallway from the right wall.
        inst.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        inst.transform.localScale = Vector3.one;

        // Scale uniformly so the panel is ~TargetHeight tall.
        var size = WorldBounds(inst).size;
        if (size.y > 0.0001f)
        {
            float s = TargetHeight / size.y;
            inst.transform.localScale = Vector3.one * s;
        }

        // Park it away from the wall, then shift so its back is flush against the wall
        // and it is centered at the desired height / depth.
        inst.transform.localPosition = new Vector3(1.0f, MountY, MountZ);
        var b = WorldBounds(inst);
        float dx = (InnerWallX - WallGap) - b.max.x;  // push +X face to the wall
        float dy = MountY - b.center.y;
        var lp = inst.transform.localPosition;
        inst.transform.localPosition = new Vector3(lp.x + dx, lp.y + dy, lp.z);

        var scene = root.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var fb = WorldBounds(inst);
        Debug.Log($"[Keypad] Placed VaultKeypad. scale={inst.transform.localScale.x:0.000} " +
                  $"localPos={inst.transform.localPosition} worldSize={fb.size} " +
                  $"max.x={fb.max.x:0.000} (wall inner={InnerWallX})");
        Selection.activeGameObject = inst;
    }

    static Bounds WorldBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }
}
