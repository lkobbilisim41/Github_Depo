using UnityEngine;
using TMPro;

public class PlayerNearYapi : MonoBehaviour
{
    public float detectionRadius = 10f;
    public TextMeshProUGUI yapiAdiText;

    private CoinSpawner[] allSpawners;

    void Start()
    {
        allSpawners = FindObjectsOfType<CoinSpawner>();
    }

    void Update()
    {
        string nearestYapi = "";

        foreach (var spawner in allSpawners)
        {
            float dist = Vector3.Distance(transform.position, spawner.targetPrefab.transform.position);

            if (dist <= detectionRadius)
            {
                nearestYapi = spawner.yapi_adi;
                break;
            }
        }

        yapiAdiText.text = nearestYapi;
    }
}
