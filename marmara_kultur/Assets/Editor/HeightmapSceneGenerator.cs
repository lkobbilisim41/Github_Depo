using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeightmapSceneGenerator : EditorWindow
{
    public DefaultAsset heightmapFolder;
    public DefaultAsset scenesOutputFolder;
    public GameObject playerPrefab;
    public GameObject seaPrefab;          // YENİ: Deniz prefabı

    public float terrainWidth = 500f;
    public float terrainLength = 500f;
    public float terrainHeight = 100f;

    [MenuItem("Tools/Heightmap Scene Generator")]
    public static void ShowWindow()
    {
        GetWindow<HeightmapSceneGenerator>("Heightmap Scene Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Heightmap → Terrain → Scene", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        heightmapFolder = (DefaultAsset)EditorGUILayout.ObjectField("Heightmap Folder", heightmapFolder, typeof(DefaultAsset), false);
        scenesOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Scenes Output Folder", scenesOutputFolder, typeof(DefaultAsset), false);
        playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(GameObject), false);
        seaPrefab = (GameObject)EditorGUILayout.ObjectField("Sea Prefab", seaPrefab, typeof(GameObject), false);   // YENİ

        EditorGUILayout.Space();

        terrainWidth = EditorGUILayout.FloatField("Terrain Width (X)", terrainWidth);
        terrainLength = EditorGUILayout.FloatField("Terrain Length (Z)", terrainLength);
        terrainHeight = EditorGUILayout.FloatField("Terrain Height (Y)", terrainHeight);

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

        string heightmapFolderPath = AssetDatabase.GetAssetPath(heightmapFolder);
        string scenesOutputFolderPath = AssetDatabase.GetAssetPath(scenesOutputFolder);

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

            CreateSceneFromHeightmap(tex, scenesOutputFolderPath);
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

    private void CreateSceneFromHeightmap(Texture2D heightmapTex, string outputFolder)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        TerrainData data = new TerrainData();
        int resolution = Mathf.Clamp(Mathf.Min(heightmapTex.width, heightmapTex.height), 33, 4097);

        data.heightmapResolution = resolution;
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

        // PLAYER
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        float groundY = terrain.SampleHeight(centerXZ);
        player.transform.position = new Vector3(centerXZ.x, groundY + 2f, centerXZ.z);

        // DENİZ PREFABI (terrain ile aynı X/Z boyutunda, Y=0.5)
        if (seaPrefab != null)
        {
            GameObject sea = (GameObject)PrefabUtility.InstantiatePrefab(seaPrefab, scene);
            sea.name = "Sea_" + heightmapTex.name;

            // Pozisyon: terrain merkezinde, Y = 0.5
            sea.transform.position = new Vector3(size.x / 2f, 0.5f, size.z / 2f);

            // Varsayım: deniz prefabı 1x1 bir plane ise,
            // X/Z scale'i terrain genişlik/uzunluğuna eşitliyoruz
            Vector3 s = sea.transform.localScale;
            s.x = terrainWidth;
            s.z = terrainLength;
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
}
