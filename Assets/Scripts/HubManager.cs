// HubManager.cs
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // ������ ��� ���������
    public GameObject characterPrefab;

    // ������ ����� ������
    public Transform[] spawnPoints;

    // ������ ���������� ��� �����
    public RuntimeAnimatorController commonBackgroundController;
    public RuntimeAnimatorController rareBackgroundController;
    public RuntimeAnimatorController epicBackgroundController;
    public RuntimeAnimatorController legendaryBackgroundController;

    void Start()
    {
        if (characterPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("���������� ��������� CharacterPrefab � SpawnPoints!");
            return;
        }

        // ��������� ���������� ���������� �� PlayerPrefs
        List<Character> savedCharacters = LoadCharactersFromPlayerPrefs();

        // ������� ������� ��������� � ����� ����
        for (int i = 0; i < savedCharacters.Count && i < spawnPoints.Length; i++)
        {
            SpawnCharacter(savedCharacters[i], spawnPoints[i]);
        }
    }

    // ����� ��� �������� ���������� �� PlayerPrefs
    private List<Character> LoadCharactersFromPlayerPrefs()
    {
        List<Character> characters = new List<Character>();

        string json = PlayerPrefs.GetString("SaveData", "");
        if (!string.IsNullOrEmpty(json))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

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
                    maxLevel = data.maxLevel
                };

                characters.Add(loadedCharacter); // ��������� ��������� � ������
                Debug.Log($"�������� ��������: {loadedCharacter.name} ({loadedCharacter.rarity})");
            }
        }
        else
        {
            Debug.Log("��� ���������� ����������.");
        }

        return characters;
    }

    // ����� ��� ������ ���������
    private void SpawnCharacter(Character character, Transform spawnPoint)
    {
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogError("���������� ��������� CharacterPrefab ��� SpawnPoint!");
            return;
        }

        // ������ ������ ���������
        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        if (characterObject != null)
        {
            // ��������� ��������� ������� ���������
            SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>();
            mainSpriteRenderer.sprite = character.sprite;

            // ��������� ������������� ��� ����� Animator
            AddAnimatedBackground(characterObject, character.rarity);

            Debug.Log($"�������� {character.name} ({character.rarity}) ������� ��������� � ����!");
        }
        else
        {
            Debug.LogError("�� ������� ������� CharacterPrefab!");
        }
    }

    // ����� ��� ���������� �������������� ���� ����� Animator
    private void AddAnimatedBackground(GameObject parentObject, Rarity rarity)
    {
        if (parentObject == null)
        {
            Debug.LogError("���������� parentObject �������� null!");
            return;
        }

        // ������ ������ ��� ����
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.parent = parentObject.transform;
        backgroundObject.transform.localPosition = Vector3.zero;

        // ��������� Sprite Renderer ��� ����
        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = GetDefaultBackgroundSprite(rarity);
        backgroundRenderer.sortingOrder = -1; // ��� ��������� ������ ��������� �������

        // ��������� Animator ���������
        Animator animator = backgroundObject.AddComponent<Animator>();
        SetAnimatorController(animator, rarity);

        // ��������� ������� ����
        backgroundRenderer.transform.localScale = new Vector3(1.2f, 1.2f, 1f); // ��� ���� ������ ��������� �������
    }

    // ����� ��� ��������� ������������ ������� ����
    private Sprite GetDefaultBackgroundSprite(Rarity rarity)
    {
        // �������� ������ ���� ����� GachaSystem
        if (GachaSystem.Instance != null)
        {
            return GachaSystem.Instance.GetBackgroundSprite(rarity);
        }
        else
        {
            Debug.LogError("GachaSystem �� ������!");
            return null;
        }
    }

    // ����� ��� ��������� Animator Controller
    private void SetAnimatorController(Animator animator, Rarity rarity)
    {
        if (animator == null)
        {
            Debug.LogError("���������� animator �������� null!");
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
            Debug.LogWarning($"������������ ���������� ��� �������� {rarity} �� ������!");
        }
    }

    
}