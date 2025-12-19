using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class toplama_3d : MonoBehaviour
{
    public PlayerData playerData;
    public TMP_Text scoreText;

    private bool isLoadingQuiz = false;
    private CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (playerData != null && playerData.is_saved)
        {
            // CharacterController açıkken position set bazen sapıtlar → geçici kapat
            cc.enabled = false;
            transform.position = playerData.playerPosition;   // ✅ +20 YOK
            cc.enabled = true;

            playerData.is_saved = false;

            // Daha önce toplananları sahnede kapat
            foreach (string nesneAdi in playerData.toplanan_nesne)
            {
                GameObject nesne = GameObject.Find(nesneAdi);
                if (nesne != null) nesne.SetActive(false);
            }
        }
        else
        {
            GameObject startPlane = GameObject.Find("baslangic");
            if (startPlane != null)
            {
                cc.enabled = false;
                transform.position = startPlane.transform.position; // ✅ +20 YOK
                cc.enabled = true;
            }
        }
    }

    void Start()
    {
        RefreshUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isLoadingQuiz) return;
        if (!collision.gameObject.CompareTag("toplanacak")) return;

        // ✅ Toplama her zaman
        playerData.skor++;
        playerData.toplanan_nesne.Add(collision.gameObject.name);

        // ✅ Quizden dönüş için "tam toplanma anındaki" konumu kaydet
        playerData.playerPosition = transform.position;
        playerData.level = SceneManager.GetActiveScene().name;

        Destroy(collision.gameObject);

        if (playerData.rand1 > 0) playerData.rand1--;

        if (playerData.rand1 <= 0)
        {
            playerData.is_saved = true;               // ✅ dönüşte pozisyonu uygulasın
            playerData.rand1 = Random.Range(1, 6);    // yeni hedef
            isLoadingQuiz = true;
            SceneManager.LoadScene("quiz");
            return;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreText != null && playerData != null)
            scoreText.text = "Skor: " + playerData.skor + "\nSonraki Soru: " + playerData.rand1;
    }

}
