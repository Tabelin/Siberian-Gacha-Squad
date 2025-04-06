// CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Скорость перемещения камеры
    public float heightChangeSpeed = 5f; // Скорость изменения высоты
    public float rotationSpeed = 2f; // Скорость поворота камеры
    public float minHeight = 2f; // Минимальная высота камеры
    public float maxHeight = 10f; // Максимальная высота камеры

    void Update()
    {
        // Перемещение камеры параллельно земле (WASD)
        MoveCamera();

        // Изменение высоты камеры (колесико мыши)
        ChangeCameraHeight();

        // Поворот камеры по оси Y (Q и E)
        RotateCamera();
    }

    // Метод для перемещения камеры
    private void MoveCamera()
    {
        float horizontal = Input.GetAxis("Horizontal"); // Влево/вправо
        float vertical = Input.GetAxis("Vertical");    // Вперед/назад

        // Определяем направление движения относительно камеры
        Vector3 movement = (transform.right * horizontal + transform.forward * vertical).normalized;

        // Игнорируем вертикальную составляющую (двигаемся только по плоскости XZ)
        movement = new Vector3(movement.x, 0f, movement.z);

        // Двигаем камеру
        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    // Метод для изменения высоты камеры
    private void ChangeCameraHeight()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newYPosition = transform.position.y - scroll * heightChangeSpeed;

        // Ограничиваем высоту камеры
        newYPosition = Mathf.Clamp(newYPosition, minHeight, maxHeight);

        // Обновляем высоту камеры
        transform.position = new Vector3(
            transform.position.x,
            newYPosition,
            transform.position.z
        );
    }

    // Метод для поворота камеры
    private void RotateCamera()
    {
        if (Input.GetKey(KeyCode.Q)) // Поворот влево
        {
            transform.RotateAround(transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E)) // Поворот вправо
        {
            transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}