using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{

    void Start()
    {
        // Bu GameObject'in altındaki tüm Button bileşenlerini bul
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            // Her buton için tıklama olayını ekle
            button.onClick.AddListener(() => LoadSceneByIndex(button.name));
        }
    }
	
    void LoadSceneByIndex(string buttonName)
    {
        // Buton adını tamsayıya dönüştür
        if (int.TryParse(buttonName, out int sceneIndex))
        {
            // Sahne indexini kontrol edin ve sahneyi yükleyin
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("Loading scene index: " + sceneIndex); // Debugging için
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError("Geçersiz sahne indexi: " + sceneIndex);
            }
        }
        else
        {
            Debug.LogError("Buton adı geçerli bir tamsayı değil: " + buttonName);
        }
    }
	
    public void Bitis()
    {
		Application.Quit();
    }	

}

