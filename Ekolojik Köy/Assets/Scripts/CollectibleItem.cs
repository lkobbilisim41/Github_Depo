using System.Collections.Generic;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public CollectibleManager manager;

    [Header("Eski sistem (Rewards boşsa kullanılır)")]
    public ResourceType resourceName;
    public float amount = 5f;

    [Header("Yeni sistem (birden fazla kaynak/etki)")]
    public List<CollectibleReward> rewards = new List<CollectibleReward>();

    public float collectDistance = 3f;

    void Start()
    {
        // Collider yoksa ekle
        if (GetComponent<Collider>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }

        // Tag yoksa Toplanacak yap
        if (CompareTag("Untagged"))
            gameObject.tag = "Toplanacak";
    }

    void Update()
    {
        if (manager == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.gameObject != gameObject) return;
                if (!CompareTag("Toplanacak")) return;

                float dist = Vector3.Distance(cam.transform.position, transform.position);
                if (dist > collectDistance) return;

                Debug.Log($"[CollectibleItem] rewards.Count = {(rewards == null ? -1 : rewards.Count)}");

                // 1) Yeni sistem
                if (rewards != null && rewards.Count > 0)
                {
                    manager.OnCollected(rewards);
                }
                else
                {
                    // 2) Eski sistem
                    if (resourceName == null)
                        Debug.LogWarning($"{name}: resourceName boş ve rewards da boş. Hiçbir kaynak artmayacak.");
                    else
                        manager.OnCollected(resourceName, amount);
                }

                Destroy(gameObject);
            }
        }
    }
}
