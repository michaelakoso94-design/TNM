using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class BuildLaserControlBoard
{
    [MenuItem("Tools/Bank Vault/Add Laser Control Board")]
    public static void Build()
    {
        var lasersRoot = GameObject.Find("Lasers");
        if (lasersRoot == null)
        {
            EditorUtility.DisplayDialog("Laser Control Board",
                "No 'Lasers' GameObject in scene. Run 'Build Prototype Scene' first.", "OK");
            return;
        }

        var existing = GameObject.Find("LaserControlBoard");
        if (existing != null) Object.DestroyImmediate(existing);

        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        var bodyMat   = new Material(litShader) { name = "BoardBody", color = new Color(0.08f, 0.08f, 0.10f) };
        var trimMat   = new Material(litShader) { name = "BoardTrim", color = new Color(0.45f, 0.4f, 0.18f) };

        // Board parent on right wall, before laser zone. Player at z=0.8, first laser at z=3.5.
        var board = new GameObject("LaserControlBoard");
        board.transform.position = new Vector3(1.95f, 1.4f, 2.5f);
        board.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        // Body cube
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(board.transform, false);
        body.transform.localScale = new Vector3(0.55f, 0.42f, 0.06f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Brass trim frame around the panel for a more "industrial" look
        for (int side = 0; side < 4; side++)
        {
            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "Trim_" + side;
            trim.transform.SetParent(board.transform, false);
            trim.GetComponent<MeshRenderer>().sharedMaterial = trimMat;
            Object.DestroyImmediate(trim.GetComponent<Collider>());
            switch (side)
            {
                case 0: // top
                    trim.transform.localPosition = new Vector3(0, 0.21f, -0.03f);
                    trim.transform.localScale = new Vector3(0.55f, 0.015f, 0.012f); break;
                case 1: // bottom
                    trim.transform.localPosition = new Vector3(0, -0.21f, -0.03f);
                    trim.transform.localScale = new Vector3(0.55f, 0.015f, 0.012f); break;
                case 2: // left
                    trim.transform.localPosition = new Vector3(-0.275f, 0, -0.03f);
                    trim.transform.localScale = new Vector3(0.015f, 0.42f, 0.012f); break;
                case 3: // right
                    trim.transform.localPosition = new Vector3(0.275f, 0, -0.03f);
                    trim.transform.localScale = new Vector3(0.015f, 0.42f, 0.012f); break;
            }
        }

        // World-space Canvas on the panel face. Following the same local-Z convention as the keypad.
        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(board.transform, false);
        canvasGo.transform.localPosition = new Vector3(0f, 0f, -0.035f);
        canvasGo.transform.localRotation = Quaternion.identity;
        canvasGo.transform.localScale = Vector3.one * 0.001f;
        var crect = canvasGo.GetComponent<RectTransform>();
        crect.sizeDelta = new Vector2(480, 360);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Title
        var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(canvasGo.transform, false);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.78f); titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = Vector2.zero;
        var titleTxt = title.GetComponent<Text>();
        titleTxt.text = "SECURITY LASERS"; titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.9f, 0.85f, 0.6f); titleTxt.font = font; titleTxt.fontSize = 38;

        // Status row: LED + text
        var ledGo = new GameObject("StatusLED", typeof(RectTransform), typeof(Image));
        ledGo.transform.SetParent(canvasGo.transform, false);
        var ledRect = ledGo.GetComponent<RectTransform>();
        ledRect.anchorMin = new Vector2(0.12f, 0.55f); ledRect.anchorMax = new Vector2(0.22f, 0.72f);
        ledRect.sizeDelta = Vector2.zero;
        var led = ledGo.GetComponent<Image>();
        led.color = new Color(1f, 0.15f, 0.1f);

        var stGo = new GameObject("StatusText", typeof(RectTransform), typeof(Text));
        stGo.transform.SetParent(canvasGo.transform, false);
        var stRect = stGo.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0.26f, 0.55f); stRect.anchorMax = new Vector2(0.92f, 0.72f);
        stRect.sizeDelta = Vector2.zero;
        var stTxt = stGo.GetComponent<Text>();
        stTxt.text = "STATUS: ARMED"; stTxt.alignment = TextAnchor.MiddleLeft;
        stTxt.color = Color.white; stTxt.font = font; stTxt.fontSize = 30;

        // Big toggle button
        var btnGo = new GameObject("ToggleButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(canvasGo.transform, false);
        var btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.15f, 0.1f); btnRect.anchorMax = new Vector2(0.85f, 0.42f);
        btnRect.sizeDelta = Vector2.zero;
        btnGo.GetComponent<Image>().color = new Color(0.4f, 0.08f, 0.08f);
        var btn = btnGo.GetComponent<Button>();

        var btnLblGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        btnLblGo.transform.SetParent(btnGo.transform, false);
        var btnLblRect = btnLblGo.GetComponent<RectTransform>();
        btnLblRect.anchorMin = Vector2.zero; btnLblRect.anchorMax = Vector2.one; btnLblRect.sizeDelta = Vector2.zero;
        var btnLbl = btnLblGo.GetComponent<Text>();
        btnLbl.text = "DISARM"; btnLbl.alignment = TextAnchor.MiddleCenter;
        btnLbl.color = Color.white; btnLbl.font = font; btnLbl.fontSize = 38;

        // Wire up component
        var lcb = board.AddComponent<LaserControlBoard>();
        lcb.lasersRoot = lasersRoot;
        lcb.toggleButton = btn;
        lcb.toggleButtonLabel = btnLbl;
        lcb.statusLED = led;
        lcb.statusText = stTxt;
        UnityEventTools.AddPersistentListener(btn.onClick, lcb.Toggle);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = board;
        Debug.Log("[BankVault] Laser Control Board added on right wall at z=2.5 (before lasers).");
    }
}
