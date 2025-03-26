// CharacterRowItem.cs
using UnityEngine;
using UnityEngine.UI;

public class CharacterRowItem : MonoBehaviour
{
    // Компоненты UI для отображения информации
    public Image characterImage; // Мини-спрайт персонажа
    public Text characterNameText; // Имя персонажа
    public Text rarityText; // Редкость персонажа
    public Text statsText; // Статы персонажа

    public Character character; // Хранит данные персонажа
    public GameObject rowItem; // Ссылка на сам элемент ряда

    // Метод для установки персонажа
    public void SetCharacter(Character newCharacter, GameObject rowItemObj, System.Action<GameObject> onAccept, System.Action<GameObject> onExchange)
    {
        character = newCharacter;
        rowItem = rowItemObj;

        // Настройка UI
        characterImage.sprite = character.sprite;
        characterNameText.text = character.name;
        rarityText.text = character.rarity.ToString();
        statsText.text = $"HP: {character.health}\nATK: {character.attack}\nDEF: {character.defense}";

        // Находим кнопки через Transform.Find
        Button acceptButton = rowItem.transform.Find("AcceptButton").GetComponent<Button>();
        Button exchangeButton = rowItem.transform.Find("ExchangeButton").GetComponent<Button>();

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(() => onAccept(rowItem));
        }

        if (exchangeButton != null)
        {
            exchangeButton.onClick.AddListener(() => onExchange(rowItem));
        }
    }
}