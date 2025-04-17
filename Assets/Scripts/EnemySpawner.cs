// EnemySpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Префаб для врага
    public GameObject enemyPrefab;

    // Тег для точек спавна врагов
    public string spawnPointTag = "EnemySpawnPoint";

    // Количество врагов для спавна в каждой точке
    public int spawnCountPerPoint = 1; // Добавляем возможность изменять количество спавненных врагов

    // Минимальный уровень врагов
    public int minEnemyLevel = 1;

    // Максимальный уровень врагов
    public int maxEnemyLevel = 5;

    // Фиксированный максимальный уровень для врагов (добавьте это!)
    public int enemyMaxLevel = 10;

    // Список спавненных врагов
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Необходимо назначить EnemyPrefab!");
            return;
        }

        // Находим все точки спавна по тегу
        Transform[] spawnPoints = FindSpawnPoints();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Нет точек спавна для врагов в текущей сцене!");
            return;
        }

        // Спавним врагов в каждой точке спавна
        SpawnEnemies(spawnPoints);
    }

    // Метод для поиска точек спавна
    private Transform[] FindSpawnPoints()
    {
        List<Transform> spawnPoints = new List<Transform>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(spawnPointTag))
        {
            spawnPoints.Add(obj.transform);
        }

        return spawnPoints.ToArray();
    }

    // Метод для спавна врагов
    private void SpawnEnemies(Transform[] spawnPoints)
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            for (int i = 0; i < spawnCountPerPoint; i++) // Создаем указанное количество врагов на каждой точке
            {
                // Создаем объект врага
                GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                if (enemyObject != null)
                {
                    // Инициализируем HealthSystem врага
                    HealthSystem healthSystem = enemyObject.GetComponent<HealthSystem>();
                    if (healthSystem != null)
                    {
                        int enemyLevel = Random.Range(minEnemyLevel, maxEnemyLevel + 1); // Генерируем случайный уровень врага

                        // Вычисляем статы в зависимости от уровня
                        float enemyHealth = 100f + enemyLevel * 50f;
                        float enemyAttackPower = 10f + enemyLevel * 5f;
                        float enemyDefense = 5f + enemyLevel * 2f;

                        // Инициализируем HealthSystem с параметром name = "Enemy"
                        healthSystem.InitializeHealth(
                            initialMaxHealth: enemyHealth,
                            initialCurrentHealth: enemyHealth,
                            initialAttackPower: enemyAttackPower,
                            initialDefense: enemyDefense,
                            initialLevel: enemyLevel,
                            initialMaxLevel: enemyMaxLevel, 
                            name: "Enemy"
                        );

                        Debug.Log($"Враг уровня {enemyLevel} создан: Здоровье - {enemyHealth}, Атака - {enemyAttackPower}, Защита - {enemyDefense}");
                    }
                    else
                    {
                        Debug.LogError("Компонент HealthSystem не найден!");
                    }

                    // Сохраняем ссылку на спавненного врага
                    spawnedEnemies.Add(enemyObject);
                }
                else
                {
                    Debug.LogError("Не удалось создать EnemyPrefab!");
                }
            }
        }

        Debug.Log($"Всего спавнено врагов: {spawnedEnemies.Count}");
    }
}