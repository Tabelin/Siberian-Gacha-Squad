using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceEntry
{
    public ResourceType type;
    public float amount;
    public float weightPerUnit = 1f; // Вес на единицу ресурса

    public float TotalWeight => amount * weightPerUnit;

    public ResourceEntry(ResourceType type, float amount)
    {
        this.type = type;
        this.amount = amount;
        this.weightPerUnit = GetWeightPerResource(type);
    }

    private float GetWeightPerResource(ResourceType type)
    {
        // Можно настроить вес каждого типа ресурса
        switch (type)
        {
            case ResourceType.Wood: return 0.5f;
            case ResourceType.Stone: return 1f;
            case ResourceType.Metal: return 1f;
            case ResourceType.Food: return 0.2f;
            default: return 1f;
        }
    }
}

public class CharacterInventory : MonoBehaviour
{
    [SerializeField] public float maxCarryWeight = 100f;
    public float MaxCarryWeight => maxCarryWeight;

    private List<ResourceEntry> resources = new List<ResourceEntry>();
    private float currentCarryWeight = 0f;

    public bool CanCarry(ResourceType type, float amount, float weightPerUnit = 1f)
    {
        float totalWeight = currentCarryWeight + (amount * weightPerUnit);
        float limit = maxCarryWeight * 2 / 3f;

        return totalWeight <= limit;
    }

    public void AddResource(ResourceType type, float amount)
    {
        if (amount <= 0) return;

        float weightPerUnit = new ResourceEntry(type, 1f).weightPerUnit;

        if (!CanCarry(type, amount, weightPerUnit))
        {
            Debug.Log($"Персонаж не может взять {type} — превышен лимит веса.");
            return;
        }

        var existing = resources.Find(r => r.type == type);
        if (existing != null)
        {
            existing.amount += amount;
        }
        else
        {
            resources.Add(new ResourceEntry(type, amount));
        }

        currentCarryWeight += amount * weightPerUnit;
        Debug.Log($"{amount} {type} добавлено. Текущий вес: {currentCarryWeight}/{maxCarryWeight}");
    }

    

    public float GetResourceAmount(ResourceType type)
    {
        var entry = resources.Find(r => r.type == type);
        return entry?.amount ?? 0f;
    }

    public float GetCurrentCarryWeight()
    {
        return currentCarryWeight;
    }

    public float GetRemainingSpace()
    {
        float limit = maxCarryWeight * 2 / 3f;
        return limit - currentCarryWeight;
    }

    public bool HasResource(ResourceType type, float amount)
    {
        var entry = resources.Find(r => r.type == type);
        return entry != null && entry.amount >= amount;
    }
    public void RemoveResource(ResourceType type, float amount)
    {
        var entry = resources.Find(r => r.type == type);
        if (entry != null && entry.amount >= amount)
        {
            entry.amount -= amount;
            currentCarryWeight = CalculateTotalWeight();
            Debug.Log($"Удалено {amount} {type}. Осталось: {entry.amount}");
        }
        else
        {
            Debug.LogWarning($"Не хватает {type} для удаления");
        }
    }
    private float CalculateTotalWeight()
    {
        float total = 0f;

        foreach (var resource in resources)
        {
            total += resource.amount * resource.weightPerUnit;
        }

        return total;
    }
}