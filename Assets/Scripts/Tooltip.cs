// Tooltip.cs
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public Text tooltipText; // Текст подсказки

    private bool isTooltipActive = false;

    void Update()
    {
        if (isTooltipActive)
        {
            // Следуем за курсором
            Vector3 mousePosition = Input.mousePosition;
            transform.position = mousePosition + new Vector3(20, 20); // Смещаем подсказку относительно курсора
        }
    }

    // Метод для активации подсказки
    public void ShowTooltip(string message)
    {
        tooltipText.text = message;
        gameObject.SetActive(true);
        isTooltipActive = true;
    }

    // Метод для деактивации подсказки
    public void HideTooltip()
    {
        gameObject.SetActive(false);
        isTooltipActive = false;
    }
}