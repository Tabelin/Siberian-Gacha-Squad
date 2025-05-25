using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceType resourceType;
    public float maxAmount = 100f;
    public float currentAmount = 100f;
    public float weightPerUnit = 1f;
    public bool isDepleted => currentAmount <= 0;
    public GameObject miniMinePrefab;


    void Start()
    {
        currentAmount = maxAmount;
    }

    public float Gather(float amount)
    {
        if (isDepleted) return 0f;

        float gathered = Mathf.Min(amount, currentAmount);
        currentAmount -= gathered;

        if (currentAmount <= 0)
        {
            Debug.Log($"{resourceType} исчерпан.");
            Deplete();
        }

        return gathered;
    }

    // Метод исчезновения
    public void Deplete()
    {
        currentAmount = 0f;

        // Можно просто скрыть объект
        //gameObject.SetActive(false);

        // Или уничтожить его
        Destroy(gameObject);

        // Спавним мини-рудник
        if (miniMinePrefab != null)
        {
            GameObject newMine = Instantiate(miniMinePrefab, transform.position, Quaternion.identity);
            Debug.Log($"Ресурс исчерпан → спавнен мини-рудник {newMine.name}");
        }
        else
        {
            Debug.LogWarning("Не назначен префаб мини-рудника");
        }
    }
}

public enum ResourceType
{
    Wood,
    Stone,
    Food,
    Metal
}