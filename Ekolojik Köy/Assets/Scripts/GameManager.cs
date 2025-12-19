using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	[Header("OYUN MESAJLARI")]
	public TMPro.TextMeshProUGUI gameMessageText;
	public float messageDuration = 3f;

	private Coroutine messageRoutine;

	[Header("KAYBETME KOŞULLARI")]
	public ResourceType foodType;         // Inspector: Food ResourceType
	public ResourceType pollutionType;    // Inspector: Pollution ResourceType

	public float loseFoodAt = 0f;         // Food <= 0 -> kayıp
	public float losePollutionAt = 50f;   // Pollution >= 50 -> kayıp

	[Header("ZAMANLA TÜKETİM")]
	public bool drainFoodOverTime = true;
	public float foodDrainPerSecond = 1f; // saniyede kaç azalacak (ör: 0.02 => 50 saniyede 1 azalır)

	private bool gameOver = false;

    [Header("KAYNAKLAR")]
    public List<ResourceEntry> resources = new List<ResourceEntry>();

    [Header("BİNA VE NESNELER")]
    public List<BuildingType> buildingTypes = new List<BuildingType>();

    [Header("UI")]
    public TextMeshProUGUI resourceText;

    void Start()
    {
		UpdateResourceUI();
		CheckLoseConditions(); // başlangıç değerleri zaten riskliyse anında yakalasın
    }

	void Update()
	{
		if (gameOver) return;

		if (drainFoodOverTime && foodType != null && foodDrainPerSecond > 0f)
		{
			ChangeResource(foodType, -foodDrainPerSecond * Time.deltaTime);
		}

		CheckLoseConditions();
	}
    // Belirli bir kaynağın kaydını bul
    public ResourceEntry GetEntry(ResourceType type)
    {
        if (type == null) return null;
        return resources.Find(r => r != null && r.type == type);
    }

    public void ChangeResource(ResourceType type, float delta)
    {
        if (type == null)
        {
            Debug.LogWarning("ChangeResource: ResourceType null!");
            return;
        }

        var entry = GetEntry(type);
        if (entry == null)
        {
            Debug.LogWarning($"ChangeResource: {type.resourceName} için ResourceEntry bulunamadı!");
            return;
        }

        entry.amount += delta;

        if (!type.canBeNegative && entry.amount < 0)
            entry.amount = 0;

        UpdateResourceUI();
    }

    public bool HasEnoughResources(ResourceEffect[] effects)
    {
        if (effects == null) return true;

        foreach (var eff in effects)
        {
            if (eff == null || eff.resource == null) continue;

            var entry = GetEntry(eff.resource);
            if (entry == null)
            {
                // Bu kaynağı hiç tanımlamamışsak, yeterli değil say
                return false;
            }

            float newAmount = entry.amount + eff.changeAmount;

            // changeAmount genelde negatif (maliyet). Yeni miktar 0'ın altına düşüyorsa ve negatif izinli değilse: yetersiz.
			if (!eff.resource.canBeNegative && newAmount < 0)
			{
				ShowMessage("Kaynak yetersiz!");
				return false;
			}
        }

        return true;
    }

    public void ApplyEffects(ResourceEffect[] effects)
    {
        if (effects == null) return;

        foreach (var eff in effects)
        {
            if (eff == null || eff.resource == null) continue;
            ChangeResource(eff.resource, eff.changeAmount);
        }
    }

    public void Collect(ResourceType resource, float amount)
    {
        if (resource == null) return;

        ChangeResource(resource, amount);
        Debug.Log($"{resource.resourceName} kaynağından {amount} toplandı!");
    }

    void UpdateResourceUI()
    {
        if (resourceText == null) return;

        List<string> parts = new List<string>();
        foreach (var entry in resources)
        {
            if (entry == null || entry.type == null) continue;
            parts.Add($"{entry.type.resourceName}: {entry.amount}");
        }

        resourceText.text = string.Join(" | ", parts);
    }

	void CheckLoseConditions()
	{
		if (gameOver) return;

		// Food kontrolü
		if (foodType != null)
		{
			var foodEntry = GetEntry(foodType);
			if (foodEntry != null && foodEntry.amount <= loseFoodAt)
			{
				LoseGame("Açlıktan öldün.");
				return;
			}
		}

		// Pollution kontrolü
		if (pollutionType != null)
		{
			var polEntry = GetEntry(pollutionType);
			if (polEntry != null && polEntry.amount >= losePollutionAt)
			{
				LoseGame("Aşırı kirlilikten öldün.");
				return;
			}
		}
	}

	void LoseGame(string reason)
	{
		if (gameOver) return;
		gameOver = true;

		ShowMessage(reason);

		Debug.LogWarning("OYUN KAYBEDİLDİ: " + reason);

		StartCoroutine(LoseAfterDelay());
	}

	IEnumerator LoseAfterDelay()
	{
		yield return new WaitForSecondsRealtime(messageDuration);
		Time.timeScale = 0f;
	}

	
	public void ShowMessage(string message)
	{
		if (gameMessageText == null) return;

		if (messageRoutine != null)
			StopCoroutine(messageRoutine);

		messageRoutine = StartCoroutine(ShowMessageRoutine(message));
	}

	IEnumerator ShowMessageRoutine(string message)
	{
		gameMessageText.gameObject.SetActive(true);
		gameMessageText.text = message;

		yield return new WaitForSeconds(messageDuration);

		gameMessageText.text = "";
		gameMessageText.gameObject.SetActive(false);
		messageRoutine = null;
	}
	
	
}

[System.Serializable]
public class ResourceEntry
{
    public ResourceType type;
    public float amount;
}
