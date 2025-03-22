using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public string name;
    public Rarity rarity; // Глобальное перечисление Rarity
    public int health;
    public int attack;
    public int defense;
    public int carryWeight; // Переносимый вес
    public int level;       // Текущий уровень
    public int maxLevel;    // Максимальный уровень в зависимости от редкости
    public Sprite sprite;   // Спрайт персонажа
    public Color outlineColor; // Цвет контура для редкости

    // Минимальные статы одинаковы для всех
    private const int MIN_HEALTH = 50;
    private const int MIN_ATTACK = 10;
    private const int MIN_DEFENSE = 5;

    // Словарь для хранения спрайтов по редкостям
    public static Dictionary<Rarity, Sprite[]> spritesByRarity = new Dictionary<Rarity, Sprite[]>();

    // Заглушка для спрайта
    public static Sprite defaultSprite;

    // Метод для генерации случайного спрайта
    public void GenerateSprite(Rarity rarity)
    {
        if (spritesByRarity.ContainsKey(rarity) && spritesByRarity[rarity].Length > 0)
        {
            sprite = spritesByRarity[rarity][Random.Range(0, spritesByRarity[rarity].Length)];
        }
        else
        {
            sprite = defaultSprite; // Используем заглушку, если спрайты не найдены
        }
    }

    // Метод для генерации статов
    public void GenerateStats(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                health = Random.Range(MIN_HEALTH, 100);
                attack = Random.Range(MIN_ATTACK, 20);
                defense = Random.Range(MIN_DEFENSE, 15);
                carryWeight = Random.Range(10, 20);
                maxLevel = 10;
                break;
            case Rarity.Rare:
                health = Random.Range(MIN_HEALTH, 150);
                attack = Random.Range(MIN_ATTACK, 35);
                defense = Random.Range(MIN_DEFENSE, 30);
                carryWeight = Random.Range(20, 30);
                maxLevel = 20;
                break;
            case Rarity.Epic:
                health = Random.Range(MIN_HEALTH, 200);
                attack = Random.Range(MIN_ATTACK, 50);
                defense = Random.Range(MIN_DEFENSE, 50);
                carryWeight = Random.Range(30, 40);
                maxLevel = 30;
                break;
            case Rarity.Legendary:
                health = Random.Range(MIN_HEALTH, 300);
                attack = Random.Range(MIN_ATTACK, 75);
                defense = Random.Range(MIN_DEFENSE, 75);
                carryWeight = Random.Range(40, 50);
                maxLevel = 50;
                break;
        }
    }

    // Массивы имён для каждой редкости
    private Dictionary<Rarity, string[]> namesByRarity = new Dictionary<Rarity, string[]>
    {
        { Rarity.Common, new string[] { "Ivan", "Petr", "Sidr", "Andrey" } },
        { Rarity.Rare, new string[] { "Hunter", "Starker", "Warior", "Survivor", "prospector", "Spiner" } },
        { Rarity.Epic, new string[] { "Iron Maiden", "Ice Warrior", "Flame Defender", "Nuclear Fighter", "Shadow Assassin", "Star Scout" } },
        { Rarity.Legendary, new string[] { "Archangel", "Overman", "King of the Apocalypse", "neko chan", "immortal cultivator", "Mutant God", "Plasma Warrior", "Guardian of the Golden Core", "Пустотный void" } }
    };

    // Метод для генерации имени
    public void GenerateName(Rarity rarity)
    {
        if (namesByRarity.ContainsKey(rarity) && namesByRarity[rarity].Length > 0)
        {
            name = namesByRarity[rarity][Random.Range(0, namesByRarity[rarity].Length)];
        }
        else
        {
            name = "Неизвестный";
        }
    }

    // Метод для генерации цвета контура
    public void GenerateOutlineColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                outlineColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Серый контур
                break;
            case Rarity.Rare:
                outlineColor = new Color(0f, 1f, 1f, 1f); // Голубой контур
                break;
            case Rarity.Epic:
                outlineColor = new Color(0f, 0f, 1f, 1f); // Синий контур
                break;
            case Rarity.Legendary:
                outlineColor = new Color(1f, 0.92f, 0.4f, 1f); // Золотой контур
                break;
        }
    }

    // Метод для полной генерации персонажа
    public void GenerateCharacter()
    {
        rarity = GetRandomRarity();
        GenerateStats(rarity);
        GenerateName(rarity);
        GenerateSprite(rarity);
        GenerateOutlineColor(rarity);
    }

    // Метод для получения случайной редкости
    private Rarity GetRandomRarity()
    {
        float randomValue = Random.Range(0f, 100f);
        if (randomValue < 70f) return Rarity.Common;
        else if (randomValue < 90f) return Rarity.Rare;
        else if (randomValue < 98f) return Rarity.Epic;
        else return Rarity.Legendary;
    }
}