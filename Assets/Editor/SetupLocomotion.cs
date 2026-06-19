using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupLocomotion
{
    [MenuItem("Tools/Bank Vault/Setup Locomotion")]
    public static void Setup()
    {
        var rig = Object.FindFirstObjectByType<OVRCameraRig>();
        if (rig == null)
        {
            EditorUtility.DisplayDialog("Setup Locomotion",
                "No OVRCameraRig found in the active scene. Open the scene with the Meta camera rig and try again.",
                "OK");
            return;
        }

        var go = rig.gameObject;
        var loco = go.GetComponent<PlayerLocomotion>();
        if (loco == null)
        {
            loco = Undo.AddComponent<PlayerLocomotion>(go);
            Debug.Log("[Locomotion] Added PlayerLocomotion to '" + go.name + "'.");
        }
        else
        {
            Debug.Log("[Locomotion] PlayerLocomotion already present on '" + go.name + "', updating refs.");
        }

        Undo.RecordObject(loco, "Configure PlayerLocomotion");
        loco.head = rig.centerEyeAnchor != null ? rig.centerEyeAnchor : rig.transform;

        EditorUtility.SetDirty(loco);
        var scene = go.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[Locomotion] Done. Rig='" + go.name + "', head='" +
                  (loco.head != null ? loco.head.name : "null") +
                  "'. Left stick = move, right stick = snap turn.");
        Selection.activeGameObject = go;
    }
}
