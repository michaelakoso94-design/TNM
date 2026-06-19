using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class BuildPlayerInventory
{
    const int HIDDEN_UV_LAYER = 8;

    [MenuItem("Tools/Bank Vault/Wrist UI Mode/HUD (always visible)")]
    public static void SetHudMode()
    {
        var origin = Object.FindFirstObjectByType<XROrigin>();
        var canvas = GameObject.Find("Wrist_Canvas");
        if (origin == null || origin.Camera == null || canvas == null)
        {
            EditorUtility.DisplayDialog("Wrist UI Mode",
                "Run 'Add Player Inventory' first.", "OK");
            return;
        }
        canvas.transform.SetParent(origin.Camera.transform, false);
        canvas.transform.localPosition = new Vector3(0.18f, -0.16f, 0.55f);
        canvas.transform.localRotation = Quaternion.Euler(20f, -20f, 0f);
        canvas.transform.localScale = Vector3.one * 0.0006f;
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BankVault] Wrist UI re-parented to camera (HUD mode). Always visible in simulator.");
    }

    [MenuItem("Tools/Bank Vault/Wrist UI Mode/Left Wrist (VR)")]
    public static void SetWristMode()
    {
        var origin = Object.FindFirstObjectByType<XROrigin>();
        var canvas = GameObject.Find("Wrist_Canvas");
        if (origin == null || canvas == null)
        {
            EditorUtility.DisplayDialog("Wrist UI Mode",
                "Run 'Add Player Inventory' first.", "OK");
            return;
        }
        var leftHand = FindLeftHand(origin);
        if (leftHand == null)
        {
            EditorUtility.DisplayDialog("Wrist UI Mode",
                "Left controller not found in rig.", "OK");
            return;
        }
        canvas.transform.SetParent(leftHand, false);
        canvas.transform.localPosition = new Vector3(0f, 0.04f, -0.08f);
        canvas.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
        canvas.transform.localScale = Vector3.one * 0.0004f;
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BankVault] Wrist UI re-parented to left controller (Wrist mode).");
    }

    [MenuItem("Tools/Bank Vault/Add Player Inventory")]
    public static void Build()
    {
        EnsureLayer(HIDDEN_UV_LAYER, "HiddenUV");

        var origin = Object.FindFirstObjectByType<XROrigin>();
        if (origin == null)
        {
            EditorUtility.DisplayDialog("Player Inventory",
                "No XR Origin found in scene. Run 'Add XR Origin' first.", "OK");
            return;
        }
        var cam = origin.Camera;
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Player Inventory",
                "XR Origin has no Camera assigned.", "OK");
            return;
        }

        CleanupExisting(origin);

        // Glasses are now built by 'Add Vest + Glasses' as a grabbable 3D object.
        // This builder no longer creates a face-mounted model.
        var leftHand = FindLeftHand(origin);
        var canvasGo = BuildWristCanvas(leftHand != null ? leftHand : cam.transform,
            out Button toggleBtn, out Text toggleLabel,
            out RectTransform itemsContainer, out RectTransform itemRowTemplate);
        BuildHiddenUVNote();
        EnsureEventSystem();

        var inv = origin.gameObject.AddComponent<PlayerInventory>();
        inv.xrCamera = cam;
        inv.hiddenUVLayer = HIDDEN_UV_LAYER;

        var ui = canvasGo.AddComponent<WristInventoryUI>();
        ui.inventory = inv;
        ui.toggleButton = toggleBtn;
        ui.toggleButtonLabel = toggleLabel;
        ui.itemsContainer = itemsContainer;
        ui.itemRowTemplate = itemRowTemplate;

        UnityEventTools.AddPersistentListener(toggleBtn.onClick, inv.ToggleOpen);

        cam.cullingMask &= ~(1 << HIDDEN_UV_LAYER);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = inv.gameObject;
        Debug.Log("[BankVault] Player Inventory added. Open it via the wrist UI 'Open' button, then tap items to use.");
    }

    static void EnsureLayer(int index, string name)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var layers = so.FindProperty("layers");
        if (layers == null || layers.arraySize <= index) return;
        var slot = layers.GetArrayElementAtIndex(index);
        if (slot.stringValue != name)
        {
            slot.stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void CleanupExisting(XROrigin origin)
    {
        var oldInv = origin.GetComponent<PlayerInventory>();
        if (oldInv != null) Object.DestroyImmediate(oldInv);
        var oldGlasses = GameObject.Find("Glasses_Model");
        if (oldGlasses != null) Object.DestroyImmediate(oldGlasses);
        var oldCanvas = GameObject.Find("Wrist_Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        var oldNote = GameObject.Find("Hidden_UV_Note");
        if (oldNote != null) Object.DestroyImmediate(oldNote);
    }

    static GameObject BuildGlassesModel(Transform parent)
    {
        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        var lensMat = new Material(litShader) { name = "Lens", color = new Color(0.05f, 0.05f, 0.1f) };
        var frameMat = new Material(litShader) { name = "Frame", color = new Color(0.15f, 0.08f, 0.04f) };

        var root = new GameObject("Glasses_Model");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0, -0.012f, 0.07f);
        root.transform.localRotation = Quaternion.identity;

        GameObject Lens(string name, float x)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = name;
            g.transform.SetParent(root.transform, false);
            g.transform.localPosition = new Vector3(x, 0, 0);
            g.transform.localScale = new Vector3(0.035f, 0.035f, 0.006f);
            g.GetComponent<MeshRenderer>().sharedMaterial = lensMat;
            Object.DestroyImmediate(g.GetComponent<Collider>());
            return g;
        }
        Lens("Lens_L", -0.032f);
        Lens("Lens_R",  0.032f);

        var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.SetParent(root.transform, false);
        bridge.transform.localPosition = Vector3.zero;
        bridge.transform.localScale = new Vector3(0.03f, 0.004f, 0.005f);
        bridge.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        Object.DestroyImmediate(bridge.GetComponent<Collider>());

        root.SetActive(false);
        return root;
    }

    static Transform FindLeftHand(XROrigin origin)
    {
        foreach (var t in origin.transform.GetComponentsInChildren<Transform>(true))
        {
            var n = t.name.ToLowerInvariant();
            if ((n.Contains("left") && (n.Contains("hand") || n.Contains("controller")))
                || n == "lefthand" || n == "left")
            {
                return t;
            }
        }
        return null;
    }

    static GameObject BuildWristCanvas(Transform parent,
        out Button toggleBtn, out Text toggleLabel,
        out RectTransform itemsContainer, out RectTransform itemRowTemplate)
    {
        var canvasGo = new GameObject("Wrist_Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(parent, false);
        canvasGo.transform.localPosition = new Vector3(0f, 0.04f, -0.08f);
        canvasGo.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.0004f;

        var rect = canvasGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 280);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Background
        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasGo.transform, false);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

        // Header bar (title + toggle button) — fixed 60px from top
        var header = new GameObject("Header", typeof(RectTransform));
        header.transform.SetParent(canvasGo.transform, false);
        var hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0, 1); hRect.anchorMax = new Vector2(1, 1);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.anchoredPosition = Vector2.zero;
        hRect.sizeDelta = new Vector2(0, 60);

        var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(header.transform, false);
        var tRect = title.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0, 0); tRect.anchorMax = new Vector2(0.65f, 1);
        tRect.sizeDelta = Vector2.zero;
        tRect.anchoredPosition = new Vector2(10, 0);
        var tTxt = title.GetComponent<Text>();
        tTxt.text = "Inventory"; tTxt.alignment = TextAnchor.MiddleLeft; tTxt.color = Color.white;
        tTxt.font = font; tTxt.fontSize = 26;

        var toggleGo = new GameObject("ToggleButton", typeof(RectTransform), typeof(Image), typeof(Button));
        toggleGo.transform.SetParent(header.transform, false);
        var togRect = toggleGo.GetComponent<RectTransform>();
        togRect.anchorMin = new Vector2(0.66f, 0.15f); togRect.anchorMax = new Vector2(0.96f, 0.85f);
        togRect.sizeDelta = Vector2.zero;
        toggleGo.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.35f, 1f);
        toggleBtn = toggleGo.GetComponent<Button>();

        var togLblGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        togLblGo.transform.SetParent(toggleGo.transform, false);
        var togLblRect = togLblGo.GetComponent<RectTransform>();
        togLblRect.anchorMin = Vector2.zero; togLblRect.anchorMax = Vector2.one; togLblRect.sizeDelta = Vector2.zero;
        toggleLabel = togLblGo.GetComponent<Text>();
        toggleLabel.text = "Open"; toggleLabel.alignment = TextAnchor.MiddleCenter; toggleLabel.color = Color.white;
        toggleLabel.font = font; toggleLabel.fontSize = 22;

        // Items container — fills space below header, holds dynamic rows in a VerticalLayoutGroup
        var listGo = new GameObject("ItemsList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        listGo.transform.SetParent(canvasGo.transform, false);
        itemsContainer = listGo.GetComponent<RectTransform>();
        itemsContainer.anchorMin = new Vector2(0, 0); itemsContainer.anchorMax = new Vector2(1, 1);
        itemsContainer.pivot = new Vector2(0.5f, 1f);
        itemsContainer.offsetMin = new Vector2(10, 10);
        itemsContainer.offsetMax = new Vector2(-10, -70); // top margin reserves the 60px header
        var vlg = listGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        var csf = listGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        listGo.SetActive(false); // closed by default

        // Item row template (hidden, used by WristInventoryUI as prefab)
        var rowGo = new GameObject("ItemRowTemplate", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        rowGo.transform.SetParent(listGo.transform, false);
        var rowRect = rowGo.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 50);
        rowGo.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.32f, 1f);
        var le = rowGo.GetComponent<LayoutElement>();
        le.preferredHeight = 50;
        le.minHeight = 50;
        itemRowTemplate = rowRect;

        var rowLblGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        rowLblGo.transform.SetParent(rowGo.transform, false);
        var rowLblRect = rowLblGo.GetComponent<RectTransform>();
        rowLblRect.anchorMin = Vector2.zero; rowLblRect.anchorMax = Vector2.one; rowLblRect.sizeDelta = Vector2.zero;
        var rowLbl = rowLblGo.GetComponent<Text>();
        rowLbl.text = "Item"; rowLbl.alignment = TextAnchor.MiddleCenter; rowLbl.color = Color.white;
        rowLbl.font = font; rowLbl.fontSize = 22;

        rowGo.SetActive(false);

        return canvasGo;
    }

    static void BuildHiddenUVNote()
    {
        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        var note = GameObject.CreatePrimitive(PrimitiveType.Cube);
        note.name = "Hidden_UV_Note";
        note.transform.position = new Vector3(1.94f, 2.0f, 10.7f);
        note.transform.rotation = Quaternion.Euler(0, -90, 0);
        note.transform.localScale = new Vector3(0.5f, 0.18f, 0.004f);
        var mat = new Material(litShader) { name = "HiddenUV_Mat", color = new Color(0.7f, 0.15f, 1f) };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.7f, 0.15f, 1f) * 6f);
        note.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.DestroyImmediate(note.GetComponent<Collider>());
        note.layer = HIDDEN_UV_LAYER;
    }

    static void EnsureEventSystem()
    {
        var existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<XRUIInputModule>();
    }
}
