// GachaSystem.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaSystem : MonoBehaviour
{

    // Singleton для доступа к данным из других скриптов
    public static GachaSystem Instance;
    public GameObject characterPrefab;  // Префаб для персонажа
    public Transform spawnPoint;         // Точка спавна персонажа
    public Button rollButton;           // Кнопка для ролла гаче-машины
    public Button summonFiveButton;      // Кнопка для ролла гаче-машины
    public Button acceptAllButton;      // Кнопка для принятия всех персонажей 
    public Button exchangeAllButton;     // Кнопка для обмена всех персонажей на ДНК
    public Button getTicketsButton;     // Кнопка для обмена всех персонажей на ДНК   CharacterImage не назначен
    // UI для отображения количества талонов
    public Image gachaTicketIcon;
    public Text gachaTicketCountText;
    // UI для отображения количества ДНК
    public Image dnaPieceIcon;
    public Text dnaPieceCountText;

    public Transform characterRowParent;                  // Родительский объект для ряда персонажей
    public GameObject characterRowItemPrefab;             // Префаб элемента ряда персонажей
    // Префаб элемента ряда персонажей
    public RuntimeAnimatorController commonController;    
    public RuntimeAnimatorController rareController;
    public RuntimeAnimatorController epicController;
    public RuntimeAnimatorController legendaryController;
    // Массивы спрайтов для персонажей
    public Sprite[] commonCharacterSprites; // Спрайты персонажей Common
    public Sprite[] rareCharacterSprites;   // Спрайты персонажей Rare
    public Sprite[] epicCharacterSprites;   // Спрайты персонажей Epic
    public Sprite[] legendaryCharacterSprites; // Спрайты персонажей Legendary
    // Новые массивы спрайтов для фонов
    public Sprite[] commonBackgroundSprites; // Фоны для Common
    public Sprite[] rareBackgroundSprites;   // Фоны для Rare
    public Sprite[] epicBackgroundSprites;   // Фоны для Epic
    public Sprite[] legendaryBackgroundSprites; // Фоны для Legendary

    // Список сохранённых персонажей
    public List<Character> currentCharacters = new List<Character>();
    private List<GameObject> uiCharacters = new List<GameObject>();             // Список текущих элементов ряда
    private int gachaTickets = 10;                                                   // Количество талонов
    private int dnaFragments = 0;
    // Стоимость персонажей через ДНК
    private Dictionary<Rarity, int> dnaCostByRarity = new Dictionary<Rarity, int>
    {
        { Rarity.Common, 10 },
        { Rarity.Rare, 50 },
        { Rarity.Epic, 200 },
        { Rarity.Legendary, 1000 }
    };

    private const int maxCharactersInRow = 6;     // Стоимость персонажей через ДНК
    

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
        if (characterRowItemPrefab == null || characterRowParent == null || spawnPoint == null ||
            rollButton == null || summonFiveButton == null || acceptAllButton == null ||
            exchangeAllButton == null || getTicketsButton == null ||
            commonCharacterSprites == null || rareCharacterSprites == null || epicCharacterSprites == null || legendaryCharacterSprites == null)
        {
            Debug.LogError("Необходимо назначить все ссылки!");
            return;
        }
        // Проверяем, что массивы спрайтов не пустые
        if (commonCharacterSprites.Length == 0 || rareCharacterSprites.Length == 0 || epicCharacterSprites.Length == 0 || legendaryCharacterSprites.Length == 0)
        {
            Debug.LogError("Не все спрайты были добавлены в инспектор!");
            return;
        }
        // Инициализируем словарь спрайтов в классе Character
        Character.spritesByRarity = new Dictionary<Rarity, Sprite[]>
        {
            { Rarity.Common, commonCharacterSprites },
            { Rarity.Rare, rareCharacterSprites },
            { Rarity.Epic, epicCharacterSprites },
            { Rarity.Legendary, legendaryCharacterSprites }
        };
        // Инициализируем словарь спрайтов в классе Character
        Character.defaultSprite = commonCharacterSprites.Length > 0 ? commonCharacterSprites[0] : null;
        // Обновляем текст талонов и ДНК
        UpdateGachaTicketCount();
        UpdateDnaFragmentCount();
        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
        LoadCharacters(); // Загружаем сохранённых персонажей при старте
    }

    public Sprite GetBackgroundSprite(Rarity rarity)
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
    // Метод для получения спрайта персонажа по редкости
    public Sprite GetCharacterSprite(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonCharacterSprites.Length > 0 ? commonCharacterSprites[0] : null,
            Rarity.Rare => rareCharacterSprites.Length > 0 ? rareCharacterSprites[0] : null,
            Rarity.Epic => epicCharacterSprites.Length > 0 ? epicCharacterSprites[0] : null,
            Rarity.Legendary => legendaryCharacterSprites.Length > 0 ? legendaryCharacterSprites[0] : null,
            _ => null
        };
    }
    // Метод для сохранения персонажей
    public void SaveCharacters()
    {
        SaveData saveData = new SaveData();

        foreach (Character character in currentCharacters)
        {
            CharacterData data = new CharacterData
            {
                name = character.name,
                rarity = character.rarity,
                health = character.health,
                attack = character.attack,
                defense = character.defense,
                carryWeight = character.carryWeight,
                level = character.level,
                maxLevel = character.maxLevel,
                isAccepted = true // Принятые персонажи помечаются как true
            };
            saveData.characters.Add(data);
        }

        // Сохраняем персонажей из UI-ряда
        foreach (GameObject rowItem in uiCharacters)
        {
            CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
            if (item != null)
            {
                CharacterData data = new CharacterData
                {
                    name = item.character.name,
                    rarity = item.character.rarity,
                    health = item.character.health,
                    attack = item.character.attack,
                    defense = item.character.defense,
                    carryWeight = item.character.carryWeight,
                    level = item.character.level,
                    maxLevel = item.character.maxLevel,
                    isAccepted = false // Персонажи в UI-ряду помечаются как false
                };
                saveData.characters.Add(data);
            }
        }

        // Сохраняем талоны и ДНК
        saveData.gachaTickets = gachaTickets;
        saveData.dnaFragments = dnaFragments;

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("SaveData", json);

        Debug.Log("Отряд, талоны и ДНК успешно сохранены!");
    }

    // Метод для загрузки отряда
    public void LoadCharacters()
    {
        string json = PlayerPrefs.GetString("SaveData", "");
        if (!string.IsNullOrEmpty(json))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // Восстанавливаем принятых персонажей
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
                    sprite = GetCharacterSprite(data.rarity) // Восстанавливаем спрайт персонажа
                };

                // Если персонаж уже принят
                if (data.isAccepted)
                {
                    currentCharacters.Add(loadedCharacter);
                    Debug.Log($"Загружен принятый персонаж: {loadedCharacter.name} ({loadedCharacter.rarity})");
                }
                // Если персонаж ещё в UI-ряду
                else
                {
                    AddCharacterToRowAndSetupUI(loadedCharacter); // Используем переименованный метод
                    Debug.Log($"Загружен персонаж из UI-ряда: {loadedCharacter.name} ({loadedCharacter.rarity})");
                }
            }

            // Восстанавливаем талоны и ДНК
            gachaTickets = saveData.gachaTickets;
            dnaFragments = saveData.dnaFragments;

            UpdateGachaTicketCount();
            UpdateDnaFragmentCount();
            UpdateAcceptAllButtonState();
            UpdateExchangeAllButtonState();
        }
        else
        {
            Debug.Log("Нет сохранённых данных.");
        }
    }
    // Метод для выполнения ролла гаче
    public void RollGacha(int count = 1)
    {
        if (gachaTickets >= count && CanAddCharacters(count))
        {
            // Метод для выполнения ролла гаче
            gachaTickets -= count;
            UpdateGachaTicketCount();

            for (int i = 0; i < count; i++)
            {
                Character newCharacter = new Character();
                newCharacter.GenerateCharacter();       // Без фиксированной редкости (случайный ролл)
                AddCharacterToRowAndSetupUI(newCharacter);        // Добавляем персонажа в ряд
            }
            SaveCharacters();// Автоматически сохраняем отряд после ролла
            // Центрируем ряд персонажей
            CenterCharacterRow();
            // Обновляем состояние кнопок
            UpdateRollButtonState();
            UpdateSummonFiveButtonState();
        }
        else
        {
            Debug.LogError("Недостаточно талонов или достигнут лимит персонажей!");
        }
    }
    // Обновляем состояние кнопок
    private bool CanAddCharacters(int count)
    {
        return uiCharacters.Count + count <= maxCharactersInRow;
    }
    // Метод для добавления персонажа в UI-ряд
    private void AddCharacterToRowAndSetupUI(Character character)
    {
        if (characterRowItemPrefab == null || characterRowParent == null)
        {
            Debug.LogError("Необходимо назначить CharacterRowItemPrefab и CharacterRowParent!");
            return;
        }
        // Создаём новый элемент ряда
        GameObject rowItem = Instantiate(characterRowItemPrefab, characterRowParent);

        if (rowItem != null)
        {
            // Находим компонент CharacterRowItem
            CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
            if (item != null)
            {
                // Устанавливаем данные персонажа
                item.SetCharacter(character, rowItem, OnAcceptCharacter, OnExchangeForDNA);

                // Явно устанавливаем спрайт фона
                SetBackgroundSprite(rowItem, character.rarity);

                // Добавляем объект в список uiCharacters
                uiCharacters.Add(rowItem);
            }
            else
            {
                Debug.LogError("Не найден компонент CharacterRowItem!");
            }

        }
        else
        {
            Debug.LogError("Не удалось создать CharacterRowItem!");
        }
        // Центрируем ряд после добавления персонажа
        CenterCharacterRow();
        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    // Метод для центрирования ряда персонажей
    private void CenterCharacterRow()
    {
        if (uiCharacters.Count == 0 || characterRowParent == null)
        {
            Debug.LogWarning("Нет персонажей для центрирования!");
            return;
        }
        // Ширина одного элемента = 150f
        float totalWidth = uiCharacters.Count * 150f;
        float offset = -(totalWidth / 2f);

        for (int i = 0; i < uiCharacters.Count; i++)
        {
            if (uiCharacters[i] != null)
            {
                uiCharacters[i].transform.localPosition = new Vector3(offset + i * 150f, 0, 0);
            }
            else
            {
                Debug.LogError("Обнаружен null в списке uiCharacters!");
            }
        }
    }
    // Метод для принятия персонажа
    private void OnAcceptCharacter(GameObject rowItem)
    {
        if (rowItem == null)
        {
            Debug.LogError("Переданный rowItem является null!");
            return;
        }

        CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
        if (item == null)
        {
            Debug.LogError("Не найден компонент CharacterRowItem!");
            return;
        }

        currentCharacters.Add(item.character); // Добавляем персонажа в отряд
        SaveCharacters(); // Сохраняем отряд после принятия персонажа

        SpawnCharacter(item.character); // Спавним персонажа на игровое поле
        RemoveCharacterFromRow(rowItem);// Удаляем элемент из ряда
        
        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // Метод для обмена персонажа на ДНК
    private void OnExchangeForDNA(GameObject rowItem)
    {
        if (rowItem == null)
        {
            Debug.LogError("Переданный rowItem является null!");
            return;
        }

        CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
        if (item == null)
        {
            Debug.LogError("Не найден компонент CharacterRowItem!");
            return;
        }

        Rarity rarity = item.character.rarity;

        int dnaAmount = rarity switch
        {
            Rarity.Common => 5,
            Rarity.Rare => 10,
            Rarity.Epic => 20,
            Rarity.Legendary => 50,
            _ => 0
        };

        dnaFragments += dnaAmount;
        UpdateDnaFragmentCount();
        SaveCharacters();
        // Удаляем элемент из ряда
        RemoveCharacterFromRow(rowItem);
        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // Метод для удаления персонажа из UI-ряда
    private void RemoveCharacterFromRow(GameObject rowItem)
    {
        if (rowItem == null)
        {
            Debug.LogError("Попытка удалить null элемент из ряда!");
            return;
        }
        // Находим дочерний объект с анимацией фона
        GameObject backgroundObject = rowItem.transform.Find("Background")?.gameObject;
        if (backgroundObject != null)
        {
            Destroy(backgroundObject);// Удаляем объект анимации фона
        }

        uiCharacters.Remove(rowItem);
        Destroy(rowItem);
        // Перестраиваем ряд
        CenterCharacterRow();
        SaveCharacters(); // Сохраняем отряд после удаления персонажа

        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // Метод для спавна персонажа
    public void SpawnCharacter(Character character)
    {
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab и SpawnPoint!");
            return;
        }

        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        if (characterObject != null)
        {
            SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>(); // Присваиваем основной спрайт
            mainSpriteRenderer.sprite = character.sprite;
            // Добавляем анимированный фон для персонажа
            AddAnimatedBackground(characterObject, character.rarity);

            Debug.Log($"Вы приняли персонажа: {character.name} ({character.rarity})!");
        }
        else
        {
            Debug.LogError("Не удалось создать CharacterPrefab!");
        }
    }
    // Метод для добавления анимированного фона
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
        // Создаём объект для фона
        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = GetDefaultBackgroundSprite(rarity);
        // Добавляем Animator
        Animator animator = backgroundObject.AddComponent<Animator>();
        SetAnimatorController(animator, rarity);
        // Настройка размера фона
        backgroundRenderer.transform.localScale = new Vector3(1f, 1f, 0.5f);
        backgroundRenderer.sortingOrder = 0;
    }
    // Метод для установки анимации фона
    private void SetAnimatorController(Animator animator, Rarity rarity)
    {
        if (animator == null)
        {
            Debug.LogError("Переданный animator является null!");
            return;
        }

        RuntimeAnimatorController controller = rarity switch
        {
            Rarity.Common => commonController,
            Rarity.Rare => rareController,
            Rarity.Epic => epicController,
            Rarity.Legendary => legendaryController,
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

    private void UpdateRollButtonState()
    {
        if (rollButton != null)
        {
            rollButton.interactable = gachaTickets > 0 && CanAddCharacters(1);
        }
        else
        {
            Debug.LogError("RollButton не назначен!");
        }
    }

    private void UpdateSummonFiveButtonState()
    {
        if (summonFiveButton != null)
        {
            summonFiveButton.interactable = gachaTickets >= 5 && CanAddCharacters(5);
        }
        else
        {
            Debug.LogError("SummonFiveButton не назначен!");
        }
    }

    private void UpdateAcceptAllButtonState()
    {
        if (acceptAllButton != null)
        {
            acceptAllButton.interactable = uiCharacters.Count > 0;
        }
        else
        {
            Debug.LogError("AcceptAllButton не назначен!");
        }
    }

    private void UpdateExchangeAllButtonState()
    {
        if (exchangeAllButton != null)
        {
            exchangeAllButton.interactable = uiCharacters.Count > 0;
        }
        else
        {
            Debug.LogError("ExchangeAllButton не назначен!");
        }
    }

    private void UpdateGetTicketsButtonState()
    {
        if (getTicketsButton != null)
        {
            getTicketsButton.interactable = true;
        }
        else
        {
            Debug.LogError("GetTicketsButton не назначен!");
        }
    }

    private void UpdateGachaTicketCount()
    {
        if (gachaTicketCountText != null)
        {
            gachaTicketCountText.text = $"x{gachaTickets}";
        }
        else
        {
            Debug.LogError("GachaTicketCountText не назначен!");
        }
    }

    private void UpdateDnaFragmentCount()
    {
        if (dnaPieceCountText != null)
        {
            dnaPieceCountText.text = $"x{dnaFragments}";
        }
        else
        {
            Debug.LogError("DnaPieceCountText не назначен!");
        }
    }

    public void AcceptAllCharacters()
    {
        foreach (GameObject rowItem in uiCharacters.ToArray())
        {
            OnAcceptCharacter(rowItem);
        }
        SaveCharacters();
        // Обновляем состояние кнопок после действия
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    public void ExchangeAllCharactersForDNA()
    {
        foreach (GameObject rowItem in uiCharacters.ToArray())
        {
            OnExchangeForDNA(rowItem);
        }
        SaveCharacters();
        // Обновляем состояние кнопок после действия
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    private void BuyCharacter(Rarity rarity)
    {
        if (!dnaCostByRarity.ContainsKey(rarity))
        {
            Debug.LogError($"Стоимость для редкости {rarity} не определена!");
            return;
        }

        int cost = dnaCostByRarity[rarity];
        if (dnaFragments >= cost && CanAddCharacters(1))
        {
            dnaFragments -= cost;
            UpdateDnaFragmentCount();

            Character guaranteedCharacter = new Character();
            guaranteedCharacter.rarity = rarity;
            guaranteedCharacter.GenerateCharacter(fixedRarity: rarity);

            AddCharacterToRowWithBackground(guaranteedCharacter);
            SaveCharacters();

            Debug.Log($"Вы купили персонажа: {guaranteedCharacter.name} ({guaranteedCharacter.rarity})!");
        }
        else
        {
            Debug.LogError("Недостаточно кусочков ДНК или достигнут лимит персонажей!");
        }
    }

    public void BuyCommonCharacter() => BuyCharacter(Rarity.Common);
    public void BuyRareCharacter() => BuyCharacter(Rarity.Rare);
    public void BuyEpicCharacter() => BuyCharacter(Rarity.Epic);
    public void BuyLegendaryCharacter() => BuyCharacter(Rarity.Legendary);

    public void AddGachaTickets(int amount)
    {
        gachaTickets += amount;
        UpdateGachaTicketCount();
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
    }
    // Метод для добавления персонажа в UI-ряд с правильным фоном
    private void AddCharacterToRowWithBackground(Character character)
    {
        if (characterRowItemPrefab == null || characterRowParent == null)
        {
            Debug.LogError("Необходимо назначить CharacterRowItemPrefab и CharacterRowParent!");
            return;
        }

        // Создаём элемент ряда
        GameObject rowItem = Instantiate(characterRowItemPrefab, characterRowParent);

        if (rowItem != null)
        {
            // Находим компонент CharacterRowItem
            CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
            if (item != null)
            {
                // Устанавливаем данные персонажа
                item.SetCharacter(character, rowItem, OnAcceptCharacter, OnExchangeForDNA);

                // Явно устанавливаем спрайт фона
                SetBackgroundSprite(rowItem, character.rarity);

                // Добавляем объект в список uiCharacters
                uiCharacters.Add(rowItem);
            }
            else
            {
                Debug.LogError("Не найден компонент CharacterRowItem!");
            }
        }
        else
        {
            Debug.LogError("Не удалось создать CharacterRowItem!");
        }

        CenterCharacterRow();
    }

    

    // Метод для установки спрайта фона
    private void SetBackgroundSprite(GameObject rowItem, Rarity rarity)
    {
        if (rowItem == null)
        {
            Debug.LogError("Переданный rowItem является null!");
            return;
        }

        // Находим компонент backgroundImage
        Image backgroundImage = rowItem.transform.Find("BackgroundImage")?.GetComponent<Image>();
        if (backgroundImage != null)
        {
            // Получаем спрайт фона из GachaSystem
            Sprite backgroundSprite = GetBackgroundSprite(rarity);
            if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite; // Устанавливаем спрайт фона
            }
            else
            {
                Debug.LogWarning($"Спрайт фона для редкости {rarity} не найден!");
            }
        }
        else
        {
            Debug.LogError("BackgroundImage не назначен!");
        }
    }

}