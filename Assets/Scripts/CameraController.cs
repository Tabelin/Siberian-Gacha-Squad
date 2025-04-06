// CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // �������� ����������� ������
    public float heightChangeSpeed = 5f; // �������� ��������� ������
    public float rotationSpeed = 2f; // �������� �������� ������
    public float minHeight = 2f; // ����������� ������ ������
    public float maxHeight = 10f; // ������������ ������ ������

    void Update()
    {
        // ����������� ������ ����������� ����� (WASD)
        MoveCamera();

        // ��������� ������ ������ (�������� ����)
        ChangeCameraHeight();

        // ������� ������ �� ��� Y (Q � E)
        RotateCamera();
    }

    // ����� ��� ����������� ������
    private void MoveCamera()
    {
        float horizontal = Input.GetAxis("Horizontal"); // �����/������
        float vertical = Input.GetAxis("Vertical");    // ������/�����

        // ���������� ����������� �������� ������������ ������
        Vector3 movement = (transform.right * horizontal + transform.forward * vertical).normalized;

        // ���������� ������������ ������������ (��������� ������ �� ��������� XZ)
        movement = new Vector3(movement.x, 0f, movement.z);

        // ������� ������
        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    // ����� ��� ��������� ������ ������
    private void ChangeCameraHeight()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newYPosition = transform.position.y - scroll * heightChangeSpeed;

        // ������������ ������ ������
        newYPosition = Mathf.Clamp(newYPosition, minHeight, maxHeight);

        // ��������� ������ ������
        transform.position = new Vector3(
            transform.position.x,
            newYPosition,
            transform.position.z
        );
    }

    // ����� ��� �������� ������
    private void RotateCamera()
    {
        if (Input.GetKey(KeyCode.Q)) // ������� �����
        {
            transform.RotateAround(transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E)) // ������� ������
        {
            transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}