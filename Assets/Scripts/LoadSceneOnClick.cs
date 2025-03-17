using UnityEngine;
using UnityEngine.SceneManagement; // Для работы со сценами
using UnityEngine.UI; // Для работы с UI

public class LoadSceneOnClick : MonoBehaviour
{

    public string sceneName; // Имя сцены, которую нужно загрузить

    // Метод вызывается при нажатии на кнопку
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is not set!");
            return;
        }

        // Загрузка сцены по имени
        SceneManager.LoadScene(sceneName);
    }
}
