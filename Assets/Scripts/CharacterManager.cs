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

    // Радиус ближней атаки
    private float meleeAttackRange = 2f;  // Радиус дальней атаки
    private float rangedAttackRange = 15f; // Время задержки между атаками
    public float detectionRadius = 20f; // Радиус обнаружения
    public float gatheringRange = 3f;

    public float meleeAttackCooldown = 1f; // Задержка для ближней атаки
    public float rangedAttackCooldown = 2f; // Задержка для дальней атаки
    private float attackTimer = 0f;
    private float currentAttackCooldown = 0f;
    public float maxStuckTime = 5f; // Через сколько времени считаем, что персонаж застрял
    public float stuckTimer = 0f;
    public float gatherTimer = 0f;
    public float gatherCooldown = 2f; // Время между сбором

    float experiencePerResource = 10f;
    float gatherAmountPerAction = 1f;

    private Rigidbody rb;
    // Центральная точка патрулирования
    private Vector3 patrolCenter;


    private Vector3 currentPatrolTarget;    // Текущая цель патрулирования
    private Vector3 lastPosition;           // Для определения реального перемещения
    private Vector3 moveTarget;             // Целевая точка для движения (приоритетная команда игрока)


    public bool isPatrolling = true; // Патрулирование
    public bool isAttacking = false; // Атака
    public bool isGathering = false; // Добыча ресурсов
    public bool isIdle = false; // Бездействие
    public bool isControlledByPlayer = false; // Флаг управления игроком

    // Корутина патрулирования
    private Coroutine patrolCoroutine;

    // Ближайший подбираемый предмет
    private GameObject nearestItem;
    private GameObject targetResource; // Цель добычи
    // Начальная точка спавна
    private Transform spawnPoint;
    // LayerMask для врагов
    public LayerMask enemyLayerMask;

    // Компонент NavMeshAgent для поиска пути
    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    // Компонент HealthSystem для здоровья персонажа
    public HealthSystem healthSystem;
    private CharacterInventory inventory;
    // Атакуемый объект
    private GameObject attackTarget;

    void Start()
    {

        inventory = GetComponent<CharacterInventory>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<CharacterInventory>();
        }
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
        // navMeshAgent.speed = moveSpeed;
        // navMeshAgent.stoppingDistance = 0.5f; // Расстояние до цели, на котором персонаж останавливается
        // Устанавливаем начальную точку патрулирования
        patrolCenter = transform.position;
        // Начинаем патрулирование
        StartPatrolling();

        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isGathering && targetResource != null)
        {

            GatheringLogic();
            isControlledByPlayer = false;
            return;
        }
        else
        {
            // 🚶‍♂️ Продолжаем движение к цели, даже если под управлением
            MoveToTarget(moveTarget);
        }

        if (moveTarget != Vector3.zero)
        {
            float distanceToTarget = Vector3.Distance(transform.position, moveTarget);

            

            if (distanceToTarget < 4f)
            {

                // Цель достигнута → сбрасываем флаг
                moveTarget = Vector3.zero;
                isControlledByPlayer = false;
                StartPatrolling();
                return;
            }
            else
            {
                // 🚶‍♂️ Продолжаем движение к цели, даже если под управлением
                MoveToTarget(moveTarget);
            }

        }

        if (isControlledByPlayer && moveTarget != Vector3.zero)
        {
            float distanceToTarget = Vector3.Distance(transform.position, moveTarget);
            // Проверяем, действительно ли мы двигаемся к цели
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < 0.02f) // Перемещение почти нулевое
            {
                stuckTimer += Time.deltaTime;

                if (stuckTimer >= maxStuckTime)
                {
 
                    isControlledByPlayer = false;
                    moveTarget = Vector3.zero;
                    stuckTimer = 0f;
                    StartPatrolling();
                }
            }
            else
            {
                stuckTimer = 0f; // Сброс, если реально двигаемся
            }

            lastPosition = transform.position;
        }

        if (isControlledByPlayer)
        {
            return; // Если персонаж под контролем игрока → не запускаем автоматические действия
        }

        // 🚨 Сначала проверяем, есть ли враги в радиусе
        if (!isAttacking) // Не проверяем, если уже в режиме атаки
        {
            DetectEnemies(); // Обнаруживает врагов и может установить новую атаку

            if (attackTarget != null)
            {
                isAttacking = true;
                isPatrolling = false;
                moveTarget = Vector3.zero; // 🧨 Сбрасываем moveTarget, если появился враг
                return; // Прерываем остальные действия
            }
        }
        if (isAttacking && attackTarget != null)
        {
            if (isControlledByPlayer)
            {
                return; // vv
            }

            // Проверяем, находится ли цель в detectionRadius
            if (attackTarget.activeInHierarchy)
            {
                float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

                // Если цель вышла за detectionRadius → останавливаем атаку
                if (distanceToTarget > detectionRadius)
                {
                    StopAttack();
                    
                }
                else if (distanceToTarget > rangedAttackRange)
                {
                    // Если цель вне дальней атаки → двигаемся к ней
                    MoveToTarget(attackTarget.transform.position);
                    return; // Прерываем атаку, если цель слишком далеко
                }
            }
            else
            {
                StopAttack();
                return;
            }
            
            // Атака, если цель в зоне
            AttackLogic();
            DetectEnemies(); // Обнаружение врагов
        }
        // Если цель недоступна, но атака всё ещё активна
        else if (isAttacking && attackTarget == null)
        {
          
            StopAttack();
            return;
        }
        else if (isIdle)
        {
            IdleControl();
        }
        else if (isPatrolling)
        {
            DetectEnemies(); // Обнаружение врагов в патруле
            Patrol(); // Патрулирование
        }
    }


    public void SetControlledByPlayer(bool controlled)
    {
        isControlledByPlayer = controlled;
    }

    private void GatheringLogic()
    {
        if (isControlledByPlayer)
        {
            return; // нееет копать с контролем
        }

        if (targetResource == null || !targetResource.activeInHierarchy)
        {
            StopGathering();
            StartPatrolling();
            return;
        }

        float distanceToResource = Vector3.Distance(transform.position, targetResource.transform.position);

        if (distanceToResource > gatheringRange)
        {
            MoveToTarget(targetResource.transform.position);
            return;
        }
        else
        {
            navMeshAgent.SetDestination(transform.position); // Останавливаемся
        }

        gatherTimer += Time.deltaTime;

        if (gatherTimer >= gatherCooldown)
        {
            Resource resourceScript = targetResource.GetComponent<Resource>();

            
                float remainingSpace = inventory.GetRemainingSpace();
                float possibleGather = Mathf.Min(remainingSpace / resourceScript.weightPerUnit, gatherAmountPerAction);

                if (possibleGather <= 0)
                {
                    Debug.Log("Инвентарь полон!");
                    StopGathering();
                    StartPatrolling();
                    return;
                }

                float gathered = resourceScript.Gather(possibleGather);
                if (gathered > 0)
                {
                    Debug.Log("ресурс!");
                    inventory.AddResource(resourceScript.resourceType, gathered);
                    float expReward = gathered * experiencePerResource;
                    healthSystem.GainExperience(expReward); // Обычный GainExperience
                    healthSystem.ShowExperiencePopup(expReward); // ✅ Вызываем из HealthSystem
                }


                if (resourceScript.isDepleted)
                {
                    Debug.Log("Ресурс исчерпан");
                    StopGathering();
                    StartPatrolling();
                }

                gatherTimer = 0f;
            
        }
    }


    private void StopGathering()
    {
        isGathering = false;
        targetResource = null;
        gatherTimer = 0f;
    }

    // Метод для автоматического обнаружения врагов
    private void DetectEnemies()
    {
        if (isControlledByPlayer)
        {
            return; // Не обнаруживаем врагов, если персонаж под контролем игрока
        }


        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayerMask);

        if (enemies.Length == 0)
        {
            if (attackTarget != null)
            {
                StopAttack();
            }
            return; // Нет врагов в радиусе
        }

        GameObject closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = enemy.gameObject;
            }
        }

        if (closestEnemy != null)
        {
            

            attackTarget = closestEnemy;
            isAttacking = true;
            isPatrolling = false;

            // Если цель в detectionRadius, но вне rangedAttackRange → двигаемся к ней
            float distanceToEnemy = shortestDistance;

            if (distanceToEnemy <= detectionRadius)
            {
                isAttacking = true;

                if (distanceToEnemy > rangedAttackRange)
                {
                    MoveToTarget(attackTarget.transform.position); // Двигаемся к врагу
                }
                else
                {
                    navMeshAgent.SetDestination(transform.position); // Останавливаемся
                }
            }
        }
    }


    // Объединенная логика атаки
    private void AttackLogic()
    {
        if (isControlledByPlayer)
        {
            return; // нееет атак с контролем
        }


        if (attackTarget == null || !attackTarget.activeInHierarchy)
        {
            StopAttack(); // Если цель недоступна, останавливаем атаку
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

        // Если цель вышла за радиус обнаружения
        if (distanceToTarget > detectionRadius)
        {
            StopAttack();
            return;
        }

        // Если враг вне дальней атаки → двигаемся к нему
        if (distanceToTarget > rangedAttackRange)
        {
            MoveToTarget(attackTarget.transform.position);
            attackTimer = 0f;
            return;
        }

        // Останавливаемся, если цель в зоне атаки
        navMeshAgent.SetDestination(transform.position);

        // Определяем текущий тип атаки
        if (distanceToTarget <= meleeAttackRange)
        {
            currentAttackCooldown = meleeAttackCooldown;
        }
        else if (distanceToTarget <= rangedAttackRange)
        {
            currentAttackCooldown = rangedAttackCooldown;
        }
        else 
        {
            return;
        }

        // Обновляем таймер для атаки
        attackTimer += Time.deltaTime;

        // Выполняем атаку
        if (attackTimer >= currentAttackCooldown)
        {
            PerformAttack(attackTarget, distanceToTarget);
            attackTimer = 0f;
        }
    }

    // Метод для начала атаки
    public void StartAttack(GameObject target)
    {
        ChangeState(() =>
        {
            StopAllActions();

            attackTarget = target;
            isPatrolling = false;
            isAttacking = true;
            isGathering = false;
            isIdle = false;

            attackTimer = 0f; // Сбрасываем таймер для атаки
            Debug.Log($"Персонаж начал атаку: {target.name}");
        });
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
            isControlledByPlayer = false;

            SelectNewPatrolPoint(); // Выбираем первую точку патрулирования
            patrolCoroutine = StartCoroutine(PatrolRoutine());
            Debug.Log("Персонаж начинает патрулирование!");
        }
    }

    // Метод для выполнения атаки
    private void PerformAttack(GameObject target, float distanceToTarget)
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null && enemy.healthSystem != null && enemy.healthSystem.isAlive)
        {
            float damage;

            if (distanceToTarget <= meleeAttackRange)
            {
                damage = CalculateDamage(); // Ближняя атака
                Debug.Log($"Ближняя атака! Нанесено урона: {damage:F2}");
            }
            else if (distanceToTarget <= rangedAttackRange)
            {
                damage = CalculateRangedDamage();
                Debug.Log($"Дальнюю атака! Нанесено урона: {damage:F2}");
            }
            else
            {
                // Если цель слишком далеко, продолжаем движение
                return;
            }

            enemy.healthSystem.TakeDamage(damage); // Отправляем урон врагу
        }
    }


    // Расчет урона от ближней атаки
    private float CalculateDamage()
    {
        if (healthSystem == null)
        {
            Debug.LogError("Компонент HealthSystem не найден!");
            return 0f;
        }

        return healthSystem.attackPower * 1.5f; // Урон равен силе атаки персонажа
    }

    // Расчет урона от дальней атаки
    private float CalculateRangedDamage()
    {
        if (healthSystem == null)
        {
            Debug.LogError("Компонент HealthSystem не найден!");
            return 0f;
        }

        // Добавляем бонус или штраф к дальней атаке
        return healthSystem.attackPower * 1f; // Пример: дальняя атака слабее
    }


    // Метод для остановки ближней атаки
    private void StopAttack()
    {
        isAttacking = false;
        attackTarget = null;
        attackTimer = 0f; // Сбрасываем таймер для атаки
        moveTarget = Vector3.zero; // 🚫 Сбрасываем moveTarget
        isControlledByPlayer = false;
        if (!isPatrolling && !isGathering && !isIdle)
        {
            StartPatrolling(); // Если других действий нет, возобновляем патрулирование
        }
    }

    public void StartGathering(GameObject resource)
    {
        ChangeState(() =>
        {
            StopAllActions();

            targetResource = resource;
            isGathering = true;
            isPatrolling = false;
            isAttacking = false;
            isIdle = false;

            isControlledByPlayer = false; //xm

            Debug.Log($"Персонаж начал добывать {resource.name}");
        });
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

    // Метод для безопасной смены состояния
    private void ChangeState(System.Action action)
    {
        if (isAttacking || isGathering)
        {
            Debug.LogWarning("Персонаж занят атакой. Новое состояние будет применено после завершения атаки.");
        }

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
            //isControlledByPlayer = false;
            // Устанавливаем цель для NavMeshAgent
            if (navMeshAgent != null)
            {
                navMeshAgent.SetDestination(targetPosition); // Устанавливаем точку назначения
                navMeshAgent.speed = moveSpeed; // Задаем скорость движения
                navMeshAgent.isStopped = false; // Разрешаем движение
                patrolCenter = targetPosition;
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