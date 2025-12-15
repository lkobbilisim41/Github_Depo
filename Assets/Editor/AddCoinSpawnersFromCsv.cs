using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddCoinSpawnersFromCsv
{
    private const string MasterCsvPath = "Assets/Data/binalar_master.csv";
    private const string PrefabRootPath = "Assets/bina_prefab/";
    private const string CoinPrefabPath = "Assets/Prefabs/sikke_yeni.prefab";
    private const string StreamingPath = "Assets/StreamingAssets/";

    // BuildingPlacerFromGeoEditor ile aynı mantıkta
    private const string DataFolderRelative = "Assets/Data";
    private const string TerrainOriginsCsvFileName = "terrain_origins.csv";
    private const float metersPerUnityUnit = 1f;
    private const float yOffset = 0.0f;

    private class BinaKaydi
    {
        public string SahneAdi;
        public string BinaAdi;
        public string PrefabName;
        public string CsvDosya;
        public float RealX;
        public float RealZ;
    }

    [MenuItem("Tools/Binalar/Master CSV'den Bina ve CoinSpawner Ekle (Tek bina merkezde, çoklu doğru konumda)")]
    public static void Run()
    {
        // Master CSV oku
        var kayitlar = LoadMasterCsv(MasterCsvPath);
        if (kayitlar == null || kayitlar.Count == 0)
        {
            Debug.LogError("Master CSV boş veya okunamadı: " + MasterCsvPath);
            return;
        }

        // Coin prefab
        var coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoinPrefabPath);
        if (coinPrefab == null)
        {
            Debug.LogError("sikke_prefab bulunamadı: " + CoinPrefabPath);
            return;
        }

        // Projedeki sahneler (Assets/Scenes altında)
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        var allScenePaths = sceneGuids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .ToList();

        // kayıtları sahne adına göre grupla
        var groups = kayitlar.GroupBy(k => k.SahneAdi);

        foreach (var group in groups)
        {
            string sahneKey = group.Key;
            string scenePath = FindScenePathForKey(sahneKey, allScenePaths);

            if (scenePath == null)
            {
                Debug.LogWarning("Bu sahne adına uygun sahne bulunamadı: " + sahneKey);
                continue;
            }

            // Sahneyi aç
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log("İşlenen sahne: " + scene.name);

            // Terrain bul
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
                terrain = UnityEngine.Object.FindObjectOfType<Terrain>();

            if (terrain == null)
            {
                Debug.LogError("Sahnede Terrain bulunamadı: " + scene.name);
                continue;
            }

            string activeSceneName = scene.name;

            // Root obje (Binalar_<sahne>) – BuildingPlacer ile uyumlu
            GameObject root = GetOrCreateRootObject(scene, activeSceneName);

            int binaSayisi = group.Count();

            // Eğer sahnede 1’den fazla bina varsa, originRealX / originRealZ gerekiyor
            float originRealX = 0f;
            float originRealZ = 0f;
            bool hasOrigin = false;

            if (binaSayisi > 1)
            {
                if (!TryGetTerrainOrigin(activeSceneName, out originRealX, out originRealZ))
                {
                    Debug.LogError($"AddCoinSpawnersFromCsv: '{activeSceneName}' için terrain_origins.csv içinde kayıt bulunamadı.");
                    continue;
                }
                hasOrigin = true;
            }

            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            foreach (var kayit in group)
            {
                string prefabPath = PrefabRootPath + kayit.PrefabName + ".prefab";
                var buildingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (buildingPrefab == null)
                {
                    Debug.LogWarning("Prefab bulunamadı: " + prefabPath);
                    continue;
                }

                // Sahneye bina instance'ı ekle
                GameObject instance = PrefabUtility.InstantiatePrefab(buildingPrefab, scene) as GameObject;
                if (instance == null)
                {
                    Debug.LogError("Prefab instantiate edilemedi: " + prefabPath);
                    continue;
                }

                instance.name = kayit.PrefabName;
                instance.transform.SetParent(root.transform);

                Vector3 worldPos = instance.transform.position;

                if (binaSayisi == 1)
                {
                    // TEK BİNA: Terrain'in tam ortasına koy
                    Vector3 centerXZ = new Vector3(
                        terrainPos.x + terrainSize.x / 2f,
                        0f,
                        terrainPos.z + terrainSize.z / 2f
                    );

                    float terrainY = terrain.SampleHeight(centerXZ) + terrainPos.y + yOffset;
                    worldPos = new Vector3(centerXZ.x, terrainY, centerXZ.z);
                }
                else
                {
                    // BİRDEN FAZLA BİNA: Gerçek koordinata göre yerleştir
                    if (!hasOrigin)
                    {
                        Debug.LogError($"AddCoinSpawnersFromCsv: '{activeSceneName}' için origin yok, bina konumu atlandı: {kayit.BinaAdi}");
                        UnityEngine.Object.DestroyImmediate(instance);
                        continue;
                    }

                    float realX = kayit.RealX;
                    float realZ = kayit.RealZ;

                    // CSV'den koordinat parse edilemediyse 0 olabilir, onu da kontrol edelim
                    if (Math.Abs(realX) < 0.0001f && Math.Abs(realZ) < 0.0001f)
                    {
                        Debug.LogWarning($"AddCoinSpawnersFromCsv: '{kayit.BinaAdi}' için realX/realZ 0 görünüyor, konumlandırma atlandı.");
                        UnityEngine.Object.DestroyImmediate(instance);
                        continue;
                    }

                    // Real → local (BuildingPlacerFromGeoEditor ile aynı)
                    float offsetRealX = realX - originRealX;
                    float offsetRealZ = realZ - originRealZ;

                    float localX = offsetRealX / metersPerUnityUnit;
                    float localZ = offsetRealZ / metersPerUnityUnit;

                    worldPos = new Vector3(
                        terrainPos.x + localX,
                        0f,
                        terrainPos.z + localZ
                    );

                    float terrainY = terrain.SampleHeight(worldPos) + terrainPos.y + yOffset;
                    worldPos.y = terrainY;
                }

                // Pozisyonu uygula
                instance.transform.position = worldPos;

                // CoinSpawner ekle / varsa al
                CoinSpawner spawner = instance.GetComponent<CoinSpawner>();
                if (spawner == null)
                    spawner = instance.AddComponent<CoinSpawner>();

                spawner.yapi_adi = kayit.BinaAdi;
                spawner.coinPrefab = coinPrefab;
                spawner.targetPrefab = instance;

                // CSV dosyası: Assets/StreamingAssets/<csv_dosya>
                spawner.csvFilePath = Path.Combine(StreamingPath, kayit.CsvDosya).Replace("\\", "/");

                // Varsayılan değerler
                if (spawner.numberOfCoins <= 0) spawner.numberOfCoins = 10;
                if (spawner.maxDistance <= 0f) spawner.maxDistance = 5f;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("İşlem tamamlandı: Tek bina olan sahnelerde bina merkezde, çoklu sahnelerde gerçek konuma yerleştirildi ve CoinSpawner'lar bağlandı.");
    }

    private static List<BinaKaydi> LoadMasterCsv(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Master CSV bulunamadı: " + path);
            return null;
        }

        var list = new List<BinaKaydi>();

        // Sistem varsayılan encoding'i – aynı BuildingPlacerFromGeoEditor mantığı
        string[] lines = File.ReadAllLines(path, Encoding.Default);
        if (lines.Length < 2) return list;

        CultureInfo ci = CultureInfo.InvariantCulture;

        // başlık: sahne_adi;bina_adi;prefab;csv_dosya;realX;realZ;olcek;konum
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(';');
            if (parts.Length < 4) continue; // En azından ilk 4 sütun şart

            string sahneAdi = parts[0].Trim();
            string binaAdi = parts[1].Trim();
            string prefabName = parts[2].Trim();
            string csvDosya = parts[3].Trim();

            float realX = 0f;
            float realZ = 0f;

            if (parts.Length >= 6)
            {
                string realXStr = parts[4].Trim();
                string realZStr = parts[5].Trim();

                if (!float.TryParse(realXStr, NumberStyles.Float, ci, out realX) ||
                    !float.TryParse(realZStr, NumberStyles.Float, ci, out realZ))
                {
                    Debug.LogWarning($"AddCoinSpawnersFromCsv: Satır {i + 1} realX/realZ parse edilemedi: {realXStr}, {realZStr}");
                    // Parse edilemese de kaydı ekliyoruz ama realX/realZ = 0 olarak kalacak
                }
            }

            list.Add(new BinaKaydi
            {
                SahneAdi = sahneAdi,
                BinaAdi = binaAdi,
                PrefabName = prefabName,
                CsvDosya = csvDosya,
                RealX = realX,
                RealZ = realZ
            });
        }

        return list;
    }

    // sahne_adi string'ine göre Assets/Scenes altındaki sahne yolunu bul
    private static string FindScenePathForKey(string key, List<string> scenePaths)
    {
        key = key.ToLowerInvariant();

        // önce tam eşleşme
        foreach (var path in scenePaths)
        {
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name == key) return path;
        }

        // sonra içinde geçen (örn: Terrain_canakkale vs.)
        foreach (var path in scenePaths)
        {
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains(key)) return path;
        }

        return null;
    }

    /// <summary>
    /// terrain_origins.csv içinden ilgili sahnenin originRealX / originRealZ değerlerini okur.
    /// BuildingPlacerFromGeoEditor.TryGetTerrainOrigin ile aynı mantık.
    /// </summary>
    private static bool TryGetTerrainOrigin(string sceneName, out float originRealX, out float originRealZ)
    {
        originRealX = 0f;
        originRealZ = 0f;

        string dataFolderAbsolute = Path.Combine(Application.dataPath, "Data");
        string terrainOriginsPath = Path.Combine(dataFolderAbsolute, TerrainOriginsCsvFileName);

        if (!File.Exists(terrainOriginsPath))
        {
            Debug.LogError($"AddCoinSpawnersFromCsv: terrain_origins.csv bulunamadı: {terrainOriginsPath}");
            return false;
        }

        string[] lines = File.ReadAllLines(terrainOriginsPath);
        CultureInfo ci = CultureInfo.InvariantCulture;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] parts = line.Split(new[] { ';', ',' }, StringSplitOptions.None);
            if (parts.Length < 3)
                continue;

            string sahneAdi = parts[0].Trim();
            if (!string.Equals(sahneAdi, sceneName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, ci, out originRealX) ||
                !float.TryParse(parts[2].Trim(), NumberStyles.Float, ci, out originRealZ))
            {
                Debug.LogError($"AddCoinSpawnersFromCsv: '{sceneName}' için origin değerleri parse edilemedi: {line}");
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Yerleştirilen binaları tutmak için sahne içinde bir root obje oluşturur/bulur.
    /// (Binalar_<sahne_adi>)
    /// </summary>
    private static GameObject GetOrCreateRootObject(Scene scene, string activeSceneName)
    {
        string rootName = $"Binalar_{activeSceneName}";

        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == rootName)
                return go;
        }

        GameObject root = new GameObject(rootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        return root;
    }
}
