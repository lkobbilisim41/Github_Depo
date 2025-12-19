using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneObjectScatterEditor : EditorWindow
{
    public GameObject[] treePrefabs;
    public GameObject[] housePrefabs;

    public float innerClearRadius = 20f;
    public float outerRadius = 30f;
    public int objectCountPerScene = 50;

    [MenuItem("Tools/Scenes/Random Tree & House Scatter")]
    public static void OpenWindow()
    {
        GetWindow<SceneObjectScatterEditor>("Scene Scatter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Random Yerleştirme Ayarları", EditorStyles.boldLabel);

        innerClearRadius = EditorGUILayout.FloatField("Boş Alan (f)", innerClearRadius);
        outerRadius = EditorGUILayout.FloatField("Yerleştirme Bitişi (f)", outerRadius);
        objectCountPerScene = EditorGUILayout.IntField("Sahne Başına Adet", objectCountPerScene);

        SerializedObject so = new SerializedObject(this);

        SerializedProperty trees = so.FindProperty("treePrefabs");
        SerializedProperty houses = so.FindProperty("housePrefabs");

        EditorGUILayout.PropertyField(trees, true);
        EditorGUILayout.PropertyField(houses, true);

        so.ApplyModifiedProperties();

        if (GUILayout.Button("TÜM SAHNELERDE YERLEŞTİR"))
        {
            ScatterAllScenes();
        }
    }

    void ScatterAllScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            ScatterInScene(scene);

            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("✔ Tüm sahnelerde yerleştirme tamamlandı.");
    }

    void ScatterInScene(Scene scene)
    {
        GameObject mainStructure = GameObject.FindGameObjectWithTag("MainStructure");
        if (!mainStructure)
        {
            Debug.LogWarning(scene.name + " sahnesinde MainStructure bulunamadı.");
            return;
        }

        Vector3 center = mainStructure.transform.position;

        for (int i = 0; i < objectCountPerScene; i++)
        {
            Vector3 randomPos = GetRandomRingPosition(center);

            if (Physics.Raycast(randomPos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f))
            {
                GameObject prefab = GetRandomPrefab();
                if (!prefab) continue;

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.transform.position = hit.point;
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                if (!obj.GetComponent<BoxCollider>())
                {
                    DestroyImmediate(obj);
                }
            }
        }
    }

    Vector3 GetRandomRingPosition(Vector3 center)
    {
        float radius = Random.Range(innerClearRadius, outerRadius);
        float angle = Random.Range(0f, Mathf.PI * 2f);

        return center + new Vector3(
            Mathf.Cos(angle) * radius,
            0,
            Mathf.Sin(angle) * radius
        );
    }

    GameObject GetRandomPrefab()
    {
        if (Random.value > 0.5f && treePrefabs.Length > 0)
            return treePrefabs[Random.Range(0, treePrefabs.Length)];

        if (housePrefabs.Length > 0)
            return housePrefabs[Random.Range(0, housePrefabs.Length)];

        return null;
    }
}
