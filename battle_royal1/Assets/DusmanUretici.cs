using UnityEngine;

public class DusmanUretici : MonoBehaviour
{
    public GameObject dusmanPrefab; // Yeni düşman nesnesi prefabı
    public float uretmeAraligi = 5f; // Yeni düşmanın üretilme aralığı (saniye)
    public float uretmeSuresi = 9999999999999999f; // Yeni düşmanın üretilme süresi (saniye)

    void Start()
    {
        InvokeRepeating("DusmanUret", uretmeAraligi, uretmeAraligi);
    }

    void DusmanUret()
    {
        Instantiate(dusmanPrefab, transform.position, Quaternion.Euler(270, 180, 0));
        Invoke("DusmanYokEt", uretmeSuresi);
    }

    void DusmanYokEt()
    {
        // Düşmanı yok etme veya başka bir işlem yapma
    }
}
