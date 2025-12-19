using UnityEngine;

[System.Serializable]
public class BuildingType
{
    public string name;             // Örn: "Ev", "Kömür Santrali", "Ağaç"
    public GameObject prefab;       // Sahneye yerleştirilecek nesne
    public bool isCollectible;      // Toplanabilir mi?

    [Header("Maliyet veya Üretim (inşa anında)")]
    public ResourceEffect[] effects; // İnşa anında uygulanacak değişimler

    [Header("Toplanabilir Ayarları")]
    [Tooltip("Bu nesne toplandığında hangi kaynağı verecek?")]
    public ResourceType collectResource;

    [Tooltip("Bu nesne toplandığında verilecek miktar.")]
    public float collectAmount = 0f;
}

[System.Serializable]
public class ResourceEffect
{
    public ResourceType resource;   // Örn: Wood, Stone, Energy
    public float changeAmount;      // + veya - etki miktarı
}
