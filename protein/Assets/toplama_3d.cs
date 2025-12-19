using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class toplama_3d : MonoBehaviour
{
    public float displayDuration = 8f;
    public int toplanan_protein = 0;
    public TextMeshProUGUI Mesaj;
    public TextMeshProUGUI Protein;
    public GameObject amino_prefab; //  amino prefab

    void Start()
    {
        Mesaj.text = "";
        Protein.text = "Protein: "+toplanan_protein;
        Mesaj.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "toplanacak") return;

        Destroy(collision.gameObject);

        // 2) Protein sayısını arttıralım
        toplanan_protein++;
        Protein.text = "Protein: " + toplanan_protein;

		if (amino_prefab != null)
		{
			for (int i = 0; i < 5; i++)
			{
				Instantiate(
					amino_prefab,
					transform.position + Vector3.up,
					Quaternion.identity
				);
			}
		}

        // Eğer başka bir mesaj gösterilecekse
        // List<string> lines = new List<string>(File.ReadAllLines(nearest.csvFilePath));
        // if (lines.Count == 0) return;
        // string randomLine = lines[Random.Range(0, lines.Count)];
        // StartCoroutine(DisplayText(randomLine));
    }

    private IEnumerator DisplayText(string message)
    {
        Mesaj.text = message;
        Mesaj.enabled = true;
        yield return new WaitForSeconds(displayDuration);
        Mesaj.enabled = false;
    }
}
