using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectibleType
{
    public string name;
    public GameObject prefab;

    [Header("Rewards (Birden fazla kaynak/etki)")]
    public List<CollectibleReward> rewards = new List<CollectibleReward>();

    [Header("Geriye dönük uyum (Rewards boşsa kullanılır)")]
    public ResourceType resource;  // eski tek kaynak
    public float amount = 5f;      // eski tek miktar

    public int spawnCount = 10;
}

public class CollectibleManager : MonoBehaviour
{
    public List<CollectibleType> collectibleTypes = new List<CollectibleType>();

    [Header("Yerleşim")]
    public float offsetY = 0.5f;

    [Header("Referanslar")]
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain == null)
        {
            Debug.LogWarning("CollectibleManager: Sahne üzerinde Terrain bulunamadı.");
            return;
        }

        foreach (var type in collectibleTypes)
        {
            if (type == null || type.prefab == null)
            {
                Debug.LogWarning("CollectibleManager: CollectibleType veya prefab null!");
                continue;
            }

            for (int i = 0; i < type.spawnCount; i++)
            {
                float randX = Random.Range(0f, terrain.terrainData.size.x);
                float randZ = Random.Range(0f, terrain.terrainData.size.z);

                float worldX = terrain.GetPosition().x + randX;
                float worldZ = terrain.GetPosition().z + randZ;
                float terrainY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrain.GetPosition().y;

                GameObject obj = Instantiate(
                    type.prefab,
                    new Vector3(worldX, terrainY + offsetY, worldZ),
                    Quaternion.identity
                );

                var item = obj.GetComponent<CollectibleItem>();
                if (item == null)
                    item = obj.AddComponent<CollectibleItem>();

                item.manager = this;

                // Rewards aktar (kopya liste)
                if (type.rewards != null && type.rewards.Count > 0)
                    item.rewards = new List<CollectibleReward>(type.rewards);
                else
                    item.rewards = new List<CollectibleReward>();

                // Eski sistem alanları da dolduralım (rewards boşsa çalışsın)
                item.resourceName = type.resource;
                item.amount = type.amount;
            }
        }
    }

    public void OnCollected(ResourceType resource, float amount)
    {
        if (resource == null) return;

        if (gameManager != null)
            gameManager.Collect(resource, amount);
        else
            Debug.LogWarning("CollectibleManager: GameManager atanmamış!");
    }

    public void OnCollected(List<CollectibleReward> rewards)
    {
        if (rewards == null || rewards.Count == 0) return;

        if (gameManager == null)
        {
            Debug.LogWarning("CollectibleManager: GameManager atanmamış!");
            return;
        }

        foreach (var r in rewards)
        {
            if (r == null || r.resource == null) continue;
            gameManager.Collect(r.resource, r.amount);
        }
    }
}
