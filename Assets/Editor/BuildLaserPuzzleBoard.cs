using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildLaserPuzzleBoard
{
    const float ButtonSpacing = 0.095f;

    [MenuItem("Tools/Bank Vault/Build Laser Puzzle Board (L0345)")]
    public static void Build()
    {
        var lasersRoot = GameObject.Find("Lasers");
        if (lasersRoot == null)
        {
            EditorUtility.DisplayDialog("Laser Puzzle Board",
                "No 'Lasers' GameObject in scene. Run 'Build Prototype Scene' first.", "OK");
            return;
        }

        // Replace the old simple board and any previous puzzle board.
        var oldBoard = GameObject.Find("LaserControlBoard");
        if (oldBoard != null) Object.DestroyImmediate(oldBoard);
        var oldPuzzle = GameObject.Find("LaserPuzzleBoard");
        if (oldPuzzle != null) Object.DestroyImmediate(oldPuzzle);

        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        Material MakeMat(string name, Color color, bool emissive = false, float ei = 2.5f)
        {
            var m = new Material(litShader) { name = name, color = color };
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * ei);
            }
            return m;
        }

        var bodyMat       = MakeMat("PuzzleBody",   new Color(0.07f, 0.07f, 0.09f));
        var litMat        = MakeMat("PuzzleLit",    new Color(0.1f, 0.85f, 0.95f), true, 3f);
        var unlitMat      = MakeMat("PuzzleUnlit",  new Color(0.35f, 0.35f, 0.38f));
        var lampArmedMat  = MakeMat("LampArmed",    new Color(1f, 0.12f, 0.08f), true, 3f);
        var lampSolvedMat = MakeMat("LampSolved",   new Color(0.1f, 0.95f, 0.2f), true, 3f);

        // Board on the right wall before the laser zone (player start z=0.8, first laser z=3.5).
        // Local +Z faces into the hallway; player-left = local +X.
        var board = new GameObject("LaserPuzzleBoard");
        Undo.RegisterCreatedObjectUndo(board, "Build Laser Puzzle Board");
        board.transform.position = new Vector3(1.97f, 1.4f, 2.5f);
        board.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(board.transform, false);
        body.transform.localScale = new Vector3(0.58f, 0.58f, 0.05f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        var puzzle = board.AddComponent<LaserPuzzleBoard>();
        puzzle.lasersRoot = lasersRoot;
        puzzle.litMaterial = litMat;
        puzzle.unlitMaterial = unlitMat;
        puzzle.lampArmedMaterial = lampArmedMat;
        puzzle.lampSolvedMaterial = lampSolvedMat;

        // 5x5 toggle buttons. Row 0 = top, col 0 = leftmost as the player sees it.
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                var btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                btn.name = $"Btn_r{row}c{col}";
                btn.transform.SetParent(board.transform, false);
                btn.transform.localPosition = new Vector3(
                    (2 - col) * ButtonSpacing,
                    (2 - row) * ButtonSpacing,
                    0.038f);
                btn.transform.localScale = new Vector3(0.08f, 0.08f, 0.025f);
                btn.GetComponent<MeshRenderer>().sharedMaterial = unlitMat;

                var box = btn.GetComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(1f, 1f, 2.5f); // deeper hitbox = easier to poke

                var cell = btn.AddComponent<LaserPuzzleButton>();
                cell.row = row;
                cell.col = col;
                cell.board = puzzle;
            }
        }

        // Title "L0345" above the grid. TextMesh fronts -Z, so flip it toward the player.
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(board.transform, false);
        titleGo.transform.localPosition = new Vector3(0f, 0.345f, 0.03f);
        titleGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var tm = titleGo.AddComponent<TextMesh>();
        tm.text = "L0345";
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 64;
        tm.characterSize = 0.008f;
        tm.color = new Color(0.9f, 0.85f, 0.6f);
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        titleGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        // Status lamp next to the title: red = armed, green = solved.
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamp.name = "StatusLamp";
        lamp.transform.SetParent(board.transform, false);
        lamp.transform.localPosition = new Vector3(0.24f, 0.345f, 0.025f);
        lamp.transform.localScale = new Vector3(0.035f, 0.035f, 0.02f);
        lamp.GetComponent<MeshRenderer>().sharedMaterial = lampArmedMat;
        Object.DestroyImmediate(lamp.GetComponent<Collider>());
        puzzle.statusLamp = lamp.GetComponent<MeshRenderer>();

        // Click / success sounds reused from the keypad asset (optional).
        var audio = board.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 1f;
        puzzle.audioSource = audio;
        puzzle.toggleClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Keypad/Sounds/sfx_keypadClick.wav");
        puzzle.solvedClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Keypad/Sounds/sfx_keypadGranted.wav");

        int tips = AddPokeTips();

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = board;
        Debug.Log($"[LaserPuzzle] Board built on right wall at z=2.5 with 5x5 grid, poke tips added: {tips}. " +
                  "Solution rows: " + string.Join(",", puzzle.solutionRows));
    }

    // Small trigger spheres on both controller tips so the player can poke buttons.
    static int AddPokeTips()
    {
        var rig = Object.FindFirstObjectByType<OVRCameraRig>();
        if (rig == null)
        {
            Debug.LogWarning("[LaserPuzzle] No OVRCameraRig found — poke tips not added.");
            return 0;
        }

        int count = 0;
        foreach (var anchorName in new[] { "LeftControllerAnchor", "RightControllerAnchor" })
        {
            var anchor = FindDeep(rig.transform, anchorName);
            if (anchor == null)
            {
                Debug.LogWarning($"[LaserPuzzle] Anchor '{anchorName}' not found in rig.");
                continue;
            }
            if (anchor.GetComponentInChildren<PokeTip>() != null) { count++; continue; }

            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "PokeTip";
            tip.transform.SetParent(anchor, false);
            tip.transform.localPosition = new Vector3(0f, 0f, 0.04f); // slightly past the controller nose
            tip.transform.localScale = Vector3.one * 0.02f;

            var mr = tip.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { name = "PokeTipMat", color = new Color(0.9f, 0.9f, 0.9f) };

            var rb = tip.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            tip.AddComponent<PokeTip>();
            count++;
        }
        return count;
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var hit = FindDeep(root.GetChild(i), name);
            if (hit != null) return hit;
        }
        return null;
    }
}
