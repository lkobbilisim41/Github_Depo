using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kaynaklar : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        
    }
	public GameObject[] resources; // Kaynak nesnelerini buraya atayın
    public int resourceCount = 50; // Kaç tane kaynak spawn edilecek
    public float spawnRange = 50f; // Spawn alanının yarıçapı (merkez nokta etrafında)

    void Start()
    {
        SpawnResources();
    }

    void SpawnResources()
    {
        for (int i = 0; i < resourceCount; i++)
        {
            // Rastgele pozisyon belirleme
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnRange, spawnRange),
                8,
                Random.Range(-spawnRange, spawnRange)
            );

            // Terrain yüksekliğini al
            spawnPosition.y = Terrain.activeTerrain.SampleHeight(spawnPosition);

            // Rastgele kaynak türü seç
            GameObject resource = resources[Random.Range(0, resources.Length)];

            // Kaynağı belirtilen konumda oluştur
            Instantiate(resource, spawnPosition, Quaternion.identity);
        }
    }	
	
	
	
	
}
