using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Swaps the existing round vault door in the active scene for a rectangular one,
// leaving the rest of the scene (keypad, lasers, XR, lights) untouched.
public static class ReplaceVaultDoor
{
    [MenuItem("Tools/Bank Vault/Replace Door (Rectangular)")]
    public static void Replace()
    {
        var vault = GameObject.Find("VaultDoor");
        if (vault == null)
        {
            EditorUtility.DisplayDialog("Replace Vault Door",
                "No 'VaultDoor' object found in the active scene.", "OK");
            return;
        }

        // Reuse the existing materials so the look stays consistent.
        var doorMat  = FindMat(vault.transform, "Door_Disc")  ?? FindMat(vault.transform, "Door_Slab");
        var brassMat = FindMat(vault.transform, "Door_Ring")  ?? FindMat(vault.transform, "Door_Wheel");

        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (doorMat == null && litShader != null)
            doorMat = new Material(litShader) { name = "VaultDoor", color = new Color(0.22f, 0.22f, 0.25f) };
        if (brassMat == null && litShader != null)
            brassMat = new Material(litShader) { name = "Brass", color = new Color(0.78f, 0.62f, 0.22f) };

        // Remove the old (round) door geometry.
        var children = new List<Transform>();
        foreach (Transform c in vault.transform) children.Add(c);
        foreach (var c in children) Undo.DestroyObjectImmediate(c.gameObject);

        BuildBankVaultScene.BuildRectangularDoor(vault.transform, doorMat, brassMat);

        var scene = vault.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BankVault] Vault door replaced with a rectangular door.");
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
