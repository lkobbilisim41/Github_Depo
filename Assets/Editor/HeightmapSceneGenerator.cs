using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeightmapSceneGenerator : EditorWindow
{
    public DefaultAsset heightmapFolder;
    public DefaultAsset scenesOutputFolder;
    public GameObject playerPrefab;
    public GameObject seaPrefab;   // water.prefab burada atanacak

    // Eski "base" değerler fallback olarak kalsın
    public float baseTerrainWidth  = 400f;
    public float baseTerrainLength = 400f;
    public float baseTerrainHeight = 20f;

    [Header("originRealY Tabanlı Ölçek")]
    [Tooltip("Formülde kullanılan sabit: terrainSizeXZ = (referansMesafe / originRealY) * birimCarpani")]
    public float referansMesafe = 340f;

    [Tooltip("Formülde kullanılan sabit: terrainSizeXZ = (referansMesafe / originRealY) * birimCarpani")]
    public float birimCarpani = 20f;

    [Tooltip("Örnek referans zoom değeri (örneğin 17).")]
    public float referenceZoom = 17f;

    [Tooltip("referenceZoom (örn. 17) için hedef yükseklik (örn. 10).")]
    public float referenceHeightAtRefZoom = 10f;

    [Tooltip("Yüksekliği biraz arttırmak / azaltmak için çarpan (örn. 1.2).")]
    public float heightMultiplier = 1.2f;

    private const string TerrainOriginsPathRelative = "Assets/Data/terrain_origins.csv";

    private class TerrainOriginRecord
    {
        public string SceneKey;
        public float OriginRealX;
        public float OriginRealZ;
        public float OriginRealY;   // originRealy (heightmapper zoom)
        public string Kod;
    }

    [MenuItem("Tools/Heightmap Scene Generator")]
    public static void ShowWindow()
    {
        GetWindow<HeightmapSceneGenerator>("Heightmap Scene Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Heightmap → Terrain → Scene", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        heightmapFolder    = (DefaultAsset)EditorGUILayout.ObjectField("Heightmap Folder",     heightmapFolder,    typeof(DefaultAsset), false);
        scenesOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Scenes Output Folder", scenesOutputFolder, typeof(DefaultAsset), false);
        playerPrefab       = (GameObject)EditorGUILayout.ObjectField("Player Prefab",          playerPrefab,       typeof(GameObject),   false);
        seaPrefab          = (GameObject)EditorGUILayout.ObjectField("Water Prefab",           seaPrefab,          typeof(GameObject),   false);

        EditorGUILayout.Space();

        GUILayout.Label("Varsayılan Terrain Boyutları (CSV kayıt yoksa kullanılacak)", EditorStyles.boldLabel);
        baseTerrainWidth  = EditorGUILayout.FloatField("Base Terrain Width (X)",  baseTerrainWidth);
        baseTerrainLength = EditorGUILayout.FloatField("Base Terrain Length (Z)", baseTerrainLength);
        baseTerrainHeight = EditorGUILayout.FloatField("Base Terrain Height (Y)", baseTerrainHeight);

        EditorGUILayout.Space();

        GUILayout.Label("originRealY Tabanlı Ölçek Parametreleri", EditorStyles.boldLabel);
        referansMesafe           = EditorGUILayout.FloatField("Referans Mesafe (örn. 340)", referansMesafe);
        birimCarpani             = EditorGUILayout.FloatField("Birim Çarpanı (örn. 20)",    birimCarpani);
        referenceZoom            = EditorGUILayout.FloatField("Referans Zoom (örn. 17)",    referenceZoom);
        referenceHeightAtRefZoom = EditorGUILayout.FloatField("Ref Zoom Yüksekliği (örn. 10)", referenceHeightAtRefZoom);
        heightMultiplier         = EditorGUILayout.FloatField("Yükseklik Çarpanı (örn. 1.2)", heightMultiplier);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Scenes From Heightmaps"))
        {
            try
            {
                GenerateScenes();
            }
            catch (System.Exception e)
            {
                Debug.LogError("HeightmapSceneGenerator hata: " + e);
            }
        }
    }

    private void GenerateScenes()
    {
        if (heightmapFolder == null || scenesOutputFolder == null || playerPrefab == null)
        {
            Debug.LogError("Heightmap Folder, Scenes Output Folder ve Player Prefab boş olamaz.");
            return;
        }

        // terrain_origins.csv oku
        var originDict = LoadTerrainOrigins(TerrainOriginsPathRelative);
        if (originDict == null)
        {
            Debug.LogError("terrain_origins.csv okunamadı veya bulunamadı: " + TerrainOriginsPathRelative);
            return;
        }

        string heightmapFolderPath     = AssetDatabase.GetAssetPath(heightmapFolder);
        string scenesOutputFolderPath  = AssetDatabase.GetAssetPath(scenesOutputFolder);

        if (!AssetDatabase.IsValidFolder(heightmapFolderPath))
        {
            Debug.LogError("Heightmap Folder geçerli bir klasör değil: " + heightmapFolderPath);
            return;
        }

        if (!AssetDatabase.IsValidFolder(scenesOutputFolderPath))
        {
            Debug.LogError("Scenes Output Folder geçerli bir klasör değil: " + scenesOutputFolderPath);
            return;
        }

        string[] allGuids = AssetDatabase.FindAssets("t:Texture2D");
        int count = 0;

        foreach (string guid in allGuids)
        {
            string texPath = AssetDatabase.GUIDToAssetPath(guid);

            // Sadece seçilen klasör altındaki texture'lar
            if (!texPath.StartsWith(heightmapFolderPath))
                continue;

            if (!EnsureTextureReadable(texPath))
            {
                Debug.LogWarning("Texture okunabilir hale getirilemedi, atlandı: " + texPath);
                continue;
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
                continue;

            string key = tex.name.ToLowerInvariant();

            TerrainOriginRecord originRec = null;
            originDict.TryGetValue(key, out originRec);

            if (originRec == null)
            {
                Debug.LogWarning($"terrain_origins.csv içinde '{key}' için kayıt bulunamadı. Varsayılan terrain boyutları kullanılacak.");
            }

            CreateSceneFromHeightmap(tex, scenesOutputFolderPath, originRec);
            count++;
        }

        Debug.Log("Toplam oluşturulan sahne sayısı: " + count);
    }

    // Importer üzerinden Read/Write ve sıkıştırma ayarını düzelt
    private bool EnsureTextureReadable(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return false;

        bool changed = false;

        if (!importer.isReadable)
        {
            importer.isReadable = true;
            changed = true;
        }

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            changed = true;
        }

        if (changed)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        return true;
    }

    // Projedeki tüm TerrainLayer asset'lerini toplar
    private TerrainLayer[] GetAllTerrainLayers()
    {
        string[] layerGuids = AssetDatabase.FindAssets("t:TerrainLayer");
        if (layerGuids == null || layerGuids.Length == 0)
            return new TerrainLayer[0];

        TerrainLayer[] layers = new TerrainLayer[layerGuids.Length];

        for (int i = 0; i < layerGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(layerGuids[i]);
            layers[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        }

        return layers;
    }

    private void CreateSceneFromHeightmap(Texture2D heightmapTex, string outputFolder, TerrainOriginRecord originRec)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        TerrainData data = new TerrainData();
        int resolution = Mathf.Clamp(Mathf.Min(heightmapTex.width, heightmapTex.height), 33, 4097);
        data.heightmapResolution = resolution;

        float terrainWidth;
        float terrainLength;
        float terrainHeight;

        if (originRec != null && originRec.OriginRealY > 0f)
        {
            float originY = originRec.OriginRealY;

            // Senin verdiğin mantık:
            // örn: originY=17 → 340/17*20 ≈ 400
            //      originY=10 → 340/10*20 = 680
            float sizeXZ = (referansMesafe / originY) * birimCarpani;

            // Yükseklik: ters orantılı + küçük bir çarpan
            float h = heightMultiplier * referenceHeightAtRefZoom * (referenceZoom / originY);

            terrainWidth  = sizeXZ;
            terrainLength = sizeXZ;
            terrainHeight = h;

            Debug.Log($"[HeightmapSceneGenerator] {heightmapTex.name}: originRealY={originY}, SizeXZ={sizeXZ}, Height={terrainHeight}");
        }
        else
        {
            // CSV kaydı bulunamazsa eski base değerleri kullan
            terrainWidth  = baseTerrainWidth;
            terrainLength = baseTerrainLength;
            terrainHeight = baseTerrainHeight;

            Debug.LogWarning($"[HeightmapSceneGenerator] {heightmapTex.name}: CSV kaydı bulunamadı, baseTerrain değerleri kullanıldı.");
        }

        data.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

        Color[] pixels = heightmapTex.GetPixels(0, 0, resolution, resolution);
        float[,] heights = new float[resolution, resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
                heights[y, x] = pixels[y * resolution + x].grayscale;
        }

        data.SetHeights(0, 0, heights);

        // TÜM TERRAIN LAYER'LARI EKLE
        TerrainLayer[] layers = GetAllTerrainLayers();
        if (layers != null && layers.Length > 0)
        {
            data.terrainLayers = layers;
        }

        GameObject terrainGO = Terrain.CreateTerrainGameObject(data);
        terrainGO.name = "Terrain_" + heightmapTex.name;

        Terrain terrain = terrainGO.GetComponent<Terrain>();
        Vector3 size = data.size;
        Vector3 centerXZ = new Vector3(size.x / 2f, 0f, size.z / 2f);

        // PLAYER (tam merkez)
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        float groundY = terrain.SampleHeight(centerXZ);
        player.transform.position = new Vector3(centerXZ.x, groundY + 2f, centerXZ.z);

        // WATER PREFAB (terrain ile aynı X/Z boyutunda, Y=0.5)
        if (seaPrefab != null)
        {
            GameObject sea = (GameObject)PrefabUtility.InstantiatePrefab(seaPrefab, scene);
            sea.name = "Water_" + heightmapTex.name;

            // Pozisyon: terrain merkezinde, Y = 0.5
            sea.transform.position = new Vector3(size.x / 2f, 0.5f, size.z / 2f);

            // Varsayım: water.prefab 1x1 plane ise
            Vector3 s = sea.transform.localScale;
            s.x = size.x;
            s.z = size.z;
            sea.transform.localScale = s;
        }

        // DIRECTIONAL LIGHT
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        string scenePath = Path.Combine(outputFolder, "" + heightmapTex.name + ".unity").Replace("\\", "/");
        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log("Oluşturuldu: " + scenePath);
    }

    /// <summary>
    /// terrain_origins.csv → Dictionary[sahne_adi(lower)] = TerrainOriginRecord
    /// Format: sahne_adi;originRealX;originRealZ;originRealY;kod
    /// </summary>
    private Dictionary<string, TerrainOriginRecord> LoadTerrainOrigins(string assetRelativePath)
    {
        var dict = new Dictionary<string, TerrainOriginRecord>();

        string fullPath;
        if (assetRelativePath.StartsWith("Assets"))
        {
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetRelativePath);
        }
        else
        {
            fullPath = assetRelativePath;
        }

        if (!File.Exists(fullPath))
        {
            Debug.LogError("Terrain origins CSV bulunamadı: " + fullPath);
            return null;
        }

        var lines = File.ReadAllLines(fullPath);
        if (lines.Length <= 1)
            return dict;

        var ci = CultureInfo.InvariantCulture;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var parts = line.Split(';');
            if (parts.Length < 4) continue;

            string sahneAdi = parts[0].Trim();
            if (string.IsNullOrEmpty(sahneAdi)) continue;

            float originRealX, originRealZ, originRealY;

            float.TryParse(parts[1].Trim(), NumberStyles.Float, ci, out originRealX);
            float.TryParse(parts[2].Trim(), NumberStyles.Float, ci, out originRealZ);
            float.TryParse(parts[3].Trim(), NumberStyles.Float, ci, out originRealY);

            string kod = parts.Length > 4 ? parts[4].Trim() : "";

            var rec = new TerrainOriginRecord
            {
                SceneKey    = sahneAdi.ToLowerInvariant(),
                OriginRealX = originRealX,
                OriginRealZ = originRealZ,
                OriginRealY = originRealY,
                Kod         = kod
            };

            dict[rec.SceneKey] = rec;
        }

        return dict;
    }
}
