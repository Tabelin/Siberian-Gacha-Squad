using UnityEngine;
using System.Collections.Generic;

// SaveData.cs
[System.Serializable]
public class SaveData
{
    public List<CharacterData> characters = new List<CharacterData>();
    public int gachaTickets; // ��������� ���������� �������
    public int dnaFragments; // ��������� ���������� ���
}

[System.Serializable]
public class GrenadeData
{
    public Grenade.GrenadeType type;
    public float baseDamage;
    public float explosionRadius;
    public float weight;
    public Sprite icon;
    public GameObject modelPrefab;
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
    public float experience;
    public float experienceToNextLevel = 1000f;

    // ���� ���������, ��� �� �������� ������ ��� ��� ��������� � ���� ����������
    public bool isAccepted;

    public Sprite sprite;
}

[System.Serializable]
public struct ResourceData
{
    public ResourceType type;
    public float amount;
}








