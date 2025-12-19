using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildMenuUI : MonoBehaviour
{
    public GameManager gameManager;
    public BuildManager buildManager;

    [Header("UI")]
    public Transform buttonContainer;  // VerticalLayoutGroup içeren GameObject
    public GameObject buttonPrefab;    // Ýçinde Button + TMP Text olan prefab

    void Start()
    {
        CreateButtons();
    }

    void CreateButtons()
    {
        foreach (var building in gameManager.buildingTypes)
        {
            BuildingType b = building; // closure fix

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);

            string label = b.name;

            // Eðer toplanabilir ise, toplandýðýnda ne kazandýrdýðýný yaz
            if (b.isCollectible && b.collectResource != null && b.collectAmount != 0)
            {
                label += $" (+{b.collectAmount} {b.collectResource.resourceName} toplanýnca)";
            }
            // Deðilse, inþa maliyet/etkilerini özetle
            else if (b.effects != null && b.effects.Length > 0)
            {
                List<string> parts = new List<string>();
                foreach (var eff in b.effects)
                {
                    if (eff.resource == null || eff.changeAmount == 0) continue;
                    string sign = eff.changeAmount > 0 ? "+" : "";
                    parts.Add($"{eff.resource.resourceName} {sign}{eff.changeAmount}");
                }

                if (parts.Count > 0)
                    label += " (" + string.Join(", ", parts) + ")";
            }

            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = label;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => buildManager.Build(b));
        }
    }
}
