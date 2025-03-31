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
    public Button getTicketsButton;     // ������ ��� ������ ���� ���������� �� ���
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
    // ������� �������� ��� ������ ��������
    public Sprite[] commonSprites;
    public Sprite[] rareSprites;
    public Sprite[] epicSprites;
    public Sprite[] legendarySprites;

    private List<GameObject> currentCharacters = new List<GameObject>();             // ������ ������� ��������� ����
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
        // �������������� Singleton
        Instance = this;
    }

    void Start()
    {
        if (characterRowItemPrefab == null || characterRowParent == null || spawnPoint == null ||
            rollButton == null || summonFiveButton == null || acceptAllButton == null ||
            exchangeAllButton == null || getTicketsButton == null ||
            commonSprites == null || rareSprites == null || epicSprites == null || legendarySprites == null)
        {
            Debug.LogError("���������� ��������� ��� ������!");
            return;
        }
        // ���������, ��� ������� �������� �� ������
        if (commonSprites.Length == 0 || rareSprites.Length == 0 || epicSprites.Length == 0 || legendarySprites.Length == 0)
        {
            Debug.LogError("�� ��� ������� ���� ��������� � ���������!");
            return;
        }
        // �������������� ������� �������� � ������ Character
        Character.spritesByRarity = new Dictionary<Rarity, Sprite[]>
        {
            { Rarity.Common, commonSprites },
            { Rarity.Rare, rareSprites },
            { Rarity.Epic, epicSprites },
            { Rarity.Legendary, legendarySprites }
        };
        // �������������� ������� �������� � ������ Character
        Character.defaultSprite = commonSprites.Length > 0 ? commonSprites[0] : null;
        // ��������� ����� ������� � ���
        UpdateGachaTicketCount();
        UpdateDnaFragmentCount();
        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
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
                AddCharacterToRow(newCharacter);        // ��������� ��������� � ���
            }
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
        return currentCharacters.Count + count <= maxCharactersInRow;
    }

    private void AddCharacterToRow(Character character)
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
            rowItem.GetComponent<CharacterRowItem>().SetCharacter(character, rowItem, OnAcceptCharacter, OnExchangeForDNA);
            currentCharacters.Add(rowItem);   // ��������� ������ �� ������� ����
            // ��������� ������������� ��� ��� ���������
            AddAnimatedBackground(rowItem, character.rarity);
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
        if (currentCharacters.Count == 0 || characterRowParent == null)
        {
            Debug.LogWarning("��� ���������� ��� �������������!");
            return;
        }
        // ������ ������ �������� = 150f
        float totalWidth = currentCharacters.Count * 150f;
        float offset = -(totalWidth / 2f);

        for (int i = 0; i < currentCharacters.Count; i++)
        {
            if (currentCharacters[i] != null)
            {
                currentCharacters[i].transform.localPosition = new Vector3(offset + i * 150f, 0, 0);
            }
            else
            {
                Debug.LogError("��������� null � ������ currentCharacters!");
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
        // ������� ������� �� ����
        RemoveCharacterFromRow(rowItem);
        // ��������� ��������� ������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }
    // ����� ��� �������� ��������� �� ����
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

        currentCharacters.Remove(rowItem);
        Destroy(rowItem);
        // ������������� ���
        CenterCharacterRow();
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
            Rarity.Common => commonSprites.Length > 0 ? commonSprites[0] : null,
            Rarity.Rare => rareSprites.Length > 0 ? rareSprites[0] : null,
            Rarity.Epic => epicSprites.Length > 0 ? epicSprites[0] : null,
            Rarity.Legendary => legendarySprites.Length > 0 ? legendarySprites[0] : null,
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
            acceptAllButton.interactable = currentCharacters.Count > 0;
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
            exchangeAllButton.interactable = currentCharacters.Count > 0;
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
        foreach (GameObject rowItem in currentCharacters.ToArray())
        {
            OnAcceptCharacter(rowItem);
        }

        // ��������� ��������� ������ ����� ��������
        UpdateRollButtonState();
        UpdateSummonFiveButtonState();
        UpdateAcceptAllButtonState();
        UpdateExchangeAllButtonState();
        UpdateGetTicketsButtonState();
    }

    public void ExchangeAllCharactersForDNA()
    {
        foreach (GameObject rowItem in currentCharacters.ToArray())
        {
            OnExchangeForDNA(rowItem);
        }

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

            AddCharacterToRow(guaranteedCharacter);

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
}