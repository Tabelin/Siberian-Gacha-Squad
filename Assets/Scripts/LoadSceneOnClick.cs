using UnityEngine;
using UnityEngine.SceneManagement; // Для работы со сценами
using UnityEngine.UI; // Для работы с UI

public class LoadSceneOnClick : MonoBehaviour
{
    // Ссылки на сцены
    public string hubSceneName; // Имя сцены хаба
    public string gachaSceneName; // Имя сцены гаче
    public string exitSceneName; // Имя сцены выхода (или null для выхода из приложения)

    // Метод для загрузки сцены хаба
    public void LoadHubScene()
    {
        if (string.IsNullOrEmpty(hubSceneName))
        {
            Debug.LogError("Имя сцены хаба не указано!");
            return;
        }

        SceneManager.LoadScene(hubSceneName);
        Debug.Log($"Загружена сцена: {hubSceneName}");
    }

    // Метод для загрузки сцены гаче
    public void LoadGachaScene()
    {
        if (string.IsNullOrEmpty(gachaSceneName))
        {
            Debug.LogError("Имя сцены гаче не указано!");
            return;
        }

        SceneManager.LoadScene(gachaSceneName);
        Debug.Log($"Загружена сцена: {gachaSceneName}");
    }

    // Метод для выхода из игры
    public void ExitGame()
    {
        if (string.IsNullOrEmpty(exitSceneName))
        {
            Debug.Log("Выход из игры...");
            Application.Quit();
        }
        else
        {
            SceneManager.LoadScene(exitSceneName);
            Debug.Log($"Загружена сцена: {exitSceneName}");
        }
    }
}