// HubManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private GameObject selectedResource;
    // Позиция начала выделения
    private Vector3 selectionStart;

    // Флаг для проверки нажатия мыши
    private bool isSelecting = false;

    // Рамка выделения (создаётся динамически)
    private GameObject selectionBox;

    // Камера
    private Camera mainCamera;

    // Слои для UI
    private const int UI_LAYER = 5; // Стандартный слой для UI (например, "UI")
    private const int IGNORE_RAYCAST_LAYER = 21; // Слой IgnoreRaycast

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
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Основная камера не найдена!");
            return;
        }
        // Загружаем персонажей из SaveData
        LoadCharactersFromSaveData();
    }




    void Update()
    {
        // Сброс выбора при клике вне персонажей
        if (Input.GetMouseButtonUp(0) && !IsClickOnCharacter())
        {
            EndSelection();
        }

        // Начало выделения при нажатии ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        // Продолжение выделения при удержании ЛКМ
        if (Input.GetMouseButton(0) && isSelecting)
        {
            ContinueSelection();
        }

        // Завершение выделения при отпускании ЛКМ
        if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }


        // Направление выбранных персонажей на место клика мыши (правая кнопка мыши)
        if (selectedCharacters.Count > 0 && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Проверяем, нажат ли на ресурс
                if (hit.collider.CompareTag("Resource"))
                {
                    GameObject resource = hit.collider.gameObject;

                    foreach (GameObject character in selectedCharacters)
                    {
                        CharacterManager manager = character.GetComponent<CharacterManager>();
                        if (manager != null)
                        {
                            manager.SetControlledByPlayer(true); // Переводим в режим управления
                            manager.StartGathering(resource);     // 🚀 Теперь передаём параметр
                        }
                    }
                }
                else
                {
                    // Если не по ресурсу — обычное движение
                    foreach (GameObject character in selectedCharacters)
                    {
                        CharacterManager manager = character.GetComponent<CharacterManager>();
                        if (manager != null)
                        {
                            manager.SetControlledByPlayer(true);
                            manager.MoveToTarget(hit.point);
                        }
                    }
                }
            }
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
                    experience = data.experience,
                    experienceToNextLevel = data.experienceToNextLevel,

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
                    initialMaxLevel: character.maxLevel,
                    initialExperience: character.experience, // Передаем опыт
                    initialExperienceToNextLevel: character.experienceToNextLevel, // Передаем требуемый опыт
                    initialCarryWeight: character.carryWeight,
                    name: character.name
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
                        break;

                    case "Gather":
                        // Убедись, что resource определён
                        if (selectedResource != null)
                        {
                            manager.SetControlledByPlayer(true);
                            manager.StartGathering(selectedResource); // Теперь передаём конкретный ресурс
                        }
                        else
                        {
                            Debug.LogWarning("Нет выбранного ресурса для добычи");
                        }
                        break;

                    case "TakeControl":
                        manager.TakeControl();
                        break;

                    default:
                        break;
                }
            }
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
    // Метод для начала выделения
    private void StartSelection()
    {
        isSelecting = true;
        selectionStart = Input.mousePosition;

        // Создаём рамку выделения
        if (selectionBox == null)
        {
            selectionBox = new GameObject("Selection Box");
            selectionBox.transform.parent = transform;
            selectionBox.layer = LayerMask.NameToLayer("UI");
            selectionBox.layer = IGNORE_RAYCAST_LAYER; // Устанавливаем слой IgnoreRaycast


            selectionBox.AddComponent<RectTransform>();
            selectionBox.AddComponent<Image>();

            Image image = selectionBox.GetComponent<Image>();
            image.color = new Color(0f, 0f, 1f, 0.2f); // Цвет рамки (прозрачный синий)
            image.raycastTarget = false; // Отключаем обработку кликов для рамки
        }

        selectionBox.SetActive(true);
    }
    // Метод для продолжения выделения
    private void ContinueSelection()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Находим размеры рамки
        float width = Mathf.Abs(mousePosition.x - selectionStart.x);
        float height = Mathf.Abs(mousePosition.y - selectionStart.y);

        // Обновляем позицию и размер рамки
        RectTransform rectTransform = selectionBox.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0, 0);
        rectTransform.position = new Vector2(
            Mathf.Min(selectionStart.x, mousePosition.x),
            Mathf.Min(selectionStart.y, mousePosition.y)
        );
        rectTransform.sizeDelta = new Vector2(width, height);
    }
    // Метод для завершения выделения
    private void EndSelection()
    {
        isSelecting = false;

        if (selectionBox != null)
        {
            selectionBox.SetActive(false); // Скрываем рамку
        }

        // Получаем границы области
        Vector3[] worldCorners = GetWorldCorners();

        // Ищем персонажей внутри области
        SelectCharactersInArea(worldCorners);

        Debug.Log($"Выбрано персонажей: {selectedCharacters.Count}");
    }
    // Метод для получения углов рамки в мировых координатах
    private Vector3[] GetWorldCorners()
    {
        if (selectionBox == null) return new Vector3[4];

        RectTransform rectTransform = selectionBox.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        return corners;
    }
    // Метод для выбора персонажей внутри области
    private void SelectCharactersInArea(Vector3[] worldCorners)
    {
        // Очищаем предыдущий выбор
        DeselectAllCharacters();

        // Находим всех персонажей
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");

        foreach (GameObject character in characters)
        {
            if (IsCharacterInArea(character, worldCorners))
            {
                SelectCharacter(character);
            }
        }
    }
    // Метод для проверки, находится ли персонаж внутри области
    private bool IsCharacterInArea(GameObject character, Vector3[] worldCorners)
    {
        Vector3 characterScreenPos = mainCamera.WorldToScreenPoint(character.transform.position);

        // Минимальные и максимальные координаты рамки
        float minX = Mathf.Min(worldCorners[0].x, worldCorners[2].x);
        float maxX = Mathf.Max(worldCorners[0].x, worldCorners[2].x);
        float minY = Mathf.Min(worldCorners[0].y, worldCorners[2].y);
        float maxY = Mathf.Max(worldCorners[0].y, worldCorners[2].y);

        // Проверяем, попадает ли персонаж в область
        return characterScreenPos.x >= minX && characterScreenPos.x <= maxX &&
               characterScreenPos.y >= minY && characterScreenPos.y <= maxY;
    }
    // Метод для выделения персонажа
    private void SelectCharacter(GameObject character)
    {
        if (!selectedCharacters.Contains(character))
        {
            selectedCharacters.Add(character);

            // Добавляем эффект выделения (например, изменение цвета спрайта)
            SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.cyan; // Выделенный цвет
            }

            Debug.Log($"Персонаж '{character.name}' выбран.");
        }
    }
    // Метод для снятия выделения со всех персонажей
    private void DeselectAllCharacters()
    {
        foreach (GameObject character in selectedCharacters)
        {
            SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white; // Возвращаем исходный цвет
            }
        }

        selectedCharacters.Clear(); // Очищаем список выбранных персонажей
    }
}