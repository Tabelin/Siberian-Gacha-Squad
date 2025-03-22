using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для работы с UI

public class GachaSystem : MonoBehaviour
{
    // Префаб для персонажа
    public GameObject characterPrefab;

    // Точка спавна персонажа
    public Transform spawnPoint;

    // Кнопка для ролла гаче-машины
    public Button rollButton; // Ссылка на кнопку "Получить персонажа"

    // UI для отображения количества талонов
    public Text gachaTicketText;

    // Массивы спрайтов для каждой редкости
    public Sprite[] commonSprites;
    public Sprite[] rareSprites;
    public Sprite[] epicSprites;
    public Sprite[] legendarySprites;

    // Количество талонов
    private int gachaTickets = 10;

    void Start()
    {
        // Инициализируем словарь спрайтов в классе Character
        Character.spritesByRarity = new Dictionary<Rarity, Sprite[]>
        {
            { Rarity.Common, commonSprites },
            { Rarity.Rare, rareSprites },
            { Rarity.Epic, epicSprites },
            { Rarity.Legendary, legendarySprites }
        };

        // Устанавливаем заглушку для спрайта
        Character.defaultSprite = commonSprites.Length > 0 ? commonSprites[0] : null;

        // Обновляем текст талонов и состояние кнопки
        UpdateGachaTicketText();
        UpdateRollButtonState();
    }

    // Метод для выполнения ролла гаче
    public void RollGacha()
    {
        if (gachaTickets > 0)
        {
            // Уменьшаем количество талонов
            gachaTickets--;

            // Создаём нового персонажа
            Character newCharacter = new Character();
            newCharacter.GenerateCharacter();

            // Спавним персонажа на игровое поле
            SpawnCharacter(newCharacter);

            // Обновляем текст талонов и состояние кнопки
            UpdateGachaTicketText();
            UpdateRollButtonState();
        }
        else
        {
            Debug.LogError("Недостаточно талонов!");
        }
    }

    // Метод для спавна персонажа
    public void SpawnCharacter(Character character)
    {
        // Создаём объект персонажа
        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        // Присваиваем основной спрайт
        SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>();
        mainSpriteRenderer.sprite = character.sprite;

        // Добавляем контурный спрайт
        AddOutline(characterObject, character.outlineColor, character.sprite);

        // Отображаем информацию о персонаже
        Debug.Log($"Вы получили персонажа: {character.name} ({character.rarity})!");
    }

    // Метод для добавления контура через второй спрайт
    private void AddOutline(GameObject characterObject, Color outlineColor, Sprite baseSprite)
    {
        // Создаём новый объект для контура
        GameObject outlineObject = new GameObject("Outline");
        outlineObject.transform.parent = characterObject.transform;
        outlineObject.transform.localPosition = Vector3.zero;

        // Добавляем Sprite Renderer для контура
        SpriteRenderer outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = baseSprite; // Используем тот же спрайт
        outlineRenderer.color = outlineColor; // Устанавливаем цвет контура
        outlineRenderer.sortingOrder = -1; // Контур находится позади основного спрайта

        // Увеличиваем размер контура
        outlineRenderer.transform.localScale = new Vector3(1.1f, 1.1f, 1f); // Немного увеличиваем спрайт
    }

    // Метод для обновления текста талонов
    private void UpdateGachaTicketText()
    {
        if (gachaTicketText != null)
        {
            gachaTicketText.text = $"Талоны: {gachaTickets}";
        }
    }

    // Метод для обновления состояния кнопки
    private void UpdateRollButtonState()
    {
        if (rollButton != null) // Проверяем, что кнопка существует
        {
            rollButton.interactable = gachaTickets > 0; // Активируем кнопку, если есть талоны
        }
    }

    // Метод для получения талонов
    public void AddGachaTickets(int amount)
    {
        gachaTickets += amount;

        // Обновляем текст талонов и состояние кнопки
        UpdateGachaTicketText();
        UpdateRollButtonState(); // Проверяем, можно ли снова активировать кнопку
    }
}