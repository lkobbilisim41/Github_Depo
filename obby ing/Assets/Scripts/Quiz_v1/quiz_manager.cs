using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class quiz_manager : MonoBehaviour
{
    public List<soru_cevap> S_C;
    public GameObject[] options;
    public int current_Question;
    public int yanlis;
    public int toplam_soru;

    public GameObject QuizPanel;
    public GameObject GoPanel;
    public Text QuestionText;
    public Text Dogru_Say;
    public Text Yanlis_Say;

    public PlayerData playerData; // 🔹 geri dönmek için
    public CSVQuestionLoader csvLoader;

    void Start()
    {
        GoPanel.SetActive(false);
        QuizPanel.SetActive(true);

        if (csvLoader != null)
            S_C = csvLoader.LoadQuestions();

        toplam_soru = S_C.Count;
        soru_uret();
    }


    void Update()
    {
        Dogru_Say.text = "Doğru : " + playerData.dogru_sayisi;
        Yanlis_Say.text = "Yanlış : " + playerData.yanlis_sayisi;
    }
    void soru_uret()
    {
        if (S_C.Count > 0)
        {
            current_Question = Random.Range(0, S_C.Count);
            QuestionText.text = S_C[current_Question].Question;
            SetAnswers();
        }
        else
        {
            GameOver();
        }
    }

    public void DogruCevap(Button btn)
    {
        playerData.dogru_sayisi++;
        btn.image.color = Color.green;

        Dogru_Say.text = "Doğru : " + playerData.dogru_sayisi;
        Yanlis_Say.text = "Yanlış : " + playerData.yanlis_sayisi;

        StartCoroutine(DogruSonrasiDon());
    }

    public void YanlisCevap(Button btn)
    {
        playerData.yanlis_sayisi++;
        btn.image.color = Color.red;

        Dogru_Say.text = "Doğru : " + playerData.dogru_sayisi;
        Yanlis_Say.text = "Yanlış : " + playerData.yanlis_sayisi;

        // ❗ yanlışta sahne değişmez
    }

    IEnumerator DogruSonrasiDon()
    {
        yield return new WaitForSeconds(2f);

        if (playerData != null && !string.IsNullOrEmpty(playerData.level))
            SceneManager.LoadScene(playerData.level);
        else
            SceneManager.LoadScene(0);
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            cevap c = options[i].GetComponent<cevap>();
            c.quizManager = this;
            c.isCorrect = false;

            options[i].GetComponent<Image>().color = Color.white;
            options[i].transform.GetChild(0).GetComponent<Text>().text =
                S_C[current_Question].Answers[i];

            if (S_C[current_Question].CorrectAnswer == i + 1)
                c.isCorrect = true;
        }
    }

    public void GameOver()
    {
        GoPanel.SetActive(true);
        QuizPanel.SetActive(false);
    }
}
