// CharacterRowItem.cs
using UnityEngine;
using UnityEngine.UI;

public class CharacterRowItem : MonoBehaviour
{
    // ���������� UI ��� ����������� ����������
    public Image characterImage; // ����-������ ���������
    public Text characterNameText; // ��� ���������
    public Text rarityText; // �������� ���������
    public Text statsText; // ����� ���������

    public Character character; // ������ ������ ���������
    public GameObject rowItem; // ������ �� ��� ������� ����

    // ����� ��� ��������� ���������
    public void SetCharacter(Character newCharacter, GameObject rowItemObj, System.Action<GameObject> onAccept, System.Action<GameObject> onExchange)
    {
        character = newCharacter;
        rowItem = rowItemObj;

        // ��������� UI
        characterImage.sprite = character.sprite;
        characterNameText.text = character.name;
        rarityText.text = character.rarity.ToString();
        statsText.text = $"HP: {character.health}\nATK: {character.attack}\nDEF: {character.defense}";

        // ������� ������ ����� Transform.Find
        Button acceptButton = rowItem.transform.Find("AcceptButton").GetComponent<Button>();
        Button exchangeButton = rowItem.transform.Find("ExchangeButton").GetComponent<Button>();

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(() => onAccept(rowItem));
        }

        if (exchangeButton != null)
        {
            exchangeButton.onClick.AddListener(() => onExchange(rowItem));
        }
    }
}