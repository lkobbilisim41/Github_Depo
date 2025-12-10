using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public GameObject targetPrefab;

    public string yapi_adi = "";
    public string csvFilePath = "Assets/StreamingAssets/";

    public int numberOfCoins = 20;
    public float minDistance = 1f;      // Minimum uzaklık (binanın dışı)
    public float maxDistance = 10f;      // Maksimum uzaklık (çevre yayılımı)

    void Start()
    {
        SpawnCoinsAroundTarget();
    }

    void SpawnCoinsAroundTarget()
    {
        if (coinPrefab == null || targetPrefab == null)
        {
            Debug.LogError("Coin prefab veya target prefab eksik!");
            return;
        }
        
		// BİNANIN GERÇEK SINIRLARINI AL
        Renderer rend = targetPrefab.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Target prefab içinde Renderer bulunamadı!");
            return;
        }
		
        Bounds b = rend.bounds;
        Vector3 targetPosition = b.center;

        // Binanın yarıçapı → en geniş ekseni alıyoruz
        float binaYaricap = Mathf.Max(b.extents.x, b.extents.z); 


        for (int i = 0; i < numberOfCoins; i++)
        {
            float angle = Random.Range(0f, 360f);

            //COIN HER ZAMAN BİNANIN DIŞINDA
            float distance = binaYaricap + Random.Range(minDistance, maxDistance);

            float x = targetPosition.x + Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
            float z = targetPosition.z + Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

            float y = Terrain.activeTerrain != null
                ? Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z))
                : targetPosition.y;

            Vector3 spawnPosition = new Vector3(x, y+1, z);

            GameObject newCoin = Instantiate(coinPrefab, spawnPosition, coinPrefab.transform.rotation);

            var info = newCoin.AddComponent<CoinInfo>();
            info.yapiAdi = yapi_adi;
            info.csvFilePath = csvFilePath;
        }
    }
}
