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

    // ������ ��������� ����������
    private List<GameObject> selectedCharacters = new List<GameObject>();

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

                if (data.isAccepted)
                {
                    SpawnCharacter(loadedCharacter, spawnPoints[i]);
                }
                else
                {
                    Debug.Log($"�������� {loadedCharacter.name} ({loadedCharacter.rarity}) ��������� � UI-���� � �� ��������� �� ����.");
                }
            }
        }
        else
        {
            Debug.Log("��� ����������� ������ � ����������.");
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
        backgroundRenderer.transform.localScale = new Vector3(1f, 1f, 1f); // ��� ���� ������ ��������� �������
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

    // ����� ��� ������ ���������
    public void SelectCharacter(bool isShiftPressed = false)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Character"))
            {
                if (isShiftPressed)
                {
                    // ���� ������ ������� Shift, ���������/������� ��������� �� ������ ���������
                    if (selectedCharacters.Contains(hitObject))
                    {
                        selectedCharacters.Remove(hitObject);
                        Debug.Log($"�������� {hitObject.name} ����� �� ������.");
                    }
                    else
                    {
                        selectedCharacters.Add(hitObject);
                        Debug.Log($"�������� {hitObject.name} �������� � �����.");
                    }
                }
                else
                {
                    // ���� Shift �� ������, ������� ���������� ����� � �������� ������ ���������
                    selectedCharacters.Clear();
                    selectedCharacters.Add(hitObject);
                    Debug.Log($"������ ��������: {hitObject.name}");
                }
            }
        }
        else
        {
            // ���� �������� �� �� ���������, ������� �����
            if (!isShiftPressed)
            {
                DeselectAllCharacters();
            }
        }
    }

    // ����� ��� ������ ������ ���� ����������
    private void DeselectAllCharacters()
    {
        if (selectedCharacters.Count > 0)
        {
            foreach (GameObject character in selectedCharacters)
            {
                CharacterManager manager = character.GetComponent<CharacterManager>();
                if (manager != null)
                {
                    // ���� �������� ��� ������, �� �� �������� ������ ��������, ������������ ��������������
                    if (manager.isIdle && !manager.isAttacking && !manager.isGathering)
                    {
                        manager.StartPatrolling(); // ������������ ��������������
                    }
                }
            }

            selectedCharacters.Clear();
            Debug.Log("����� ���������� ������.");
        }
    }


    // ����� ��� �������� ������ ��������� ����������
    public void SendCommand(string command)
    {
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("����� �� ������!");
            return;
        }

        foreach (GameObject character in selectedCharacters)
        {
            CharacterManager manager = character.GetComponent<CharacterManager>();
            if (manager != null)
            {
                switch (command)
                {
                    case "Patrol":
                        manager.StartPatrolling();
                        break;

                    case "Attack":
                        manager.StartAttack();
                        break;

                    case "Gather":
                        manager.StartGathering();
                        break;

                    case "TakeControl":
                        manager.TakeControl();
                        break;

                    default:
                        Debug.LogWarning($"����������� �������: {command}");
                        break;
                }
            }
            else
            {
                Debug.LogError("��������� CharacterManager �� ������!");
            }
        }
    }
    // ����� ��� ����������� ��������� ���������� �� ����� ����� ����
    public void SendMoveCommandToSelectedCharacters()
    {
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("����� �� ������ ��� ��������!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 targetPosition = hit.point;

            foreach (GameObject character in selectedCharacters)
            {
                CharacterManager manager = character.GetComponent<CharacterManager>();
                if (manager != null)
                {
                    manager.MoveToTarget(targetPosition); // ���������� ������� �� ��������

                    // ���� �������� ����� ������ ���������, ������������� �� ����
                    if (manager.isAttacking || manager.isGathering)
                    {
                        Debug.LogWarning($"�������� {character.name} ��������� ������ �������� ��� ���������� ������� ������."); 
                    }
                }
                else
                {
                    Debug.LogError("��������� CharacterManager �� ������!");
                }
            }
        }
        else
        {
            Debug.LogWarning("���� ���� �� ����� �� ����������� �����!");
        }
    }



    // ����� ��� ���������� ����������� ����������
    private void UpdateCharacterRotation()
    {
        foreach (GameObject character in spawnedCharacters)
        {
            if (character != null && Camera.main != null)
            {
                AlignCharacterToCamera(character);
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

        Vector3 direction = (Camera.main.transform.position - characterObject.transform.position).normalized;
        float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(-15f, angleY, 0f);
        characterObject.transform.rotation = Quaternion.Lerp(characterObject.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void Update()
    {
        // ����� ��������� (������� ����)
        if (Input.GetMouseButtonDown(0))
        {
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            SelectCharacter(isShiftPressed);
        }

        // ����� ������ ��� ����� ��� ����������
        if (Input.GetMouseButtonUp(0) && !IsClickOnCharacter())
        {
            DeselectAllCharacters();
        }

        // ����������� ��������� ���������� �� ����� ����� ���� (������ ������ ����)
        if (Input.GetMouseButtonDown(1)) // ��������� ���� ������ ������� ����
        {
            SendMoveCommandToSelectedCharacters(); // �������� ����� ��� ����������� ����������
        }

        // �������� ������ ��������� ����������
        if (Input.GetKeyDown(KeyCode.T)) // ���������
        {
            SendCommand("Attack");
        }

        if (Input.GetKeyDown(KeyCode.G)) // �������� �������
        {
            SendCommand("Gather");
        }

        if (Input.GetKeyDown(KeyCode.C)) // ����� ��������
        {
            SendCommand("TakeControl");
        }

        if (Input.GetKeyDown(KeyCode.P)) // ������ ��������������
        {
            SendCommand("Patrol");
        }

        // ������ ���� ��������� ����������� ����������
        UpdateCharacterRotation();
    }

    // ��������, ����� �� ���� �� ���������
    private bool IsClickOnCharacter()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject.CompareTag("Character");
        }

        return false;
    }
}