using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class toplama_3d : MonoBehaviour
{
    public float displayDuration = 8f;
    public int toplanan_sikke = 0;
    public TextMeshProUGUI Mesaj;
    public TextMeshProUGUI Sikke;

    private CoinSpawner[] allSpawners;

    void Start()
    {
        Mesaj.text = "";
        Sikke.text = "Sikke: "+toplanan_sikke;
        Mesaj.enabled = false;

        allSpawners = FindObjectsOfType<CoinSpawner>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "toplanacak") return;

        // 1) Coini yok edelim
        Destroy(collision.gameObject);

        toplanan_sikke++;
        Sikke.text = "Sikke: "+toplanan_sikke;

        // 2) Yakındaki spawner’ı bulalım
        CoinSpawner nearest = GetNearestSpawner();

        if (nearest == null)
        {
            Debug.LogError("Yakında bir yapı bulunamadı!");
            return;
        }

        // 3) CSV dosyasını okuyalım
        if (!File.Exists(nearest.csvFilePath))
        {
            Debug.LogError("CSV bulunamadı: " + nearest.csvFilePath);
            return;
        }

        List<string> lines = new List<string>(File.ReadAllLines(nearest.csvFilePath));

        if (lines.Count == 0) return;

        string randomLine = lines[Random.Range(0, lines.Count)];

        StartCoroutine(DisplayText(randomLine));
    }

    private CoinSpawner GetNearestSpawner()
    {
        CoinSpawner nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var spawner in allSpawners)
        {
            float dist = Vector3.Distance(transform.position, spawner.targetPrefab.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = spawner;
            }
        }

        return nearest;
    }

    private IEnumerator DisplayText(string message)
    {
        Mesaj.text = message;
        Mesaj.enabled = true;
        yield return new WaitForSeconds(displayDuration);
        Mesaj.enabled = false;
    }
}
