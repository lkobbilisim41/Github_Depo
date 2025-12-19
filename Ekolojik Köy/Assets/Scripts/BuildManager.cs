using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public GameManager gameManager;
    public float buildDistance = 3f;

    [Header("Toplanabilir Nesne Yönetimi")]
    public CollectibleManager collectibleManager;   // Inspector'dan atayabilirsin

    void Awake()
    {
        // Inspector'dan vermezsen sahneden otomatik bulsun
        if (collectibleManager == null)
        {
            collectibleManager = FindObjectOfType<CollectibleManager>();
        }
    }

    public void Build(BuildingType building)
    {
        if (!gameManager.HasEnoughResources(building.effects))
        {
            Debug.Log("Kaynak yetersiz!");
            return;
        }

        Vector3 buildPos = Camera.main.transform.position + Camera.main.transform.forward * buildDistance;

        if (Physics.Raycast(buildPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 50f))
            buildPos.y = hit.point.y;

        GameObject obj = Instantiate(building.prefab, buildPos, Quaternion.identity);

        // İnşa anındaki maliyet / ödül etkileri
        gameManager.ApplyEffects(building.effects);

        // Eğer bu bina aynı zamanda toplanabilir ise:
        if (building.isCollectible)
        {
            // CollectibleItem bileşenini ekle / bul
            var item = obj.GetComponent<CollectibleItem>();
            if (item == null)
                item = obj.AddComponent<CollectibleItem>();

            // CollectibleManager referansı
            if (collectibleManager == null)
                collectibleManager = FindObjectOfType<CollectibleManager>();

            item.manager = collectibleManager;
            item.resourceName = building.collectResource;
            item.amount = building.collectAmount;

            // Güvenlik: Tag ve collider normal prefab’ta yoksa CollectibleItem.Start zaten hallediyor
        }

        Debug.Log($"{building.name} inşa edildi!");
    }
}
