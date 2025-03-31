// CharacterRowItem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRowItem : MonoBehaviour
{
    // Отображение информации о персонаже
    public Image characterImage; // Мини-спрайт персонажа
    public Text characterNameText; // Имя персонажа
    public Text rarityText; // Редкость персонажа
    public Text statsText; // Статы персонажа

    // Кнопки действий
    public Button acceptButton; // Принять персонажа
    public Button exchangeButton; // Обменять на ДНК

    // Переменная для анимированного фона
    public Image backgroundImage; // Фоновая анимация

    // Массивы спрайтов для фона каждой редкости (добавляем сюда)
    public List<Sprite> commonBackgroundSprites = new List<Sprite>();
    public List<Sprite> rareBackgroundSprites = new List<Sprite>();
    public List<Sprite> epicBackgroundSprites = new List<Sprite>();
    public List<Sprite> legendaryBackgroundSprites = new List<Sprite>();

    // Данные персонажа
    public Character character;

    // Ссылка на элемент ряда
    public GameObject rowItem;

    // Метод для установки персонажа
    public void SetCharacter(Character newCharacter, GameObject rowItemObj, System.Action<GameObject> onAccept, System.Action<GameObject> onExchange)
    {
        if (newCharacter == null || rowItemObj == null)
        {
            Debug.LogError("Переданный персонаж или объект ряда является null!");
            return;
        }

        character = newCharacter;
        rowItem = rowItemObj;

        // Настройка UI
        if (characterImage != null)
        {
            characterImage.sprite = character.sprite; // Устанавливаем спрайт персонажа

        }
        else
        {
            Debug.LogWarning("CharacterImage не назначен!");
        }

        if (characterNameText != null)
        {
            characterNameText.text = character.name; // Устанавливаем имя
        }
        else
        {
            Debug.LogWarning("CharacterNameText не назначен!");
        }

        if (rarityText != null)
        {
            rarityText.text = character.rarity.ToString(); // Устанавливаем редкость
        }
        else
        {
            Debug.LogWarning("RarityText не назначен!");
        }

        if (statsText != null)
        {
            statsText.text = $"HP: {character.health}\nATK: {character.attack}\nDEF: {character.defense}"; // Устанавливаем статы
        }
        else
        {
            Debug.LogWarning("StatsText не назначен!");
        }
        // Устанавливаем начальную прозрачность фона
        Color currentColor = backgroundImage.color;
        backgroundImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f); // Alpha = 0.5 (50% прозрачности)

        // Запускаем анимацию фона
        if (backgroundImage != null)
        {
            StartCoroutine(AnimateBackground(character.rarity));
        }
        else
        {
            Debug.LogWarning("BackgroundImage не назначен!");
        }

        // Настройка событий кнопок
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(() => onAccept(rowItem));
        }
        else
        {
            Debug.LogWarning("AcceptButton не назначен!");
        }

        if (exchangeButton != null)
        {
            exchangeButton.onClick.AddListener(() => onExchange(rowItem));
        }
        else
        {
            Debug.LogWarning("ExchangeButton не назначен!");
        }
    }

    // Корутина для воспроизведения анимации фона
    private IEnumerator AnimateBackground(Rarity rarity)
    {
        if (backgroundImage == null)
        {
            yield break;
        }

        // Получаем соответствующие спрайты фона
        List<Sprite> backgroundSprites = GetBackgroundSprites(rarity);
        if (backgroundSprites == null || backgroundSprites.Count == 0)
        {
            Debug.LogWarning($"Анимация фона для редкости {rarity} не найдена!");
            yield break;
        }

        while (true)
        {
            foreach (Sprite frame in backgroundSprites)
            {
                backgroundImage.sprite = frame; // Изменяем спрайт фона
                yield return new WaitForSeconds(0.1f); // Время между кадрами
            }
        }
    }

    // Метод для получения спрайтов фона по редкости
    private List<Sprite> GetBackgroundSprites(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonBackgroundSprites,
            Rarity.Rare => rareBackgroundSprites,
            Rarity.Epic => epicBackgroundSprites,
            Rarity.Legendary => legendaryBackgroundSprites,
            _ => null
        };
    }
}