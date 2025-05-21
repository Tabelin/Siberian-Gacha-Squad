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


    private float experience = 0f;// ���� ���������   
    public float currentExperience = 0f;// ������� ���� (��� ����������� � ������)   
    public float experienceToNextLevel = 1000f;// ����������� ���� ��� ���������� ������

    // ������������ �������� ���� ��� ������
    public int healthPerLevelMin = 5;    // ����������� ������� ��������
    public int healthPerLevelMax = 25;  // ������������ ������� ��������
    public int attackPerLevelMin = 3;   // ����������� ������� �����
    public int attackPerLevelMax = 7;  // ������������ ������� �����
    public int defensePerLevelMin = 1;  // ����������� ������� ������
    public int defensePerLevelMax = 3; // ������������ ������� ������

    // ��������� ���� (����� ������� ���������/�����)
    public Text hpText;                // ����� �������� 
    public Text levelText;             // ����� ������ 
    public Text expText;               // ����� ��� �����
    // ������
    public Text damagePopup;          // ��� �����
    public Text experiencePopup;      // ��� �����
    public Text levelPopup;           // ��� ������

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
    // ����� ��� �������������
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
        // ��������� ������� �� ����
        float expReward = CalculateExperienceReward(); 

        // ���������� ���� ���������� ���������
        SendExperienceToClosestCharacter();

        Destroy(gameObject); // ���������� ������
    }

    // ����� ��� �������� �����
    private void SendExperienceToClosestCharacter()
    {
        // ������� ���� ����������
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character"); // ���������!

        if (characters.Length == 0) return;

        GameObject closestCharacter = FindClosestCharacter(characters);
        if (closestCharacter != null)
        {
            CharacterManager characterManager = closestCharacter.GetComponent<CharacterManager>();
            if (characterManager?.healthSystem != null)
            {
                float expReward = CalculateExperienceReward();
                characterManager.healthSystem.GainExperience(expReward);

                // ���������� ����� ����� ��� ����������
                characterManager.healthSystem.ShowExperiencePopup(expReward);
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
                saveData.characters[i].experience = experience; // ��������� ����
                saveData.characters[i].experienceToNextLevel = experienceToNextLevel; // ��������� ��������� ����


                Debug.Log($"������ ��������� '{characterName}' ��������� � ����������.");
                SaveUpdatedData(saveData);
                break;
            }
        }
    }
    // ����� ��� ��������� �����
    public void GainExperience(float expAmount)
    {
        if (!isAlive || level >= maxLevel) return;

        experience += expAmount;
        currentExperience = experience; // ��� ����������� � ������

        // ���������� ����� �����
        ShowExperiencePopup(expAmount);

        UpdateUI();

        // �������� �� ��������� ������
        while (experience >= experienceToNextLevel && level < maxLevel)
        {
            LevelUp();
        }
    }

    // ����� ��� ������ ������ �����
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
            Debug.Log($"�������� '{characterName}' ������ ������������� ������ ({maxLevel})!");
            return;
        }

        experienceToNextLevel *= 1.5f;

        level++;

        maxHealth += Random.Range(healthPerLevelMin, healthPerLevelMax); // ������� �������� (float)
        currentHealth = maxHealth;
        attackPower += (float)Random.Range((int)attackPerLevelMin, (int)attackPerLevelMax); // ����������� � int, ����� � float
        defense += (float)Random.Range((int)defensePerLevelMin, (int)defensePerLevelMax); // �� �� ��� ������

        Debug.Log($"�������� '{characterName}' ������ ������ ������! �������: {level}");

        if (hpText != null && levelText != null)
        {
            UpdateUI();
        }

        SaveCharacterData();

        // ���������� ����� ������
        ShowLevelUpPopup();
    }

    // ����� ��� ������ ������ ������
    private void ShowLevelUpPopup()
    {
        Text levelClone = Instantiate(levelPopup, levelPopup.transform.parent);
        levelClone.text = $"Level {level}!";
        levelClone.color = Color.green;

        StartCoroutine(MovePopupUpAndDestroy(levelClone));
    }

    // �������� ��� �������� ������ ����� � �������
    private IEnumerator MovePopupUpAndHide(Text popup)
    {
        float duration = 1f;
        Vector3 originalPosition = popup.transform.position;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            // �������� ����� � ����������
            float movement = Mathf.Sin(t / duration * Mathf.PI * 1f) * 2f;
            popup.transform.position = originalPosition + Vector3.up * movement;
            yield return null;
        }

        popup.gameObject.SetActive(false);
    }

    // ������ ������� �� ����
    private float CalculateExperienceReward()
    {
        return 50f + level * 10f; // ��������� �������: ������� ���� + ����� �� �������
    }



    // ���������� ������ �������� � ������
    private void UpdateUI()
    {
        // ��� HP
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";
        }

        // ��� ������
        if (levelText != null)
        {
            levelText.text = $"lvl {level}:";
        }

        // ��� �����
        if (expText != null)
        {
            expText.text = $"EXP: {experience:F0}/{experienceToNextLevel:F0}";
        }
    }
    // ����� ��� ������ �����
    private void ShowDamageNumbers(float damage)
    {
        if (damagePopup == null) return;

        // ��������� Text �� Canvas
        Text damageClone = Instantiate(damagePopup, damagePopup.transform.parent);
        damageClone.text = $"-{damage}";
        damageClone.color = Color.red;

        // �������� ����� ����� ��������
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

    // ����� ��� ������� ������ (����� ��������)
    private IEnumerator HideDamagePopup()
    {
        yield return new WaitForSeconds(1f);
        damagePopup.gameObject.SetActive(false);
    }
}