// NumberPopup.cs
using UnityEngine;
using UnityEngine.UI;

public class NumberPopup : MonoBehaviour
{
    // Текст для отображения числа
    public Text text;

    // Скорость движения вверх
    public float moveSpeed = 2f;

    // Продолжительность существования
    public float lifetime = 1f;

    // Инициализация текста и цвета
    public void Initialize(string newText, Color newColor)
    {
        if (text != null)
        {
            text.text = newText;
            text.color = newColor;
        }
    }

    void Update()
    {
        if (text != null)
        {
            // Движение вверх
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            // Исчезновение через lifetime секунд
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject); // Уничтожаем объект
            }
        }
    }
}