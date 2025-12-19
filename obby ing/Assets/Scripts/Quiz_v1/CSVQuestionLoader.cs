using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVQuestionLoader : MonoBehaviour
{
    public string fileName = "sorular.csv";

    public List<soru_cevap> LoadQuestions()
    {
        List<soru_cevap> list = new List<soru_cevap>();

        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("CSV bulunamadý: " + path);
            return list;
        }

        string[] lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++) // ilk satýr baþlýk
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 6) continue;

            soru_cevap sc = new soru_cevap();
            sc.Question = parts[0];
            sc.Answers[0] = parts[1];
            sc.Answers[1] = parts[2];
            sc.Answers[2] = parts[3];
            sc.Answers[3] = parts[4];
            sc.CorrectAnswer = int.Parse(parts[5]);

            list.Add(sc);
        }

        return list;
    }
}
