using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.Text; // UTF-8 için

public class toplama_3d : MonoBehaviour
{
    public float displayDuration = 10f;
    private bool isDisplayingText = false;

    public TextMeshProUGUI Mesaj;
    public TextMeshProUGUI Sikke;
    public int sikke = 0;

    private CoinSpawner[] allSpawners;

    void Start()
    {
        if (Mesaj != null)
        {
            Mesaj.text = "";
            Mesaj.enabled = false;
        }

        if (Sikke != null)
        {
            Sikke.text = "Sikke: " + sikke;
        }

        // Sahnedeki tüm CoinSpawner bileşenlerini bul
        allSpawners = FindObjectsOfType<CoinSpawner>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("toplanacak"))
            return;

        sikke++;
        if (Sikke != null)
            Sikke.text = "Sikke: " + sikke;

        Destroy(collision.gameObject);

        // ——— Buradan sonrası CoinSpawner mantığı ———

        CoinSpawner nearest = GetNearestSpawner();
        if (nearest == null)
        {
            Debug.LogError("Yakında CoinSpawner bulunamadı!");
            return;
        }

        if (string.IsNullOrEmpty(nearest.csvFilePath))
        {
            Debug.LogError("CoinSpawner.csvFilePath boş!");
            return;
        }

        if (!File.Exists(nearest.csvFilePath))
        {
            Debug.LogError("CSV bulunamadı: " + nearest.csvFilePath);
            return;
        }

        List<string> lines = ReadAllLinesUtf8(nearest.csvFilePath);

        // ilk satır başlık, onu atla
        if (lines == null || lines.Count <= 1)
        {
            Debug.LogWarning("CSV dosyasında başlık dışında satır yok: " + nearest.csvFilePath);
            return;
        }

        int randomIndex = Random.Range(1, lines.Count); // 0 = başlık
        string line = lines[randomIndex];

        // virgülle ayrılmış CSV kabulü

        // Mesaj kolonun kaçıncı olduğuna göre burayı değiştir
        string message = line;

        Debug.Log($"Seçilen satır: {randomIndex}, mesaj: {message}");

        StartCoroutine(DisplayText(message));
    }

    private CoinSpawner GetNearestSpawner()
    {
        CoinSpawner nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var spawner in allSpawners)
        {
            if (spawner == null) continue;

            Vector3 pos = spawner.targetPrefab != null
                ? spawner.targetPrefab.transform.position
                : spawner.transform.position;

            float dist = Vector3.Distance(transform.position, pos);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = spawner;
            }
        }

        return nearest;
    }

    private List<string> ReadAllLinesUtf8(string path)
    {
        var result = new List<string>();

        using (var reader = new StreamReader(path, Encoding.UTF8, true))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                    result.Add(line.Trim());
            }
        }

        return result;
    }

    private IEnumerator DisplayText(string message)
    {
        isDisplayingText = true;

        if (Mesaj != null)
        {
            Mesaj.text = message;
            Mesaj.enabled = true;
        }

        yield return new WaitForSeconds(displayDuration);

        if (Mesaj != null)
            Mesaj.enabled = false;

        isDisplayingText = false;
    }
}
