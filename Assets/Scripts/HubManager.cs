// HubManager.cs
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // Singleton ��� ������� � HubManager
    public static HubManager Instance;

    // ������ ��� ���������
    public GameObject characterPrefab;

    // ��� ��� ����� ������
    public string spawnPointTag = "SpawnPoint";

    // ������ ���������� ��� �����
    public RuntimeAnimatorController commonBackgroundController;
    public RuntimeAnimatorController rareBackgroundController;
    public RuntimeAnimatorController epicBackgroundController;
    public RuntimeAnimatorController legendaryBackgroundController;

    // ������� �������� ��� ���������� � �����
    public Sprite[] commonSprites;
    public Sprite[] rareSprites;
    public Sprite[] epicSprites;
    public Sprite[] legendarySprites;

    public Sprite[] commonBackgroundSprites;
    public Sprite[] rareBackgroundSprites;
    public Sprite[] epicBackgroundSprites;
    public Sprite[] legendaryBackgroundSprites;

    // ������ ���������� ����������
    private List<GameObject> spawnedCharacters = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ������������ ����������� ����� �������
        }
        else
        {
            Destroy(gameObject); // ���������� �������� �������
        }
    }

    void Start()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("���������� ��������� CharacterPrefab!");
            return;
        }

        // ������� ��� ����� ������ �� ����
        Transform[] spawnPoints = FindSpawnPoints();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("��� ����� ������ � ������� �����!");
            return;
        }

        // ��������� ���������� �� SaveData
        LoadCharactersFromSaveData(spawnPoints);
    }

    void Update()
    {
        // ������ ���� ��������� ����������� ����������
        UpdateCharacterRotation();
    }

    // ����� ��� ���������� ����������� ����������
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

    // ����� ��� �������� ��������� � ������
    private void AlignCharacterToCamera(GameObject characterObject)
    {
        if (characterObject == null || Camera.main == null)
        {
            Debug.LogError("���������� ������ ��� �������� ������ �������� null!");
            return;
        }

        // ���������� ����������� �� ��������� � ������
        Vector3 direction = (Camera.main.transform.position - characterObject.transform.position).normalized;

        // ��������� ���� �������� ������ �� ��� Y
        float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // ������������ ���������
        Quaternion targetRotation = Quaternion.Euler(-15f, angleY, 0f); // 15f � ��������� ���� ������� �����

        // ��������� ������� ������� (�����������)
        characterObject.transform.rotation = Quaternion.Lerp(characterObject.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // ����� ��� ������ ����� ������
    private Transform[] FindSpawnPoints()
    {
        List<Transform> spawnPoints = new List<Transform>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(spawnPointTag))
        {
            spawnPoints.Add(obj.transform);
        }

        return spawnPoints.ToArray();
    }

    // ����� ��� �������� ���������� �� SaveData
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
                    sprite = GetCharacterSprite(data.rarity) // �������� ������ ���������
                };

                // ���� �������� ��� ������
                if (data.isAccepted)
                {
                    SpawnCharacter(loadedCharacter, spawnPoints[i]);
                }
                // ���� �������� ��� � UI-����, ���������� ��� �� ����
                else
                {
                    Debug.Log($"�������� {loadedCharacter.name} ({loadedCharacter.rarity}) ��������� � UI-���� � �� ��������� �� ����.");
                }
            }
        }
        else
        {
            Debug.Log("��� ���������� ������ � ����������.");
        }
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
        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, Quaternion.identity);

        if (characterObject != null)
        {
            // ��������� ��������� ������� ���������
            SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>();
            if (mainSpriteRenderer != null)
            {
                mainSpriteRenderer.sprite = character.sprite;
            }
            else
            {
                Debug.LogError("SpriteRenderer �� ������!");
            }

            // ��������� ������������� ���
            AddAnimatedBackground(characterObject, character.rarity);

            // ��������� ������ �� ����������� ���������
            spawnedCharacters.Add(characterObject);

            Debug.Log($"�������� {character.name} ({character.rarity}) ������� ��������� � �����!");
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

    // ����� ��� ��������� ������� ��������� �� ��������
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

    // ����� ��� ��������� ������������ ������� ����
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