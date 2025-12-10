using System.Collections.Generic;
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
    private const string CoinPrefabPath = "Assets/Prefabs/sikke_prefab.prefab";
    private const string StreamingPath = "Assets/StreamingAssets/";

    private class BinaKaydi
    {
        public string SahneAdi;
        public string BinaAdi;
        public string PrefabName;
        public string CsvDosya;
    }

    [MenuItem("Tools/Binalar/Master CSV'den Bina ve CoinSpawner Ekle")]
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

        // Projedeki sahnelerin yolları
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        var allScenePaths = sceneGuids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .ToList();

        // kayitları sahne adına göre grupla
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

                // CoinSpawner ekle / varsa al
                CoinSpawner spawner = instance.GetComponent<CoinSpawner>();
                if (spawner == null)
                    spawner = instance.AddComponent<CoinSpawner>();

                spawner.yapi_adi = kayit.BinaAdi;
                spawner.coinPrefab = coinPrefab;
                spawner.targetPrefab = instance;

                // CSV dosyası: Assets/StreamingAssets/<csv_dosya>
                spawner.csvFilePath = Path.Combine(StreamingPath, kayit.CsvDosya).Replace("\\", "/");

                // İstersen default değerler
                if (spawner.numberOfCoins <= 0) spawner.numberOfCoins = 20;
                if (spawner.maxDistance  <= 0f) spawner.maxDistance  = 50f;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("İşlem tamamlandı: Master CSV'ye göre tüm sahnelere binalar ve CoinSpawner'lar eklendi.");
    }

    private static List<BinaKaydi> LoadMasterCsv(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Master CSV bulunamadı: " + path);
            return null;
        }

        var list = new List<BinaKaydi>();
        var lines = File.ReadAllLines(path, Encoding.UTF8);

        if (lines.Length < 2) return list;

        // başlık: sahne_adi,bina_adi,prefab,csv_dosya
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 4) continue;

            list.Add(new BinaKaydi
            {
                SahneAdi = parts[0].Trim(),
                BinaAdi = parts[1].Trim(),
                PrefabName = parts[2].Trim(),
                CsvDosya = parts[3].Trim()
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

        // sonra içinde geçen (Scene_canakkale gibi)
        foreach (var path in scenePaths)
        {
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains(key)) return path;
        }

        return null;
    }
}
