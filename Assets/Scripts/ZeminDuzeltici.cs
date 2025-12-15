using System.Collections.Generic;
using UnityEngine;

// Editörde de çalýþabilsin diye:
[ExecuteInEditMode]
public class ZeminDuzeltici : MonoBehaviour
{
    [Header("Terrain Ayarlar")]
    public Terrain terrain;               // Sahnedeki Terrain'i buraya sürükle
    public float extraMargin = 1f;        // Binanýn etrafýnda ne kadar fazladan alan düzleþsin

    [Header("Bina Ayarlar")]
    public List<Transform> hedefBinalar;  // Zemini düzleþtirilecek bina instance'larý
    public float yOffset = 0f;            // Pivot tabanda deðilse biraz yukarý almak için

    [Header("Ã‡alÄ±ÅŸ")]
    public bool CalistirBirKez = false;   // Inspector'dan iþaretleyip bir kere çalýþtýr

    void Update()
    {
        // Inspector'da tik koyduðunda bir kere çalýþtýrýyoruz
        if (CalistirBirKez)
        {
            CalistirBirKez = false;  // Hemen geri al ki tekrar tekrar çalýþmasýn
            DuzeltVeYerles();
        }
    }

    public void DuzeltVeYerles()
    {
        if (terrain == null)
        {
            Debug.LogError("[ZeminDuzeltici] Terrain atanmadý.");
            return;
        }

        if (hedefBinalar == null || hedefBinalar.Count == 0)
        {
            Debug.LogWarning("[ZeminDuzeltici] Hedef bina listesi boþ.");
            return;
        }

        Debug.Log("[ZeminDuzeltici] Ýþlem baþladý. Bina sayýsý: " + hedefBinalar.Count);

        foreach (Transform bina in hedefBinalar)
        {
            if (bina == null)
            {
                Debug.LogWarning("[ZeminDuzeltici] Listede null bina var, atlýyorum.");
                continue;
            }

            Debug.Log("[ZeminDuzeltici] Bina iþleniyor: " + bina.name);

            bool flattenOk = FlattenUnderBuilding(bina);
            if (!flattenOk)
            {
                Debug.LogWarning("[ZeminDuzeltici] Flatten baþarýsýz veya atlandý: " + bina.name);
                continue;
            }

            SnapBuildingToTerrain(bina);

            Debug.Log("[ZeminDuzeltici] Bina tamam: " + bina.name);
        }

        Debug.Log("[ZeminDuzeltici] Tüm iþlemler bitti.");
    }

    private bool FlattenUnderBuilding(Transform bina)
    {
        TerrainData tData = terrain.terrainData;

        Renderer rend = bina.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("[ZeminDuzeltici] Renderer bulunamadý, bina: " + bina.name);
            return false;
        }

        Bounds bounds = rend.bounds;
        bounds.Expand(extraMargin);

        Vector3 terrainPos = terrain.transform.position;
        Vector3 tSize = tData.size;

        // World -> normalize (0–1) terrain koordinatlarý
        float xMinN = (bounds.min.x - terrainPos.x) / tSize.x;
        float xMaxN = (bounds.max.x - terrainPos.x) / tSize.x;
        float zMinN = (bounds.min.z - terrainPos.z) / tSize.z;
        float zMaxN = (bounds.max.z - terrainPos.z) / tSize.z;

        // Sýnýr içinde mi?
        if (xMaxN < 0f || xMinN > 1f || zMaxN < 0f || zMinN > 1f)
        {
            Debug.LogWarning("[ZeminDuzeltici] Bina terrain sýnýrlarý dýþýnda görünüyor: " + bina.name);
            return false;
        }

        xMinN = Mathf.Clamp01(xMinN);
        xMaxN = Mathf.Clamp01(xMaxN);
        zMinN = Mathf.Clamp01(zMinN);
        zMaxN = Mathf.Clamp01(zMaxN);

        int res = tData.heightmapResolution - 1;

        int xMin = Mathf.RoundToInt(xMinN * res);
        int xMax = Mathf.RoundToInt(xMaxN * res);
        int zMin = Mathf.RoundToInt(zMinN * res);
        int zMax = Mathf.RoundToInt(zMaxN * res);

        if (xMax < xMin || zMax < zMin)
        {
            Debug.LogWarning("[ZeminDuzeltici] Geçersiz heightmap aralýðý, bina: " + bina.name);
            return false;
        }

        int width = xMax - xMin + 1;
        int height = zMax - zMin + 1;

        Debug.Log("[ZeminDuzeltici] " + bina.name +
                  " için height alaný: x(" + xMin + "-" + xMax +
                  "), z(" + zMin + "-" + zMax +
                  "), size: " + width + "x" + height);

        float[,] heights = tData.GetHeights(xMin, zMin, width, height);

        float sum = 0f;
        foreach (float h in heights) sum += h;
        float avgHeight = sum / (width * height);

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                heights[z, x] = avgHeight;
            }
        }

        tData.SetHeights(xMin, zMin, heights);

        return true;
    }

    private void SnapBuildingToTerrain(Transform bina)
    {
        Vector3 pos = bina.position;
        float terrainY = terrain.SampleHeight(pos) + terrain.transform.position.y;

        float eskiY = pos.y;
        pos.y = terrainY + yOffset;
        bina.position = pos;

        Debug.Log("[ZeminDuzeltici] " + bina.name +
                  " Y pozisyonu: " + eskiY.ToString("F2") + " -> " + pos.y.ToString("F2"));
    }
}
