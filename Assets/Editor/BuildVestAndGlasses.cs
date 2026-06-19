using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class BuildVestAndGlasses
{
    [MenuItem("Tools/Bank Vault/Add Wall Glasses")]
    public static void Build()
    {
        var origin = Object.FindFirstObjectByType<XROrigin>();
        if (origin == null)
        {
            EditorUtility.DisplayDialog("Wall Glasses",
                "No XR Origin found. Run 'Add XR Origin' first.", "OK");
            return;
        }

        var cam = origin.Camera;
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Wall Glasses",
                "XR Origin has no Camera assigned.", "OK");
            return;
        }

        var inv = origin.GetComponent<PlayerInventory>();
        if (inv == null)
        {
            EditorUtility.DisplayDialog("Wall Glasses",
                "PlayerInventory missing. Run 'Add Player Inventory' first.", "OK");
            return;
        }

        CleanupOldObjects();

        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        var hookMat = new Material(litShader) { name = "GlassesWallHook", color = new Color(0.75f, 0.58f, 0.20f) };
        var lensMat = new Material(litShader) { name = "GlassesVisibleLens", color = new Color(0.10f, 0.75f, 1f, 0.72f) };
        var frameMat = new Material(litShader) { name = "GlassFrame", color = new Color(0.05f, 0.05f, 0.08f) };
        lensMat.EnableKeyword("_EMISSION");
        lensMat.SetColor("_EmissionColor", new Color(0.10f, 0.75f, 1f) * 1.4f);
        frameMat.EnableKeyword("_EMISSION");
        frameMat.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 1f) * 0.7f);

        // Right wall, before the first laser, low enough to grab comfortably in VR.
        var mount = new GameObject("WallGlassesMount");
        mount.transform.position = new Vector3(1.92f, 1.45f, 2.35f);
        mount.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        var hook = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hook.name = "GlassesHook";
        hook.transform.SetParent(mount.transform, false);
        hook.transform.localPosition = new Vector3(0f, -0.08f, 0.035f);
        hook.transform.localScale = new Vector3(0.22f, 0.025f, 0.08f);
        hook.GetComponent<MeshRenderer>().sharedMaterial = hookMat;
        Object.DestroyImmediate(hook.GetComponent<Collider>());

        var wallHome = new GameObject("WallGlassesAnchor");
        wallHome.transform.SetParent(mount.transform, false);
        wallHome.transform.localPosition = new Vector3(0f, -0.01f, 0.18f);
        wallHome.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        var zone = new GameObject("WallGlassesZone");
        zone.transform.SetParent(mount.transform, false);
        zone.transform.localPosition = wallHome.transform.localPosition;
        var zoneCol = zone.AddComponent<BoxCollider>();
        zoneCol.isTrigger = true;
        zoneCol.size = new Vector3(0.60f, 0.50f, 0.45f);

        var headPoint = new GameObject("HeadEquipPoint");
        headPoint.transform.SetParent(cam.transform, false);
        headPoint.transform.localPosition = new Vector3(0f, -0.01f, 0.07f);
        headPoint.transform.localRotation = Quaternion.identity;

        var glasses = new GameObject("Glasses");
        glasses.transform.SetParent(wallHome.transform, false);
        glasses.transform.localPosition = Vector3.zero;
        glasses.transform.localRotation = Quaternion.identity;

        BuildLens(glasses.transform, new Vector3(-0.07f, 0f, 0f), lensMat);
        BuildLens(glasses.transform, new Vector3( 0.07f, 0f, 0f), lensMat);
        BuildRing(glasses.transform, new Vector3(-0.07f, 0f, 0f), 0.055f, 18, frameMat);
        BuildRing(glasses.transform, new Vector3( 0.07f, 0f, 0f), 0.055f, 18, frameMat);

        var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.SetParent(glasses.transform, false);
        bridge.transform.localPosition = Vector3.zero;
        bridge.transform.localScale = new Vector3(0.05f, 0.008f, 0.008f);
        bridge.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        Object.DestroyImmediate(bridge.GetComponent<Collider>());

        for (int s = -1; s <= 1; s += 2)
        {
            var temple = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temple.name = "Temple_" + (s < 0 ? "L" : "R");
            temple.transform.SetParent(glasses.transform, false);
            temple.transform.localPosition = new Vector3(s * 0.12f, 0f, -0.06f);
            temple.transform.localScale = new Vector3(0.008f, 0.008f, 0.13f);
            temple.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
            Object.DestroyImmediate(temple.GetComponent<Collider>());
        }

        var rb = glasses.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var col = glasses.AddComponent<SphereCollider>();
        col.radius = 0.22f;
        col.isTrigger = false;

        var grab = glasses.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.useDynamicAttach = true;
        grab.smoothPosition = true;
        grab.smoothRotation = true;

        var glassesScript = glasses.AddComponent<Glasses>();
        glassesScript.headEquipPoint = headPoint.transform;
        glassesScript.homeAnchor = wallHome.transform;
        glassesScript.homeZone = zoneCol;
        glassesScript.inventory = inv;
        glassesScript.equipRadius = 0.3f;

        inv.glassesModel = glasses;

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = mount;
        Debug.Log("[BankVault] Wall-mounted grabbable glasses added. Grab them from the right wall and bring them to your face.");
    }

    [MenuItem("Tools/Bank Vault/Add Vest + Glasses")]
    public static void BuildLegacyMenuAlias() => Build();

    static void CleanupOldObjects()
    {
        DestroyByName("Vest");
        DestroyByName("WallGlassesMount");
        DestroyByName("HeadEquipPoint");
        DestroyAllByName("Glasses");
        DestroyAllByName("Glasses_Model");
    }

    static void DestroyByName(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }

    static void DestroyAllByName(string name)
    {
        foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name == name)
                Object.DestroyImmediate(transform.gameObject);
        }
    }

    static void BuildRing(Transform parent, Vector3 localCenter, float radius, int segments, Material mat)
    {
        var ring = new GameObject("LensRing");
        ring.transform.SetParent(parent, false);
        ring.transform.localPosition = localCenter;
        float segLen = (Mathf.PI * 2f * radius / segments) * 1.18f;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "Seg_" + i;
            seg.transform.SetParent(ring.transform, false);
            seg.transform.localPosition = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            seg.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f);
            seg.transform.localScale = new Vector3(segLen, 0.0035f, 0.0035f);
            seg.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(seg.GetComponent<Collider>());
        }
    }

    static void BuildLens(Transform parent, Vector3 localCenter, Material mat)
    {
        var lens = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lens.name = "VisibleLens";
        lens.transform.SetParent(parent, false);
        lens.transform.localPosition = localCenter;
        lens.transform.localScale = new Vector3(0.09f, 0.09f, 0.012f);
        lens.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.DestroyImmediate(lens.GetComponent<Collider>());
    }
}
