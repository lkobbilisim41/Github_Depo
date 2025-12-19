using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class panel_hazirla : MonoBehaviour
{
    void Start()
    {
        // Bu GameObject'in altındaki tüm Button bileşenlerini bul
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            // Button'un içindeki TextMeshProUGUI bileşenini al
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            // Eğer TextMeshProUGUI bileşeni varsa, Button adını Text'e yaz
            if (buttonText != null)
            {
                buttonText.text = button.name;
            }
        }
    }
}
