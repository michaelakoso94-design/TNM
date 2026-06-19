using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Retrofits the vault door already in the active scene:
//   - removes the old round locking wheel + spokes,
//   - adds a vertical bar pull-handle on the right edge,
//   - adds the square black/white pattern panel on the door face.
// Re-running is safe: it clears any previously generated handle/pattern first.
public static class AddDoorPatternAndHandle
{
    static readonly string[] RemovePrefixes =
    {
        "Door_Wheel", "Door_Spoke", "Door_Handle", "Pattern_",
    };

    [MenuItem("Tools/Bank Vault/Add Door Pattern + Rod Handle")]
    public static void Apply()
    {
        var vault = GameObject.Find("VaultDoor");
        if (vault == null)
        {
            EditorUtility.DisplayDialog("Door Pattern + Handle",
                "No 'VaultDoor' object found in the active scene.", "OK");
            return;
        }

        // Reuse an existing brass/metal material for the handle if one is present.
        var metalMat = FindMat(vault.transform, "Door_Frame_Top")
                       ?? FindMat(vault.transform, "Door_Ring")
                       ?? FindMat(vault.transform, "Bolt_T0");
        if (metalMat == null)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null) metalMat = new Material(lit) { name = "Brass", color = new Color(0.78f, 0.62f, 0.22f) };
        }

        // Remove the old handle and any prior generated pattern.
        var toRemove = new List<GameObject>();
        foreach (Transform c in vault.transform)
            foreach (var p in RemovePrefixes)
                if (c.name.StartsWith(p)) { toRemove.Add(c.gameObject); break; }
        foreach (var go in toRemove) Undo.DestroyObjectImmediate(go);

        BuildBankVaultScene.BuildRodHandle(vault.transform, metalMat);
        BuildBankVaultScene.BuildDoorPattern(vault.transform, BuildBankVaultScene.DoorPattern);

        var scene = vault.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BankVault] Added rod handle + pattern panel to the vault door.");
        Selection.activeGameObject = vault;
    }

    static Material FindMat(Transform parent, string childName)
    {
        var t = parent.Find(childName);
        if (t == null) return null;
        var r = t.GetComponent<MeshRenderer>();
        return r != null ? r.sharedMaterial : null;
    }
}
