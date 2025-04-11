// CharacterManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : MonoBehaviour
{
    // Скорость движения персонажа
    public float moveSpeed = 3f;
    // Радиус патрулирования
    public float patrolRadius = 2f;

    // Таймер между атаками
    private float attackCooldown = 2f; // Фиксированное время между атаками (в секундах)
    private float nextAttackTime = 0f; // Время следующей атаки

    // Центральная точка патрулирования
    private Vector3 patrolCenter;

    // Текущая цель патрулирования
    private Vector3 currentPatrolTarget;

    // Целевая точка для движения (приоритетная команда игрока)
    private Vector3 moveTarget;

    // Булевые переменные для состояний
    public bool isPatrolling = true; // Патрулирование
    public bool isAttacking = false; // Атака
    public bool isGathering = false; // Добыча ресурсов
    public bool isIdle = false; // Бездействие

    // Корутина патрулирования
    private Coroutine patrolCoroutine;

    // Ближайший подбираемый предмет
    private GameObject nearestItem;

    // Начальная точка спавна
    private Transform spawnPoint;

    // Компонент NavMeshAgent для поиска пути
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    // Компонент HealthSystem для здоровья персонажа
    public HealthSystem healthSystem;

    // Атакуемый объект
    private GameObject attackTarget;

    void Start()
    {
        // Устанавливаем начальную точку патрулирования
        patrolCenter = transform.position;

        // Получаем компонент NavMeshAgent
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("Компонент NavMeshAgent не найден!");
            return;
        }
        // Инициализируем систему здоровья
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogError("Компонент HealthSystem не найден!");
            return;
        }

        // Настройка параметров NavMeshAgent
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = 0.5f; // Расстояние до цели, на котором персонаж останавливается

        // Начинаем патрулирование
        StartPatrolling();
    }

    void Update()
    {
        if (isPatrolling && !isAttacking)
        {
            DetectEnemies(); // Проверяем наличие врагов
            Patrol();
        }
        else if (isAttacking)
        {
            AttackLogic(); // Вызываем единую логику атаки
        }
        else if (moveTarget != Vector3.zero)
        {
            MoveToTarget(moveTarget);

            // Если достигли цели с учетом радиуса, начинаем патрулирование
            if (Vector3.Distance(transform.position, moveTarget) < 2f)
            {
                moveTarget = Vector3.zero; // Сбрасываем целевую точку

                // Начинаем патрулирование
                StartPatrolling();
                Patrol();
            }
        }
        else if (isIdle)
        {
            IdleControl();
            StartPatrolling();
        }
    }

    // Метод для автоматического обнаружения врагов
    private void DetectEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Находим всех врагов
        if (enemies.Length > 0)
        {
            GameObject closestEnemy = FindClosestEnemy(enemies); // Находим ближайшего врага
            if (closestEnemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, closestEnemy.transform.position);

                if (distanceToEnemy <= 15f && !isAttacking) // Если враг в пределах видимости (10 единиц) и персонаж не атакует
                {
                    isPatrolling = false;
                    isAttacking = true;
                    isGathering = false;
                    isIdle = false;

                    attackTarget = closestEnemy;
                    Debug.Log($"Персонаж начал атаковать: {closestEnemy.name}");
                }
            }
        }
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

            // Если цель уничтожена, останавливаем атаку
            if (!attackTarget.activeInHierarchy)
            {
                StopAttack();
            }
        }
    }
    // Метод для поиска ближайшего врага
    private GameObject FindClosestEnemy(GameObject[] enemies)
    {
        GameObject closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }


    // Метод для начала патрулирования
    public void StartPatrolling()
    {
      
        {
            StopAllActions();

            isPatrolling = true;
            isAttacking = false;
            isGathering = false;
            isIdle = false;

            SelectNewPatrolPoint(); // Выбираем первую точку патрулирования
            patrolCoroutine = StartCoroutine(PatrolRoutine());
            Debug.Log("Персонаж начинает патрулирование!");
        }
    }

    // Метод для начала атаки
    public void StartAttack(GameObject target)
    {
        
        {
            StopAllActions();

            attackTarget = target;
            isPatrolling = false;
            isAttacking = true;
            isGathering = false;
            isIdle = false;

            Debug.Log($"Персонаж атакует: {target.name}");

            PerformAttack(target);
        }
    }


    // Метод для выполнения атаки
    private void PerformAttack(GameObject target)
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null && enemy.healthSystem != null && enemy.healthSystem.isAlive)
        {
            float damage = CalculateDamage(); // Вычисляем урон
            enemy.healthSystem.TakeDamage(damage); // Отправляем урон врагу
            Debug.Log($"Персонаж атакует! Нанесено урона: {damage:F2}");

            // Ждём перед следующей атакой
            StartCoroutine(WaitBeforeNextAttack(3f));
        }
    }
    // Корутина для паузы между атаками
    private IEnumerator WaitBeforeNextAttack(float waitTime)
    {
        yield return new WaitForSeconds(30000000f); // Пауза между атаками
    }

    // Расчет урона на основе статов персонажа
    private float CalculateDamage()
    {
        if (healthSystem == null)
        {
            Debug.LogError("Компонент HealthSystem не найден!");
            return 0f;
        }

        return healthSystem.attackPower; // Урон равен силе атаки персонажа
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
        else if (isIdle)
        {
            IdleControl(); // Если было бездействие, продолжаем его
        }
    }
    // Метод для начала добычи ресурсов
    public void StartGathering()
    {
        StopAllActions();

        isPatrolling = false;
        isAttacking = false;
        isGathering = true;
        isIdle = false;

        Debug.Log("Персонаж начинает добывать ресурсы!");
    }


    // Метод для взятия контроля над персонажем
    public void TakeControl()
    {
        
        {
            StopAllActions();

            isPatrolling = false;
            isAttacking = false;
            isGathering = false;
            isIdle = true;

            Debug.Log("Вы получили контроль над персонажем!");
        }
    }

    // Метод для безопасной смены действия
    private void ChangeAction(System.Action action)
    {
        if (isAttacking || isGathering)
        {
            Debug.LogWarning("Персонаж занят важным действием. Вы можете изменить задачу, но текущее действие не прервётся.");
        }

        // Выполняем новое действие
        action?.Invoke();
    }

    // Метод для остановки всех действий
    private void StopAllActions()
    {
        StopPatrol();
        isAttacking = false;
        isGathering = false;

    }

    // Метод для остановки патрулирования
    private void StopPatrol()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true; // Останавливаем движение NavMeshAgent
        }

        isPatrolling = false;
    }

    // Метод для патрулирования
    private void Patrol()
    {
        if (navMeshAgent != null && Vector3.Distance(transform.position, currentPatrolTarget) > 1f)
        {
            navMeshAgent.SetDestination(currentPatrolTarget); // Устанавливаем следующую цель патрулирования
        }
    }

    // Корутина для патрулирования
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (isPatrolling && moveTarget == Vector3.zero)
            {
                // Ждём, пока персонаж не дойдет до цели
                yield return new WaitWhile(() => navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance);

                // Добавляем паузу перед выбором новой точки
                yield return new WaitForSeconds(1f);

                // Выбираем новую точку патрулирования
                SelectNewPatrolPoint();
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
        // Находим случайную точку вокруг центра патрулирования
        Vector3 randomPoint = patrolCenter + Random.insideUnitSphere * patrolRadius;
        randomPoint.y = patrolCenter.y; // Ограничиваем движение только по XZ

        // Используем NavMesh для получения ближайшей проходимой точки
        UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas);
        currentPatrolTarget = hit.position;

        // Устанавливаем цель для NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(currentPatrolTarget); // Устанавливаем новую цель
            navMeshAgent.isStopped = false; // Разрешаем движение
        }
    }
    // Метод для добычи ресурсов
    private void Gather()
    {
        // Здесь можно добавить логику добычи ресурсов (например, взаимодействие с объектом)
        Debug.Log("Персонаж добывает ресурсы!");
    }

    // Метод для бездействия и управления игроком
    private void IdleControl()
    {
        // Обработка управления игрока (WASD)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            if (navMeshAgent != null)
            {
                Vector3 movement = (transform.right * horizontal + transform.forward * vertical).normalized;
                navMeshAgent.SetDestination(transform.position + movement); // Устанавливаем новое направление
            }
        }
    }
    // Метод для движения к целевой точке с использованием NavMeshAgent
    public void MoveToTarget(Vector3 targetPosition)
    {
        if (targetPosition != Vector3.zero)
        {
            moveTarget = targetPosition;

            // Останавливаем текущее патрулирование
            StopPatrol();

            // Устанавливаем цель для NavMeshAgent
            if (navMeshAgent != null)
            {
                navMeshAgent.SetDestination(targetPosition); // Устанавливаем точку назначения
                navMeshAgent.speed = moveSpeed; // Задаем скорость движения
                navMeshAgent.isStopped = false; // Разрешаем движение
                Debug.Log($"Персонаж движется к точке: {targetPosition}");
            }
        }
    }
    // Метод для автоподбора предметов
    private IEnumerator FindNearestItem()
    {
        while (true)
        {
            if (isPatrolling || isIdle) // Подбираем предметы только в режиме патрулирования или бездействия
            {
                GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

                if (items.Length > 0)
                {
                    nearestItem = FindClosestItem(items);

                    if (nearestItem != null)
                    {
                        MoveToItem(nearestItem.transform.position);
                        PickUpItem(nearestItem);
                    }
                }
            }

            yield return new WaitForSeconds(2f); // Проверяем каждые 2 секунды
        }
    }

    // Метод для нахождения ближайшего предмета
    private GameObject FindClosestItem(GameObject[] items)
    {
        GameObject closestItem = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject item in items)
        {
            float distance = Vector3.Distance(transform.position, item.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }

    // Метод для движения к предмету
    private void MoveToItem(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    // Метод для подбора предмета
    private void PickUpItem(GameObject item)
    {
        if (item == null)
        {
            Debug.LogError("Ближайший предмет является null!");
            return;
        }

        float distanceToItem = Vector3.Distance(transform.position, item.transform.position);
        if (distanceToItem < 1f) // Если достаточно близко
        {
            Destroy(item); // Уничтожаем предмет
            Debug.Log("Предмет подобран!");
        }
    }
}