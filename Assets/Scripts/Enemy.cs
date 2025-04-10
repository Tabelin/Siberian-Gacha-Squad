// Enemy.cs
using UnityEngine;
using System.Collections;
using UnityEngine.AI; // ��� ������������� NavMeshAgent

public class Enemy : MonoBehaviour
{

    // ������ ��������������
    public float patrolRadius = 2f;
    private float moveSpeed = 2f;
    // ����������� ����� ��������������
    private Vector3 patrolCenter;

    // ������� ���� ��������������
    private Vector3 currentPatrolTarget;

    // ��������� NavMeshAgent ��� ������ ����
    private NavMeshAgent navMeshAgent;

    // ������� ���������� ��� ���������
    public bool isPatrolling = false; // ��������������
    public bool isAttacking = false; // �����
    public bool isIdle = true;      // �����������

    // ������� ��������� �����
    public int level = 1;          // ������� �����
    public float baseHealth = 100f; // ������� ��������
    public float baseAttackPower = 40f; // ������� ���� �����
    public float baseDefense = 5f;     // ������� ������
    public float baseSpeed = 3f;       // ������� ��������
    public float maxHealth;

    // ��������� ������
    private GameObject attackTarget;

    // ������ ��������
    public HealthSystem healthSystem;
    
    void Start()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                Debug.LogError("��������� NavMeshAgent �� ������!");
                return;
            }
        }

        // ������������� ��������� ����� ��������������
        patrolCenter = transform.position;

        // �������� ��������������
        StartPatrolling();

        // ������� ��������� HealthSystem
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogError($"��������� HealthSystem �� ������ �� ������� {name}! ���� �� ������ ������������ ��������.");
        }
    }

    void Update()
    {
        if (isPatrolling)
        {
            Patrol();
        }
        else if (isAttacking && attackTarget != null)
        {
            AttackTarget(attackTarget);
        }
        else if (isIdle)
        {
            IdleControl();
        }
        // ������������ ���������� ��� �����
        DetectCharacters();
        // ������������ ����� � ������
        AlignToCamera();
    }

    // ����� ��� ������ ��������������
    public void StartPatrolling()
    {
        ChangeState(() =>
        {
            isPatrolling = true;
            isAttacking = false;
            isIdle = false;

            StartCoroutine(PatrolRoutine());
            Debug.Log("���� �������� ��������������!");
        });
    }

    // ����� ��� ����� ����
    private void AttackTarget(GameObject target)
    {
        if (target == null || !target.activeInHierarchy)
        {
            // ���� ���� ����������, ������������� �����
            StopAttack();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget > 2f)
        {
            // ���� ���� ��������� ��� ������� �����, ��������� � ���
            navMeshAgent.SetDestination(target.transform.position);
        }
        else
        {
            // ���� ���������� ������, ��������� �����
            PerformAttack(target);
        }
    }

    // �������� ��� ��������������
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (isPatrolling)
            {
                SelectNewPatrolPoint();

                yield return new WaitWhile(() => navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance);

                yield return new WaitForSeconds(1f); // ����� ����� ������� ����� �����
            }
            else
            {
                yield return null;
            }
        }
    }

    // ����� ��� ������ ����� ����� ��������������
    private void SelectNewPatrolPoint()
    {
        currentPatrolTarget = patrolCenter + Random.insideUnitSphere * patrolRadius;
        currentPatrolTarget.y = patrolCenter.y; // ������������ �������� ������ �� XZ

        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(currentPatrolTarget); // ������������� ����� ����
        }
    }

    // ����� ��� ��������������
    private void Patrol()
    {
        if (navMeshAgent != null && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            navMeshAgent.speed = moveSpeed; // ������ �������� ��������
        }
    }


    // ����� ��� ���������� �����
    private void PerformAttack(GameObject target)
    {
        CharacterManager characterManager = target.GetComponent<CharacterManager>();
        if (characterManager != null && characterManager.healthSystem != null && characterManager.healthSystem.isAlive)
        {
            float damage = baseAttackPower + level * 5f; // ���� � ����������� �� ������
            characterManager.TakeDamage(damage); // ���������� ���� ���������
            Debug.Log($"���� �������! �������� �����: {damage}");
        }
    }

    // ����� ��� �����������
    private void IdleControl()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true; // ������������� ��������
        }
    }

    // ����� ��� ���������� ����� ���������
    private void ChangeState(System.Action action)
    {
        if (isAttacking)
        {
            Debug.LogWarning("���� ����� ������. ����� ��������� ����� ��������� ����� ���������� �����.");
        }

        action?.Invoke();
    }

    // ����� ��� �������� ����� � ������
    private void AlignToCamera()
    {
        if (Camera.main != null)
        {
            Vector3 direction = (Camera.main.transform.position - transform.position).normalized;
            float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0f, angleY, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // ����� ��� ����������� ����������
    private void DetectCharacters()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject character in characters)
        {
            CharacterManager characterManager = character.GetComponent<CharacterManager>();
            if (characterManager != null && characterManager.healthSystem != null && characterManager.healthSystem.isAlive)
            {
                float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position);
                if (distanceToCharacter < 10f) // ���� �������� ��������� � �������� ���������
                {
                    StartAttack(character); // �������� �����
                    break;
                }
            }
        }
    }

    // ����� ��� ������ �����
    public void StartAttack(GameObject target)
    {
        ChangeState(() =>
        {
            isPatrolling = false;
            isAttacking = true;
            isIdle = false;

            Debug.Log("���� �������� ���������!");
        });
    }

    // ����� ��� ��������� �����
    private void StopAttack()
    {
        attackTarget = null;
        StartPatrolling(); // ����� ��������� ����� ������������ ��������������
    }

    // ����� ��� ��������� ���� ��������
    private void StopAllActions()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true; // ������������� ��������
        }

        isPatrolling = false;
        isAttacking = false;
        isIdle = true;
    }
    // ����� ��� ������������� ���������� �����
    public void Initialize(int enemyLevel)
    {
        if (healthSystem == null)
        {
            Debug.LogError("��������� HealthSystem �� ������! ���������� ���������������� �����.");
            return;
        }

        level = enemyLevel;

        // ��������� ��������
        maxHealth = CalculateHealthBasedOnLevel();
        healthSystem.maxHealth = maxHealth;
        healthSystem.currentHealth = maxHealth;

        // ��������� ��������
        moveSpeed = CalculateSpeedBasedOnLevel();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed; // ��������� �������� � NavMeshAgent
        }

        Debug.Log($"���� ������ {level} ������: �������� - {maxHealth}, �������� - {moveSpeed}");
    }

    // ������ �������� � ����������� �� ������
    private float CalculateHealthBasedOnLevel()
    {
        return baseHealth + level * 50f; // ��������� �������: ������� �������� + ����� �� ������ �������
    }

    // ������ �������� � ����������� �� ������
    private float CalculateSpeedBasedOnLevel()
    {
        return baseSpeed + level * 0.1f; // ��������� �������: ������� �������� + ����� �� ������ �������
    }
    // ������ ����� �� ������ ������ �����
    private float CalculateDamage()
    {
        if (healthSystem == null)
        {
            Debug.LogError("��������� HealthSystem �� ������!");
            return 0f;
        }

        // ���������� ����� �� HealthSystem
        return healthSystem.attackPower; // ���� ����� ���� ����� �����
    }
}