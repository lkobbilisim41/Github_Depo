using UnityEngine;
using UnityEngine.EventSystems;

public class SceneReloader : MonoBehaviour
{
    public void Reload()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        GameObject baslangic = GameObject.FindGameObjectWithTag("baslangic");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (baslangic == null || player == null)
        {
            Debug.LogWarning("baslangic veya Player bulunamadı!");
            return;
        }

        CharacterController cc = player.GetComponent<CharacterController>();

        Vector3 startPos = baslangic.transform.position; // +20 YOK

        // 🔴 EN KRİTİK KISIM
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = startPos+ Vector3.up * 20f; ;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = startPos+ Vector3.up * 20f; ;
        }
    }
}
