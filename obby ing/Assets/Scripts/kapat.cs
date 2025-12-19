using UnityEngine;
using UnityEngine.SceneManagement;

public class kapat : MonoBehaviour
{
    public PlayerData playerData;

    public void Kapat()
    {
        // Burada hangi alanlarýn sýfýrlanacaðýný sen belirle:
        playerData.skor = 0;
        playerData.rand1 = 0;
        playerData.toplanan_nesne.Clear();
        playerData.is_saved = false;
        playerData.level = "";
        playerData.playerPosition = Vector3.zero;

        Application.Quit();
    }
}
