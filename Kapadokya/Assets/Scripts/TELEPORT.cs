using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform targetLocation;  // Işınlanmak istediğiniz hedefin konumu
    public KeyCode teleportKey = KeyCode.T;  // Işınlanma için kullanılacak tuş (T tuşu)

    void Update()
    {
        // Eğer ışınlanma tuşuna basılmışsa
        if (Input.GetKeyDown(teleportKey))
        {
            TeleportToLocation();
        }
    }

    void TeleportToLocation()
    {
        // Işınlanan objeyi hedef konuma taşıyoruz
        transform.position = targetLocation.position;
        
        // Işınlanma sonrası hareket devam etsin diye herhangi bir hız sıfırlama işlemi yapılmaz.
    }
}
