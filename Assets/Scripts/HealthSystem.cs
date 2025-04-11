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
    // ������� �������
    public int level;
    // ��� ��������� (��� �������������)
    public string characterName;

    // �������� ��������� (���� ����� ��� ����������)
    public Rarity rarity;
    // ���� ��� �������� �����
    public bool isAlive = true;

    // ��������� ���� ��� ����������� ��������
    public Text hpText;

    void Start()
    {
        currentHealth = maxHealth; // ��� ������ �������� ������

        if (hpText != null)
        {
            UpdateHealthUI(); // ��������� UI ��� ������
        }
    }

    void Update()
    {
        if (hpText != null)
        {
            UpdateHealthUI(); // ��������� UI ������ ����
        }
    }
    // ����� ��� ������������� �������� � ������
    public void InitializeHealth(float initialMaxHealth, float initialCurrentHealth, float initialAttackPower, float initialDefense, int initialLevel, string name = "Enemy")
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;
        characterName = name; // ������������� ��� �������

        if (hpText != null)
        {
            UpdateHealthUI(); // ��������� UI ����� �������������
        }

        Debug.Log($"HealthSystem ����������������: Name - {characterName}, MaxHealth - {maxHealth:F2}, CurrentHealth - {currentHealth:F2}, AttackPower - {attackPower:F2}, Defense - {defense:F2}, Level - {level}");
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
            UpdateHealthUI(); // ��������� UI ����� ��������� �����
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

        Destroy(gameObject); // ���������� ������
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
        string updatedJson = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("SaveData", updatedJson);
        PlayerPrefs.Save(); // ��������� ������
        Debug.Log("���������� ���������.");
    }

    // ����� ��� ���������� ������ ��������
    private void UpdateHealthUI()
    {
        if (hpText != null && Camera.main != null)
        {
            // ���������� ������� � ������������ ��������
            hpText.text = $"HP: {Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";

            // ������������� ����� ��� ������� �������
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            hpText.transform.position = screenPosition;
        }
    }
    
}