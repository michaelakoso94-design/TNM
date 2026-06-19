using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;

public static class BuildXROrigin
{
    [MenuItem("Tools/Bank Vault/Add XR Origin")]
    public static void Build()
    {
        // Remove existing XR Origin if any (idempotent).
        var existingOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (existingOrigin != null) Object.DestroyImmediate(existingOrigin.gameObject);

        // Remove the preview Main Camera so the XR Origin's camera becomes the only one.
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam.GetComponentInParent<XROrigin>() != null) continue;
            if (cam.gameObject.name == "Main Camera" || cam.CompareTag("MainCamera"))
            {
                Object.DestroyImmediate(cam.gameObject);
            }
        }

        // Use Unity's built-in XR Origin (VR) menu to create a properly-configured rig.
        bool ok = EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (VR)");
        if (!ok) ok = EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Action-based)");
        if (!ok) ok = EditorApplication.ExecuteMenuItem("GameObject/XR/Convert Main Camera To XR Rig");
        if (!ok)
        {
            EditorUtility.DisplayDialog("Add XR Origin",
                "Could not find a GameObject → XR menu entry. Make sure XR Interaction Toolkit is installed and try again, " +
                "or add manually via GameObject → XR.", "OK");
            return;
        }

        var origin = Object.FindFirstObjectByType<XROrigin>();
        if (origin == null)
        {
            Debug.LogError("[BankVault] XR Origin was created by the menu but couldn't be found afterward.");
            return;
        }

        // Move to player start.
        Undo.RecordObject(origin.transform, "Position XR Origin");
        origin.transform.position = new Vector3(0f, 0f, 0.8f);
        origin.transform.rotation = Quaternion.identity;

        // Floor-relative tracking (so the user stands on the floor at y=0).
        try { origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor; } catch { }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = origin.gameObject;
        Debug.Log("[BankVault] XR Origin added at player start (0, 0, 0.8).");
    }
}
