using UnityEngine;

public class Cannon : MonoBehaviour
{
    public float force = 10f; // Küreye uygulanacak kuvvet
    public GameObject spherePrefab; // Yollanacak küre prefabı
    public Transform launchPoint; // Yollama noktası
    public float maxDistance = 100f; // Raycast'in maksimum mesafesi

    public int scorePerHit = 10; // Her vuruşta kazanılacak puan miktarı
    private int score = 0; // Toplam skor

    // Skorun güncellendiği yer
    void UpdateScore()
    {
        Debug.Log("Score: " + score);
        // Burada skorun gösterilmesi veya başka bir işlem yapılabilir
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol tık kontrolü
        {
            // Raycast'i oluştur
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                // Raycast'in ulaştığı noktaya doğru bir vektör hesapla
                Vector3 direction = (hit.point - launchPoint.position).normalized;

                // Küreyi yolla
                LaunchSphere(direction);
            }
        }
    }

    void LaunchSphere(Vector3 direction)
    {
        // Küreyi yolla
        GameObject sphere = Instantiate(spherePrefab, launchPoint.position, Quaternion.identity);
        Rigidbody rb = sphere.GetComponent<Rigidbody>();

        // Küreye kuvvet uygula
        rb.AddForce(direction * force, ForceMode.Impulse);

    }

    // Hedefi vurduğunda çağrılan fonksiyon
    public void HitTarget()
    {
        score += scorePerHit; // Skoru arttır
        UpdateScore(); // Skoru güncelle
    }

    // Küre hedefe çarptığında çağrılan fonksiyon
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player" || collision.gameObject.tag != "zemin")
        {
            Debug.Log(collision.gameObject.tag);
            if (collision.gameObject.CompareTag("hedef"))
            {
                // Eğer çarpılan obje "Target" etiketine sahipse

                Destroy(collision.gameObject);
                // Hedefi vurulduğunda puan kazanmak için HitTarget fonksiyonunu çağır
            }
        }
    }
}
