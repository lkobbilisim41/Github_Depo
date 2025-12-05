using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    // Çağırmak için: SceneReloader.Instance.Reload(); ya da bir butona bağlayın
    public void Reload()
    {
        // Eğer oyunu pause'ladıysanız timeScale'i sıfırlayın
        Time.timeScale = 1f;
        // Aynı sahneyi ismiyle yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // veya build index ile:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
