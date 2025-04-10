// Enemy.cs
using UnityEngine;
using System.Collections;
using UnityEngine.AI; // Для использования NavMeshAgent

public class Enemy : MonoBehaviour
{

    // Радиус патрулирования
    public float patrolRadius = 2f;
    private float moveSpeed = 2f;
    // Центральная точка патрулирования
    private Vector3 patrolCenter;

    // Текущая цель патрулирования
    private Vector3 currentPatrolTarget;

    // Компонент NavMeshAgent для поиска пути
    private NavMeshAgent navMeshAgent;

    // Булевые переменные для состояний
    public bool isPatrolling = false; // Патрулирование
    public bool isAttacking = false; // Атака
    public bool isIdle = true;      // Бездействие

    // Базовые параметры врага
    public int level = 1;          // Уровень врага
    public float baseHealth = 100f; // Базовое здоровье
    public float baseAttackPower = 40f; // Базовая сила атаки
    public float baseDefense = 5f;     // Базовая защита
    public float baseSpeed = 3f;       // Базовая скорость
    public float maxHealth;

    // Атакуемый объект
    private GameObject attackTarget;

    // Логика здоровья
    public HealthSystem healthSystem;
    
    void Start()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                Debug.LogError("Компонент NavMeshAgent не найден!");
                return;
            }
        }

        // Устанавливаем начальную точку патрулирования
        patrolCenter = transform.position;

        // Начинаем патрулирование
        StartPatrolling();

        // Находим компонент HealthSystem
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogError($"Компонент HealthSystem не найден на объекте {name}! Враг не сможет использовать здоровье.");
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
        // Обнаруживаем персонажей для атаки
        DetectCharacters();
        // Поворачиваем врага к камере
        AlignToCamera();
    }

    // Метод для начала патрулирования
    public void StartPatrolling()
    {
        ChangeState(() =>
        {
            isPatrolling = true;
            isAttacking = false;
            isIdle = false;

            StartCoroutine(PatrolRoutine());
            Debug.Log("Враг начинает патрулирование!");
        });
    }

    // Метод для атаки цели
    private void AttackTarget(GameObject target)
    {
        if (target == null || !target.activeInHierarchy)
        {
            // Если цель недоступна, останавливаем атаку
            StopAttack();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget > 2f)
        {
            // Если цель находится вне радиуса атаки, двигаемся к ней
            navMeshAgent.SetDestination(target.transform.position);
        }
        else
        {
            // Если достаточно близко, выполняем атаку
            PerformAttack(target);
        }
    }

    // Корутина для патрулирования
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (isPatrolling)
            {
                SelectNewPatrolPoint();

                yield return new WaitWhile(() => navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance);

                yield return new WaitForSeconds(1f); // Пауза перед выбором новой точки
            }
            else
            {
                yield return null;
            }
        }
    }

    // Метод для выбора новой точки патрулирования
    private void SelectNewPatrolPoint()
    {
        currentPatrolTarget = patrolCenter + Random.insideUnitSphere * patrolRadius;
        currentPatrolTarget.y = patrolCenter.y; // Ограничиваем движение только по XZ

        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(currentPatrolTarget); // Устанавливаем новую цель
        }
    }

    // Метод для патрулирования
    private void Patrol()
    {
        if (navMeshAgent != null && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            navMeshAgent.speed = moveSpeed; // Задаем скорость движения
        }
    }


    // Метод для выполнения атаки
    private void PerformAttack(GameObject target)
    {
        CharacterManager characterManager = target.GetComponent<CharacterManager>();
        if (characterManager != null && characterManager.healthSystem != null && characterManager.healthSystem.isAlive)
        {
            float damage = baseAttackPower + level * 5f; // Урон в зависимости от уровня
            characterManager.TakeDamage(damage); // Отправляем урон персонажу
            Debug.Log($"Враг атакует! Нанесено урона: {damage}");
        }
    }

    // Метод для бездействия
    private void IdleControl()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true; // Останавливаем движение
        }
    }

    // Метод для безопасной смены состояния
    private void ChangeState(System.Action action)
    {
        if (isAttacking)
        {
            Debug.LogWarning("Враг занят атакой. Новое состояние будет применено после завершения атаки.");
        }

        action?.Invoke();
    }

    // Метод для поворота врага к камере
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

    // Метод для обнаружения персонажей
    private void DetectCharacters()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject character in characters)
        {
            CharacterManager characterManager = character.GetComponent<CharacterManager>();
            if (characterManager != null && characterManager.healthSystem != null && characterManager.healthSystem.isAlive)
            {
                float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position);
                if (distanceToCharacter < 10f) // Если персонаж находится в пределах видимости
                {
                    StartAttack(character); // Начинаем атаку
                    break;
                }
            }
        }
    }

    // Метод для начала атаки
    public void StartAttack(GameObject target)
    {
        ChangeState(() =>
        {
            isPatrolling = false;
            isAttacking = true;
            isIdle = false;

            Debug.Log("Враг начинает атаковать!");
        });
    }

    // Метод для остановки атаки
    private void StopAttack()
    {
        attackTarget = null;
        StartPatrolling(); // После окончания атаки возобновляем патрулирование
    }

    // Метод для остановки всех действий
    private void StopAllActions()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true; // Останавливаем движение
        }

        isPatrolling = false;
        isAttacking = false;
        isIdle = true;
    }
    // Метод для инициализации параметров врага
    public void Initialize(int enemyLevel)
    {
        if (healthSystem == null)
        {
            Debug.LogError("Компонент HealthSystem не найден! Невозможно инициализировать врага.");
            return;
        }

        level = enemyLevel;

        // Настройка здоровья
        maxHealth = CalculateHealthBasedOnLevel();
        healthSystem.maxHealth = maxHealth;
        healthSystem.currentHealth = maxHealth;

        // Настройка скорости
        moveSpeed = CalculateSpeedBasedOnLevel();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed; // Применяем скорость к NavMeshAgent
        }

        Debug.Log($"Враг уровня {level} создан: Здоровье - {maxHealth}, Скорость - {moveSpeed}");
    }

    // Расчет здоровья в зависимости от уровня
    private float CalculateHealthBasedOnLevel()
    {
        return baseHealth + level * 50f; // Примерная формула: базовое здоровье + бонус за каждый уровень
    }

    // Расчет скорости в зависимости от уровня
    private float CalculateSpeedBasedOnLevel()
    {
        return baseSpeed + level * 0.1f; // Примерная формула: базовая скорость + бонус за каждый уровень
    }
    // Расчет урона на основе статов врага
    private float CalculateDamage()
    {
        if (healthSystem == null)
        {
            Debug.LogError("Компонент HealthSystem не найден!");
            return 0f;
        }

        // Используем атаку из HealthSystem
        return healthSystem.attackPower; // Урон равен силе атаки врага
    }
}