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


    private float experience = 0f;// Опыт персонажа   
    public float currentExperience = 0f;// Текущий опыт (для отображения в попапе)   
    public float experienceToNextLevel = 1000f;// Необходимый опыт для следующего уровня

    // Коэффициенты прироста стат при уровне
    public int healthPerLevelMin = 5;    // Минимальный прирост здоровья
    public int healthPerLevelMax = 25;  // Максимальный прирост здоровья
    public int attackPerLevelMin = 3;   // Минимальный прирост атаки
    public int attackPerLevelMax = 7;  // Максимальный прирост атаки
    public int defensePerLevelMin = 1;  // Минимальный прирост защиты
    public int defensePerLevelMax = 3; // Максимальный прирост защиты

    // Текстовые поля (часть префаба персонажа/врага)
    public Text hpText;                // Текст здоровья 
    public Text levelText;             // Текст уровня 
    public Text expText;               // Текст для опыта
    // Попапы
    public Text damagePopup;          // Для урона
    public Text experiencePopup;      // Для опыта
    public Text levelPopup;           // Для уровня

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
    // Метод для инициализации
    public void InitializeHealth(int initialMaxHealth, int initialCurrentHealth, int initialAttackPower, int initialDefense, int initialLevel, int initialMaxLevel, float initialExperience, float initialExperienceToNextLevel, string name)
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;
        maxLevel = initialMaxLevel;
        experience = initialExperience;
        experienceToNextLevel = initialExperienceToNextLevel;
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
        // Вычисляем награду за опыт
        float expReward = CalculateExperienceReward(); 

        // Отправляем опыт ближайшему персонажу
        SendExperienceToClosestCharacter();

        Destroy(gameObject); // Уничтожаем объект
    }

    // Метод для отправки опыта
    private void SendExperienceToClosestCharacter()
    {
        // Находим всех персонажей
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character"); // Добавлено!

        if (characters.Length == 0) return;

        GameObject closestCharacter = FindClosestCharacter(characters);
        if (closestCharacter != null)
        {
            CharacterManager characterManager = closestCharacter.GetComponent<CharacterManager>();
            if (characterManager?.healthSystem != null)
            {
                float expReward = CalculateExperienceReward();
                characterManager.healthSystem.GainExperience(expReward);

                // Показываем попап опыта над персонажем
                characterManager.healthSystem.ShowExperiencePopup(expReward);
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
                saveData.characters[i].experience = experience; // Добавляем опыт
                saveData.characters[i].experienceToNextLevel = experienceToNextLevel; // Добавляем требуемый опыт


                Debug.Log($"Данные персонажа '{characterName}' обновлены в сохранении.");
                SaveUpdatedData(saveData);
                break;
            }
        }
    }
    // Метод для получения опыта
    public void GainExperience(float expAmount)
    {
        if (!isAlive || level >= maxLevel) return;

        experience += expAmount;
        currentExperience = experience; // Для отображения в попапе

        // Показываем попап опыта
        ShowExperiencePopup(expAmount);

        UpdateUI();

        // Проверка на повышение уровня
        while (experience >= experienceToNextLevel && level < maxLevel)
        {
            LevelUp();
        }
    }

    // Метод для показа попапа опыта
    private void ShowExperiencePopup(float exp)
    {
        Text expClone = Instantiate(experiencePopup, experiencePopup.transform.parent);
        expClone.text = $"+{exp:F0} XP";
        expClone.color = Color.yellow;

        StartCoroutine(MovePopupUpAndDestroy(expClone));
    }

    private void LevelUp()
    {
        if (level >= maxLevel)
        {
            Debug.Log($"Персонаж '{characterName}' достиг максимального уровня ({maxLevel})!");
            return;
        }

        experienceToNextLevel *= 1.5f;

        level++;

        maxHealth += Random.Range(healthPerLevelMin, healthPerLevelMax); // Прирост здоровья (float)
        currentHealth = maxHealth;
        attackPower += (float)Random.Range((int)attackPerLevelMin, (int)attackPerLevelMax); // Преобразуем в int, затем в float
        defense += (float)Random.Range((int)defensePerLevelMin, (int)defensePerLevelMax); // То же для защиты

        Debug.Log($"Персонаж '{characterName}' достиг нового уровня! Уровень: {level}");

        if (hpText != null && levelText != null)
        {
            UpdateUI();
        }

        SaveCharacterData();

        // Показываем попап уровня
        ShowLevelUpPopup();
    }

    // Метод для показа попапа уровня
    private void ShowLevelUpPopup()
    {
        Text levelClone = Instantiate(levelPopup, levelPopup.transform.parent);
        levelClone.text = $"Level {level}!";
        levelClone.color = Color.green;

        StartCoroutine(MovePopupUpAndDestroy(levelClone));
    }

    // Корутина для движения попапа вверх и скрытия
    private IEnumerator MovePopupUpAndHide(Text popup)
    {
        float duration = 1f;
        Vector3 originalPosition = popup.transform.position;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            // Движение вверх с ускорением
            float movement = Mathf.Sin(t / duration * Mathf.PI * 1f) * 2f;
            popup.transform.position = originalPosition + Vector3.up * movement;
            yield return null;
        }

        popup.gameObject.SetActive(false);
    }

    // Расчет награды за опыт
    private float CalculateExperienceReward()
    {
        return 50f + level * 10f; // Примерная формула: базовый опыт + бонус за уровень
    }



    // Обновление текста здоровья и уровня
    private void UpdateUI()
    {
        // Для HP
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";
        }

        // Для уровня
        if (levelText != null)
        {
            levelText.text = $"lvl {level}:";
        }

        // Для опыта
        if (expText != null)
        {
            expText.text = $"EXP: {experience:F0}/{experienceToNextLevel:F0}";
        }
    }
    // Метод для показа урона
    private void ShowDamageNumbers(float damage)
    {
        if (damagePopup == null) return;

        // Клонируем Text из Canvas
        Text damageClone = Instantiate(damagePopup, damagePopup.transform.parent);
        damageClone.text = $"-{damage}";
        damageClone.color = Color.red;

        // Движение вверх через корутину
        StartCoroutine(MovePopupUpAndDestroy(damageClone));
    }

    private IEnumerator MovePopupUpAndDestroy(Text popup)
    {
        float duration = 1f;
        Vector3 startPosition = popup.transform.position;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float movement = Mathf.Sin(t / duration * Mathf.PI * 0.5f) * 2f;
            popup.transform.position = startPosition + Vector3.up * movement;
            yield return null;
        }

        Destroy(popup.gameObject);
    }

    // Метод для скрытия попапа (через корутину)
    private IEnumerator HideDamagePopup()
    {
        yield return new WaitForSeconds(1f);
        damagePopup.gameObject.SetActive(false);
    }
}