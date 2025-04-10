// HealthSystem.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // Текущее здоровье
    public float currentHealth;

    // Максимальное здоровье
    public float maxHealth;

    // Сила атаки
    public float attackPower;

    // Защита
    public float defense;

    // Уровень объекта
    public int level;

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

    // Метод для получения урона
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        // Применяем защиту
        float effectiveDamage = Mathf.Max(damage - defense, 0f);

        currentHealth -= effectiveDamage;
        if (currentHealth <= 0)
        {
            Die(); // Если здоровье <= 0, объект умирает
        }

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

        Debug.Log("Объект убит!");
    }
    // Метод для обновления текста здоровья
    private void UpdateHealthUI()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {Mathf.Round(currentHealth)}/{maxHealth}"; // Отображаем текущее и максимальное здоровье

            // Позиционируем текст над головой объекта
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            hpText.transform.position = screenPosition;
        }
    }
    // Метод для инициализации здоровья и статов
    public void InitializeHealth(float initialMaxHealth, float initialCurrentHealth, float initialAttackPower, float initialDefense, int initialLevel)
    {
        maxHealth = initialMaxHealth;
        currentHealth = initialCurrentHealth;
        attackPower = initialAttackPower;
        defense = initialDefense;
        level = initialLevel;

        if (hpText != null)
        {
            UpdateHealthUI(); // Обновляем UI при инициализации
        }

        Debug.Log($"Здоровье инициализировано: MaxHealth - {maxHealth}, CurrentHealth - {currentHealth}, AttackPower - {attackPower}, Defense - {defense}, Level - {level}");
    }
}