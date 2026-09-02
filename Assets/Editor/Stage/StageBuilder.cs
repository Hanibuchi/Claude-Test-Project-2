using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace ProjectBootstrap
{
    public static class StageBuilder
    {
        const string SpritesDir = "Assets/Sprites";
        const string TilesDir = "Assets/Sprites/Tiles";
        const string ScenesDir = "Assets/Scenes";
        const string ScenePath = ScenesDir + "/GameScene.unity";
        const int PixelsPerUnit = 16;
        const int TileTextureSize = 16;

        // Invoke with: -executeMethod ProjectBootstrap.StageBuilder.BuildAndExit
        public static void BuildAndExit()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StageBuilder] Failed: {e}");
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/2D Platformer/Build Demo Stage")]
        public static void Build()
        {
            EnsureFolder(SpritesDir);
            EnsureFolder(TilesDir);
            EnsureFolder(ScenesDir);
            EnsureFolder("Assets/UI");
            EnsureFolder("Assets/Materials");

            Sprite groundSprite = CreateTileSprite("GroundTile", new Color32(120, 80, 40, 255), new Color32(60, 40, 20, 255), new Color32(90, 200, 70, 255));
            Sprite hazardSprite = CreateTileSprite("HazardTile", new Color32(235, 235, 235, 200), new Color32(200, 200, 200, 200), null);
            Sprite playerSprite = CreatePlayerSprite();

            Tile groundTile = CreateTile("GroundTile", groundSprite);
            Tile hazardTile = CreateTile("HazardTile", hazardSprite);

            Material toxicWaterMat = CreateToxicWaterMaterial();
            PanelSettings panelSettings = CreatePanelSettings();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Grid + Tilemaps ---
            GameObject gridGO = new GameObject("Grid", typeof(Grid));
            Grid grid = gridGO.GetComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            GameObject groundGO = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
            groundGO.transform.SetParent(gridGO.transform);
            Tilemap groundTilemap = groundGO.GetComponent<Tilemap>();

            int hazardLayerIndex = LayerMask.NameToLayer("Hazard");
            GameObject hazardGO = new GameObject("Hazard", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
            hazardGO.transform.SetParent(gridGO.transform);
            hazardGO.layer = hazardLayerIndex;
            Tilemap hazardTilemap = hazardGO.GetComponent<Tilemap>();
            TilemapCollider2D hazardCollider = hazardGO.GetComponent<TilemapCollider2D>();
            hazardCollider.isTrigger = true;
            hazardGO.GetComponent<TilemapRenderer>().sharedMaterial = toxicWaterMat;

            Vector3 spawnPosition = PaintStage(groundTilemap, hazardTilemap, groundTile, hazardTile);

            // --- Player ---
            GameObject playerGO = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlayerController));
            playerGO.transform.position = spawnPosition;
            playerGO.GetComponent<SpriteRenderer>().sprite = playerSprite;

            BoxCollider2D playerCollider = playerGO.GetComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(0.8f, 0.9f);
            playerCollider.offset = Vector2.zero;

            Rigidbody2D playerRb = playerGO.GetComponent<Rigidbody2D>();
            playerRb.gravityScale = 3f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            SerializedObject playerSO = new SerializedObject(playerController);
            playerSO.FindProperty("groundLayer").intValue = 1 << 0;
            playerSO.FindProperty("hazardLayer").intValue = 1 << hazardLayerIndex;
            playerSO.ApplyModifiedPropertiesWithoutUndo();

            // --- Camera ---
            GameObject camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
            camGO.tag = "MainCamera";
            Camera cam = camGO.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGO.transform.position = new Vector3(spawnPosition.x, spawnPosition.y + 1.5f, -10f);
            camGO.GetComponent<CameraFollow>().SetTarget(playerGO.transform);

            // --- UI ---
            GameObject uiManagerGO = new GameObject("UIManager", typeof(UIManager));

            GameObject mainMenuGO = new GameObject("MainMenuUI", typeof(UIDocument));
            mainMenuGO.transform.SetParent(uiManagerGO.transform);
            UIDocument mainMenuDoc = mainMenuGO.GetComponent<UIDocument>();
            mainMenuDoc.panelSettings = panelSettings;
            mainMenuDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/MainMenu.uxml");

            GameObject hudGO = new GameObject("HUDUI", typeof(UIDocument));
            hudGO.transform.SetParent(uiManagerGO.transform);
            UIDocument hudDoc = hudGO.GetComponent<UIDocument>();
            hudDoc.panelSettings = panelSettings;
            hudDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/HUD.uxml");

            GameObject gameOverGO = new GameObject("GameOverUI", typeof(UIDocument));
            gameOverGO.transform.SetParent(uiManagerGO.transform);
            UIDocument gameOverDoc = gameOverGO.GetComponent<UIDocument>();
            gameOverDoc.panelSettings = panelSettings;
            gameOverDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/GameOver.uxml");

            UIManager uiManager = uiManagerGO.GetComponent<UIManager>();
            SerializedObject uiSO = new SerializedObject(uiManager);
            uiSO.FindProperty("mainMenuDocument").objectReferenceValue = mainMenuDoc;
            uiSO.FindProperty("hudDocument").objectReferenceValue = hudDoc;
            uiSO.FindProperty("gameOverDocument").objectReferenceValue = gameOverDoc;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            // --- Managers ---
            GameObject startPointGO = new GameObject("StartPoint");
            startPointGO.transform.position = spawnPosition;

            GameObject gameManagerGO = new GameObject("GameManager", typeof(GameManager));
            GameManager gameManager = gameManagerGO.GetComponent<GameManager>();
            SerializedObject gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("player").objectReferenceValue = playerController;
            gmSO.FindProperty("startPoint").objectReferenceValue = startPointGO.transform;
            gmSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject adManagerGO = new GameObject("AdManager", typeof(AdManager));

            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[StageBuilder] Demo stage built at " + ScenePath);
        }

        static Vector3 PaintStage(Tilemap groundTilemap, Tilemap hazardTilemap, Tile groundTile, Tile hazardTile)
        {
            const int groundStartX = -8;
            const int groundEndX = 24;
            const int groundTopY = 0;
            const int groundDepth = 4;
            HashSet<int> pitColumns = new HashSet<int> { 6, 7, 8 };

            for (int x = groundStartX; x <= groundEndX; x++)
            {
                if (pitColumns.Contains(x))
                {
                    continue;
                }

                for (int y = groundTopY; y > groundTopY - groundDepth; y--)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                }
            }

            foreach (int x in pitColumns)
            {
                hazardTilemap.SetTile(new Vector3Int(x, groundTopY - groundDepth + 1, 0), hazardTile);
            }

            for (int x = 12; x <= 14; x++)
            {
                groundTilemap.SetTile(new Vector3Int(x, 3, 0), groundTile);
            }

            for (int x = 18; x <= 20; x++)
            {
                groundTilemap.SetTile(new Vector3Int(x, 6, 0), groundTile);
            }

            return new Vector3(groundStartX + 2, groundTopY + 1.5f, 0f);
        }

        static Sprite CreateTileSprite(string name, Color32 fill, Color32 border, Color32? topStripe)
        {
            string path = $"{TilesDir}/{name}.png";
            Texture2D tex = new Texture2D(TileTextureSize, TileTextureSize, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[TileTextureSize * TileTextureSize];

            for (int y = 0; y < TileTextureSize; y++)
            {
                for (int x = 0; x < TileTextureSize; x++)
                {
                    bool isBorder = x == 0 || y == 0 || x == TileTextureSize - 1 || y == TileTextureSize - 1;
                    bool isTopStripe = topStripe.HasValue && y >= TileTextureSize - 3 && !isBorder;

                    Color32 color;
                    if (isBorder)
                    {
                        color = border;
                    }
                    else if (isTopStripe)
                    {
                        color = topStripe.Value;
                    }
                    else
                    {
                        color = fill;
                    }

                    pixels[y * TileTextureSize + x] = color;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            ConfigureSpriteImporter(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Sprite CreatePlayerSprite()
        {
            string path = $"{SpritesDir}/Player.png";
            int size = TileTextureSize;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32 body = new Color32(70, 160, 235, 255);
            Color32 outline = new Color32(20, 30, 60, 255);
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inHead = y >= 11 && y < 15 && x >= 5 && x < 11;
                    bool inBody = y >= 2 && y < 11 && x >= 3 && x < 13;
                    bool isShape = inHead || inBody;

                    Color32 color = clear;
                    if (isShape)
                    {
                        bool onEdge = (inHead && (y == 14 || x == 5 || x == 10)) ||
                                      (inBody && (y == 2 || x == 3 || x == 12));
                        color = onEdge ? outline : body;
                    }

                    pixels[y * size + x] = color;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            ConfigureSpriteImporter(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void ConfigureSpriteImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        static Tile CreateTile(string name, Sprite sprite)
        {
            string path = $"{TilesDir}/{name}.asset";
            Tile existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
            Tile tile = existing != null ? existing : ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;

            if (existing == null)
            {
                AssetDatabase.CreateAsset(tile, path);
            }
            else
            {
                EditorUtility.SetDirty(tile);
            }

            return tile;
        }

        static Material CreateToxicWaterMaterial()
        {
            string path = "Assets/Materials/ToxicWaterMat.mat";
            Shader shader = Shader.Find("Custom/2D/ToxicWater");
            if (shader == null)
            {
                Debug.LogError("[StageBuilder] Could not find shader 'Custom/2D/ToxicWater'.");
            }

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            Material mat = existing != null ? existing : new Material(shader);
            mat.shader = shader;
            mat.SetColor("_Color", new Color(0.55f, 0.35f, 0.85f, 0.75f));
            mat.SetFloat("_WaveSpeed", 1.5f);
            mat.SetFloat("_WaveFrequency", 12f);
            mat.SetFloat("_WaveAmplitude", 0.05f);
            mat.SetFloat("_ScrollSpeed", 0.15f);

            if (existing == null)
            {
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                EditorUtility.SetDirty(mat);
            }

            return mat;
        }

        static PanelSettings CreatePanelSettings()
        {
            string path = "Assets/UI/PanelSettings.asset";
            PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            PanelSettings settings = existing != null ? existing : ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(720, 1280);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;

            if (existing == null)
            {
                AssetDatabase.CreateAsset(settings, path);
            }
            else
            {
                EditorUtility.SetDirty(settings);
            }

            return settings;
        }

        static void RegisterSceneInBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == scenePath);

            string guid = AssetDatabase.AssetPathToGUID(scenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(new GUID(guid), true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
