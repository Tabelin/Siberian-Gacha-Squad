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

    // Центральная точка патрулирования
    private Vector3 patrolCenter;

    // Текущая цель патрулирования
    private Vector3 currentPatrolTarget;

    // Целевая точка для движения (приоритетная команда игрока)
    private Vector3 moveTarget;

    // Булевые переменные для состояний
    public bool isPatrolling = false; // Патрулирование
    public bool isAttacking = false; // Атака
    public bool isGathering = false; // Добыча ресурсов
    public bool isIdle = true; // Бездействие

    // Корутина патрулирования
    private Coroutine patrolCoroutine;

    // Ближайший подбираемый предмет
    private GameObject nearestItem;

    // Начальная точка спавна
    private Transform spawnPoint;

    // Компонент NavMeshAgent для поиска пути
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

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

        // Настройка параметров NavMeshAgent
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = 0.5f; // Расстояние до цели, на котором персонаж останавливается

        // Начинаем патрулирование
        StartPatrolling();
    }

    void Update()
    {
        if (isAttacking)
        {
            Attack();
        }
        else if (isGathering)
        {
            Gather();
        }
        else if (moveTarget != Vector3.zero) // Если есть команда на движение
        {
            MoveToTarget(moveTarget);

            // Если достигли цели с учетом радиуса, сбрасываем её
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                moveTarget = Vector3.zero;

                // Обновляем центр патрулирования на текущую позицию персонажа
                patrolCenter = transform.position;

                // Начинаем патрулирование в новом месте
                StartPatrolling();
            }
        }
        else if (isPatrolling && moveTarget == Vector3.zero)
        {
            Patrol();
        }
        else if (isIdle || moveTarget == Vector3.zero)
        {
            IdleControl(); // Управление при бездействии
        }
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
    public void StartAttack()
    {
        StopAllActions();

        isPatrolling = false;
        isAttacking = true;
        isGathering = false;
        isIdle = false;

        Debug.Log("Персонаж начинает атаковать!");
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

    // Метод для атаки
    private void Attack()
    {
        // Здесь можно добавить логику атаки (например, поиск врага)
        Debug.Log("Персонаж атакует!");
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