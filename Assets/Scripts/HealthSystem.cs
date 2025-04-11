// HealthSystem.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
   
    public float maxHealth; // Максимальное здоровье
    public float currentHealth; // Текущее здоровье
    public float attackPower;// Сила атаки
    public float defense;// Защита
    // Уровень объекта
    public int level;
    // Имя персонажа (для идентификации)
    public string characterName;

    // Редкость персонажа (если нужно для сохранения)
    public Rarity rarity;
    // Флаг для проверки жизни
    public bool isAlive = true;

    // Текстовое поле для отображения здоровья
    public Text hpText;

    void Start()
    {
        currentHealth = maxHealth; // При старте здоровье полное

        if (hpText != null)
        {
            UpdateHealthUI(); // Обновляем UI при старте
        }
    }

    void Update()
    {
        if (hpText != null)
        {
            UpdateHealthUI(); // Обновляем UI каждый кадр
        }
    }
    // Метод для инициализации здоровья и статов
    public void InitializeHealth(float initialMaxHealth, float initialCurrentHealth, float initialAttackPower, float initialDefense, int initialLevel, string name = "Enemy")
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;
        characterName = name; // Устанавливаем имя объекта

        if (hpText != null)
        {
            UpdateHealthUI(); // Обновляем UI после инициализации
        }

        Debug.Log($"HealthSystem инициализировано: Name - {characterName}, MaxHealth - {maxHealth:F2}, CurrentHealth - {currentHealth:F2}, AttackPower - {attackPower:F2}, Defense - {defense:F2}, Level - {level}");
    }
    // Метод для получения урона
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        // Вычисляем эффективный урон, учитывая защиту
        float effectiveDamage = Mathf.Max(damage - defense, 0f);

        currentHealth -= effectiveDamage;
        if (currentHealth <= 0)
        {
            Die(); // Если здоровье достигло нуля, объект умирает
        }

        Debug.Log($"Объект '{characterName}' получил урон: {effectiveDamage:F2}. Осталось здоровья: {currentHealth:F2}/{maxHealth:F2}");


        if (hpText != null)
        {
            UpdateHealthUI(); // Обновляем UI после получения урона
        }
    }
    // Метод для смерти
    private void Die()
    {
        isAlive = false;
        currentHealth = 0f;

        Debug.Log($"Объект '{characterName}' убит!");
        if (!string.IsNullOrEmpty(characterName))
        {
            RemoveCharacterFromSaveData(characterName); // Удаляем персонажа из сохранения
        }

        Destroy(gameObject); // Уничтожаем объект
    }

    // Метод для удаления персонажа из сохранения
    private void RemoveCharacterFromSaveData(string characterName)
    {
        // Загружаем текущие сохраненные данные
        string json = PlayerPrefs.GetString("SaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Сохраненных данных о персонажах нет.");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // Находим персонажа по имени и удаляем его из списка
        for (int i = 0; i < saveData.characters.Count; i++)
        {
            if (saveData.characters[i].name == characterName)
            {
                saveData.characters.RemoveAt(i); // Удаляем персонажа из списка
                Debug.Log($"Персонаж '{characterName}' удален из сохранения.");

                // Сохраняем обновленные данные
                SaveUpdatedData(saveData);
                break;
            }
        }
    }

    // Метод для сохранения обновленных данных
    private void SaveUpdatedData(SaveData saveData)
    {
        string updatedJson = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("SaveData", updatedJson);
        PlayerPrefs.Save(); // Сохраняем данные
        Debug.Log("Сохранение обновлено.");
    }

    // Метод для обновления текста здоровья
    private void UpdateHealthUI()
    {
        if (hpText != null && Camera.main != null)
        {
            // Отображаем текущее и максимальное здоровье
            hpText.text = $"HP: {Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";

            // Позиционируем текст над головой объекта
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            hpText.transform.position = screenPosition;
        }
    }
    
}