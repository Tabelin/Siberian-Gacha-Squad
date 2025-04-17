// HealthSystem.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
   
    public float maxHealth; // ������������ ��������
    public float currentHealth; // ������� ��������
    public float attackPower;// ���� �����
    public float defense;// ������
    // ��� ��������� (��� �������������)
    public string characterName;
    // ������� �������
    public int level = 1;

    // ������������ ������� (����������� �� ����������)
    public int maxLevel = 10; // �������� �� ���������
    // �������� ��������� (���� ����� ��� ����������)
    public Rarity rarity;
    // ���� ��� �������� �����
    public bool isAlive = true;

    // ���� ���������
    private float experience = 0f;
    // ����������� ���� ��� ���������� ������
    public float experienceToNextLevel = 100f;

    // ������������ �������� ���� ��� ������
    public float healthPerLevelMin = 5f; // ����������� ������� ��������
    public float healthPerLevelMax = 25f; // ������������ ������� ��������
    public float attackPerLevelMin = 3f; // ����������� ������� �����
    public float attackPerLevelMax = 7f; // ������������ ������� �����
    public float defensePerLevelMin = 1f; // ����������� ������� ������
    public float defensePerLevelMax = 3f; // ������������ ������� ������

    // ��������� ���� ��� ����������� ��������
    public Text hpText;

    // ��������� ���� ��� ����������� ������
    public Text levelText;

    void Start()
    {
        currentHealth = maxHealth; // ��� ������ �������� ������

        if (hpText != null)
        {
            UpdateUI(); // ��������� UI ��� ������
        }
    }

    void Update()
    {
        if (hpText != null)
        {
            UpdateUI(); // ��������� UI ������ ����
        }
    }
    // ����� ��� ������������� �������� � ������ �� ����������
    public void InitializeHealth(float initialMaxHealth, float initialCurrentHealth, float initialAttackPower, float initialDefense, int initialLevel, int initialMaxLevel, string name)
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;
        maxLevel = initialMaxLevel; // ��������� ������������ ������� �� ����������
        characterName = name;

        if (hpText != null && levelText != null)
        {
            UpdateUI(); // ��������� UI ����� �������������
        }

        Debug.Log($"HealthSystem ����������������: Name - {characterName}, Level - {level}, MaxLevel - {maxLevel}, MaxHealth - {maxHealth:F2}, CurrentHealth - {currentHealth:F2}, AttackPower - {attackPower:F2}, Defense - {defense:F2}");
    }
    // ����� ��� ��������� �����
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        // ��������� ����������� ����, �������� ������
        float effectiveDamage = Mathf.Max(damage - defense, 0f);

        currentHealth -= effectiveDamage;
        if (currentHealth <= 0)
        {
            Die(); // ���� �������� �������� ����, ������ �������
        }

        Debug.Log($"������ '{characterName}' ������� ����: {effectiveDamage:F2}. �������� ��������: {currentHealth:F2}/{maxHealth:F2}");


        if (hpText != null)
        {
            UpdateUI(); ; // ��������� UI ����� ��������� �����
            ShowDamageNumbers(effectiveDamage); // ���������� ����� �����
        }
    }
    // ����� ��� ������
    private void Die()
    {
        isAlive = false;
        currentHealth = 0f;

        Debug.Log($"������ '{characterName}' ����!");
        if (!string.IsNullOrEmpty(characterName))
        {
            RemoveCharacterFromSaveData(characterName); // ������� ��������� �� ����������
        }
        else if (hpText != null)
        {
            ShowExperienceNumbers(); // ���������� ���������� ����� �����
        }

        // ���������� ���� ���������� ���������
        SendExperienceToClosestCharacter();

        Destroy(gameObject); // ���������� ������
    }

    // ����� ��� �������� ����� ���������� ���������
    private void SendExperienceToClosestCharacter()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character"); // ������� ���� ����������
        if (characters.Length > 0)
        {
            GameObject closestCharacter = FindClosestCharacter(characters); // ������� ���������� ���������
            if (closestCharacter != null)
            {
                CharacterManager characterManager = closestCharacter.GetComponent<CharacterManager>();
                if (characterManager != null && characterManager.healthSystem != null)
                {
                    float expAmount = CalculateExperienceReward(); // ������������ ������� �� ����
                    characterManager.healthSystem.GainExperience(expAmount); // ���������� ���� ���������

                    Debug.Log($"���� �������� ���� ��������� '{closestCharacter.name}': +{expAmount:F2} XP");
                }
            }
        }
    }

    // ����� ��� ������ ���������� ���������
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

    // ����� ��� �������� ��������� �� ����������
    private void RemoveCharacterFromSaveData(string characterName)
    {
        // ��������� ������� ����������� ������
        string json = PlayerPrefs.GetString("SaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("����������� ������ � ���������� ���.");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // ������� ��������� �� ����� � ������� ��� �� ������
        for (int i = 0; i < saveData.characters.Count; i++)
        {
            if (saveData.characters[i].name == characterName)
            {
                saveData.characters.RemoveAt(i); // ������� ��������� �� ������
                Debug.Log($"�������� '{characterName}' ������ �� ����������.");

                // ��������� ����������� ������
                SaveUpdatedData(saveData);
                break;
            }
        }
    }

    // ����� ��� ���������� ����������� ������
    private void SaveUpdatedData(SaveData saveData)
    {
        // 1. ����������� SaveData ������� � JSON
        string updatedJson = JsonUtility.ToJson(saveData);
        // 2. ���������� JSON � PlayerPrefs
        PlayerPrefs.SetString("SaveData", updatedJson);
        PlayerPrefs.Save(); // ��������� ������
        Debug.Log("���������� ���������.");
    }

    // ����� ��� ���������� ��������� ���������
    private void SaveCharacterData()
    {
        // ��������� ������� ����������� ������
        string json = PlayerPrefs.GetString("SaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("����������� ������ � ���������� ���.");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // ������� ��������� �� ����� � ��������� ��� ������
        for (int i = 0; i < saveData.characters.Count; i++)
        {
            if (saveData.characters[i].name == characterName)
            {
                saveData.characters[i].level = level;
                saveData.characters[i].health = (int)maxHealth; // ����������� float � int
                saveData.characters[i].attack = (int)attackPower; // ����������� float � int
                saveData.characters[i].defense = (int)defense; // ����������� float � int

                Debug.Log($"������ ��������� '{characterName}' ��������� � ����������.");
                SaveUpdatedData(saveData);
                break;
            }
        }
    }
    // ����� ��� ��������� �����
    public void GainExperience(float expAmount)
    {
        if (!isAlive)
        {
            Debug.LogWarning($"�������� '{characterName}' ���� � �� ����� �������� ����!");
            return;
        }

        experience += expAmount;
        Debug.Log($"�������� '{characterName}' ������� ����: +{expAmount:F2}. ����� ����: {experience:F2}/{experienceToNextLevel:F2}");

        while (experience >= experienceToNextLevel && level < maxLevel)
        {
            LevelUp(); // ��������� �������
        }
    }

    private void LevelUp()
    {
        if (level >= maxLevel)
        {
            Debug.Log($"�������� '{characterName}' ������ ������������� ������ ({maxLevel})!");
            return;
        }

        level++;
        maxHealth += Random.Range(healthPerLevelMin, healthPerLevelMax); // ������� �������� (float)
        currentHealth = maxHealth;
        attackPower += (float)Random.Range((int)attackPerLevelMin, (int)attackPerLevelMax); // ����������� � int, ����� � float
        defense += (float)Random.Range((int)defensePerLevelMin, (int)defensePerLevelMax); // �� �� ��� ������

        experienceToNextLevel *= 1.5f;

        Debug.Log($"�������� '{characterName}' ������ ������ ������! �������: {level}");

        if (hpText != null && levelText != null)
        {
            UpdateUI();
        }

        SaveCharacterData();
    }

    // ������ ������� �� ����
    private float CalculateExperienceReward()
    {
        return 50f + level * 10f; // ��������� �������: ������� ���� + ����� �� �������
    }

    // ����� ��� ������ ���� �����
    private void ShowDamageNumbers(float damage)
    {
        InstantiateNumberPopup(damage.ToString(), Color.red); // ������� popup � ������ (������� ����)
    }

    // ����� ��� ������ ���� �����
    private void ShowExperienceNumbers()
    {
        float expReward = CalculateExperienceReward();
        InstantiateNumberPopup(expReward.ToString() + " XP", Color.green); // ������� popup � ������ (������� ����)
    }


    // ����� ��� ���������� ������ �������� � ������
    private void UpdateUI()
    {
        if (hpText != null && Camera.main != null)
        {
            // ���������� ������� � ������������ ��������
            hpText.text = $"HP: {Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";

            // ������������� ����� ��� ������� �������
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            hpText.transform.position = screenPosition;
        }

        if (levelText != null && Camera.main != null)
        {
            // ���������� ������� �������
            levelText.text = $"Level: {level}";

            // ������������� ����� ���� ��������
            Vector3 levelScreenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(6f, 3.5f, 0f));
            levelText.transform.position = levelScreenPosition;
        }
    }
    private void InstantiateNumberPopup(string text, Color color)
    {
        GameObject numberPopupPrefab = Resources.Load<GameObject>("NumberPopup"); // ���������, ��� ������ ��������� � ����� Resources
        if (numberPopupPrefab == null)
        {
            Debug.LogError("������ NumberPopup �� ������!");
            return;
        }

        // ������ ����� �� ������� ��� ��������
        Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f);
        GameObject popupInstance = Instantiate(numberPopupPrefab, spawnPosition, Quaternion.identity);

        // ��������� ������ � �����
        Text popupText = popupInstance.GetComponent<Text>();
        if (popupText != null)
        {
            popupText.text = text;
            popupText.color = color;

            // ����������� ������ ������
            popupText.fontSize = 25;
        }

        // ����������� ����� �������������
        NumberPopup popupScript = popupInstance.GetComponent<NumberPopup>();
        if (popupScript != null)
        {
            popupScript.lifetime = 2f; // ������ ������ ���������� 2 �������
        }
    }
}