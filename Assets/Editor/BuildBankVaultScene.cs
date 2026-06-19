using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildBankVaultScene
{
    const int HIDDEN_UV_LAYER = 8;

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

    [MenuItem("Tools/Bank Vault/Build Prototype Scene")]
    public static void Build()
    {
        EnsureLayer(HIDDEN_UV_LAYER, "HiddenUV");

        var scene = EditorSceneManager.GetActiveScene();

        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null)
        {
            EditorUtility.DisplayDialog("Build Bank Vault",
                "URP Lit shader not found. This project must use URP.", "OK");
            return;
        }

        Material MakeMat(string name, Color color, bool emissive = false, float ei = 3f)
        {
            var m = new Material(litShader) { name = name, color = color };
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * ei);
            }
            return m;
        }

        var floorMat   = MakeMat("Floor",          new Color(0.32f, 0.30f, 0.28f));
        var wallMat    = MakeMat("Wall",           new Color(0.82f, 0.76f, 0.62f));
        var ceilMat    = MakeMat("Ceiling",        new Color(0.7f, 0.7f, 0.7f));
        var doorMat    = MakeMat("VaultDoor",      new Color(0.22f, 0.22f, 0.25f));
        var brassMat   = MakeMat("Brass",          new Color(0.78f, 0.62f, 0.22f));
        var padMat     = MakeMat("KeypadBody",     new Color(0.08f, 0.08f, 0.08f));
        var keyMat     = MakeMat("Key",            new Color(0.88f, 0.88f, 0.88f));
        var dispMat    = MakeMat("KeypadDisplay",  new Color(0.1f, 0.6f, 0.15f), true, 4f);
        var laserMat   = MakeMat("Laser",          new Color(1f, 0.03f, 0.03f),  true, 6f);
        var emitterMat = MakeMat("LaserEmitter",   new Color(0.12f, 0.12f, 0.14f));

        var existing = GameObject.Find("BankVault");
        if (existing != null) Object.DestroyImmediate(existing);
        var root = new GameObject("BankVault");
        Undo.RegisterCreatedObjectUndo(root, "Build BankVault");

        GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 p, Vector3 s, Quaternion r, Material m)
        {
            var g = GameObject.CreatePrimitive(t);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = p;
            g.transform.localRotation = r;
            g.transform.localScale = s;
            g.GetComponent<MeshRenderer>().sharedMaterial = m;
            return g;
        }

        // Hallway 4w x 3h x 12 long. Player at z=0, vault at z=12.
        Prim(PrimitiveType.Cube, "Floor",      root.transform, new Vector3(0, -0.05f, 6f),   new Vector3(4, 0.1f, 12), Quaternion.identity, floorMat);
        Prim(PrimitiveType.Cube, "Ceiling",    root.transform, new Vector3(0,  3.05f, 6f),   new Vector3(4, 0.1f, 12), Quaternion.identity, ceilMat);
        Prim(PrimitiveType.Cube, "Wall_Left",  root.transform, new Vector3(-2.05f, 1.5f, 6f), new Vector3(0.1f, 3f, 12), Quaternion.identity, wallMat);
        Prim(PrimitiveType.Cube, "Wall_Right", root.transform, new Vector3( 2.05f, 1.5f, 6f), new Vector3(0.1f, 3f, 12), Quaternion.identity, wallMat);
        Prim(PrimitiveType.Cube, "Wall_Back",  root.transform, new Vector3(0, 1.5f, -0.05f),  new Vector3(4, 3f, 0.1f), Quaternion.identity, wallMat);
        Prim(PrimitiveType.Cube, "Wall_Front", root.transform, new Vector3(0, 1.5f, 12.05f),  new Vector3(4, 3f, 0.1f), Quaternion.identity, wallMat);

        // Vault door at front wall (rectangular leaf).
        var vault = new GameObject("VaultDoor");
        vault.transform.SetParent(root.transform, false);
        vault.transform.localPosition = new Vector3(0, 1.5f, 11.95f);
        BuildRectangularDoor(vault.transform, doorMat, brassMat);

        // Keypad mounted on right wall, just before door.
        var pad = new GameObject("Keypad");
        pad.transform.SetParent(root.transform, false);
        pad.transform.localPosition = new Vector3(1.95f, 1.3f, 10.7f);
        pad.transform.localRotation = Quaternion.Euler(0, -90, 0);
        Prim(PrimitiveType.Cube, "Pad_Body",    pad.transform, Vector3.zero,                  new Vector3(0.32f, 0.46f, 0.06f),  Quaternion.identity, padMat);
        Prim(PrimitiveType.Cube, "Pad_Display", pad.transform, new Vector3(0, 0.13f, -0.035f), new Vector3(0.26f, 0.08f, 0.015f), Quaternion.identity, dispMat);
        string[] labels = { "1","2","3","4","5","6","7","8","9","star","0","hash" };
        int idx = 0;
        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 3; col++)
            {
                float x = (col - 1) * 0.085f;
                float y = -0.05f - row * 0.075f;
                Prim(PrimitiveType.Cube, "Key_" + labels[idx], pad.transform,
                    new Vector3(x, y, -0.035f), new Vector3(0.065f, 0.055f, 0.018f), Quaternion.identity, keyMat);
                idx++;
            }

        // Wall-mounted security lasers: 3 from each wall, static emitters, beams pivot up/down.
        var lasers = new GameObject("Lasers");
        lasers.transform.SetParent(root.transform, false);

        float[] leftZs  = { 3.5f, 6f, 8.5f };
        float[] rightZs = { 4.5f, 7f, 9.5f };
        float[] phasesL = { 0f, 0.7f, 1.4f };
        float[] phasesR = { 0.35f, 1.05f, 1.75f };

        for (int i = 0; i < leftZs.Length; i++)
        {
            BuildLaser(lasers.transform, "Laser_L_" + i, new Vector3(-2f, 1.5f, leftZs[i]),  fromLeft: true,  phase: phasesL[i], laserMat, emitterMat);
            BuildLaser(lasers.transform, "Laser_R_" + i, new Vector3( 2f, 1.5f, rightZs[i]), fromLeft: false, phase: phasesR[i], laserMat, emitterMat);
        }

        // Hallway lights.
        var lightsP = new GameObject("HallLights");
        lightsP.transform.SetParent(root.transform, false);
        for (int i = 0; i < 4; i++)
        {
            var lgo = new GameObject("HallLight_" + i);
            lgo.transform.SetParent(lightsP.transform, false);
            lgo.transform.localPosition = new Vector3(0, 2.8f, 1.5f + i * 3f);
            var lt = lgo.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 6f;
            lt.intensity = 2.2f;
            lt.color = new Color(1f, 0.94f, 0.8f);
        }
        var spotGo = new GameObject("VaultSpot");
        spotGo.transform.SetParent(lightsP.transform, false);
        spotGo.transform.localPosition = new Vector3(0, 2.8f, 9.5f);
        spotGo.transform.localRotation = Quaternion.Euler(35, 0, 0);
        var spot = spotGo.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.range = 8f;
        spot.spotAngle = 60f;
        spot.intensity = 3f;
        spot.color = new Color(1f, 0.92f, 0.78f);

        // Preview camera at player start (will be replaced by XR Origin later).
        var cam = Camera.main;
        if (cam != null)
        {
            Undo.RecordObject(cam.transform, "Position preview camera");
            cam.transform.position = new Vector3(0, 1.7f, 0.8f);
            cam.transform.rotation = Quaternion.identity;
            cam.fieldOfView = 70f;
        }

        var dl = GameObject.Find("Directional Light");
        if (dl != null)
        {
            var l = dl.GetComponent<Light>();
            if (l != null)
            {
                Undo.RecordObject(l, "Dim directional light");
                l.intensity = 0.3f;
                l.color = new Color(0.85f, 0.85f, 1f);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BankVault] Prototype scene built and saved: " + scene.path);
        Selection.activeGameObject = root;
    }

    static GameObject PrimS(PrimitiveType t, string name, Transform parent, Vector3 p, Vector3 s, Quaternion r, Material m)
    {
        var g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = p;
        g.transform.localRotation = r;
        g.transform.localScale = s;
        g.GetComponent<MeshRenderer>().sharedMaterial = m;
        return g;
    }

    // Builds a rectangular vault door (leaf + brass frame + central locking wheel + perimeter bolts)
    // as children of "vault". The hallway-facing side is local -z. Reused by the full scene build
    // and by the standalone "Replace Door" menu command.
    public static void BuildRectangularDoor(Transform vault, Material doorMat, Material brassMat)
    {
        // Door leaf.
        PrimS(PrimitiveType.Cube, "Door_Slab", vault, Vector3.zero, new Vector3(2.2f, 2.6f, 0.16f), Quaternion.identity, doorMat);

        // Brass frame around the leaf.
        PrimS(PrimitiveType.Cube, "Door_Frame_Top",    vault, new Vector3(0f,  1.38f, -0.02f), new Vector3(2.56f, 0.2f,  0.12f), Quaternion.identity, brassMat);
        PrimS(PrimitiveType.Cube, "Door_Frame_Bottom", vault, new Vector3(0f, -1.38f, -0.02f), new Vector3(2.56f, 0.2f,  0.12f), Quaternion.identity, brassMat);
        PrimS(PrimitiveType.Cube, "Door_Frame_Left",   vault, new Vector3(-1.18f, 0f, -0.02f), new Vector3(0.2f,  2.96f, 0.12f), Quaternion.identity, brassMat);
        PrimS(PrimitiveType.Cube, "Door_Frame_Right",  vault, new Vector3( 1.18f, 0f, -0.02f), new Vector3(0.2f,  2.96f, 0.12f), Quaternion.identity, brassMat);

        // Bar-shaped pull handle on the right edge (clears the pattern panel).
        BuildRodHandle(vault, brassMat);

        // Square black/white pattern panel on the door face.
        BuildDoorPattern(vault, DoorPattern);

        // Bolts along the rectangular perimeter.
        float bx = 0.95f, by = 1.15f;
        float[] topXs = { -0.9f, -0.45f, 0f, 0.45f, 0.9f };
        for (int i = 0; i < topXs.Length; i++)
        {
            PrimS(PrimitiveType.Sphere, "Bolt_T" + i, vault, new Vector3(topXs[i],  by, -0.05f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.identity, brassMat);
            PrimS(PrimitiveType.Sphere, "Bolt_B" + i, vault, new Vector3(topXs[i], -by, -0.05f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.identity, brassMat);
        }
        float[] sideYs = { -0.6f, 0f, 0.6f };
        for (int i = 0; i < sideYs.Length; i++)
        {
            PrimS(PrimitiveType.Sphere, "Bolt_L" + i, vault, new Vector3(-bx, sideYs[i], -0.05f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.identity, brassMat);
            PrimS(PrimitiveType.Sphere, "Bolt_R" + i, vault, new Vector3( bx, sideYs[i], -0.05f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.identity, brassMat);
        }
    }

    static Material MakeMatS(string name, Color color, bool emissive = false, float ei = 1.5f)
    {
        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        var m = new Material(litShader) { name = name, color = color };
        if (emissive)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * ei);
        }
        return m;
    }

    // Door pattern as the player reads it: 4 rows top-to-bottom, 7 columns left-to-right.
    // 'B' = black cell, 'W' = white cell. Edit and re-run the menu command to change it.
    public static readonly string[] DoorPattern =
    {
        "WBWWWWBB",
        "BWWBBWWB",
        "WBBBWWBW",
        "WWBBWBWB",
    };

    // Vertical bar pull-handle mounted on the right edge of the door, standing proud of the face.
    public static void BuildRodHandle(Transform vault, Material metalMat)
    {
        const float x = 0.92f;
        PrimS(PrimitiveType.Cube,     "Door_Handle_BracketTop",    vault, new Vector3(x,  0.55f, -0.13f), new Vector3(0.08f, 0.06f, 0.14f), Quaternion.identity, metalMat);
        PrimS(PrimitiveType.Cube,     "Door_Handle_BracketBottom", vault, new Vector3(x, -0.55f, -0.13f), new Vector3(0.08f, 0.06f, 0.14f), Quaternion.identity, metalMat);
        PrimS(PrimitiveType.Cylinder, "Door_Handle_Rod",           vault, new Vector3(x,  0f,    -0.18f), new Vector3(0.05f, 0.6f,  0.05f), Quaternion.identity, metalMat);
    }

    // Square pattern panel (white border + black grid base + black/white cells) on the door face.
    public static void BuildDoorPattern(Transform vault, string[] pattern)
    {
        int rows = pattern.Length;
        int cols = rows > 0 ? pattern[0].Length : 0;
        if (rows == 0 || cols == 0) return;

        var whiteMat = MakeMatS("PatternWhite", new Color(0.95f, 0.95f, 0.95f), true, 1.2f);
        var blackMat = MakeMatS("PatternBlack", new Color(0.03f, 0.03f, 0.03f));

        // Panel placed on the door face, left of the right-edge handle so the handle never covers it.
        Vector3 center = new Vector3(-0.12f, 0.12f, 0f);
        const float width = 1.5f, height = 0.86f, gap = 0.018f;

        PrimS(PrimitiveType.Cube, "Pattern_Border", vault, center + new Vector3(0, 0, -0.085f), new Vector3(width + 0.06f, height + 0.06f, 0.02f), Quaternion.identity, whiteMat);
        PrimS(PrimitiveType.Cube, "Pattern_Base",   vault, center + new Vector3(0, 0, -0.095f), new Vector3(width, height, 0.02f), Quaternion.identity, blackMat);

        float cellW = width / cols, cellH = height / rows;
        float tileW = cellW - gap, tileH = cellH - gap;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols && col < pattern[row].Length; col++)
            {
                bool black = pattern[row][col] == 'B' || pattern[row][col] == 'b' || pattern[row][col] == '1';
                float cx = center.x - width / 2f + cellW * (col + 0.5f);
                float cy = center.y + height / 2f - cellH * (row + 0.5f);
                PrimS(PrimitiveType.Cube, $"Pattern_{row}_{col}", vault,
                    new Vector3(cx, cy, -0.105f), new Vector3(tileW, tileH, 0.02f), Quaternion.identity,
                    black ? blackMat : whiteMat);
            }
        }
    }

    static void BuildLaser(Transform parent, string name, Vector3 wallPos, bool fromLeft, float phase, Material laserMat, Material emitterMat)
    {
        var assembly = new GameObject(name);
        assembly.transform.SetParent(parent, false);
        assembly.transform.localPosition = wallPos;

        // Static emitter mount on wall.
        var emitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emitter.name = "Emitter";
        emitter.transform.SetParent(assembly.transform, false);
        emitter.transform.localPosition = new Vector3(fromLeft ? -0.05f : 0.05f, 0f, 0f);
        emitter.transform.localScale = new Vector3(0.1f, 0.15f, 0.15f);
        emitter.GetComponent<MeshRenderer>().sharedMaterial = emitterMat;
        Object.DestroyImmediate(emitter.GetComponent<Collider>());

        // BeamPivot rotates to sweep the beam; sits at the wall anchor.
        var pivot = new GameObject("BeamPivot");
        pivot.transform.SetParent(assembly.transform, false);

        // Beam cylinder extends from pivot across the hallway, overshooting opposite wall.
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "Beam";
        beam.transform.SetParent(pivot.transform, false);
        if (fromLeft)
        {
            beam.transform.localRotation = Quaternion.Euler(0, 0, -90);
            beam.transform.localPosition = new Vector3(2.25f, 0f, 0f);
        }
        else
        {
            beam.transform.localRotation = Quaternion.Euler(0, 0, 90);
            beam.transform.localPosition = new Vector3(-2.25f, 0f, 0f);
        }
        beam.transform.localScale = new Vector3(0.04f, 2.25f, 0.04f);
        beam.GetComponent<MeshRenderer>().sharedMaterial = laserMat;
        beam.GetComponent<Collider>().isTrigger = true;
        beam.layer = HIDDEN_UV_LAYER;

        var detector = beam.AddComponent<LaserBeam>();
        detector.emitterId = name;

        var sweep = pivot.AddComponent<LaserSweep>();
        sweep.minAngle = -10f;
        sweep.maxAngle = 10f;
        sweep.period = 4f;
        sweep.phase = phase;
    }
}
