// HubManager.cs
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // Singleton для доступа к HubManager
    public static HubManager Instance;

    // Префаб для персонажа
    public GameObject characterPrefab;

    // Тег для точек спавна
    public string spawnPointTag = "SpawnPoint";

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
        if (characterPrefab == null)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab!");
            return;
        }

        // Находим все точки спавна по тегу
        Transform[] spawnPoints = FindSpawnPoints();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Нет точек спавна в текущей сцене!");
            return;
        }

        // Загружаем персонажей из SaveData
        LoadCharactersFromSaveData(spawnPoints);
    }

    void Update()
    {
        // Каждый кадр обновляем направление персонажей
        UpdateCharacterRotation();
    }

    // Метод для обновления направления персонажей
    private void UpdateCharacterRotation()
    {
        foreach (GameObject characterObject in spawnedCharacters)
        {
            if (characterObject != null && Camera.main != null)
            {
                AlignCharacterToCamera(characterObject);
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

        // Определяем направление от персонажа к камере
        Vector3 direction = (Camera.main.transform.position - characterObject.transform.position).normalized;

        // Вычисляем угол вращения только по оси Y
        float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // Поворачиваем персонажа
        Quaternion targetRotation = Quaternion.Euler(-15f, angleY, 0f); // 15f — небольшой угол наклона вверх

        // Добавляем плавный поворот (опционально)
        characterObject.transform.rotation = Quaternion.Lerp(characterObject.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // Метод для поиска точек спавна
    private Transform[] FindSpawnPoints()
    {
        List<Transform> spawnPoints = new List<Transform>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(spawnPointTag))
        {
            spawnPoints.Add(obj.transform);
        }

        return spawnPoints.ToArray();
    }

    // Метод для загрузки персонажей из SaveData
    private void LoadCharactersFromSaveData(Transform[] spawnPoints)
    {
        string json = PlayerPrefs.GetString("SaveData", "");
        if (!string.IsNullOrEmpty(json))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            for (int i = 0; i < saveData.characters.Count && i < spawnPoints.Length; i++)
            {
                CharacterData data = saveData.characters[i];
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

                // Если персонаж уже принят
                if (data.isAccepted)
                {
                    SpawnCharacter(loadedCharacter, spawnPoints[i]);
                }
                // Если персонаж ещё в UI-ряду, игнорируем его на хабе
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

    // Метод для спавна персонажа
    private void SpawnCharacter(Character character, Transform spawnPoint)
    {
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab или SpawnPoint!");
            return;
        }

        // Создаём объект персонажа
        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, Quaternion.identity);

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
        backgroundRenderer.transform.localScale = new Vector3(1.2f, 1.2f, 1f); // Фон чуть больше основного спрайта
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
}