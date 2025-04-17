// NumberPopup.cs
using UnityEngine;
using UnityEngine.UI;

public class NumberPopup : MonoBehaviour
{
    // ����� ��� ����������� �����
    public Text text;

    // �������� �������� �����
    public float moveSpeed = 2f;

    // ����������������� �������������
    public float lifetime = 1f;

    // ������������� ������ � �����
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
            // �������� �����
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            // ������������ ����� lifetime ������
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject); // ���������� ������
            }
        }
    }
}