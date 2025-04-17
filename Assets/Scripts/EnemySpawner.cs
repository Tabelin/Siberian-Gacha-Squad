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

    // ���������� ������ ��� ������ � ������ �����
    public int spawnCountPerPoint = 1; // ��������� ����������� �������� ���������� ���������� ������

    // ����������� ������� ������
    public int minEnemyLevel = 1;

    // ������������ ������� ������
    public int maxEnemyLevel = 5;

    // ������������� ������������ ������� ��� ������ (�������� ���!)
    public int enemyMaxLevel = 10;

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
            for (int i = 0; i < spawnCountPerPoint; i++) // ������� ��������� ���������� ������ �� ������ �����
            {
                // ������� ������ �����
                GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                if (enemyObject != null)
                {
                    // �������������� HealthSystem �����
                    HealthSystem healthSystem = enemyObject.GetComponent<HealthSystem>();
                    if (healthSystem != null)
                    {
                        int enemyLevel = Random.Range(minEnemyLevel, maxEnemyLevel + 1); // ���������� ��������� ������� �����

                        // ��������� ����� � ����������� �� ������
                        float enemyHealth = 100f + enemyLevel * 50f;
                        float enemyAttackPower = 10f + enemyLevel * 5f;
                        float enemyDefense = 5f + enemyLevel * 2f;

                        // �������������� HealthSystem � ���������� name = "Enemy"
                        healthSystem.InitializeHealth(
                            initialMaxHealth: enemyHealth,
                            initialCurrentHealth: enemyHealth,
                            initialAttackPower: enemyAttackPower,
                            initialDefense: enemyDefense,
                            initialLevel: enemyLevel,
                            initialMaxLevel: enemyMaxLevel, 
                            name: "Enemy"
                        );

                        Debug.Log($"���� ������ {enemyLevel} ������: �������� - {enemyHealth}, ����� - {enemyAttackPower}, ������ - {enemyDefense}");
                    }
                    else
                    {
                        Debug.LogError("��������� HealthSystem �� ������!");
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

        Debug.Log($"����� �������� ������: {spawnedEnemies.Count}");
    }
}