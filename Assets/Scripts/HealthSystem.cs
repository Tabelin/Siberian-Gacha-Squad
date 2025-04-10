// HealthSystem.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // ������� ��������
    public float currentHealth;

    // ������������ ��������
    public float maxHealth;

    // ���� �����
    public float attackPower;

    // ������
    public float defense;

    // ������� �������
    public int level;

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

    // ����� ��� ��������� �����
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        // ��������� ������
        float effectiveDamage = Mathf.Max(damage - defense, 0f);

        currentHealth -= effectiveDamage;
        if (currentHealth <= 0)
        {
            Die(); // ���� �������� <= 0, ������ �������
        }

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

        Debug.Log("������ ����!");
    }
    // ����� ��� ���������� ������ ��������
    private void UpdateHealthUI()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {Mathf.Round(currentHealth)}/{maxHealth}"; // ���������� ������� � ������������ ��������

            // ������������� ����� ��� ������� �������
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            hpText.transform.position = screenPosition;
        }
    }
    // ����� ��� ������������� �������� � ������
    public void InitializeHealth(float initialMaxHealth, float initialCurrentHealth, float initialAttackPower, float initialDefense, int initialLevel)
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;

        if (hpText != null)
        {
            UpdateHealthUI(); // ��������� UI ��� �������������
        }

        Debug.Log($"�������� ����������������: MaxHealth - {maxHealth}, CurrentHealth - {currentHealth}, AttackPower - {attackPower}, Defense - {defense}, Level - {level}");
    }
}