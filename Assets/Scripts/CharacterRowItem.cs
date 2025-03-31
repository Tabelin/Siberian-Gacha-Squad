// CharacterRowItem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRowItem : MonoBehaviour
{
    // ����������� ���������� � ���������
    public Image characterImage; // ����-������ ���������
    public Text characterNameText; // ��� ���������
    public Text rarityText; // �������� ���������
    public Text statsText; // ����� ���������

    // ������ ��������
    public Button acceptButton; // ������� ���������
    public Button exchangeButton; // �������� �� ���

    // ���������� ��� �������������� ����
    public Image backgroundImage; // ������� ��������

    // ������� �������� ��� ���� ������ �������� (��������� ����)
    public List<Sprite> commonBackgroundSprites = new List<Sprite>();
    public List<Sprite> rareBackgroundSprites = new List<Sprite>();
    public List<Sprite> epicBackgroundSprites = new List<Sprite>();
    public List<Sprite> legendaryBackgroundSprites = new List<Sprite>();

    // ������ ���������
    public Character character;

    // ������ �� ������� ����
    public GameObject rowItem;

    // ����� ��� ��������� ���������
    public void SetCharacter(Character newCharacter, GameObject rowItemObj, System.Action<GameObject> onAccept, System.Action<GameObject> onExchange)
    {
        if (newCharacter == null || rowItemObj == null)
        {
            Debug.LogError("���������� �������� ��� ������ ���� �������� null!");
            return;
        }

        character = newCharacter;
        rowItem = rowItemObj;

        // ��������� UI
        if (characterImage != null)
        {
            characterImage.sprite = character.sprite; // ������������� ������ ���������

        }
        else
        {
            Debug.LogWarning("CharacterImage �� ��������!");
        }

        if (characterNameText != null)
        {
            characterNameText.text = character.name; // ������������� ���
        }
        else
        {
            Debug.LogWarning("CharacterNameText �� ��������!");
        }

        if (rarityText != null)
        {
            rarityText.text = character.rarity.ToString(); // ������������� ��������
        }
        else
        {
            Debug.LogWarning("RarityText �� ��������!");
        }

        if (statsText != null)
        {
            statsText.text = $"HP: {character.health}\nATK: {character.attack}\nDEF: {character.defense}"; // ������������� �����
        }
        else
        {
            Debug.LogWarning("StatsText �� ��������!");
        }
        // ������������� ��������� ������������ ����
        Color currentColor = backgroundImage.color;
        backgroundImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f); // Alpha = 0.5 (50% ������������)

        // ��������� �������� ����
        if (backgroundImage != null)
        {
            StartCoroutine(AnimateBackground(character.rarity));
        }
        else
        {
            Debug.LogWarning("BackgroundImage �� ��������!");
        }

        // ��������� ������� ������
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(() => onAccept(rowItem));
        }
        else
        {
            Debug.LogWarning("AcceptButton �� ��������!");
        }

        if (exchangeButton != null)
        {
            exchangeButton.onClick.AddListener(() => onExchange(rowItem));
        }
        else
        {
            Debug.LogWarning("ExchangeButton �� ��������!");
        }
    }

    // �������� ��� ��������������� �������� ����
    private IEnumerator AnimateBackground(Rarity rarity)
    {
        if (backgroundImage == null)
        {
            yield break;
        }

        // �������� ��������������� ������� ����
        List<Sprite> backgroundSprites = GetBackgroundSprites(rarity);
        if (backgroundSprites == null || backgroundSprites.Count == 0)
        {
            Debug.LogWarning($"�������� ���� ��� �������� {rarity} �� �������!");
            yield break;
        }

        while (true)
        {
            foreach (Sprite frame in backgroundSprites)
            {
                backgroundImage.sprite = frame; // �������� ������ ����
                yield return new WaitForSeconds(0.1f); // ����� ����� �������
            }
        }
    }

    // ����� ��� ��������� �������� ���� �� ��������
    private List<Sprite> GetBackgroundSprites(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonBackgroundSprites,
            Rarity.Rare => rareBackgroundSprites,
            Rarity.Epic => epicBackgroundSprites,
            Rarity.Legendary => legendaryBackgroundSprites,
            _ => null
        };
    }
}