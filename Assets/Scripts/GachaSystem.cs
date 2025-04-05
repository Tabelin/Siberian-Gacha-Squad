// GachaSystem.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaSystem : MonoBehaviour
{

    // Singleton ��� ������� � ������ �� ������ ��������
    public static GachaSystem Instance;
    public GameObject characterPrefab;  // ������ ��� ���������
    public Transform spawnPoint;         // ����� ������ ���������
    public Button rollButton;           // ������ ��� ����� ����-������
    public Button summonFiveButton;      // ������ ��� ����� ����-������
    public Button acceptAllButton;      // ������ ��� �������� ���� ���������� 
    public Button exchangeAllButton;     // ������ ��� ������ ���� ���������� �� ���
    public Button getTicketsButton;     // ������ ��� ������ ���� ���������� �� ���   CharacterImage �� ��������
    // UI ��� ����������� ���������� �������
    public Image gachaTicketIcon;
    public Text gachaTicketCountText;
    // UI ��� ����������� ���������� ���
    public Image dnaPieceIcon;
    public Text dnaPieceCountText;

    public Transform characterRowParent;                  // ������������ ������ ��� ���� ����������
    public GameObject characterRowItemPrefab;             // ������ �������� ���� ����������
    // ������ �������� ���� ����������
    public RuntimeAnimatorController commonController;    
    public RuntimeAnimatorController rareController;
    public RuntimeAnimatorController epicController;
    public RuntimeAnimatorController legendaryController;
    // ������� �������� ��� ����������
    public Sprite[] commonCharacterSprites; // ������� ���������� Common
    public Sprite[] rareCharacterSprites;   // ������� ���������� Rare
    public Sprite[] epicCharacterSprites;   // ������� ���������� Epic
    public Sprite[] legendaryCharacterSprites; // ������� ���������� Legendary
    // ����� ������� �������� ��� �����
    public Sprite[] commonBackgroundSprites; // ���� ��� Common
    public Sprite[] rareBackgroundSprites;   // ���� ��� Rare
    public Sprite[] epicBackgroundSprites;   // ���� ��� Epic
    public Sprite[] legendaryBackgroundSprites; // ���� ��� Legendary

    // ������ ���������� ����������
    public List<Character> currentCharacters = new List<Character>();
    private List<GameObject> uiCharacters = new List<GameObject>();             // ������ ������� ��������� ����
    private int gachaTickets = 10;                                                   // ���������� �������
    private int dnaFragments = 0;
    // ��������� ���������� ����� ���
    private Dictionary<Rarity, int> dnaCostByRarity = new Dictionary<Rarity, int>
    {
        { Rarity.Common, 10 },
        { Rarity.Rare, 50 },
        { Rarity.Epic, 200 },
        { Rarity.Legendary, 1000 }
    };

    private const int maxCharactersInRow = 6;     // ��������� ���������� ����� ���
    

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
        if (characterRowItemPrefab == null || characterRowParent == null || spawnPoint == null ||
            rollButton == null || summonFiveButton == null || acceptAllButton == null ||
            exchangeAllButton == null || getTicketsButton == null ||
            commonCharacterSprites == null || rareCharacterSprites == null || epicCharacterSprites == null || legendaryCharacterSprites == null)
        {
            Debug.LogError("���������� ��������� ��� ������!");
            return;
        }
        // ���������, ��� ������� �������� �� ������
        if (commonCharacterSprites.Length == 0 || rareCharacterSprites.Length == 0 || epicCharacterSprites.Length == 0 || legendaryCharacterSprites.Length == 0)
        {
            Debug.LogError("�� ��� ������� ���� ��������� � ���������!");
            return;
        }
        // �������������� ������� �������� � ������ Character
        Character.spritesByRarity = new Dictionary<Rarity, Sprite[]>
        {
            { Rarity.Common, commonCharacterSprites },
            { Rarity.Rare, rareCharacterSprites },
            { Rarity.Epic, epicCharacterSprites },
            { Rarity.Legendary, legendaryCharacterSprites }
        };
        // �������������� ������� �������� � ������ Character
        Character.defaultSprite = commonCharacterSprites.Length > 0 ? commonCharacterSprites[0] : null;
        // ��������� ����� ������� � ���
        UpdateGachaTicketCount();
        UpdateDnaFragmentCount();
        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
        LoadCharacters(); // ��������� ���������� ���������� ��� ������
    }

    public Sprite GetBackgroundSprite(Rarity rarity)
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
    // ����� ��� ��������� ������� ��������� �� ��������
    public Sprite GetCharacterSprite(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonCharacterSprites.Length > 0 ? commonCharacterSprites[0] : null,
            Rarity.Rare => rareCharacterSprites.Length > 0 ? rareCharacterSprites[0] : null,
            Rarity.Epic => epicCharacterSprites.Length > 0 ? epicCharacterSprites[0] : null,
            Rarity.Legendary => legendaryCharacterSprites.Length > 0 ? legendaryCharacterSprites[0] : null,
            _ => null
        };
    }
    // ����� ��� ���������� ����������
    public void SaveCharacters()
    {
        SaveData saveData = new SaveData();

        foreach (Character character in currentCharacters)
        {
            CharacterData data = new CharacterData
            {
                name = character.name,
                rarity = character.rarity,
                health = character.health,
                attack = character.attack,
                defense = character.defense,
                carryWeight = character.carryWeight,
                level = character.level,
                maxLevel = character.maxLevel,
                isAccepted = true // �������� ��������� ���������� ��� true
            };
            saveData.characters.Add(data);
        }

        // ��������� ���������� �� UI-����
        foreach (GameObject rowItem in uiCharacters)
        {
            CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
            if (item != null)
            {
                CharacterData data = new CharacterData
                {
                    name = item.character.name,
                    rarity = item.character.rarity,
                    health = item.character.health,
                    attack = item.character.attack,
                    defense = item.character.defense,
                    carryWeight = item.character.carryWeight,
                    level = item.character.level,
                    maxLevel = item.character.maxLevel,
                    isAccepted = false // ��������� � UI-���� ���������� ��� false
                };
                saveData.characters.Add(data);
            }
        }

        // ��������� ������ � ���
        saveData.gachaTickets = gachaTickets;
        saveData.dnaFragments = dnaFragments;

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("SaveData", json);

        Debug.Log("�����, ������ � ��� ������� ���������!");
    }

    // ����� ��� �������� ������
    public void LoadCharacters()
    {
        string json = PlayerPrefs.GetString("SaveData", "");
        if (!string.IsNullOrEmpty(json))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // ��������������� �������� ����������
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
                    maxLevel = data.maxLevel,
                    sprite = GetCharacterSprite(data.rarity) // ��������������� ������ ���������
                };

                // ���� �������� ��� ������
                if (data.isAccepted)
                {
                    currentCharacters.Add(loadedCharacter);
                    Debug.Log($"�������� �������� ��������: {loadedCharacter.name} ({loadedCharacter.rarity})");
                }
                // ���� �������� ��� � UI-����
                else
                {
                    AddCharacterToRowAndSetupUI(loadedCharacter); // ���������� ��������������� �����
                    Debug.Log($"�������� �������� �� UI-����: {loadedCharacter.name} ({loadedCharacter.rarity})");
                }
            }

            // ��������������� ������ � ���
            gachaTickets = saveData.gachaTickets;
            dnaFragments = saveData.dnaFragments;

            UpdateGachaTicketCount();
            UpdateDnaFragmentCount();
            UpdateAcceptAllButtonState();
            UpdateExchangeAllButtonState();
        }
        else
        {
            Debug.Log("��� ���������� ������.");
        }
    }
    // ����� ��� ���������� ����� ����
    public void RollGacha(int count = 1)
    {
        if (gachaTickets >= count && CanAddCharacters(count))
        {
            // ����� ��� ���������� ����� ����
            gachaTickets -= count;
            UpdateGachaTicketCount();

            for (int i = 0; i < count; i++)
            {
                Character newCharacter = new Character();
                newCharacter.GenerateCharacter();       // ��� ������������� �������� (��������� ����)
                AddCharacterToRowAndSetupUI(newCharacter);        // ��������� ��������� � ���
            }
            SaveCharacters();// ������������� ��������� ����� ����� �����
            // ���������� ��� ����������
            CenterCharacterRow();
            // ��������� ��������� ������
            UpdateRollButtonState();
            UpdateSummonFiveButtonState();
        }
        else
        {
            Debug.LogError("������������ ������� ��� ��������� ����� ����������!");
        }
    }
    // ��������� ��������� ������
    private bool CanAddCharacters(int count)
    {
        return uiCharacters.Count + count <= maxCharactersInRow;
    }
    // ����� ��� ���������� ��������� � UI-���
    private void AddCharacterToRowAndSetupUI(Character character)
    {
        if (characterRowItemPrefab == null || characterRowParent == null)
        {
            Debug.LogError("���������� ��������� CharacterRowItemPrefab � CharacterRowParent!");
            return;
        }
        // ������ ����� ������� ����
        GameObject rowItem = Instantiate(characterRowItemPrefab, characterRowParent);

        if (rowItem != null)
        {
            // ������� ��������� CharacterRowItem
            CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
            if (item != null)
            {
                // ������������� ������ ���������
                item.SetCharacter(character, rowItem, OnAcceptCharacter, OnExchangeForDNA);

                // ���� ������������� ������ ����
                SetBackgroundSprite(rowItem, character.rarity);

                // ��������� ������ � ������ uiCharacters
                uiCharacters.Add(rowItem);
            }
            else
            {
                Debug.LogError("�� ������ ��������� CharacterRowItem!");
            }

        }
        else
        {
            Debug.LogError("�� ������� ������� CharacterRowItem!");
        }
        // ���������� ��� ����� ���������� ���������
        CenterCharacterRow();
        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    // ����� ��� ������������� ���� ����������
    private void CenterCharacterRow()
    {
        if (uiCharacters.Count == 0 || characterRowParent == null)
        {
            Debug.LogWarning("��� ���������� ��� �������������!");
            return;
        }
        // ������ ������ �������� = 150f
        float totalWidth = uiCharacters.Count * 150f;
        float offset = -(totalWidth / 2f);

        for (int i = 0; i < uiCharacters.Count; i++)
        {
            if (uiCharacters[i] != null)
            {
                uiCharacters[i].transform.localPosition = new Vector3(offset + i * 150f, 0, 0);
            }
            else
            {
                Debug.LogError("��������� null � ������ uiCharacters!");
            }
        }
    }
    // ����� ��� �������� ���������
    private void OnAcceptCharacter(GameObject rowItem)
    {
        if (rowItem == null)
        {
            Debug.LogError("���������� rowItem �������� null!");
            return;
        }

        CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
        if (item == null)
        {
            Debug.LogError("�� ������ ��������� CharacterRowItem!");
            return;
        }

        currentCharacters.Add(item.character); // ��������� ��������� � �����
        SaveCharacters(); // ��������� ����� ����� �������� ���������

        SpawnCharacter(item.character); // ������� ��������� �� ������� ����
        RemoveCharacterFromRow(rowItem);// ������� ������� �� ����
        
        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // ����� ��� ������ ��������� �� ���
    private void OnExchangeForDNA(GameObject rowItem)
    {
        if (rowItem == null)
        {
            Debug.LogError("���������� rowItem �������� null!");
            return;
        }

        CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
        if (item == null)
        {
            Debug.LogError("�� ������ ��������� CharacterRowItem!");
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
        SaveCharacters();
        // ������� ������� �� ����
        RemoveCharacterFromRow(rowItem);
        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // ����� ��� �������� ��������� �� UI-����
    private void RemoveCharacterFromRow(GameObject rowItem)
    {
        if (rowItem == null)
        {
            Debug.LogError("������� ������� null ������� �� ����!");
            return;
        }
        // ������� �������� ������ � ��������� ����
        GameObject backgroundObject = rowItem.transform.Find("Background")?.gameObject;
        if (backgroundObject != null)
        {
            Destroy(backgroundObject);// ������� ������ �������� ����
        }

        uiCharacters.Remove(rowItem);
        Destroy(rowItem);
        // ������������� ���
        CenterCharacterRow();
        SaveCharacters(); // ��������� ����� ����� �������� ���������

        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // ����� ��� ������ ���������
    public void SpawnCharacter(Character character)
    {
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogError("���������� ��������� CharacterPrefab � SpawnPoint!");
            return;
        }

        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        if (characterObject != null)
        {
            SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>(); // ����������� �������� ������
            mainSpriteRenderer.sprite = character.sprite;
            // ��������� ������������� ��� ��� ���������
            AddAnimatedBackground(characterObject, character.rarity);

            Debug.Log($"�� ������� ���������: {character.name} ({character.rarity})!");
        }
        else
        {
            Debug.LogError("�� ������� ������� CharacterPrefab!");
        }
    }
    // ����� ��� ���������� �������������� ����
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
        // ������ ������ ��� ����
        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = GetDefaultBackgroundSprite(rarity);
        // ��������� Animator
        Animator animator = backgroundObject.AddComponent<Animator>();
        SetAnimatorController(animator, rarity);
        // ��������� ������� ����
        backgroundRenderer.transform.localScale = new Vector3(1f, 1f, 0.5f);
        backgroundRenderer.sortingOrder = 0;
    }
    // ����� ��� ��������� �������� ����
    private void SetAnimatorController(Animator animator, Rarity rarity)
    {
        if (animator == null)
        {
            Debug.LogError("���������� animator �������� null!");
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
            Debug.LogWarning($"������������ ���������� ��� �������� {rarity} �� ������!");
        }
    }

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

    private void UpdateRollButtonState()
    {
        if (rollButton != null)
        {
            rollButton.interactable = gachaTickets > 0 && CanAddCharacters(1);
        }
        else
        {
            Debug.LogError("RollButton �� ��������!");
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
            Debug.LogError("SummonFiveButton �� ��������!");
        }
    }

    private void UpdateAcceptAllButtonState()
    {
        if (acceptAllButton != null)
        {
            acceptAllButton.interactable = uiCharacters.Count > 0;
        }
        else
        {
            Debug.LogError("AcceptAllButton �� ��������!");
        }
    }

    private void UpdateExchangeAllButtonState()
    {
        if (exchangeAllButton != null)
        {
            exchangeAllButton.interactable = uiCharacters.Count > 0;
        }
        else
        {
            Debug.LogError("ExchangeAllButton �� ��������!");
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
            Debug.LogError("GetTicketsButton �� ��������!");
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
            Debug.LogError("GachaTicketCountText �� ��������!");
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
            Debug.LogError("DnaPieceCountText �� ��������!");
        }
    }

    public void AcceptAllCharacters()
    {
        foreach (GameObject rowItem in uiCharacters.ToArray())
        {
            OnAcceptCharacter(rowItem);
        }
        SaveCharacters();
        // ��������� ��������� ������ ����� ��������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    public void ExchangeAllCharactersForDNA()
    {
        foreach (GameObject rowItem in uiCharacters.ToArray())
        {
            OnExchangeForDNA(rowItem);
        }
        SaveCharacters();
        // ��������� ��������� ������ ����� ��������
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
            Debug.LogError($"��������� ��� �������� {rarity} �� ����������!");
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

            AddCharacterToRowWithBackground(guaranteedCharacter);
            SaveCharacters();

            Debug.Log($"�� ������ ���������: {guaranteedCharacter.name} ({guaranteedCharacter.rarity})!");
        }
        else
        {
            Debug.LogError("������������ �������� ��� ��� ��������� ����� ����������!");
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
    // ����� ��� ���������� ��������� � UI-��� � ���������� �����
    private void AddCharacterToRowWithBackground(Character character)
    {
        if (characterRowItemPrefab == null || characterRowParent == null)
        {
            Debug.LogError("���������� ��������� CharacterRowItemPrefab � CharacterRowParent!");
            return;
        }

        // ������ ������� ����
        GameObject rowItem = Instantiate(characterRowItemPrefab, characterRowParent);

        if (rowItem != null)
        {
            // ������� ��������� CharacterRowItem
            CharacterRowItem item = rowItem.GetComponent<CharacterRowItem>();
            if (item != null)
            {
                // ������������� ������ ���������
                item.SetCharacter(character, rowItem, OnAcceptCharacter, OnExchangeForDNA);

                // ���� ������������� ������ ����
                SetBackgroundSprite(rowItem, character.rarity);

                // ��������� ������ � ������ uiCharacters
                uiCharacters.Add(rowItem);
            }
            else
            {
                Debug.LogError("�� ������ ��������� CharacterRowItem!");
            }
        }
        else
        {
            Debug.LogError("�� ������� ������� CharacterRowItem!");
        }

        CenterCharacterRow();
    }

    

    // ����� ��� ��������� ������� ����
    private void SetBackgroundSprite(GameObject rowItem, Rarity rarity)
    {
        if (rowItem == null)
        {
            Debug.LogError("���������� rowItem �������� null!");
            return;
        }

        // ������� ��������� backgroundImage
        Image backgroundImage = rowItem.transform.Find("BackgroundImage")?.GetComponent<Image>();
        if (backgroundImage != null)
        {
            // �������� ������ ���� �� GachaSystem
            Sprite backgroundSprite = GetBackgroundSprite(rarity);
            if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite; // ������������� ������ ����
            }
            else
            {
                Debug.LogWarning($"������ ���� ��� �������� {rarity} �� ������!");
            }
        }
        else
        {
            Debug.LogError("BackgroundImage �� ��������!");
        }
    }

}