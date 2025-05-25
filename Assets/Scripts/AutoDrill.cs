using System.Collections;
using UnityEngine;

public class AutoDrill : MonoBehaviour
{
    [Header("Настройки добычи")]
    public ResourceType resourceType = ResourceType.Metal;
    public float miningSpeed = 5f; // Время между сбором
    public float amountPerGather = 10f; // Сколько добывает за раз
    public float capacity = 100f; // Максимальное хранилище
    public float weightPerUnit = 2f; // Для CharacterInventory

    private float storedResources = 0f;
    private bool isMining = true;

    private void Start()
    {
        StartCoroutine(MineResource());
    }

    private IEnumerator MineResource()
    {
        while (isMining)
        {
            if (storedResources < capacity)
            {
                storedResources += amountPerGather;
                Debug.Log($"Бур добыл {amountPerGather} {resourceType}. Всего: {storedResources}/{capacity}");
            }

            yield return new WaitForSeconds(miningSpeed);
        }
    }

    // Вызывается персонажем, когда хочет забрать ресурсы
    public float TakeResources(float amount, out float weight)
    {
        float actualTake = Mathf.Min(amount, storedResources);

        storedResources -= actualTake;
        weight = actualTake * weightPerUnit;

        Debug.Log($"Забрано {actualTake} {resourceType} из бура");
        return actualTake;
    }

    // Позволяет узнать, есть ли что забрать
    public bool HasResources => storedResources > 0;

    public float GetStoredAmount() => storedResources;
}