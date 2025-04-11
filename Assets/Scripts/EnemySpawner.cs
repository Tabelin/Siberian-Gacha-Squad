// EnemySpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ������ ��� �����
    public GameObject enemyPrefab;

    // ��� ��� ����� ������ ������
    public string spawnPointTag = "EnemySpawnPoint";

    // ������ ���������� ������
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("���������� ��������� EnemyPrefab!");
            return;
        }

        // ������� ��� ����� ������ �� ����
        Transform[] spawnPoints = FindSpawnPoints();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("��� ����� ������ ��� ������ � ������� �����!");
            return;
        }

        // ������� ������ � ������ ����� ������
        SpawnEnemies(spawnPoints);
    }

    // ����� ��� ������ ����� ������
    private Transform[] FindSpawnPoints()
    {
        List<Transform> spawnPoints = new List<Transform>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(spawnPointTag))
        {
            spawnPoints.Add(obj.transform);
        }

        return spawnPoints.ToArray();
    }

    // ����� ��� ������ ������
    private void SpawnEnemies(Transform[] spawnPoints)
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            // ������� ������ �����
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            if (enemyObject != null)
            {
                // �������� ��������� Enemy
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                if (enemy == null)
                {
                    Debug.LogError($"��������� Enemy �� ������ �� ������� {enemyObject.name}!");
                    continue; // ���������� ���� �����
                }

                // �������������� HealthSystem �����
                HealthSystem healthSystem = enemyObject.GetComponent<HealthSystem>();
                if (healthSystem != null)
                {
                    int enemyLevel = Random.Range(1, 10); // ���������� ��������� ������� �����
                    float enemyHealth = 100f + enemyLevel * 30f; // ��������� ������� ��� ��������
                    float enemyAttackPower = 10f + enemyLevel * 5f; // ��������� ������� ��� �����
                    float enemyDefense = 5f + enemyLevel * 2f; // ��������� ������� ��� ������

                    // �������������� HealthSystem � ���������� name = "Enemy"
                    healthSystem.InitializeHealth(
                        initialMaxHealth: enemyHealth,
                        initialCurrentHealth: enemyHealth,
                        initialAttackPower: enemyAttackPower,
                        initialDefense: enemyDefense,
                        initialLevel: enemyLevel,
                        name: "Enemy" // �������� ����������� ��� "Enemy"
                    );
                }
                else
                {
                    Debug.LogError("��������� HealthSystem �� ������!");
                }

                // �������������� �����
                try
                {
                    enemy.Initialize(1); // ������� ����� (����� ��������)
                    Debug.Log($"���� ������� ��������� � ����� {spawnPoint.position}!");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"������ ��� ������������� �����: {ex.Message}");
                }

                // ��������� ������ �� ����������� �����
                spawnedEnemies.Add(enemyObject);
            }
            else
            {
                Debug.LogError("�� ������� ������� EnemyPrefab!");
            }
        }
    }
}