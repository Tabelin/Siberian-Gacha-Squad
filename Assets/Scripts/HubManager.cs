// HubManager.cs
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // Singleton для доступа к HubManager
    public static HubManager Instance;

    // Префаб для персонажа
    public GameObject characterPrefab;

    // Радиус изменения позиции спавна
    public float spawnRadius = 2f;

    // Основная точка спавна (один спавн-поинт для всех)
    public Transform mainSpawnPoint;

    // Атласы аниматоров для фонов
    public RuntimeAnimatorController commonBackgroundController;
    public RuntimeAnimatorController rareBackgroundController;
    public RuntimeAnimatorController epicBackgroundController;
    public RuntimeAnimatorController legendaryBackgroundController;

    // Массивы спрайтов для персонажей и фонов
    public Sprite[] commonSprites;
    public Sprite[] rareSprites;
    public Sprite[] epicSprites;
    public Sprite[] legendarySprites;

    public Sprite[] commonBackgroundSprites;
    public Sprite[] rareBackgroundSprites;
    public Sprite[] epicBackgroundSprites;
    public Sprite[] legendaryBackgroundSprites;

    // Список спавненных персонажей
    private List<GameObject> spawnedCharacters = new List<GameObject>();

    // Список выбранных персонажей
    private List<GameObject> selectedCharacters = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Обеспечиваем доступность между сценами
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликат объекта
        }
    }

    void Start()
    {
        if (characterPrefab == null || mainSpawnPoint == null)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab и MainSpawnPoint!");
            return;
        }

        // Загружаем персонажей из SaveData
        LoadCharactersFromSaveData();
    }

    // Метод для поиска ближайшего врага
    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Находим всех врагов
        if (enemies.Length == 0)
        {
            Debug.LogWarning("Враги не найдены!");
            return null;
        }

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

        return closestEnemy; // Возвращаем ближайшего врага
    }


    // Метод для загрузки персонажей из SaveData
    private void LoadCharactersFromSaveData()
    {
        string json = PlayerPrefs.GetString("SaveData", "");  //PlayerPrefs.GetString() 
        if (!string.IsNullOrEmpty(json))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json); //JsonUtility.FromJson<SaveData>(json)

            foreach (CharacterData data in saveData.characters)
            {
                Character loadedCharacter = new Character
                {
                    name = data.name,
                    rarity = data.rarity,
                    health = data.health,
                    attack = data.attack,
                    defense = data.defense,
                    carryWeight = data.carryWeight,
                    level = data.level,
                    maxLevel = data.maxLevel,
                    sprite = GetCharacterSprite(data.rarity) // Получаем спрайт персонажа
                };

                if (data.isAccepted)
                {
                    SpawnCharacter(loadedCharacter);
                }
                else
                {
                    Debug.Log($"Персонаж {loadedCharacter.name} ({loadedCharacter.rarity}) находится в UI-ряду и не спавнится на хабе.");
                }
            }
        }
        else
        {
            Debug.Log("Нет сохранённых данных о персонажах.");
        }
    }

    // Метод для спавна персонажа с изменением позиции спавна
    private void SpawnCharacter(Character character)
    {
        if (characterPrefab == null || mainSpawnPoint == null)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab или MainSpawnPoint!");
            return;
        }

        // Выбираем случайную позицию спавна в радиусе основной точки
        Vector3 spawnPosition = mainSpawnPoint.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = mainSpawnPoint.position.y; // Ограничиваем движение только по XZ

        // Создаём объект персонажа
        GameObject characterObject = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

        if (characterObject != null)
        {
            // Настройка основного спрайта персонажа
            SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>();
            if (mainSpriteRenderer != null)
            {
                mainSpriteRenderer.sprite = character.sprite;
            }
            else
            {
                Debug.LogError("SpriteRenderer не найден!");
            }

            // Инициализируем систему здоровья
            HealthSystem healthSystem = characterObject.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.InitializeHealth(
                     initialMaxHealth: character.health,
                     initialCurrentHealth: character.health,
                     initialAttackPower: character.attack,
                     initialDefense: character.defense,
                     initialLevel: character.level,
                     initialMaxLevel: character.maxLevel, // Добавлено!
                     name: character.name // Передаем имя персонажа из сохранения
                 );
            }
            else
            {
                Debug.LogError("Компонент HealthSystem не найден!");
            } 

            // Добавляем анимированный фон
            AddAnimatedBackground(characterObject, character.rarity);

            // Сохраняем ссылку на спавненного персонажа
            spawnedCharacters.Add(characterObject);

            Debug.Log($"Персонаж {character.name} ({character.rarity}) успешно спавнится в сцене!");
        }
        else
        {
            Debug.LogError("Не удалось создать CharacterPrefab!");
        }
    }



    // Метод для добавления анимированного фона через Animator
    private void AddAnimatedBackground(GameObject parentObject, Rarity rarity)
    {
        if (parentObject == null)
        {
            Debug.LogError("Переданный parentObject является null!");
            return;
        }

        // Создаём объект для фона
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.parent = parentObject.transform;
        backgroundObject.transform.localPosition = Vector3.zero;

        // Добавляем Sprite Renderer для фона
        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = GetDefaultBackgroundSprite(rarity);
        backgroundRenderer.sortingOrder = -1; // Фон находится позади основного спрайта

        // Добавляем Animator компонент
        Animator animator = backgroundObject.AddComponent<Animator>();
        SetAnimatorController(animator, rarity);

        // Настройка размера фона
        backgroundRenderer.transform.localScale = new Vector3(1f, 1f, 1f); // Фон чуть больше основного спрайта
    }

    // Метод для установки Animator Controller
    private void SetAnimatorController(Animator animator, Rarity rarity)
    {
        if (animator == null)
        {
            Debug.LogError("Переданный animator является null!");
            return;
        }

        RuntimeAnimatorController controller = rarity switch
        {
            Rarity.Common => commonBackgroundController,
            Rarity.Rare => rareBackgroundController,
            Rarity.Epic => epicBackgroundController,
            Rarity.Legendary => legendaryBackgroundController,
            _ => null
        };

        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning($"Анимационный контроллер для редкости {rarity} не найден!");
        }
    }

    // Метод для получения спрайта персонажа по редкости
    private Sprite GetCharacterSprite(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonSprites.Length > 0 ? commonSprites[0] : null,
            Rarity.Rare => rareSprites.Length > 0 ? rareSprites[0] : null,
            Rarity.Epic => epicSprites.Length > 0 ? epicSprites[0] : null,
            Rarity.Legendary => legendarySprites.Length > 0 ? legendarySprites[0] : null,
            _ => null
        };
    }

    // Метод для получения заглушечного спрайта фона
    private Sprite GetDefaultBackgroundSprite(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonBackgroundSprites.Length > 0 ? commonBackgroundSprites[0] : null,
            Rarity.Rare => rareBackgroundSprites.Length > 0 ? rareBackgroundSprites[0] : null,
            Rarity.Epic => epicBackgroundSprites.Length > 0 ? epicBackgroundSprites[0] : null,
            Rarity.Legendary => legendaryBackgroundSprites.Length > 0 ? legendaryBackgroundSprites[0] : null,
            _ => null
        };
    }

    // Метод для выбора персонажа
    public void SelectCharacter(bool isShiftPressed = false)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Character"))
            {
                if (isShiftPressed)
                {
                    // Если нажата клавиша Shift, добавляем/удаляем персонажа из списка выбранных
                    if (selectedCharacters.Contains(hitObject))
                    {
                        selectedCharacters.Remove(hitObject);
                        Debug.Log($"Персонаж {hitObject.name} удалён из выбора.");
                    }
                    else
                    {
                        selectedCharacters.Add(hitObject);
                        Debug.Log($"Персонаж {hitObject.name} добавлен в выбор.");
                    }
                }
                else
                {
                    // Если Shift не нажата, очищаем предыдущий выбор и выбираем нового персонажа
                    selectedCharacters.Clear();
                    selectedCharacters.Add(hitObject);
                    Debug.Log($"Выбран персонаж: {hitObject.name}");
                }
            }
        }
        else
        {
            // Если кликнули не на персонажа, очищаем выбор
            if (!isShiftPressed)
            {
                DeselectAllCharacters();
            }
        }
    }

    // Метод для сброса выбора всех персонажей
    private void DeselectAllCharacters()
    {
        if (selectedCharacters.Count > 0)
        {
            foreach (GameObject character in selectedCharacters)
            {
                CharacterManager manager = character.GetComponent<CharacterManager>();
                if (manager != null)
                {
                    // Если персонаж был выбран, но не выполнял важные действия, возобновляем патрулирование
                    if (manager.isIdle && !manager.isAttacking && !manager.isGathering)
                    {
                        manager.StartPatrolling(); // Возобновляем патрулирование
                    }
                }
            }

            selectedCharacters.Clear();
            Debug.Log("Выбор персонажей очищен.");
        }
    }


    // Метод для отправки команд выбранным персонажам
    public void SendCommand(string command)
    {
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("Никто не выбран!");
            return;
        }

        foreach (GameObject character in selectedCharacters)
        {
            CharacterManager manager = character.GetComponent<CharacterManager>();
            if (manager != null)
            {
                switch (command)
                {
                    case "Patrol":
                        manager.StartPatrolling();
                        break;

                    case "Attack":
                        GameObject attackTarget = FindClosestEnemy(); // Находим ближайшего врага
                        if (attackTarget != null)
                        {
                            manager.StartAttack(attackTarget); // Отправляем команду атаки с указанием цели
                        }
                        else
                        {
                            Debug.LogWarning("Ближайший враг не найден!");
                        }
                        break;

                    case "Gather":
                        manager.StartGathering();
                        break;

                    case "TakeControl":
                        manager.TakeControl();
                        break;

                    default:
                        Debug.LogWarning($"Неизвестная команда: {command}");
                        break;
                }
            }
            else
            {
                Debug.LogError("Компонент CharacterManager не найден!");
            }
        }
    }
    // Метод для направления выбранных персонажей на место клика мыши
    public void SendMoveCommandToSelectedCharacters()
    {
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("Никто не выбран для движения!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 targetPosition = hit.point;

            foreach (GameObject character in selectedCharacters)
            {
                CharacterManager manager = character.GetComponent<CharacterManager>();
                if (manager != null)
                {
                    manager.MoveToTarget(targetPosition); // Отправляем команду на движение

                    // Если персонаж занят важным действием, предупреждаем об этом
                    if (manager.isAttacking || manager.isGathering)
                    {
                        Debug.LogWarning($"Персонаж {character.name} прерывает важное действие для выполнения команды игрока."); 
                    }
                }
                else
                {
                    Debug.LogError("Компонент CharacterManager не найден!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Клик мыши не попал на поверхность земли!");
        }
    }



    // Метод для обновления направления персонажей
    private void UpdateCharacterRotation()
    {
        foreach (GameObject character in spawnedCharacters)
        {
            if (character != null && Camera.main != null)
            {
                AlignCharacterToCamera(character);
            }
        }
    }

    // Метод для поворота персонажа к камере
    private void AlignCharacterToCamera(GameObject characterObject)
    {
        if (characterObject == null || Camera.main == null)
        {
            Debug.LogError("Переданный объект или основная камера является null!");
            return;
        }

        Vector3 direction = (Camera.main.transform.position - characterObject.transform.position).normalized;
        float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(-15f, angleY, 0f);
        characterObject.transform.rotation = Quaternion.Lerp(characterObject.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void Update()
    {
        // Выбор персонажа (нажатие мыши)
        if (Input.GetMouseButtonDown(0))
        {
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            SelectCharacter(isShiftPressed);
        }

        // Сброс выбора при клике вне персонажей
        if (Input.GetMouseButtonUp(0) && !IsClickOnCharacter())
        {
            DeselectAllCharacters();
        }

        // Направление выбранных персонажей на место клика мыши (правая кнопка мыши)
        if (Input.GetMouseButtonDown(1)) // Проверяем клик правой кнопкой мыши
        {
            SendMoveCommandToSelectedCharacters(); // Вызываем метод для направления персонажей
        }

        // Отправка команд выбранным персонажам
        if (Input.GetKeyDown(KeyCode.T)) // Атаковать
        {
            SendCommand("Attack");
        }

        if (Input.GetKeyDown(KeyCode.G)) // Добывать ресурсы
        {
            SendCommand("Gather");
        }

        if (Input.GetKeyDown(KeyCode.C)) // Взять контроль
        {
            SendCommand("TakeControl");
        }

        if (Input.GetKeyDown(KeyCode.P)) // Начать патрулирование
        {
            SendCommand("Patrol");
        }

        // Каждый кадр обновляем направление персонажей
        UpdateCharacterRotation();
    }

    // Проверка, попал ли клик на персонажа
    private bool IsClickOnCharacter()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject.CompareTag("Character");
        }

        return false;
    }
}