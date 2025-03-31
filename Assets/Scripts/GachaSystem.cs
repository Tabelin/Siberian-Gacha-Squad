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
    public Button getTicketsButton;     // Кнопка для обмена всех персонажей на ДНК
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
    // Массивы спрайтов для каждой редкости
    public Sprite[] commonSprites;
    public Sprite[] rareSprites;
    public Sprite[] epicSprites;
    public Sprite[] legendarySprites;

    private List<GameObject> currentCharacters = new List<GameObject>();             // Список текущих элементов ряда
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
        // Инициализируем Singleton
        Instance = this;
    }

    void Start()
    {
        if (characterRowItemPrefab == null || characterRowParent == null || spawnPoint == null ||
            rollButton == null || summonFiveButton == null || acceptAllButton == null ||
            exchangeAllButton == null || getTicketsButton == null ||
            commonSprites == null || rareSprites == null || epicSprites == null || legendarySprites == null)
        {
            Debug.LogError("Необходимо назначить все ссылки!");
            return;
        }
        // Проверяем, что массивы спрайтов не пустые
        if (commonSprites.Length == 0 || rareSprites.Length == 0 || epicSprites.Length == 0 || legendarySprites.Length == 0)
        {
            Debug.LogError("Не все спрайты были добавлены в инспектор!");
            return;
        }
        // Инициализируем словарь спрайтов в классе Character
        Character.spritesByRarity = new Dictionary<Rarity, Sprite[]>
        {
            { Rarity.Common, commonSprites },
            { Rarity.Rare, rareSprites },
            { Rarity.Epic, epicSprites },
            { Rarity.Legendary, legendarySprites }
        };
        // Инициализируем словарь спрайтов в классе Character
        Character.defaultSprite = commonSprites.Length > 0 ? commonSprites[0] : null;
        // Обновляем текст талонов и ДНК
        UpdateGachaTicketCount();
        UpdateDnaFragmentCount();
        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
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
                AddCharacterToRow(newCharacter);        // Добавляем персонажа в ряд
            }
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
        return currentCharacters.Count + count <= maxCharactersInRow;
    }

    private void AddCharacterToRow(Character character)
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
            rowItem.GetComponent<CharacterRowItem>().SetCharacter(character, rowItem, OnAcceptCharacter, OnExchangeForDNA);
            currentCharacters.Add(rowItem);   // Сохраняем ссылку на элемент ряда
            // Добавляем анимированный фон для персонажа
            AddAnimatedBackground(rowItem, character.rarity);
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
        if (currentCharacters.Count == 0 || characterRowParent == null)
        {
            Debug.LogWarning("Нет персонажей для центрирования!");
            return;
        }
        // Ширина одного элемента = 150f
        float totalWidth = currentCharacters.Count * 150f;
        float offset = -(totalWidth / 2f);

        for (int i = 0; i < currentCharacters.Count; i++)
        {
            if (currentCharacters[i] != null)
            {
                currentCharacters[i].transform.localPosition = new Vector3(offset + i * 150f, 0, 0);
            }
            else
            {
                Debug.LogError("Обнаружен null в списке currentCharacters!");
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
        // Удаляем элемент из ряда
        RemoveCharacterFromRow(rowItem);
        // Обновляем состояние кнопок
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // Метод для удаления персонажа из ряда
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

        currentCharacters.Remove(rowItem);
        Destroy(rowItem);
        // Перестраиваем ряд
        CenterCharacterRow();
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
            Rarity.Common => commonSprites.Length > 0 ? commonSprites[0] : null,
            Rarity.Rare => rareSprites.Length > 0 ? rareSprites[0] : null,
            Rarity.Epic => epicSprites.Length > 0 ? epicSprites[0] : null,
            Rarity.Legendary => legendarySprites.Length > 0 ? legendarySprites[0] : null,
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
            acceptAllButton.interactable = currentCharacters.Count > 0;
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
            exchangeAllButton.interactable = currentCharacters.Count > 0;
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
        foreach (GameObject rowItem in currentCharacters.ToArray())
        {
            OnAcceptCharacter(rowItem);
        }

        // Обновляем состояние кнопок после действия
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    public void ExchangeAllCharactersForDNA()
    {
        foreach (GameObject rowItem in currentCharacters.ToArray())
        {
            OnExchangeForDNA(rowItem);
        }

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

            AddCharacterToRow(guaranteedCharacter);

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
}