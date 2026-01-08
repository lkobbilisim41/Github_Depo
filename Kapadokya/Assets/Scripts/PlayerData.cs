using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public Vector3 playerPosition= new Vector3(-4,4,0);
    public int meyveSayisi;
    public int dogru_sayisi;
    public int yanlis_sayisi;
    public string level;
    public bool is_saved=false;
    public List<string> toplanan_nesne = new List<string>();
    public List<int> cevaplanan_soru = new List<int>();
    // Diğer gerekli verileri buraya ekleyebilirsiniz.
}
