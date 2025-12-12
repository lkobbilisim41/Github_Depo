#if UNITY_EDITOR

using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor aracı:
/// Assets/Data altındaki binalar_master.csv + terrain_origins.csv dosyalarına göre
/// aktif sahnedeki Terrain üzerine bina prefablarını yerleştirir veya var olanları yeniden konumlandırır.
/// </summary>
public static class BuildingPlacerFromGeoEditor
{
    // Binayı zeminden biraz yukarı kaldırmak için offset
    private const float yOffset = 0.0f;

    // Projede sabit yollar
    private const string DataFolderRelative = "Assets/Data";
    private const string BuildingsCsvFileName = "binalar_master.csv";
    private const string TerrainOriginsCsvFileName = "terrain_origins.csv";
    private const string PrefabFolderRelative = "Assets/bina_prefab";

    // ==========================
    // 1) İLK YERLEŞTİRME (ESKİ DAVRANIŞ)
    // ==========================
    [MenuItem("Tools/Place Buildings For Active Scene")]
    public static void PlaceBuildingsForActiveScene()
    {
        // Eski davranış: XZ swap YOK
        ProcessBuildingsForActiveScene(instantiateIfMissing: true, swapXZ: true);
    }

    // XZ swap'lı versiyon
    [MenuItem("Tools/Place Buildings For Active Scene (Swap XZ)")]
    public static void PlaceBuildingsForActiveSceneSwapXZ()
    {
        ProcessBuildingsForActiveScene(instantiateIfMissing: true, swapXZ: true);
    }

    // ==========================
    // 2) SADECE MEVCUTLARI YENİDEN KONUMLANDIR
    // ==========================
    [MenuItem("Tools/Building Placer/Reposition Existing Buildings For Active Scene")]
    public static void RepositionExistingBuildingsForActiveScene()
    {
        // Eski davranış: XZ swap YOK
        ProcessBuildingsForActiveScene(instantiateIfMissing: false, swapXZ: false);
    }

    // XZ swap'lı versiyon
    [MenuItem("Tools/Building Placer/Reposition Existing Buildings For Active Scene (Swap XZ)")]
    public static void RepositionExistingBuildingsForActiveSceneSwapXZ()
    {
        ProcessBuildingsForActiveScene(instantiateIfMissing: false, swapXZ: true);
    }

    /// <summary>
    /// Ortak iş mantığı: CSV'yi okur, hedef world pozisyonu hesaplar.
    /// instantiateIfMissing = true ise eksikse prefab instantiate eder,
    /// false ise eksik olanları atlar, sadece mevcut objeyi hareket ettirir.
    /// swapXZ = true ise localX ve localZ yer değiştirir (harita ters döndüyse).
    /// </summary>
    private static void ProcessBuildingsForActiveScene(bool instantiateIfMissing, bool swapXZ)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            Debug.LogError("BuildingPlacerFromGeoEditor: Aktif sahne yüklü değil.");
            return;
        }

        string activeSceneName = scene.name;
        Debug.Log($"BuildingPlacerFromGeoEditor: Aktif sahne: {activeSceneName}, swapXZ={swapXZ}");

        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("BuildingPlacerFromGeoEditor: Aktif sahnede Terrain bulunamadı.");
            return;
        }

        if (!TryGetTerrainOrigin(activeSceneName, out float originRealX, out float originRealZ, out float originRealY))
        {
            Debug.LogError($"BuildingPlacerFromGeoEditor: '{activeSceneName}' için terrain_origins.csv içinde kayıt bulunamadı.");
            return;
        }

        string buildingsCsvPath = Path.Combine(Application.dataPath, "Data", BuildingsCsvFileName);
        if (!File.Exists(buildingsCsvPath))
        {
            Debug.LogError($"BuildingPlacerFromGeoEditor: binalar_master.csv bulunamadı: {buildingsCsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(buildingsCsvPath);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("BuildingPlacerFromGeoEditor: binalar_master.csv boş ya da sadece başlık içeriyor.");
            return;
        }

        Vector3 terrainSize = terrain.terrainData.size;
        CultureInfo ci = CultureInfo.InvariantCulture;

        GameObject root = GetOrCreateRootObject(scene, activeSceneName);

        int processedCount = 0;

        // 0. satır başlık
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] parts = line.Split(new[] { ';', ',' }, StringSplitOptions.None);
            if (parts.Length < 6)
            {
                Debug.LogWarning($"BuildingPlacerFromGeoEditor: Satır {i + 1} eksik sütun: {line}");
                continue;
            }

            string sahneAdi = parts[0].Trim();
            string binaAdi  = parts[1].Trim();
            string prefabAdi = parts[2].Trim();
            string csvDosya = parts[3].Trim(); // kullanılmıyor
            string realXStr = parts[4].Trim();
            string realZStr = parts[5].Trim();

            Debug.LogWarning($"Sahne adı: Satır {sahneAdi} koordinat: {realXStr}, {realZStr}");

            if (!string.Equals(sahneAdi, activeSceneName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!float.TryParse(realXStr, NumberStyles.Float, ci, out float realX) ||
                !float.TryParse(realZStr, NumberStyles.Float, ci, out float realZ))
            {
                Debug.LogWarning($"BuildingPlacerFromGeoEditor: Satır {i + 1} koordinat parse edilemedi: {realXStr}, {realZStr}");
                continue;
            }

            // Real → local
            float offsetRealX = realX - originRealX;
            float offsetRealZ = realZ - originRealZ;
            float olcek = originRealY;
            float olcek2 = 1f / Mathf.Pow(2, originRealY);
            float metersPerPixel = 0.0001f;
            Debug.LogWarning($"offsetRealX: '{offsetRealX}'");
            Debug.LogWarning($"offsetRealZ: '{offsetRealZ}'");
            Debug.LogWarning($"originRealY: '{originRealY}'");
            Debug.LogWarning($"Ölçek2: '{olcek2}'");
            Debug.LogWarning($"metersPerPixel: '{metersPerPixel}'");

            float localZ = offsetRealZ / olcek2;
            float localX = offsetRealX / olcek2;

            // İSTENİRSE XZ TAKASI
            if (swapXZ)
            {
                float tmp = localX;
                localX = localZ;
                localZ = tmp;
            }

            /* Eğer hala sınır kontrolü yapmak istersen burayı açabilirsin
            if (localX < 0 || localZ < 0 || localX > terrainSize.x || localZ > terrainSize.z)
            {
                Debug.LogWarning(
                    $"BuildingPlacerFromGeoEditor: '{binaAdi}' terrain dışında. " +
                    $"localX={localX}, localZ={localZ}, terrainSize=({terrainSize.x}, {terrainSize.z})");
                continue;
            }
            */

            Vector3 worldPos = new Vector3(
                terrain.transform.position.x + localX,
                0f,
                terrain.transform.position.z + localZ
            );

            float terrainY = terrain.SampleHeight(worldPos) + terrain.transform.position.y + yOffset;
            worldPos.y = terrainY;

            // --- BURADA ÖNCE MEVCUT NESNEYİ ARIYORUZ ---
            GameObject instance = FindExistingBuilding(root, binaAdi, prefabAdi);

            if (instance == null)
            {
                if (!instantiateIfMissing)
                {
                    // Sadece yeniden konumlandırma modundayız, eksikleri atla.
                    Debug.LogWarning(
                        $"BuildingPlacerFromGeoEditor: '{binaAdi}' için sahnede mevcut bina bulunamadı, " +
                        "Reposition modunda olduğumuz için yeni prefab instantiate edilmedi.");
                    continue;
                }

                // İlk yerleştirme modunda isek prefab instantiate et.
                string prefabPath = $"{PrefabFolderRelative}/{prefabAdi}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    Debug.LogError($"BuildingPlacerFromGeoEditor: Prefab bulunamadı: {prefabPath}");
                    continue;
                }

                instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                if (instance == null)
                {
                    Debug.LogError($"BuildingPlacerFromGeoEditor: Prefab instantiate edilemedi: {prefabPath}");
                    continue;
                }

                instance.transform.SetParent(root.transform);
                instance.name = binaAdi;
            }

            // Ortak: mevcut instance'ın pozisyonunu güncelle
            instance.transform.position = worldPos;

            processedCount++;
        }

        string mode = instantiateIfMissing ? "yerleştirildi / güncellendi" : "yeniden konumlandırıldı";
        Debug.Log($"BuildingPlacerFromGeoEditor: '{activeSceneName}' sahnesi için {processedCount} bina {mode} (swapXZ={swapXZ}).");

        if (processedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    /// <summary>
    /// Sahnedeki mevcut binaları bulmaya çalışır.
    /// Önce root altında binaAdi adında child arar,
    /// bulamazsa prefabAdi adına göre arama yapar.
    /// </summary>
    private static GameObject FindExistingBuilding(GameObject root, string binaAdi, string prefabAdi)
    {
        // 1) Root altında binaAdi ismiyle child var mı?
        Transform t = root.transform.Find(binaAdi);
        if (t != null)
            return t.gameObject;

        // 2) Root altındaki tüm çocuklarda isim kontrolü
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == root.transform) continue;
            if (child.name == binaAdi || child.name == prefabAdi)
                return child.gameObject;
        }

        // 3) Son çare: tüm sahnede GameObject.Find ile dene
        GameObject go = GameObject.Find(binaAdi);
        if (go != null) return go;

        go = GameObject.Find(prefabAdi);
        return go;
    }

    /// <summary>
    /// terrain_origins.csv içinden originRealX / originRealZ okur.
    /// Format: sahne_adi;originRealX;originRealZ;originRealY
    /// </summary>
    private static bool TryGetTerrainOrigin(string sceneName, out float originRealX, out float originRealZ, out float originRealY)
    {
        originRealX = 0f;
        originRealY = 0f;
        originRealZ = 0f;
        string terrainOriginsPath = Path.Combine(Application.dataPath, "Data", TerrainOriginsCsvFileName);
        if (!File.Exists(terrainOriginsPath))
        {
            Debug.LogError($"BuildingPlacerFromGeoEditor: terrain_origins.csv bulunamadı: {terrainOriginsPath}");
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
            if (parts.Length < 4)
                continue;

            string sahneAdi = parts[0].Trim();
            if (!string.Equals(sahneAdi, sceneName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, ci, out originRealX) ||
                !float.TryParse(parts[2].Trim(), NumberStyles.Float, ci, out originRealZ) ||
                !float.TryParse(parts[3].Trim(), NumberStyles.Float, ci, out originRealY))
            {
                Debug.LogError($"BuildingPlacerFromGeoEditor: '{sceneName}' için origin değerleri parse edilemedi: {line}");
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Yerleştirilen binaları tutmak için sahne içinde bir root obje oluşturur/bulur.
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

#endif
