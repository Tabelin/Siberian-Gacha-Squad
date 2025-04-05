using UnityEngine;
using System.Collections.Generic;

// SaveData.cs
[System.Serializable]
public class SaveData
{
    public List<CharacterData> characters = new List<CharacterData>();
    public int gachaTickets; // Сохраняем количество талонов
    public int dnaFragments; // Сохраняем количество ДНК
}

[System.Serializable]
public class CharacterData
{
    public string name;
    public Rarity rarity;
    public int health;
    public int attack;
    public int defense;
    public int carryWeight;
    public int level;
    public int maxLevel;

    // Новое поле: указывает, был ли персонаж принят
    public bool isAccepted;
}