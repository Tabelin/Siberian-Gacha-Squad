using UnityEngine;
using UnityEngine.SceneManagement; // ��� ������ �� �������
using UnityEngine.UI; // ��� ������ � UI

public class LoadSceneOnClick : MonoBehaviour
{
    // ������ �� �����
    public string hubSceneName; // ��� ����� ����
    public string gachaSceneName; // ��� ����� ����
    public string exitSceneName; // ��� ����� ������ (��� null ��� ������ �� ����������)

    // ����� ��� �������� ����� ����
    public void LoadHubScene()
    {
        if (string.IsNullOrEmpty(hubSceneName))
        {
            Debug.LogError("��� ����� ���� �� �������!");
            return;
        }

        SceneManager.LoadScene(hubSceneName);
        Debug.Log($"��������� �����: {hubSceneName}");
    }

    // ����� ��� �������� ����� ����
    public void LoadGachaScene()
    {
        if (string.IsNullOrEmpty(gachaSceneName))
        {
            Debug.LogError("��� ����� ���� �� �������!");
            return;
        }

        SceneManager.LoadScene(gachaSceneName);
        Debug.Log($"��������� �����: {gachaSceneName}");
    }

    // ����� ��� ������ �� ����
    public void ExitGame()
    {
        if (string.IsNullOrEmpty(exitSceneName))
        {
            Debug.Log("����� �� ����...");
            Application.Quit();
        }
        else
        {
            SceneManager.LoadScene(exitSceneName);
            Debug.Log($"��������� �����: {exitSceneName}");
        }
    }
}