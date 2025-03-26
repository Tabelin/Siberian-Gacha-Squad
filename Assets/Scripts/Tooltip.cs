// Tooltip.cs
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public Text tooltipText; // ����� ���������

    private bool isTooltipActive = false;

    void Update()
    {
        if (isTooltipActive)
        {
            // ������� �� ��������
            Vector3 mousePosition = Input.mousePosition;
            transform.position = mousePosition + new Vector3(20, 20); // ������� ��������� ������������ �������
        }
    }

    // ����� ��� ��������� ���������
    public void ShowTooltip(string message)
    {
        tooltipText.text = message;
        gameObject.SetActive(true);
        isTooltipActive = true;
    }

    // ����� ��� ����������� ���������
    public void HideTooltip()
    {
        gameObject.SetActive(false);
        isTooltipActive = false;
    }
}