using UnityEngine;
using UnityEngine.SceneManagement; // ��� ������ �� �������
using UnityEngine.UI; // ��� ������ � UI

public class LoadSceneOnClick : MonoBehaviour
{

    public string sceneName; // ��� �����, ������� ����� ���������

    // ����� ���������� ��� ������� �� ������
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is not set!");
            return;
        }

        // �������� ����� �� �����
        SceneManager.LoadScene(sceneName);
    }
}
