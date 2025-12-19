using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public Vector3 playerPosition = new Vector3(-4,4,0);
    public int skor;
    public int rand1;
    public int dogru_sayisi;
    public int yanlis_sayisi;
    public string level;
    public bool is_saved;
    public List<string> toplanan_nesne = new List<string>();
    public List<int> cevaplanan_soru = new List<int>();
    // Diğer gerekli verileri buraya ekleyebilirsiniz.
    public void ResetData()
    {
    playerPosition = new Vector3(-4, 4, 0);
    skor=0;
    rand1 = Random.Range(1, 6); 
    dogru_sayisi=0;
    yanlis_sayisi=0;
    level="";
    toplanan_nesne.Clear();
    cevaplanan_soru.Clear();
    }
}
