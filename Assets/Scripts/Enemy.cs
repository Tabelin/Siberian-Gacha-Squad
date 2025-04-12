// Enemy.cs
using UnityEngine;
using System.Collections;
using UnityEngine.AI; // Для использования NavMeshAgent

public class Enemy : MonoBehaviour
{

    // Радиус патрулирования
    public float patrolRadius = 3f;
    private float moveSpeed = 4f;

    // Таймер между атаками
    private float attackCooldown = 2f; // Фиксированное время между атаками (в секундах)
    private float nextAttackTime = 0f; // Время следующей атаки

    
















    // Центральная точка патрулирования
    private Vector3 patrolCenter;

    // Текущая цель патрулирования
    private Vector3 currentPatrolTarget;

    // Компонент NavMeshAgent для поиска пути
    private NavMeshAgent navMeshAgent;

    // Булевые переменные для состояний
    public bool isPatrolling = false; // Патрулирование
    public bool isAttacking = false; // Атака
    

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
        if (isPatrolling && !isAttacking)
        {
            DetectCharacters(); // Проверяем наличие персонажей
            Patrol();
        }
        else if (isAttacking && attackTarget != null)
        {
            AttackLogic(); // Вызываем логику атаки
        }
        else if (isAttacking && attackTarget == null)
        {
            StopAttack();
            
            StartPatrolling();
        }
        else if(isPatrolling)
        {
            Patrol();
        }
        // Поворачиваем врага к камере
        AlignToCamera();
    }

    // Единый метод для логики атаки
    private void AttackLogic()
    {
        if (attackTarget == null || !attackTarget.activeInHierarchy)
        {
            // Если цель недоступна, останавливаем атаку
            StopAttack();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

        if (distanceToTarget > 2.5f)
        {
            // Если цель находится вне радиуса атаки, двигаемся к ней
            if (navMeshAgent != null)
            {
                navMeshAgent.SetDestination(attackTarget.transform.position);
            }
        }
        else
        {
            // Если достаточно близко, выполняем атаку с учетом таймера
            if (Time.time >= nextAttackTime)
            {
                PerformAttack(attackTarget);

                // Обновляем время следующей атаки
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    // Метод для начала патрулирования
    public void StartPatrolling()
    {
        ChangeState(() =>
        {
            isPatrolling = true;
            isAttacking = false;
            

            StartCoroutine(PatrolRoutine());
            Debug.Log("Враг начинает патрулирование!");
        });
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
            float damage = CalculateDamage(); // Вычисляем урон
            characterManager.healthSystem.TakeDamage(damage); // Отправляем урон в HealthSystem персонажа
            Debug.Log($"Враг атакует! Нанесено урона: {damage}");

            // Ждём перед следующей атакой
            StartCoroutine(WaitBeforeNextAttack(4f));
        }
    }

    // Корутина для паузы между атаками
    private IEnumerator WaitBeforeNextAttack(float waitTime)
    {
        yield return new WaitForSeconds(waitTime); // Пауза между атаками
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

            Quaternion targetRotation = Quaternion.Euler(-15f, angleY, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // Метод для автоматического обнаружения персонажей
    private void DetectCharacters()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character"); // Находим всех персонажей
        if (characters.Length > 0)
        {
            GameObject closestCharacter = FindClosestCharacter(characters); // Находим ближайшего персонажа
            if (closestCharacter != null)
            {
                float distanceToCharacter = Vector3.Distance(transform.position, closestCharacter.transform.position);

                if (distanceToCharacter <= 16f && !isAttacking) // Если персонаж в пределах видимости (10 единиц) и враг не атакует
                {
                    isPatrolling = false;
                    isAttacking = true;
                    

                    attackTarget = closestCharacter;
                    Debug.Log($"Враг начал атаковать: {closestCharacter.name}");
                }
            }
        }
    }

    // Метод для поиска ближайшего персонажа
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

    // Метод для начала атаки
    public void StartAttack(GameObject target)
    {
        ChangeState(() =>
        {
            isPatrolling = false;
            isAttacking = true;
           

            Debug.Log("Враг начинает атаковать!");
        });
    }

    // Метод для остановки атаки
    private void StopAttack()
    {
        isAttacking = false;
        attackTarget = null;

        if (isPatrolling)
        {
            StartPatrolling(); // После окончания атаки возобновляем патрулирование
        }
        
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

        return healthSystem.attackPower; // Урон равен силе атаки врага
    }
}