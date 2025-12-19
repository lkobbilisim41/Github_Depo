using UnityEngine;

public class DusmanYenidenUretici : MonoBehaviour
{
    public Transform yenidenUretmeNoktasi; // Düşmanların yeniden üretileceği nokta
    public float yenidenUretmeAraligi = 5f; // Düşmanların yeniden üretme aralığı (saniye)

    void Start()
    {
        InvokeRepeating("DusmanlariYenidenUret", yenidenUretmeAraligi, yenidenUretmeAraligi);
    }

    void DusmanlariYenidenUret()
    {
        GameObject[] dusmanlar = GameObject.FindGameObjectsWithTag("Dusman");

        foreach (GameObject dusman in dusmanlar)
        {
            dusman.transform.position = yenidenUretmeNoktasi.position;
            dusman.transform.rotation = Quaternion.identity;
        }
    }
}
