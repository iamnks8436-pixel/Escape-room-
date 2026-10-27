using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// EscapeRoomSceneBuilder is the master automated scene constructor for
/// "VR Escape Room — The Lost Relic" (Phase 3).
/// 
/// Unity Menu:
/// Tools → VR Escape Room → Build Complete Escape Room
/// 
/// Automatically constructs the entire 8x8m stone chamber, medieval architecture, furniture,
/// clues, relic puzzle box with keypad (527), antique brass key, exit door, exterior garden,
/// VR Canvas UI, and Dual VR/Desktop Player Rig from code using primitives and generated materials.
/// </summary>
public static class EscapeRoomSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/EscapeRoom.unity";

    [MenuItem("Tools/VR Escape Room/Build Complete Escape Room", false, 1)]
    [MenuItem("VR Escape Room/1. Build Complete Escape Room", false, 1)]
    public static void MenuBuildCompleteRoom()
    {
        BuildAndSaveScene(!Application.isBatchMode);
    }

    [MenuItem("Tools/VR Escape Room/Add EscapeRoom to Build Settings", false, 2)]
    [MenuItem("VR Escape Room/2. Add Scene to Build Settings", false, 2)]
    public static void AddSceneToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == ScenePath)
            {
                Debug.Log($"[VR Escape Room] {ScenePath} is already in Build Settings.");
                return;
            }
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = newScenes;
        Debug.Log($"<color=green><b>[VR Escape Room]</b> Added {ScenePath} to Build Settings!</color>");
    }

    [MenuItem("Tools/VR Escape Room/Open EscapeRoom Scene", false, 3)]
    [MenuItem("VR Escape Room/3. Open Scene", false, 3)]
    public static void OpenScene()
    {
        if (File.Exists(ScenePath))
        {
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log($"[VR Escape Room] Opened {ScenePath}");
        }
        else
        {
            EditorUtility.DisplayDialog("Scene Missing",
                "The scene does not exist yet. Please run 'Tools -> VR Escape Room -> Build Complete Escape Room'.",
                "OK");
        }
    }

    /// <summary>
    /// Headless entry point for Unity -batchmode CLI builds.
    /// </summary>
    public static void GenerateAndSaveScene()
    {
        BuildAndSaveScene(false);
    }

    public static void BuildAndSaveScene(bool showDialog)
    {
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();
        }

        // Create empty scene to guarantee no stray duplicate cameras/lights
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        BuildCompleteRoomInternal();

        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        if (saved)
        {
            AssetDatabase.Refresh();
            AddSceneToBuildSettings();

            Debug.Log($"<color=green><b>[VR Escape Room]</b> Scene successfully built and saved to: {ScenePath}</color>");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("VR Escape Room Built Successfully",
                    $"The complete 8-panel VR Escape Room scene has been generated!\n\nScene: {ScenePath}\n\nPress PLAY to test immediately in Desktop mode (WASD + Mouse) or put on your VR headset!",
                    "Let's Play!");
            }
        }
        else
        {
            Debug.LogError($"[VR Escape Room] Failed to save scene at {ScenePath}");
        }
    }

    public static void BuildCompleteRoomInternal()
    {
        // 0. Clean up any stray root objects
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l != null && l.transform.parent == null) Object.DestroyImmediate(l.gameObject);
        }
        foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (c != null && c.transform.parent == null) Object.DestroyImmediate(c.gameObject);
        }

        GameObject oldRoot = GameObject.Find("EscapeRoom_Environment");
        if (oldRoot != null) Object.DestroyImmediate(oldRoot);

        // 1. Root Container
        GameObject roomRoot = new GameObject("EscapeRoom_Environment");

        // 2. Procedural Materials
        Material floorMat = CreateMaterial("Mat_StoneFloor", new Color(0.24f, 0.22f, 0.20f), 0.1f, 0.25f);
        Material wallMat = CreateMaterial("Mat_StoneWall", new Color(0.32f, 0.33f, 0.35f), 0.05f, 0.2f);
        Material ceilingMat = CreateMaterial("Mat_StoneCeiling", new Color(0.20f, 0.20f, 0.22f), 0.05f, 0.1f);
        Material beamMat = CreateMaterial("Mat_TimberBeams", new Color(0.25f, 0.16f, 0.10f), 0.05f, 0.3f);
        Material woodFurnitureMat = CreateMaterial("Mat_WoodFurniture", new Color(0.36f, 0.24f, 0.14f), 0.1f, 0.4f);
        Material metalBoxMat = CreateMaterial("Mat_MetalChest", new Color(0.22f, 0.16f, 0.12f), 0.3f, 0.6f);
        Material brassKeyMat = CreateMaterial("Mat_AntiqueBrass", new Color(0.95f, 0.80f, 0.25f), 0.85f, 0.85f);
        Material keypadMat = CreateMaterial("Mat_KeypadHousing", new Color(0.12f, 0.12f, 0.15f), 0.4f, 0.5f);
        Material screenMat = CreateMaterial("Mat_KeypadScreen", new Color(0.04f, 0.07f, 0.09f), 0.1f, 0.9f);
        Material doorMat = CreateMaterial("Mat_HeavyWoodDoor", new Color(0.30f, 0.18f, 0.10f), 0.15f, 0.4f);
        Material parchmentMat = CreateMaterial("Mat_ParchmentPaper", new Color(0.92f, 0.87f, 0.72f), 0.0f, 0.2f);
        Material bookCoverMat = CreateMaterial("Mat_CelestialBookCover", new Color(0.55f, 0.15f, 0.15f), 0.1f, 0.3f);
        Material lanternMat = CreateMaterial("Mat_OilLanternMetal", new Color(0.15f, 0.15f, 0.15f), 0.8f, 0.7f);
        Material stoneArchMat = CreateMaterial("Mat_StoneArchway", new Color(0.38f, 0.38f, 0.40f), 0.05f, 0.25f);
        Material patioMat = CreateMaterial("Mat_PatioPavers", new Color(0.44f, 0.42f, 0.40f), 0.1f, 0.3f);
        Material grassMat = CreateMaterial("Mat_GardenGrass", new Color(0.22f, 0.45f, 0.18f), 0.0f, 0.15f);
        Material rugMat = CreateMaterial("Mat_StudyRug", new Color(0.48f, 0.14f, 0.14f), 0.0f, 0.1f);
        Material foliageMat = CreateMaterial("Mat_GardenFoliage", new Color(0.18f, 0.38f, 0.15f), 0.0f, 0.2f);

        // Load Default Font Asset for TextMeshPro
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // 3. Game Managers
        GameObject managersObj = new GameObject("--- GAME MANAGERS ---");
        managersObj.transform.SetParent(roomRoot.transform);

        GameManager gameManager = managersObj.AddComponent<GameManager>();
        TimerManager timerManager = managersObj.AddComponent<TimerManager>();
        ScoreManager scoreManager = managersObj.AddComponent<ScoreManager>();
        SoundManager soundManager = managersObj.AddComponent<SoundManager>();
        UIManager uiManager = managersObj.AddComponent<UIManager>();

        // Attach XRInteractionManager if XR Interaction Toolkit is installed
        System.Type xrMgrType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRInteractionManager, Unity.XR.Interaction.Toolkit");
        if (xrMgrType != null && Object.FindFirstObjectByType(xrMgrType) == null)
        {
            GameObject xrMgrObj = new GameObject("XR_Interaction_Manager");
            xrMgrObj.transform.SetParent(managersObj.transform);
            xrMgrObj.AddComponent(xrMgrType);
        }

        // 4. Chamber Architecture (8m wide x 8m long x 3.8m high)
        GameObject roomGeo = new GameObject("Chamber_Architecture");
        roomGeo.transform.SetParent(roomRoot.transform);

        // Stone Floor (8x8m)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Stone_Floor";
        floor.transform.SetParent(roomGeo.transform);
        floor.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        floor.transform.localScale = new Vector3(8.0f, 0.1f, 8.0f);
        floor.GetComponent<Renderer>().material = floorMat;

        // Stone Ceiling (8x8m)
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Stone_Ceiling";
        ceiling.transform.SetParent(roomGeo.transform);
        ceiling.transform.localPosition = new Vector3(0f, 3.85f, 0f);
        ceiling.transform.localScale = new Vector3(8.0f, 0.1f, 8.0f);
        ceiling.GetComponent<Renderer>().material = ceilingMat;

        // Heavy Wooden Ceiling Beams (Rafters across ceiling)
        for (int i = -3; i <= 3; i += 2)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = $"Ceiling_Beam_{i}";
            beam.transform.SetParent(roomGeo.transform);
            beam.transform.localPosition = new Vector3(0f, 3.68f, i * 1.15f);
            beam.transform.localScale = new Vector3(8.0f, 0.24f, 0.28f);
            beam.GetComponent<Renderer>().material = beamMat;
        }

        // North Wall (Z = +4.0)
        GameObject wallNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallNorth.name = "Wall_North";
        wallNorth.transform.SetParent(roomGeo.transform);
        wallNorth.transform.localPosition = new Vector3(0f, 1.9f, 4.05f);
        wallNorth.transform.localScale = new Vector3(8.0f, 3.8f, 0.1f);
        wallNorth.GetComponent<Renderer>().material = wallMat;

        // West Wall (X = -4.0)
        GameObject wallWest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallWest.name = "Wall_West";
        wallWest.transform.SetParent(roomGeo.transform);
        wallWest.transform.localPosition = new Vector3(-4.05f, 1.9f, 0f);
        wallWest.transform.localScale = new Vector3(0.1f, 3.8f, 8.0f);
        wallWest.GetComponent<Renderer>().material = wallMat;

        // East Wall with Gothic High Window (X = +4.0)
        GameObject wallEast = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallEast.name = "Wall_East";
        wallEast.transform.SetParent(roomGeo.transform);
        wallEast.transform.localPosition = new Vector3(4.05f, 1.9f, 0f);
        wallEast.transform.localScale = new Vector3(0.1f, 3.8f, 8.0f);
        wallEast.GetComponent<Renderer>().material = wallMat;

        // South Wall Left of Door (Z = -4.0)
        GameObject wallSouthL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallSouthL.name = "Wall_South_Left";
        wallSouthL.transform.SetParent(roomGeo.transform);
        wallSouthL.transform.localPosition = new Vector3(-2.5f, 1.9f, -4.05f);
        wallSouthL.transform.localScale = new Vector3(3.0f, 3.8f, 0.1f);
        wallSouthL.GetComponent<Renderer>().material = wallMat;

        // South Wall Right of Door (Z = -4.0)
        GameObject wallSouthR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallSouthR.name = "Wall_South_Right";
        wallSouthR.transform.SetParent(roomGeo.transform);
        wallSouthR.transform.localPosition = new Vector3(2.5f, 1.9f, -4.05f);
        wallSouthR.transform.localScale = new Vector3(3.0f, 3.8f, 0.1f);
        wallSouthR.GetComponent<Renderer>().material = wallMat;

        // South Wall Top Transom above Door
        GameObject wallSouthTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallSouthTop.name = "Wall_South_Top";
        wallSouthTop.transform.SetParent(roomGeo.transform);
        wallSouthTop.transform.localPosition = new Vector3(0f, 3.15f, -4.05f);
        wallSouthTop.transform.localScale = new Vector3(2.0f, 1.3f, 0.1f);
        wallSouthTop.GetComponent<Renderer>().material = wallMat;

        // 5. Lighting Setup (Phase 4 Atmospheric Lighting)
        GameObject lightsParent = new GameObject("Chamber_Lighting");
        lightsParent.transform.SetParent(roomRoot.transform);

        // Cold Blue Ambient Moonlight
        GameObject dirLightObj = new GameObject("Moonlight_Ambient");
        dirLightObj.transform.SetParent(lightsParent.transform);
        Light dirLight = dirLightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(0.50f, 0.60f, 0.78f);
        dirLight.intensity = 0.40f;
        dirLight.shadows = LightShadows.Soft;
        dirLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Warm Cozy Oil Lantern Light on Study Desk
        GameObject tableLightObj = new GameObject("Lantern_Warm_Light");
        tableLightObj.transform.SetParent(lightsParent.transform);
        tableLightObj.transform.position = new Vector3(2.15f, 1.25f, 1.35f);
        Light tableLight = tableLightObj.AddComponent<Light>();
        tableLight.type = LightType.Point;
        tableLight.color = new Color(1.0f, 0.75f, 0.45f);
        tableLight.intensity = 2.2f;
        tableLight.range = 6.0f;
        tableLight.shadows = LightShadows.None;

        // Wall Sconce Light near Bookshelf
        GameObject sconceLightObj = new GameObject("Bookshelf_Sconce_Light");
        sconceLightObj.transform.SetParent(lightsParent.transform);
        sconceLightObj.transform.position = new Vector3(-3.8f, 2.3f, 1.5f);
        Light sconceLight = sconceLightObj.AddComponent<Light>();
        sconceLight.type = LightType.Point;
        sconceLight.color = new Color(1.0f, 0.80f, 0.50f);
        sconceLight.intensity = 1.8f;
        sconceLight.range = 5.0f;
        sconceLight.shadows = LightShadows.None;

        // 6. Furniture & Props (Study Area, Bookshelf, Chair, Rug, Crate)
        GameObject furnitureParent = new GameObject("Chamber_Furniture");
        furnitureParent.transform.SetParent(roomRoot.transform);

        // Persian Rug beneath Study Area
        GameObject rugObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rugObj.name = "Study_Persian_Rug";
        rugObj.transform.SetParent(furnitureParent.transform);
        rugObj.transform.position = new Vector3(1.5f, 0.005f, 0.5f);
        rugObj.transform.localScale = new Vector3(2.5f, 0.01f, 2.3f);
        rugObj.GetComponent<Renderer>().material = rugMat;

        // Study Desk Table (x=1.5, z=1.0)
        GameObject tableObj = new GameObject("Study_Desk");
        tableObj.transform.SetParent(furnitureParent.transform);
        tableObj.transform.position = new Vector3(1.5f, 0f, 1.0f);

        GameObject tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tableTop.name = "Desk_Top";
        tableTop.transform.SetParent(tableObj.transform);
        tableTop.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        tableTop.transform.localScale = new Vector3(1.8f, 0.08f, 1.1f);
        tableTop.GetComponent<Renderer>().material = woodFurnitureMat;

        // Table Legs
        float[,] legOffsets = { { -0.8f, -0.45f }, { 0.8f, -0.45f }, { -0.8f, 0.45f }, { 0.8f, 0.45f } };
        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"Desk_Leg_{i + 1}";
            leg.transform.SetParent(tableObj.transform);
            leg.transform.localPosition = new Vector3(legOffsets[i, 0], 0.38f, legOffsets[i, 1]);
            leg.transform.localScale = new Vector3(0.08f, 0.38f, 0.08f);
            leg.GetComponent<Renderer>().material = woodFurnitureMat;
        }

        // Oil Lantern on Desk (Rear right corner)
        GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lantern.name = "Oil_Lantern";
        lantern.transform.SetParent(furnitureParent.transform);
        lantern.transform.position = new Vector3(2.15f, 0.98f, 1.35f);
        lantern.transform.localScale = new Vector3(0.12f, 0.16f, 0.12f);
        lantern.GetComponent<Renderer>().material = lanternMat;

        // Inkwell & Quill on Desk
        GameObject inkObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        inkObj.name = "Desk_Inkwell";
        inkObj.transform.SetParent(furnitureParent.transform);
        inkObj.transform.position = new Vector3(0.85f, 0.85f, 1.25f);
        inkObj.transform.localScale = new Vector3(0.05f, 0.035f, 0.05f);
        inkObj.GetComponent<Renderer>().material = lanternMat;

        GameObject quillObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        quillObj.name = "Desk_Quill";
        quillObj.transform.SetParent(furnitureParent.transform);
        quillObj.transform.position = new Vector3(0.86f, 0.92f, 1.26f);
        quillObj.transform.rotation = Quaternion.Euler(22f, 18f, 0f);
        quillObj.transform.localScale = new Vector3(0.008f, 0.08f, 0.008f);
        quillObj.GetComponent<Renderer>().material = parchmentMat;

        // Wooden Chair at Desk (x=1.5, z=0.0)
        GameObject chairObj = new GameObject("Study_Chair");
        chairObj.transform.SetParent(furnitureParent.transform);
        chairObj.transform.position = new Vector3(1.5f, 0f, 0.0f);

        GameObject chairSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chairSeat.name = "Seat";
        chairSeat.transform.SetParent(chairObj.transform);
        chairSeat.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        chairSeat.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        chairSeat.GetComponent<Renderer>().material = woodFurnitureMat;

        GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chairBack.name = "Backrest";
        chairBack.transform.SetParent(chairObj.transform);
        chairBack.transform.localPosition = new Vector3(0f, 0.8f, -0.22f);
        chairBack.transform.localScale = new Vector3(0.5f, 0.7f, 0.05f);
        chairBack.GetComponent<Renderer>().material = woodFurnitureMat;

        for (int i = 0; i < 4; i++)
        {
            GameObject cLeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cLeg.name = $"Chair_Leg_{i + 1}";
            cLeg.transform.SetParent(chairObj.transform);
            cLeg.transform.localPosition = new Vector3((i % 2 == 0 ? -0.2f : 0.2f), 0.22f, (i < 2 ? -0.2f : 0.2f));
            cLeg.transform.localScale = new Vector3(0.04f, 0.22f, 0.04f);
            cLeg.GetComponent<Renderer>().material = woodFurnitureMat;
        }

        ChairController chairCtrl = chairObj.AddComponent<ChairController>();
        chairCtrl.seatPoint = chairSeat.transform;
        BoxCollider chairCol = chairObj.AddComponent<BoxCollider>();
        chairCol.center = new Vector3(0f, 0.55f, 0f);
        chairCol.size = new Vector3(0.6f, 1.1f, 0.6f);

        // Bookshelf on West Wall (x=-3.95, z=1.5)
        GameObject bookshelfObj = new GameObject("Tall_Bookshelf");
        bookshelfObj.transform.SetParent(furnitureParent.transform);
        bookshelfObj.transform.position = new Vector3(-3.95f, 0f, 1.5f);
        bookshelfObj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        GameObject bsBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bsBack.name = "BackPanel";
        bsBack.transform.SetParent(bookshelfObj.transform);
        bsBack.transform.localPosition = new Vector3(0f, 1.25f, -0.22f);
        bsBack.transform.localScale = new Vector3(1.6f, 2.5f, 0.05f);
        bsBack.GetComponent<Renderer>().material = woodFurnitureMat;

        GameObject bsSideL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bsSideL.name = "SideLeft";
        bsSideL.transform.SetParent(bookshelfObj.transform);
        bsSideL.transform.localPosition = new Vector3(-0.78f, 1.25f, 0f);
        bsSideL.transform.localScale = new Vector3(0.05f, 2.5f, 0.5f);
        bsSideL.GetComponent<Renderer>().material = woodFurnitureMat;

        GameObject bsSideR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bsSideR.name = "SideRight";
        bsSideR.transform.SetParent(bookshelfObj.transform);
        bsSideR.transform.localPosition = new Vector3(0.78f, 1.25f, 0f);
        bsSideR.transform.localScale = new Vector3(0.05f, 2.5f, 0.5f);
        bsSideR.GetComponent<Renderer>().material = woodFurnitureMat;

        for (int s = 0; s < 4; s++)
        {
            GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = $"Shelf_{s + 1}";
            shelf.transform.SetParent(bookshelfObj.transform);
            shelf.transform.localPosition = new Vector3(0f, 0.2f + s * 0.7f, 0f);
            shelf.transform.localScale = new Vector3(1.52f, 0.04f, 0.48f);
            shelf.GetComponent<Renderer>().material = woodFurnitureMat;
        }

        // Decorative Books on Shelves
        Color[] bookColors = { new Color(0.6f, 0.2f, 0.2f), new Color(0.2f, 0.4f, 0.6f), new Color(0.2f, 0.5f, 0.3f), new Color(0.7f, 0.5f, 0.2f) };
        for (int b = 0; b < 6; b++)
        {
            GameObject decoBook = GameObject.CreatePrimitive(PrimitiveType.Cube);
            decoBook.name = $"DecoBook_{b + 1}";
            decoBook.transform.SetParent(bookshelfObj.transform);
            decoBook.transform.localPosition = new Vector3(-0.55f + b * 0.18f, 1.05f, 0f);
            decoBook.transform.localScale = new Vector3(0.12f, 0.32f, 0.28f);
            decoBook.GetComponent<Renderer>().material = CreateMaterial($"Mat_DecoBook_{b}", bookColors[b % bookColors.Length], 0.05f, 0.3f);
        }

        // Corner Storage Crate
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Storage_Crate";
        crate.transform.SetParent(furnitureParent.transform);
        crate.transform.position = new Vector3(-3.3f, 0.4f, -3.3f);
        crate.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        crate.GetComponent<Renderer>().material = woodFurnitureMat;

        // 7. Relic Puzzle Chest + Keypad + Brass Key (Phases 9, 10, 11)
        GameObject puzzleParent = new GameObject("Relic_Puzzle_Chest");
        puzzleParent.transform.SetParent(roomRoot.transform);
        puzzleParent.transform.position = new Vector3(1.5f, 0.84f, 1.0f); // Center of study desk

        // Chest Body
        GameObject boxBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boxBase.name = "Chest_Base";
        boxBase.transform.SetParent(puzzleParent.transform);
        boxBase.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        boxBase.transform.localScale = new Vector3(0.48f, 0.16f, 0.36f);
        boxBase.GetComponent<Renderer>().material = metalBoxMat;

        // Chest Lid Pivot Hinge
        GameObject lidHinge = new GameObject("Chest_Lid_Hinge");
        lidHinge.transform.SetParent(puzzleParent.transform);
        lidHinge.transform.localPosition = new Vector3(0f, 0.16f, 0.18f);

        GameObject boxLid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boxLid.name = "Chest_Lid_Mesh";
        boxLid.transform.SetParent(lidHinge.transform);
        boxLid.transform.localPosition = new Vector3(0f, 0.02f, -0.18f);
        boxLid.transform.localScale = new Vector3(0.50f, 0.04f, 0.38f);
        boxLid.GetComponent<Renderer>().material = metalBoxMat;

        BoxController boxCtrl = puzzleParent.AddComponent<BoxController>();
        boxCtrl.lidTransform = lidHinge.transform;
        boxCtrl.openAngleX = -105f;

        // Golden Unlock Particles
        GameObject particleObj = new GameObject("Chest_Unlock_Particles");
        particleObj.transform.SetParent(puzzleParent.transform);
        particleObj.transform.localPosition = new Vector3(0f, 0.14f, 0f);
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var psMain = ps.main;
        psMain.startColor = new Color(1.0f, 0.85f, 0.35f, 0.95f);
        psMain.startSize = 0.04f;
        psMain.startSpeed = 0.6f;
        psMain.startLifetime = 1.2f;
        psMain.loop = false;
        psMain.playOnAwake = false;
        var psEmission = ps.emission;
        psEmission.rateOverTime = 0;
        psEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) });
        var psShape = ps.shape;
        psShape.shapeType = ParticleSystemShapeType.Box;
        psShape.scale = new Vector3(0.35f, 0.05f, 0.25f);
        boxCtrl.unlockParticles = ps;

        // Chest Interior Warm Light
        GameObject chestLightObj = new GameObject("Chest_Interior_Light");
        chestLightObj.transform.SetParent(puzzleParent.transform);
        chestLightObj.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        Light chestLight = chestLightObj.AddComponent<Light>();
        chestLight.type = LightType.Point;
        chestLight.color = new Color(1.0f, 0.85f, 0.35f);
        chestLight.intensity = 1.6f;
        chestLight.range = 1.2f;
        boxCtrl.interiorLight = chestLight;

        // Keypad Housing (Mounted on front of chest at 20 deg tilt)
        GameObject keypadObj = new GameObject("Keypad_Lock");
        keypadObj.transform.SetParent(puzzleParent.transform);
        keypadObj.transform.localPosition = new Vector3(0f, 0.10f, -0.22f);
        keypadObj.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

        GameObject keypadHousing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keypadHousing.name = "Keypad_Housing";
        keypadHousing.transform.SetParent(keypadObj.transform);
        keypadHousing.transform.localPosition = Vector3.zero;
        keypadHousing.transform.localScale = new Vector3(0.32f, 0.26f, 0.03f);
        keypadHousing.GetComponent<Renderer>().material = keypadMat;

        // Keypad LCD Screen
        GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screenObj.name = "Keypad_LCD_Screen";
        screenObj.transform.SetParent(keypadObj.transform);
        screenObj.transform.localPosition = new Vector3(0f, 0.085f, -0.018f);
        screenObj.transform.localScale = new Vector3(0.24f, 0.050f, 1f);
        screenObj.GetComponent<Renderer>().material = screenMat;

        GameObject screenTextObj = new GameObject("Screen_Text");
        screenTextObj.transform.SetParent(screenObj.transform);
        screenTextObj.transform.localPosition = new Vector3(0f, 0f, -0.010f);
        screenTextObj.transform.localRotation = Quaternion.identity;
        TextMeshPro screenTMP = screenTextObj.AddComponent<TextMeshPro>();
        if (defaultFont != null) screenTMP.font = defaultFont;
        screenTMP.text = "_ _ _";
        screenTMP.alignment = TextAlignmentOptions.Center;
        screenTMP.fontStyle = FontStyles.Bold;
        screenTMP.fontSize = 0.45f;
        screenTMP.color = Color.cyan;
        screenTMP.rectTransform.sizeDelta = new Vector2(0.24f, 0.050f);

        KeypadController keypadCtrl = keypadObj.AddComponent<KeypadController>();
        keypadCtrl.targetBox = boxCtrl;
        keypadCtrl.screenText3D = screenTMP;

        // 3x4 Keypad Buttons
        string[,] buttonLabels = {
            { "1", "2", "3" },
            { "4", "5", "6" },
            { "7", "8", "9" },
            { "C", "0", "E" }
        };
        float btnStartX = -0.080f;
        float btnStartY = 0.035f;
        float btnSpacingX = 0.080f;
        float btnSpacingY = 0.046f;

        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                string label = buttonLabels[r, c];
                GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                btnObj.name = $"Keypad_Btn_{label}";
                btnObj.transform.SetParent(keypadObj.transform);
                btnObj.transform.localPosition = new Vector3(btnStartX + c * btnSpacingX, btnStartY - r * btnSpacingY, -0.020f);
                btnObj.transform.localScale = new Vector3(0.060f, 0.034f, 0.02f);
                btnObj.GetComponent<Renderer>().material = CreateMaterial($"Mat_Btn_{label}", new Color(0.20f, 0.22f, 0.26f), 0.2f, 0.5f);

                KeypadButton btnComp = btnObj.AddComponent<KeypadButton>();
                btnComp.keyCharacter = (label == "C") ? "CLEAR" : (label == "E" ? "ENTER" : label);
                btnComp.keypadController = keypadCtrl;
                btnComp.buttonRenderer = btnObj.GetComponent<Renderer>();

                XRInteractableAdapter btnXr = btnObj.AddComponent<XRInteractableAdapter>();
                btnXr.interactionType = XRInteractableAdapter.InteractableType.Simple;
                btnXr.onSelected.AddListener(btnComp.Press);

                GameObject btnTextObj = new GameObject("Label");
                btnTextObj.transform.SetParent(btnObj.transform);
                btnTextObj.transform.localPosition = new Vector3(0f, 0f, -0.55f);
                btnTextObj.transform.localRotation = Quaternion.identity;
                TextMeshPro btnTMP = btnTextObj.AddComponent<TextMeshPro>();
                if (defaultFont != null) btnTMP.font = defaultFont;
                btnTMP.text = label;
                btnTMP.alignment = TextAlignmentOptions.Center;
                btnTMP.fontStyle = FontStyles.Bold;
                btnTMP.fontSize = 0.26f;
                btnTMP.color = (label == "C") ? new Color(1.0f, 0.3f, 0.3f) : (label == "E" ? new Color(0.3f, 1.0f, 0.4f) : Color.white);
                btnTMP.rectTransform.sizeDelta = new Vector2(0.060f, 0.034f);
            }
        }

        // Antique Brass Key (Inside Chest - Phase 11)
        GameObject keyObj = new GameObject("Antique_Brass_Key");
        keyObj.transform.SetParent(puzzleParent.transform);
        keyObj.transform.localPosition = new Vector3(0f, 0.10f, 0f);

        GameObject keyStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        keyStem.name = "Key_Stem";
        keyStem.transform.SetParent(keyObj.transform);
        keyStem.transform.localPosition = Vector3.zero;
        keyStem.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        keyStem.transform.localScale = new Vector3(0.015f, 0.06f, 0.015f);
        keyStem.GetComponent<Renderer>().material = brassKeyMat;

        GameObject keyRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        keyRing.name = "Key_Ring";
        keyRing.transform.SetParent(keyObj.transform);
        keyRing.transform.localPosition = new Vector3(0f, 0f, -0.065f);
        keyRing.transform.localScale = new Vector3(0.045f, 0.01f, 0.045f);
        keyRing.GetComponent<Renderer>().material = brassKeyMat;

        GameObject keyTooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyTooth.name = "Key_Tooth";
        keyTooth.transform.SetParent(keyObj.transform);
        keyTooth.transform.localPosition = new Vector3(0.015f, 0f, 0.045f);
        keyTooth.transform.localScale = new Vector3(0.02f, 0.008f, 0.025f);
        keyTooth.GetComponent<Renderer>().material = brassKeyMat;

        BoxCollider keyCollider = keyObj.AddComponent<BoxCollider>();
        keyCollider.size = new Vector3(0.06f, 0.04f, 0.16f);
        Rigidbody keyRb = keyObj.AddComponent<Rigidbody>();
        keyRb.mass = 0.5f;

        GameObject keyGlow = new GameObject("Key_Glow_Light");
        keyGlow.transform.SetParent(keyObj.transform);
        keyGlow.transform.localPosition = Vector3.zero;
        Light kLight = keyGlow.AddComponent<Light>();
        kLight.type = LightType.Point;
        kLight.color = new Color(1.0f, 0.85f, 0.3f);
        kLight.intensity = 1.2f;
        kLight.range = 0.6f;

        KeyController keyCtrl = keyObj.AddComponent<KeyController>();
        keyCtrl.glowLight = kLight;

        XRInteractableAdapter keyXr = keyObj.AddComponent<XRInteractableAdapter>();
        keyXr.interactionType = XRInteractableAdapter.InteractableType.Grab;
        keyXr.onSelected.AddListener(keyCtrl.OnGrabbed);
        keyXr.onDeselected.AddListener(keyCtrl.OnReleased);

        boxCtrl.keyInside = keyObj;

        // 8. Clues (Panels 2, 3, 4)
        GameObject cluesParent = new GameObject("Chamber_Clues");
        cluesParent.transform.SetParent(roomRoot.transform);

        // CLUE 1: Celestial Star Journal on Bookshelf (First Digit = 5)
        GameObject clue1Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        clue1Obj.name = "Clue_1_Book_Of_Stars";
        clue1Obj.transform.SetParent(cluesParent.transform);
        clue1Obj.transform.position = new Vector3(-3.85f, 1.62f, 1.2f);
        clue1Obj.transform.rotation = Quaternion.Euler(0f, 15f, 0f);
        clue1Obj.transform.localScale = new Vector3(0.22f, 0.05f, 0.30f);
        clue1Obj.GetComponent<Renderer>().material = bookCoverMat;
        clue1Obj.AddComponent<Rigidbody>();

        GameObject starEmblem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        starEmblem.name = "Celestial_Star_Emblem";
        starEmblem.transform.SetParent(clue1Obj.transform);
        starEmblem.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        starEmblem.transform.localScale = new Vector3(0.38f, 0.04f, 0.38f);
        starEmblem.GetComponent<Renderer>().material = brassKeyMat;

        ClueInteraction clue1 = clue1Obj.AddComponent<ClueInteraction>();
        clue1.clueId = "Clue_1_Book_Of_Stars";
        clue1.clueIndex = 1;
        clue1.clueTitle = "Celestial Star Journal";
        clue1.clueText = "Astronomer's Note:\nThe sacred star has 5 golden points.";
        clue1.digitHint = "FIRST DIGIT = 5";

        XRInteractableAdapter clue1Xr = clue1Obj.AddComponent<XRInteractableAdapter>();
        clue1Xr.interactionType = XRInteractableAdapter.InteractableType.Grab;
        clue1Xr.onSelected.AddListener(clue1.InspectClue);
        clue1Xr.onDeselected.AddListener(clue1.StopInspectClue);

        // CLUE 2: Diary Scrap on Desk (Second Digit = 2)
        GameObject clue2Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        clue2Obj.name = "Clue_2_Parchment_Note";
        clue2Obj.transform.SetParent(cluesParent.transform);
        clue2Obj.transform.position = new Vector3(1.05f, 0.85f, 0.9f);
        clue2Obj.transform.localScale = new Vector3(0.38f, 0.008f, 0.48f);
        clue2Obj.GetComponent<Renderer>().material = parchmentMat;
        clue2Obj.AddComponent<Rigidbody>();

        GameObject letterTextObj = new GameObject("Diary_Printed_Text");
        letterTextObj.transform.SetParent(clue2Obj.transform);
        letterTextObj.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        letterTextObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        TextMeshPro letterTMP = letterTextObj.AddComponent<TextMeshPro>();
        if (defaultFont != null) letterTMP.font = defaultFont;
        letterTMP.text = "DIARY NOTE:\n\nHow many eyes behold this room?\nHow many ears listen in the dark?\n\n[Second Digit = 2]";
        letterTMP.alignment = TextAlignmentOptions.Center;
        letterTMP.fontSize = 0.32f;
        letterTMP.color = new Color(0.18f, 0.10f, 0.05f);
        letterTMP.rectTransform.sizeDelta = new Vector2(0.35f, 0.44f);

        ClueInteraction clue2 = clue2Obj.AddComponent<ClueInteraction>();
        clue2.clueId = "Clue_2_Parchment_Note";
        clue2.clueIndex = 2;
        clue2.clueTitle = "Diary Scrap (Study Desk)";
        clue2.clueText = "DIARY NOTE:\nHow many eyes behold this room?\nHow many ears listen in the dark?";
        clue2.digitHint = "SECOND DIGIT = 2";

        XRInteractableAdapter clue2Xr = clue2Obj.AddComponent<XRInteractableAdapter>();
        clue2Xr.interactionType = XRInteractableAdapter.InteractableType.Grab;
        clue2Xr.onSelected.AddListener(clue2.InspectClue);
        clue2Xr.onDeselected.AddListener(clue2.StopInspectClue);

        // CLUE 3: Chiseled Wall Inscription on South Wall (Third Digit = 7)
        GameObject clue3Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        clue3Obj.name = "Clue_3_Wall_Carving";
        clue3Obj.transform.SetParent(cluesParent.transform);
        clue3Obj.transform.position = new Vector3(1.8f, 1.7f, -4.08f);
        clue3Obj.transform.localScale = new Vector3(1.3f, 0.65f, 0.03f);
        clue3Obj.GetComponent<Renderer>().material = parchmentMat;

        GameObject c3Text = new GameObject("Carving_Text");
        c3Text.transform.SetParent(clue3Obj.transform);
        c3Text.transform.localPosition = new Vector3(0f, 0f, 0.55f);
        c3Text.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMeshPro c3TMP = c3Text.AddComponent<TextMeshPro>();
        if (defaultFont != null) c3TMP.font = defaultFont;
        c3TMP.text = "Seven days to walk the earth.\nSeven wonders of the old world.\n[Third Digit = 7]";
        c3TMP.alignment = TextAlignmentOptions.Center;
        c3TMP.fontSize = 0.42f;
        c3TMP.color = new Color(0.25f, 0.12f, 0.05f);
        c3TMP.rectTransform.sizeDelta = new Vector2(1.2f, 0.6f);

        ClueInteraction clue3 = clue3Obj.AddComponent<ClueInteraction>();
        clue3.clueId = "Clue_3_Wall_Carving";
        clue3.clueIndex = 3;
        clue3.clueTitle = "Chiseled Wall Inscription";
        clue3.clueText = "Chiseled Wall Inscription:\nSeven days to walk the earth.\nSeven wonders of the old world.";
        clue3.digitHint = "THIRD DIGIT = 7";

        // 9. Exit Doorway & Keyhole (Phases 12, 13)
        GameObject doorSystem = new GameObject("Exit_Doorway_System");
        doorSystem.transform.SetParent(roomRoot.transform);
        doorSystem.transform.position = new Vector3(0f, 0f, -4.25f);

        // Door Frame
        GameObject frameL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameL.name = "Frame_Left";
        frameL.transform.SetParent(doorSystem.transform);
        frameL.transform.localPosition = new Vector3(-0.95f, 1.25f, 0f);
        frameL.transform.localScale = new Vector3(0.12f, 2.5f, 0.32f);
        frameL.GetComponent<Renderer>().material = woodFurnitureMat;

        GameObject frameR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameR.name = "Frame_Right";
        frameR.transform.SetParent(doorSystem.transform);
        frameR.transform.localPosition = new Vector3(0.95f, 1.25f, 0f);
        frameR.transform.localScale = new Vector3(0.12f, 2.5f, 0.32f);
        frameR.GetComponent<Renderer>().material = woodFurnitureMat;

        GameObject frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameTop.name = "Frame_Top";
        frameTop.transform.SetParent(doorSystem.transform);
        frameTop.transform.localPosition = new Vector3(0f, 2.45f, 0f);
        frameTop.transform.localScale = new Vector3(2.0f, 0.12f, 0.32f);
        frameTop.GetComponent<Renderer>().material = woodFurnitureMat;

        // Door Hinge Pivot (x=-0.9)
        GameObject doorHinge = new GameObject("Door_Hinge_Pivot");
        doorHinge.transform.SetParent(doorSystem.transform);
        doorHinge.transform.localPosition = new Vector3(-0.9f, 0f, 0f);

        // Door Panel
        GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorPanel.name = "Door_Panel";
        doorPanel.transform.SetParent(doorHinge.transform);
        doorPanel.transform.localPosition = new Vector3(0.9f, 1.25f, 0f);
        doorPanel.transform.localScale = new Vector3(1.78f, 2.40f, 0.08f);
        doorPanel.GetComponent<Renderer>().material = doorMat;

        // Brass Lock Plate & Keyhole
        GameObject lockPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lockPlate.name = "Lock_Plate";
        lockPlate.transform.SetParent(doorPanel.transform);
        lockPlate.transform.localPosition = new Vector3(0.65f, -0.08f, 0.050f);
        lockPlate.transform.localScale = new Vector3(0.24f, 0.42f, 0.020f);
        lockPlate.GetComponent<Renderer>().material = brassKeyMat;

        GameObject keyholeObj = new GameObject("Door_Keyhole_Trigger");
        keyholeObj.transform.SetParent(doorPanel.transform);
        keyholeObj.transform.localPosition = new Vector3(0.65f, -0.15f, 0f);
        BoxCollider keyholeCol = keyholeObj.AddComponent<BoxCollider>();
        keyholeCol.isTrigger = true;
        keyholeCol.size = new Vector3(0.8f, 0.8f, 0.8f);

        DoorKeyholeTrigger dkt = keyholeObj.AddComponent<DoorKeyholeTrigger>();

        // Bright Exterior Daylight outside the door
        GameObject exteriorLightObj = new GameObject("Exterior_Sunlight");
        exteriorLightObj.transform.SetParent(doorSystem.transform);
        exteriorLightObj.transform.position = new Vector3(0f, 2.5f, -6.0f);
        Light extLight = exteriorLightObj.AddComponent<Light>();
        extLight.type = LightType.Point;
        extLight.color = new Color(1.0f, 0.96f, 0.88f);
        extLight.intensity = 0.3f;
        extLight.range = 14.0f;

        DoorController doorCtrl = doorSystem.AddComponent<DoorController>();
        doorCtrl.doorPanel = doorHinge.transform;
        doorCtrl.keyholeTransform = keyholeObj.transform;
        doorCtrl.exteriorLight = extLight;
        doorCtrl.openAngleY = 95f;
        dkt.doorController = doorCtrl;

        // Escape Trigger Zone in doorway
        GameObject escapeZoneObj = new GameObject("Escape_Zone_Trigger");
        escapeZoneObj.transform.SetParent(doorSystem.transform);
        escapeZoneObj.transform.position = new Vector3(0f, 1.2f, -5.0f);
        BoxCollider escapeCol = escapeZoneObj.AddComponent<BoxCollider>();
        escapeCol.isTrigger = true;
        escapeCol.size = new Vector3(2.0f, 2.4f, 1.2f);
        escapeZoneObj.AddComponent<EscapeZone>();
        doorCtrl.escapeTrigger = escapeCol;

        // Exterior Garden Courtyard (Walkway, Lawn, Pillars, Foliage Trees)
        GameObject extGarden = new GameObject("Exterior_Garden_Courtyard");
        extGarden.transform.SetParent(doorSystem.transform);

        GameObject patio = GameObject.CreatePrimitive(PrimitiveType.Cube);
        patio.name = "Exterior_Patio_Pavers";
        patio.transform.SetParent(extGarden.transform);
        patio.transform.localPosition = new Vector3(0f, -0.06f, -3.2f);
        patio.transform.localScale = new Vector3(5.0f, 0.12f, 6.0f);
        patio.GetComponent<Renderer>().material = patioMat;

        GameObject lawn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lawn.name = "Exterior_Lawn";
        lawn.transform.SetParent(extGarden.transform);
        lawn.transform.localPosition = new Vector3(0f, -0.12f, -6.0f);
        lawn.transform.localScale = new Vector3(22.0f, 0.12f, 15.0f);
        lawn.GetComponent<Renderer>().material = grassMat;

        float[,] colCoords = { { -2.3f, -4.2f }, { 2.3f, -4.2f }, { -2.3f, -7.2f }, { 2.3f, -7.2f } };
        for (int c = 0; c < 4; c++)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"Pillar_{c + 1}";
            pillar.transform.SetParent(extGarden.transform);
            pillar.transform.localPosition = new Vector3(colCoords[c, 0], 1.4f, colCoords[c, 1]);
            pillar.transform.localScale = new Vector3(0.35f, 1.4f, 0.35f);
            pillar.GetComponent<Renderer>().material = stoneArchMat;
        }

        float[,] treeCoords = { { -5.2f, -8.0f }, { 5.2f, -8.0f }, { -3.6f, -11.5f }, { 3.6f, -11.5f } };
        for (int t = 0; t < 4; t++)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = $"Tree_{t + 1}_Trunk";
            trunk.transform.SetParent(extGarden.transform);
            trunk.transform.localPosition = new Vector3(treeCoords[t, 0], 1.2f, treeCoords[t, 1]);
            trunk.transform.localScale = new Vector3(0.25f, 1.2f, 0.25f);
            trunk.GetComponent<Renderer>().material = beamMat;

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = $"Tree_{t + 1}_Canopy";
            canopy.transform.SetParent(trunk.transform);
            canopy.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            canopy.transform.localScale = new Vector3(3.2f, 2.2f, 3.2f);
            canopy.GetComponent<Renderer>().material = foliageMat;
        }

        // 10. VR World Space Canvas UI (Phase 16)
        BuildVRWorldCanvas(roomRoot, uiManager, timerManager, scoreManager, defaultFont);

        // 11. Dual Player Rig (VR XR Origin + Desktop Fallback - Phase 2 & 5)
        BuildPlayerRig(roomRoot);

        Debug.Log("[EscapeRoomSceneBuilder] Complete Escape Room environment, puzzle systems, UI, and player rig constructed successfully.");
    }

    private static void BuildVRWorldCanvas(GameObject root, UIManager ui, TimerManager timer, ScoreManager score, TMP_FontAsset font)
    {
        // Ensure EventSystem exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.transform.SetParent(root.transform);
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
                es.AddComponent(inputModuleType);
            else
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameObject canvasObj = new GameObject("VR_WorldSpace_Canvas");
        canvasObj.transform.SetParent(root.transform);
        canvasObj.transform.position = new Vector3(0f, 1.8f, 1.8f);
        canvasObj.transform.rotation = Quaternion.identity;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform cRt = canvasObj.GetComponent<RectTransform>();
        cRt.sizeDelta = new Vector2(1000f, 700f);
        cRt.localScale = Vector3.one * 0.002f;

        // 1. INTRO PANEL
        GameObject introPanel = CreatePanel(canvasObj, "IntroPanel", new Color(0.04f, 0.06f, 0.10f, 0.95f));
        CreateText(introPanel, "Title", "VR ESCAPE ROOM CHALLENGE", 48, new Vector2(0f, 200f), new Vector2(900f, 80f), Color.cyan, FontStyles.Bold, font);
        CreateText(introPanel, "Sub", "Find the combination and escape before time runs out.\n\nExplore -> Discover clues -> Solve code -> Open relic box -> Obtain key -> Unlock door -> Escape", 24, new Vector2(0f, 60f), new Vector2(850f, 150f), Color.white, FontStyles.Normal, font);
        CreateText(introPanel, "TimePreview", "TIME LIMIT: 05:00", 30, new Vector2(0f, -70f), new Vector2(600f, 50f), new Color(1f, 0.85f, 0.4f), FontStyles.Bold, font);
        GameObject startBtn = CreateButton(introPanel, "StartButton", "START CHALLENGE", new Vector2(0f, -180f), new Vector2(340f, 70f), new Color(0.2f, 0.65f, 0.35f), font);

        ui.introPanel = introPanel;
        ui.introTitleText = introPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        ui.introSubtitleText = introPanel.transform.Find("Sub").GetComponent<TextMeshProUGUI>();
        ui.startButton = startBtn.GetComponent<Button>();

        // 2. HUD PANEL
        GameObject hudPanel = CreatePanel(canvasObj, "HUDPanel", new Color(0.03f, 0.04f, 0.06f, 0.85f));
        RectTransform hudRt = hudPanel.GetComponent<RectTransform>();
        hudRt.offsetMin = new Vector2(80f, 480f);
        hudRt.offsetMax = new Vector2(-80f, -20f);

        CreateText(hudPanel, "HUDTitle", "ESCAPE ROOM", 26, new Vector2(0f, 55f), new Vector2(700f, 35f), new Color(0.7f, 0.8f, 1.0f), FontStyles.Bold, font);
        GameObject timerTxtObj = CreateText(hudPanel, "TimerText", "TIME: 05:00", 38, new Vector2(-240f, 0f), new Vector2(280f, 55f), Color.white, FontStyles.Bold, font);
        GameObject cluesTxtObj = CreateText(hudPanel, "CluesText", "CLUES: 0/3", 38, new Vector2(0f, 0f), new Vector2(240f, 55f), new Color(1f, 0.85f, 0.35f), FontStyles.Bold, font);
        GameObject scoreTxtObj = CreateText(hudPanel, "ScoreText", "SCORE: 1000", 38, new Vector2(240f, 0f), new Vector2(280f, 55f), Color.white, FontStyles.Bold, font);

        ui.hudPanel = hudPanel;
        ui.timerText = timerTxtObj.GetComponent<TextMeshProUGUI>();
        ui.cluesText = cluesTxtObj.GetComponent<TextMeshProUGUI>();
        ui.scoreText = scoreTxtObj.GetComponent<TextMeshProUGUI>();
        timer.timerTextUGUI = ui.timerText;
        score.scoreTextUGUI = ui.scoreText;

        // 3. CLUE INSPECT OVERLAY
        GameObject cluePanel = CreatePanel(canvasObj, "ClueInspectPanel", new Color(0.05f, 0.05f, 0.08f, 0.96f));
        CreateText(cluePanel, "Header", "CLUE DISCOVERED", 40, new Vector2(0f, 210f), new Vector2(800f, 60f), new Color(1f, 0.85f, 0.4f), FontStyles.Bold, font);
        GameObject clueTitle = CreateText(cluePanel, "Title", "Clue Title", 32, new Vector2(0f, 140f), new Vector2(800f, 50f), Color.cyan, FontStyles.Bold, font);
        GameObject clueBody = CreateText(cluePanel, "Body", "Clue riddle text goes here...", 26, new Vector2(0f, 30f), new Vector2(780f, 140f), Color.white, FontStyles.Normal, font);
        GameObject clueDigit = CreateText(cluePanel, "Digit", "FIRST DIGIT = 5", 36, new Vector2(0f, -90f), new Vector2(700f, 60f), new Color(1f, 0.9f, 0.2f), FontStyles.Bold, font);
        GameObject clueBtn = CreateButton(cluePanel, "ContinueBtn", "CONTINUE", new Vector2(0f, -190f), new Vector2(260f, 60f), new Color(0.25f, 0.45f, 0.75f), font);

        ui.clueInspectPanel = cluePanel;
        ui.clueTitleText = clueTitle.GetComponent<TextMeshProUGUI>();
        ui.clueBodyText = clueBody.GetComponent<TextMeshProUGUI>();
        ui.clueDigitText = clueDigit.GetComponent<TextMeshProUGUI>();
        ui.clueContinueButton = clueBtn.GetComponent<Button>();

        // 4. ALL CLUES FOUND BANNER
        GameObject allCluesPanel = CreatePanel(canvasObj, "AllCluesPanel", new Color(0.05f, 0.06f, 0.10f, 0.96f));
        CreateText(allCluesPanel, "Header", "ALL CLUES FOUND!", 44, new Vector2(0f, 180f), new Vector2(850f, 65f), new Color(0.3f, 1f, 0.4f), FontStyles.Bold, font);
        GameObject allCode = CreateText(allCluesPanel, "CodeText", "THE COMPLETE CODE IS:\n\n5 - 2 - 7", 38, new Vector2(0f, 30f), new Vector2(800f, 160f), new Color(1f, 0.95f, 0.3f), FontStyles.Bold, font);
        CreateText(allCluesPanel, "Prompt", "Enter 5-2-7 into the wooden chest keypad to claim the relic key.", 24, new Vector2(0f, -90f), new Vector2(800f, 50f), Color.white, FontStyles.Normal, font);
        GameObject allBtn = CreateButton(allCluesPanel, "AllContinueBtn", "CONTINUE", new Vector2(0f, -180f), new Vector2(260f, 60f), new Color(0.2f, 0.65f, 0.35f), font);

        ui.allCluesPanel = allCluesPanel;
        ui.allCluesCodeText = allCode.GetComponent<TextMeshProUGUI>();
        ui.allCluesContinueButton = allBtn.GetComponent<Button>();

        // 5. TOAST NOTIFICATION
        GameObject toastPanel = CreatePanel(canvasObj, "ToastPanel", new Color(0.08f, 0.12f, 0.18f, 0.92f));
        RectTransform toastRt = toastPanel.GetComponent<RectTransform>();
        toastRt.offsetMin = new Vector2(150f, 360f);
        toastRt.offsetMax = new Vector2(-150f, -240f);
        GameObject toastText = CreateText(toastPanel, "ToastMsg", "NOTIFICATION", 32, Vector2.zero, new Vector2(650f, 70f), new Color(1f, 0.85f, 0.35f), FontStyles.Bold, font);

        ui.toastPanel = toastPanel;
        ui.toastMessageText = toastText.GetComponent<TextMeshProUGUI>();

        // 6. WIN PANEL
        GameObject winPanel = CreatePanel(canvasObj, "WinPanel", new Color(0.04f, 0.08f, 0.05f, 0.96f));
        GameObject winTitle = CreateText(winPanel, "WinTitle", "VICTORY!\nESCAPED!", 54, new Vector2(0f, 180f), new Vector2(800f, 120f), new Color(0.3f, 1f, 0.4f), FontStyles.Bold, font);
        GameObject winTime = CreateText(winPanel, "WinTime", "Final Time: 03:42", 34, new Vector2(0f, 40f), new Vector2(700f, 50f), Color.white, FontStyles.Normal, font);
        GameObject winScore = CreateText(winPanel, "WinScore", "Score: 1450", 38, new Vector2(0f, -25f), new Vector2(700f, 55f), new Color(1f, 0.85f, 0.35f), FontStyles.Bold, font);
        GameObject winRestartBtn = CreateButton(winPanel, "WinRestartBtn", "PLAY AGAIN", new Vector2(0f, -170f), new Vector2(300f, 70f), new Color(0.2f, 0.65f, 0.35f), font);

        ui.winPanel = winPanel;
        ui.winTitleText = winTitle.GetComponent<TextMeshProUGUI>();
        ui.winTimeText = winTime.GetComponent<TextMeshProUGUI>();
        ui.winScoreText = winScore.GetComponent<TextMeshProUGUI>();
        ui.winRestartButton = winRestartBtn.GetComponent<Button>();

        // 7. LOSE PANEL
        GameObject losePanel = CreatePanel(canvasObj, "LosePanel", new Color(0.08f, 0.03f, 0.03f, 0.96f));
        GameObject loseTitle = CreateText(losePanel, "LoseTitle", "TIME'S UP!\nYOU ARE TRAPPED.", 52, new Vector2(0f, 160f), new Vector2(850f, 120f), Color.red, FontStyles.Bold, font);
        GameObject loseSub = CreateText(losePanel, "LoseSub", "You failed to escape before the 5-minute timer expired.", 26, new Vector2(0f, 30f), new Vector2(800f, 60f), Color.white, FontStyles.Normal, font);
        GameObject loseRestartBtn = CreateButton(losePanel, "LoseRestartBtn", "RESTART", new Vector2(0f, -150f), new Vector2(300f, 70f), new Color(0.8f, 0.25f, 0.25f), font);

        ui.losePanel = losePanel;
        ui.loseTitleText = loseTitle.GetComponent<TextMeshProUGUI>();
        ui.loseSubtitleText = loseSub.GetComponent<TextMeshProUGUI>();
        ui.loseRestartButton = loseRestartBtn.GetComponent<Button>();

        // 8. PAUSE PANEL
        GameObject pausePanel = CreatePanel(canvasObj, "PausePanel", new Color(0.04f, 0.05f, 0.08f, 0.95f));
        CreateText(pausePanel, "PauseTitle", "GAME PAUSED", 48, new Vector2(0f, 120f), new Vector2(700f, 70f), Color.cyan, FontStyles.Bold, font);
        GameObject resumeBtn = CreateButton(pausePanel, "ResumeBtn", "RESUME", new Vector2(0f, 10f), new Vector2(280f, 60f), new Color(0.2f, 0.55f, 0.8f), font);
        GameObject pRestartBtn = CreateButton(pausePanel, "PauseRestartBtn", "RESTART", new Vector2(0f, -90f), new Vector2(280f, 60f), new Color(0.7f, 0.25f, 0.25f), font);

        ui.pausePanel = pausePanel;
        ui.pauseResumeButton = resumeBtn.GetComponent<Button>();
        ui.pauseRestartButton = pRestartBtn.GetComponent<Button>();

        // Initially show Intro
        ui.ShowIntro();
    }

    private static void BuildPlayerRig(GameObject root)
    {
        Camera existingCam = Camera.main;

        GameObject playerRig = new GameObject("Player_Rig");
        playerRig.transform.SetParent(root.transform);
        // Panel 1 Spawn Position: (0, 0.05, 3.0) facing +Z towards table and door
        playerRig.transform.position = new Vector3(0f, 0.05f, 3.0f);
        playerRig.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Facing room interior
        playerRig.tag = "Player";

        CharacterController charCtrl = playerRig.AddComponent<CharacterController>();
        charCtrl.height = 1.8f;
        charCtrl.radius = 0.35f;
        charCtrl.center = new Vector3(0f, 0.9f, 0f);
        charCtrl.skinWidth = 0.02f;
        charCtrl.minMoveDistance = 0f;

        GameObject camOffset = new GameObject("Camera_Offset");
        camOffset.transform.SetParent(playerRig.transform);
        camOffset.transform.localPosition = new Vector3(0f, 1.6f, 0f); // 1.60m standing eye level

        GameObject camObj;
        if (existingCam != null)
        {
            camObj = existingCam.gameObject;
            camObj.transform.SetParent(camOffset.transform);
            camObj.transform.localPosition = Vector3.zero;
            camObj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(camOffset.transform);
            camObj.transform.localPosition = Vector3.zero;
            camObj.transform.localRotation = Quaternion.identity;
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            camObj.AddComponent<AudioListener>();
        }

        // Attach TrackedPoseDriver for 6DOF VR Headset Tracking
        System.Type tpdType = System.Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem")
                           ?? System.Type.GetType("UnityEngine.SpatialTracking.TrackedPoseDriver, UnityEngine.SpatialTracking");
        if (tpdType != null && camObj.GetComponent(tpdType) == null)
        {
            camObj.AddComponent(tpdType);
        }

        // Desktop Fallback locomotion and interaction controller
        playerRig.AddComponent<VRDesktopTester>();

        // Controller Anchors for VR
        GameObject leftHand = new GameObject("Left_Controller_Anchor");
        leftHand.transform.SetParent(camOffset.transform);
        leftHand.transform.localPosition = new Vector3(-0.25f, -0.2f, 0.4f);

        GameObject rightHand = new GameObject("Right_Controller_Anchor");
        rightHand.transform.SetParent(camOffset.transform);
        rightHand.transform.localPosition = new Vector3(0.25f, -0.2f, 0.4f);

        Material gloveMat = CreateMaterial("Mat_VRGlove", new Color(0.20f, 0.22f, 0.26f), 0.2f, 0.5f);
        Material seamMat = CreateMaterial("Mat_VRGloveSeam", new Color(0.15f, 0.85f, 1.0f), 0.8f, 0.9f);
        BuildStylizedVRHand(leftHand, true, gloveMat, seamMat);
        BuildStylizedVRHand(rightHand, false, gloveMat, seamMat);

        // Attach XROrigin if Unity.XR.CoreUtils is available
        System.Type xrOriginType = System.Type.GetType("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils");
        if (xrOriginType != null)
        {
            var xrOrigin = playerRig.AddComponent(xrOriginType);
            var cameraProp = xrOriginType.GetProperty("Camera");
            if (cameraProp != null) cameraProp.SetValue(xrOrigin, camObj.GetComponent<Camera>());
            var floorProp = xrOriginType.GetProperty("CameraFloorOffsetObject");
            if (floorProp != null) floorProp.SetValue(xrOrigin, camOffset);
        }
    }

    private static void BuildStylizedVRHand(GameObject anchor, bool isLeft, Material gloveMat, Material seamMat)
    {
        GameObject handVisual = new GameObject(isLeft ? "Left_Hand_Visual" : "Right_Hand_Visual");
        handVisual.transform.SetParent(anchor.transform, false);

        GameObject wrist = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wrist.name = "Wrist_Cuff";
        wrist.transform.SetParent(handVisual.transform, false);
        wrist.transform.localPosition = new Vector3(0f, 0f, -0.06f);
        wrist.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        wrist.transform.localScale = new Vector3(0.045f, 0.025f, 0.045f);
        wrist.GetComponent<Renderer>().material = gloveMat;
        Object.DestroyImmediate(wrist.GetComponent<Collider>());

        GameObject palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        palm.name = "Palm";
        palm.transform.SetParent(handVisual.transform, false);
        palm.transform.localPosition = new Vector3(0f, 0f, 0.02f);
        palm.transform.localScale = new Vector3(0.065f, 0.025f, 0.075f);
        palm.GetComponent<Renderer>().material = gloveMat;
        Object.DestroyImmediate(palm.GetComponent<Collider>());

        // Pointing index finger for poking buttons
        GameObject indexFinger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indexFinger.name = "Index_Finger";
        indexFinger.transform.SetParent(handVisual.transform, false);
        indexFinger.transform.localPosition = new Vector3(isLeft ? 0.025f : -0.025f, 0.005f, 0.08f);
        indexFinger.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        indexFinger.transform.localScale = new Vector3(0.012f, 0.035f, 0.012f);
        indexFinger.GetComponent<Renderer>().material = seamMat;
        Object.DestroyImmediate(indexFinger.GetComponent<Collider>());
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return panel;
    }

    private static GameObject CreateText(GameObject parent, string name, string content, float fontSize, Vector2 anchoredPos, Vector2 size, Color color, FontStyles style, TMP_FontAsset font)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        return textObj;
    }

    private static GameObject CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color btnColor, TMP_FontAsset font)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = btnColor;
        colors.highlightedColor = btnColor * 1.25f;
        colors.pressedColor = btnColor * 0.8f;
        btn.colors = colors;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 26f;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return btnObj;
    }

    private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("URP/Lit")
                     ?? Shader.Find("Standard");

        Material mat = new Material(shader) { name = name };
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

        return mat;
    }
}
