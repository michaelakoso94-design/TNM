using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;

public static class BuildControllerVisuals
{
    [MenuItem("Tools/Bank Vault/Add Controller Visuals (debug cubes + rays)")]
    public static void Build()
    {
        var origin = Object.FindFirstObjectByType<XROrigin>();
        if (origin == null)
        {
            EditorUtility.DisplayDialog("Controller Visuals",
                "No XR Origin found. Run 'Add XR Origin' first.", "OK");
            return;
        }

        var unlit = Shader.Find("Universal Render Pipeline/Unlit");
        var leftMat  = new Material(unlit) { name = "CtrlLeft",  color = new Color(0.3f, 0.7f, 1f) };
        var rightMat = new Material(unlit) { name = "CtrlRight", color = new Color(1f, 0.6f, 0.3f) };

        var controllers = new List<Transform>();
        foreach (var t in origin.transform.GetComponentsInChildren<Transform>(true))
        {
            var n = t.name.ToLowerInvariant();
            if (!n.Contains("controller")) continue;
            if (!(n.Contains("left") || n.Contains("right"))) continue;
            // Skip Visual* children we add ourselves
            if (t.parent != null && t.parent.name.ToLowerInvariant().Contains("controller")) continue;
            controllers.Add(t);
        }

        if (controllers.Count == 0)
        {
            EditorUtility.DisplayDialog("Controller Visuals",
                "No Left/Right controller GameObjects found under XR Origin.", "OK");
            return;
        }

        foreach (var ctrl in controllers)
        {
            bool isLeft = ctrl.name.ToLowerInvariant().Contains("left");
            var mat = isLeft ? leftMat : rightMat;

            var oldCube = ctrl.Find("VisualCube");
            if (oldCube != null) Object.DestroyImmediate(oldCube.gameObject);
            var oldRay = ctrl.Find("VisualRay");
            if (oldRay != null) Object.DestroyImmediate(oldRay.gameObject);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "VisualCube";
            cube.transform.SetParent(ctrl, false);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(0.04f, 0.04f, 0.09f);
            cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(cube.GetComponent<Collider>());

            var ray = new GameObject("VisualRay");
            ray.transform.SetParent(ctrl, false);
            ray.transform.localPosition = Vector3.zero;
            ray.transform.localRotation = Quaternion.identity;
            var lr = ray.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, new Vector3(0f, 0f, 5f));
            lr.startWidth = 0.006f;
            lr.endWidth = 0.001f;
            lr.material = mat;
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BankVault] Added debug visuals to " + controllers.Count + " controller(s). Blue=Left, Orange=Right.");
    }
}
