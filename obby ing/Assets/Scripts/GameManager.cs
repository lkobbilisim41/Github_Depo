using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject obje;

    public static GameManager instance;

    public Vector3 playerPosition; // Oyuncu pozisyonunu saklamak için
    public float x, y, z;
    
	public TextMeshProUGUI meyve_say;
    public TextMeshProUGUI dogru_Say;
    public TextMeshProUGUI yanlis_Say;

    public PlayerData playerData; // Unity Editor'da atayabileceğiniz ScriptableObject
	
    void Start(){
		
       meyve_say.text = "Skor : " + playerData.skor;
       dogru_Say.text = "Doğru : " + playerData.dogru_sayisi;
       yanlis_Say.text = "Yanlış : " + playerData.yanlis_sayisi;

	   if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
		
		
    }

    public void LoadGameScene()
    {
        if (instance != null)
        {
            SceneManager.LoadScene("game");
        }
    }

    public void LoadQuizScene()
    {
        if (instance != null)
        {
            SceneManager.LoadScene("quiz");
        }
    }
    public void Game_Finish()
    {
		Debug.Log("Finish");
        if (instance != null)
        {
            SceneManager.LoadScene("gameover");
        }
    }	
}
