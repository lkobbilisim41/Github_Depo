using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class RenameFoldersAndCreatePrefabs : EditorWindow
{
    private const string SourceRoot = "Assets/Binalar/";
    private const string TargetFolder = "Assets/bina_prefab/";

    [MenuItem("Tools/Binalar/Klasörleri Temizle ve Prefab Oluþtur")]
    public static void CleanAndCreatePrefabs()
    {
        if (!Directory.Exists(SourceRoot))
        {
            Debug.LogError("Kaynak klasör bulunamadý: " + SourceRoot);
            return;
        }

        // Hedef klasör yoksa oluþtur
        if (!Directory.Exists(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
            AssetDatabase.Refresh();
        }

        // Assets/Binalar altýndaki tüm klasörleri al
        string[] folders = Directory.GetDirectories(SourceRoot);

        foreach (string folder in folders)
        {
            // Eski klasör adý
            string oldFolderName = Path.GetFileName(folder);
            string cleanName = CleanName(oldFolderName);

            string newFolderPath = Path.Combine(SourceRoot, cleanName).Replace("\\", "/");

            // 1) Klasörü yeniden adlandýr
            if (folder != newFolderPath)
            {
                string error = AssetDatabase.MoveAsset(folder, newFolderPath);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError("Klasör taþýnamadý: " + folder + " -> " + newFolderPath + " | " + error);
                    continue;
                }

                Debug.Log("Klasör yeniden adlandýrýldý: " + oldFolderName + " -> " + cleanName);
            }

            // 2) Bu klasörün içindeki .fbx veya .skp model dosyasýný bul
            string modelPath = FindModelInFolder(newFolderPath);
            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogWarning("Model (.fbx / .skp) bulunamadý: " + newFolderPath);
                continue;
            }

            // 3) Modelden bir prefab oluþtur ve bina_prefab altýna kaydet
            string prefabPath = TargetFolder + cleanName + ".prefab";

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogError("Model GameObject olarak yüklenemedi: " + modelPath);
                continue;
            }

            // Modelden geçici bir instance oluþturup prefab haline getiriyoruz
            GameObject tempInstance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (tempInstance == null)
            {
                Debug.LogError("Modelden instance oluþturulamadý: " + modelPath);
                continue;
            }

            // Prefab'i kaydet
            PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
            Debug.Log("Prefab oluþturuldu: " + prefabPath);

            // Geçici instance'ý sahneden sil
            GameObject.DestroyImmediate(tempInstance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("ÝÞLEM TAMAMLANDI: Klasör isimleri temizlendi ve bina_prefab altýnda prefablar üretildi.");
    }

    // Klasör içindeki ilk .fbx veya .skp model dosyasýný bul
    private static string FindModelInFolder(string folderPath)
    {
        // .fbx ve .skp dosyalarýný sýrayla ara (alt klasörlerle birlikte)
        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
        if (fbxFiles.Length > 0)
            return fbxFiles[0].Replace("\\", "/");

        string[] skpFiles = Directory.GetFiles(folderPath, "*.skp", SearchOption.AllDirectories);
        if (skpFiles.Length > 0)
            return skpFiles[0].Replace("\\", "/");

        return null;
    }

    // Türkçe karakter ve özel karakter temizleme
    private static string CleanName(string input)
    {
        string s = input.ToLower();

        s = s.Replace("ö", "o")
             .Replace("ü", "u")
             .Replace("ý", "i")
             .Replace("ð", "g")
             .Replace("þ", "s")
             .Replace("ç", "c");

        // harf, rakam dýþýndaki her þeyi alt çizgiye çevir
        s = Regex.Replace(s, @"[^a-z0-9]+", "_");

        // baþ ve sondaki alt çizgileri temizle
        s = s.Trim('_');

        return s;
    }
}
