using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // ��� ������ � UI

public class GachaSystem : MonoBehaviour
{
    // ������ ��� ���������
    public GameObject characterPrefab;

    // ����� ������ ���������
    public Transform spawnPoint;

    // ������ ��� ����� ����-������
    public Button rollButton; // ������ �� ������ "�������� ���������"

    // UI ��� ����������� ���������� �������
    public Text gachaTicketText;

    // ������� �������� ��� ������ ��������
    public Sprite[] commonSprites;
    public Sprite[] rareSprites;
    public Sprite[] epicSprites;
    public Sprite[] legendarySprites;

    // ���������� �������
    private int gachaTickets = 10;

    void Start()
    {
        // �������������� ������� �������� � ������ Character
        Character.spritesByRarity = new Dictionary<Rarity, Sprite[]>
        {
            { Rarity.Common, commonSprites },
            { Rarity.Rare, rareSprites },
            { Rarity.Epic, epicSprites },
            { Rarity.Legendary, legendarySprites }
        };

        // ������������� �������� ��� �������
        Character.defaultSprite = commonSprites.Length > 0 ? commonSprites[0] : null;

        // ��������� ����� ������� � ��������� ������
        UpdateGachaTicketText();
        UpdateRollButtonState();
    }

    // ����� ��� ���������� ����� ����
    public void RollGacha()
    {
        if (gachaTickets > 0)
        {
            // ��������� ���������� �������
            gachaTickets--;

            // ������ ������ ���������
            Character newCharacter = new Character();
            newCharacter.GenerateCharacter();

            // ������� ��������� �� ������� ����
            SpawnCharacter(newCharacter);

            // ��������� ����� ������� � ��������� ������
            UpdateGachaTicketText();
            UpdateRollButtonState();
        }
        else
        {
            Debug.LogError("������������ �������!");
        }
    }

    // ����� ��� ������ ���������
    public void SpawnCharacter(Character character)
    {
        // ������ ������ ���������
        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        // ����������� �������� ������
        SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>();
        mainSpriteRenderer.sprite = character.sprite;

        // ��������� ��������� ������
        AddOutline(characterObject, character.outlineColor, character.sprite);

        // ���������� ���������� � ���������
        Debug.Log($"�� �������� ���������: {character.name} ({character.rarity})!");
    }

    // ����� ��� ���������� ������� ����� ������ ������
    private void AddOutline(GameObject characterObject, Color outlineColor, Sprite baseSprite)
    {
        // ������ ����� ������ ��� �������
        GameObject outlineObject = new GameObject("Outline");
        outlineObject.transform.parent = characterObject.transform;
        outlineObject.transform.localPosition = Vector3.zero;

        // ��������� Sprite Renderer ��� �������
        SpriteRenderer outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = baseSprite; // ���������� ��� �� ������
        outlineRenderer.color = outlineColor; // ������������� ���� �������
        outlineRenderer.sortingOrder = -1; // ������ ��������� ������ ��������� �������

        // ����������� ������ �������
        outlineRenderer.transform.localScale = new Vector3(1.1f, 1.1f, 1f); // ������� ����������� ������
    }

    // ����� ��� ���������� ������ �������
    private void UpdateGachaTicketText()
    {
        if (gachaTicketText != null)
        {
            gachaTicketText.text = $"������: {gachaTickets}";
        }
    }

    // ����� ��� ���������� ��������� ������
    private void UpdateRollButtonState()
    {
        if (rollButton != null) // ���������, ��� ������ ����������
        {
            rollButton.interactable = gachaTickets > 0; // ���������� ������, ���� ���� ������
        }
    }

    // ����� ��� ��������� �������
    public void AddGachaTickets(int amount)
    {
        gachaTickets += amount;

        // ��������� ����� ������� � ��������� ������
        UpdateGachaTicketText();
        UpdateRollButtonState(); // ���������, ����� �� ����� ������������ ������
    }
}