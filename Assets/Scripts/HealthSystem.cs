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
    // Имя персонажа (для идентификации)
    public string characterName;
    // Уровень объекта
    public int level = 1;

    // Максимальный уровень (загружается из сохранения)
    public int maxLevel = 10; // Значение по умолчанию
    // Редкость персонажа (если нужно для сохранения)
    public Rarity rarity;
    // Флаг для проверки жизни
    public bool isAlive = true;

    // Опыт персонажа
    private float experience = 0f;
    // Необходимый опыт для следующего уровня
    public float experienceToNextLevel = 100f;

    // Коэффициенты прироста стат при уровне
    public float healthPerLevelMin = 5f; // Минимальный прирост здоровья
    public float healthPerLevelMax = 25f; // Максимальный прирост здоровья
    public float attackPerLevelMin = 3f; // Минимальный прирост атаки
    public float attackPerLevelMax = 7f; // Максимальный прирост атаки
    public float defensePerLevelMin = 1f; // Минимальный прирост защиты
    public float defensePerLevelMax = 3f; // Максимальный прирост защиты

    // Текстовое поле для отображения здоровья
    public Text hpText;

    // Текстовое поле для отображения уровня
    public Text levelText;

    void Start()
    {
        currentHealth = maxHealth; // При старте здоровье полное

        if (hpText != null)
        {
            UpdateUI(); // Обновляем UI при старте
        }
    }

    void Update()
    {
        if (hpText != null)
        {
            UpdateUI(); // Обновляем UI каждый кадр
        }
    }
    // Метод для инициализации здоровья и статов из сохранения
    public void InitializeHealth(float initialMaxHealth, float initialCurrentHealth, float initialAttackPower, float initialDefense, int initialLevel, int initialMaxLevel, string name)
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;
        maxLevel = initialMaxLevel; // Загружаем максимальный уровень из сохранения
        characterName = name;

        if (hpText != null && levelText != null)
        {
            UpdateUI(); // Обновляем UI после инициализации
        }

        Debug.Log($"HealthSystem инициализировано: Name - {characterName}, Level - {level}, MaxLevel - {maxLevel}, MaxHealth - {maxHealth:F2}, CurrentHealth - {currentHealth:F2}, AttackPower - {attackPower:F2}, Defense - {defense:F2}");
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
            UpdateUI(); ; // Обновляем UI после получения урона
            ShowDamageNumbers(effectiveDamage); // Показываем цифры урона
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
        else if (hpText != null)
        {
            ShowExperienceNumbers(); // Показываем выпадающие цифры опыта
        }

        // Отправляем опыт ближайшему персонажу
        SendExperienceToClosestCharacter();

        Destroy(gameObject); // Уничтожаем объект
    }

    // Метод для отправки опыта ближайшему персонажу
    private void SendExperienceToClosestCharacter()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character"); // Находим всех персонажей
        if (characters.Length > 0)
        {
            GameObject closestCharacter = FindClosestCharacter(characters); // Находим ближайшего персонажа
            if (closestCharacter != null)
            {
                CharacterManager characterManager = closestCharacter.GetComponent<CharacterManager>();
                if (characterManager != null && characterManager.healthSystem != null)
                {
                    float expAmount = CalculateExperienceReward(); // Рассчитываем награду за опыт
                    characterManager.healthSystem.GainExperience(expAmount); // Отправляем опыт персонажу

                    Debug.Log($"Враг отправил опыт персонажу '{closestCharacter.name}': +{expAmount:F2} XP");
                }
            }
        }
    }

    // Метод для поиска ближайшего персонажа
    private GameObject FindClosestCharacter(GameObject[] characters)
    {
        GameObject closestCharacter = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject character in characters)
        {
            float distance = Vector3.Distance(transform.position, character.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestCharacter = character;
            }
        }

        return closestCharacter;
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
        // 1. Преобразуем SaveData обратно в JSON
        string updatedJson = JsonUtility.ToJson(saveData);
        // 2. Записываем JSON в PlayerPrefs
        PlayerPrefs.SetString("SaveData", updatedJson);
        PlayerPrefs.Save(); // Сохраняем данные
        Debug.Log("Сохранение обновлено.");
    }

    // Метод для сохранения изменений персонажа
    private void SaveCharacterData()
    {
        // Загружаем текущие сохраненные данные
        string json = PlayerPrefs.GetString("SaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Сохраненных данных о персонажах нет.");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // Находим персонажа по имени и обновляем его данные
        for (int i = 0; i < saveData.characters.Count; i++)
        {
            if (saveData.characters[i].name == characterName)
            {
                saveData.characters[i].level = level;
                saveData.characters[i].health = (int)maxHealth; // Преобразуем float в int
                saveData.characters[i].attack = (int)attackPower; // Преобразуем float в int
                saveData.characters[i].defense = (int)defense; // Преобразуем float в int

                Debug.Log($"Данные персонажа '{characterName}' обновлены в сохранении.");
                SaveUpdatedData(saveData);
                break;
            }
        }
    }
    // Метод для получения опыта
    public void GainExperience(float expAmount)
    {
        if (!isAlive)
        {
            Debug.LogWarning($"Персонаж '{characterName}' мёртв и не может получить опыт!");
            return;
        }

        experience += expAmount;
        Debug.Log($"Персонаж '{characterName}' получил опыт: +{expAmount:F2}. Общий опыт: {experience:F2}/{experienceToNextLevel:F2}");

        while (experience >= experienceToNextLevel && level < maxLevel)
        {
            LevelUp(); // Поднимаем уровень
        }
    }

    private void LevelUp()
    {
        if (level >= maxLevel)
        {
            Debug.Log($"Персонаж '{characterName}' достиг максимального уровня ({maxLevel})!");
            return;
        }

        level++;
        maxHealth += Random.Range(healthPerLevelMin, healthPerLevelMax); // Прирост здоровья (float)
        currentHealth = maxHealth;
        attackPower += (float)Random.Range((int)attackPerLevelMin, (int)attackPerLevelMax); // Преобразуем в int, затем в float
        defense += (float)Random.Range((int)defensePerLevelMin, (int)defensePerLevelMax); // То же для защиты

        experienceToNextLevel *= 1.5f;

        Debug.Log($"Персонаж '{characterName}' достиг нового уровня! Уровень: {level}");

        if (hpText != null && levelText != null)
        {
            UpdateUI();
        }

        SaveCharacterData();
    }

    // Расчет награды за опыт
    private float CalculateExperienceReward()
    {
        return 50f + level * 10f; // Примерная формула: базовый опыт + бонус за уровень
    }

    // Метод для показа цифр урона
    private void ShowDamageNumbers(float damage)
    {
        InstantiateNumberPopup(damage.ToString(), Color.red); // Создаем popup с уроном (красный цвет)
    }

    // Метод для показа цифр опыта
    private void ShowExperienceNumbers()
    {
        float expReward = CalculateExperienceReward();
        InstantiateNumberPopup(expReward.ToString() + " XP", Color.green); // Создаем popup с опытом (зеленый цвет)
    }


    // Метод для обновления текста здоровья и уровня
    private void UpdateUI()
    {
        if (hpText != null && Camera.main != null)
        {
            // Отображаем текущее и максимальное здоровье
            hpText.text = $"HP: {Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";

            // Позиционируем текст над головой объекта
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            hpText.transform.position = screenPosition;
        }

        if (levelText != null && Camera.main != null)
        {
            // Отображаем текущий уровень
            levelText.text = $"Level: {level}";

            // Позиционируем текст выше здоровья
            Vector3 levelScreenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(6f, 3.5f, 0f));
            levelText.transform.position = levelScreenPosition;
        }
    }
    private void InstantiateNumberPopup(string text, Color color)
    {
        GameObject numberPopupPrefab = Resources.Load<GameObject>("NumberPopup"); // Убедитесь, что префаб находится в папке Resources
        if (numberPopupPrefab == null)
        {
            Debug.LogError("Префаб NumberPopup не найден!");
            return;
        }

        // Создаём попап на позиции над объектом
        Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f);
        GameObject popupInstance = Instantiate(numberPopupPrefab, spawnPosition, Quaternion.identity);

        // Настройка текста и цвета
        Text popupText = popupInstance.GetComponent<Text>();
        if (popupText != null)
        {
            popupText.text = text;
            popupText.color = color;

            // Увеличиваем размер текста
            popupText.fontSize = 25;
        }

        // Увеличиваем время существования
        NumberPopup popupScript = popupInstance.GetComponent<NumberPopup>();
        if (popupScript != null)
        {
            popupScript.lifetime = 2f; // Теперь попапы существуют 2 секунды
        }
    }
}