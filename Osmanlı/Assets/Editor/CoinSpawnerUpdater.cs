using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class CoinSpawnerUpdater : EditorWindow
{
    [MenuItem("Tools/Update CoinSpawner Values")]
    public static void UpdateCoinSpawners()
    {
        // Sabitlenecek değerler
        int numberOfCoinsValue = 20;
        float minDistanceValue = 1f;
        float maxDistanceValue = 10f;

        // Sahne yollarını al (Build Settings içindeki tüm sahneler)
        var scenes = EditorBuildSettings.scenes;

        foreach (var scene in scenes)
        {
            if (!scene.enabled) continue;

            // Sahneyi aç
            var sceneObj = EditorSceneManager.OpenScene(scene.path);
            Debug.Log("Taranıyor: " + scene.path);

            // CoinSpawner component’lerini bul
            var spawners = GameObject.FindObjectsOfType<CoinSpawner>(true);

            foreach (var spawner in spawners)
            {
                Undo.RecordObject(spawner, "Update CoinSpawner");

                spawner.numberOfCoins = numberOfCoinsValue;
                spawner.minDistance = minDistanceValue;
                spawner.maxDistance = maxDistanceValue;

                EditorUtility.SetDirty(spawner);
            }

            // Sahne değiştiyse kaydet
            EditorSceneManager.SaveScene(sceneObj);
        }

        Debug.Log("CoinSpawner değerleri tüm sahnelerde güncellendi!");
    }
}
