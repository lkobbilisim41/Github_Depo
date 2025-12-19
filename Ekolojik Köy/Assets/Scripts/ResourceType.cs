using UnityEngine;

[CreateAssetMenu(menuName = "Game/Resource Type")]
public class ResourceType : ScriptableObject
{
    [Header("Görsel ve Ýsim")]
    public string resourceName;   // Örn: "Wood", "Stone", "Energy"
    public Sprite icon;

    [Header("Kýsýtlar")]
    public bool canBeNegative = false; // Örn: Pollution negatif olabilir mi?
}
