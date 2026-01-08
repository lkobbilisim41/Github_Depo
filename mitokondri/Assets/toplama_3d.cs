using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class toplama_3d : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Glikoz_Say;
    [SerializeField] private TextMeshProUGUI Oksijen_Say;
    [SerializeField] private TextMeshProUGUI Enerji_Say;
    [SerializeField] public TextMeshProUGUI Mesaj;

    int seker;
    int oksijen;
    int enerji;

    public GameObject prefabToSpawn;
    public GameObject su_buhari;
    public GameObject karbondioksit;
    public GameObject elektrik;

    public float displayDuration = 3.0f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("seker"))
        {
            seker++;
            Glikoz_Say.text = seker.ToString();
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("oksijen"))
        {
            oksijen++;
            Oksijen_Say.text = oksijen.ToString();
            Destroy(collision.gameObject);
        }

        if (seker >= 1 && oksijen >= 6)
        {
            enerji += 36;
            Enerji_Say.text = enerji.ToString();

            // Mesajı göster ve 5 saniye sonra gizle
            ShowMessage("6 oksijen+1 glikoz=>6 co2+ 6 h2o + 36 atp", 5.0f);

            seker -= 1;
            oksijen -= 6;

            StartCoroutine(SpawnAndDestroyPrefab());

            Vector3 offset = new Vector3(0, 1, 0);

            GameObject su_buhari_nesne = Instantiate(su_buhari, transform.position + offset, Quaternion.identity);
            Destroy(su_buhari_nesne, 3.0f);

            GameObject karbondioksit_nesne = Instantiate(karbondioksit, transform.position + offset, Quaternion.identity);
            Destroy(karbondioksit_nesne, 3.0f);

            GameObject elektrik_prefab = Instantiate(elektrik, transform.position + offset, Quaternion.identity);
            Destroy(elektrik_prefab, 3.0f);
        }
    }

    IEnumerator SpawnAndDestroyPrefab()
    {
        GameObject spawnedPrefab = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(displayDuration);
        Destroy(spawnedPrefab);
    }

    // Mesajı göster ve belirli bir süre sonra gizle
    private void ShowMessage(string message, float duration)
    {
        Mesaj.text = message; // Mesajı ayarla
        Mesaj.gameObject.SetActive(true); // Mesajı görünür yap
        StartCoroutine(HideMessageAfterTime(duration)); // Süre dolunca mesajı gizle
    }

    private IEnumerator HideMessageAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration); // Belirtilen süre kadar bekle
        Mesaj.gameObject.SetActive(false); // Mesajı gizle
    }
}
