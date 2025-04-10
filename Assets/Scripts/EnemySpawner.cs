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
            // Создаем объект врага
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            if (enemyObject != null)
            {
                // Получаем компонент Enemy
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                if (enemy == null)
                {
                    Debug.LogError($"Компонент Enemy не найден на объекте {enemyObject.name}!");
                    continue; // Пропускаем этот врага
                }

                // Получаем компонент HealthSystem
                HealthSystem healthSystem = enemyObject.GetComponent<HealthSystem>();
                if (healthSystem == null)
                {
                    Debug.LogError($"Компонент HealthSystem не найден на объекте {enemyObject.name}!");
                    continue; // Пропускаем этот врага
                }

                // Инициализируем врага
                try
                {
                    enemy.Initialize(1); // Уровень врага (можно изменить)
                    Debug.Log($"Враг успешно спавнится в точке {spawnPoint.position}!");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Ошибка при инициализации врага: {ex.Message}");
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
}