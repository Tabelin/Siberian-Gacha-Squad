using UnityEngine;

public class MiniMine : MonoBehaviour
{
    private GameObject currentDrill = null;

    public bool HasDrill()
    {
        return currentDrill != null && currentDrill.activeInHierarchy;
    }

    public void AssignDrill(GameObject drill)
    {
        currentDrill = drill;
        Debug.Log($"����� '{gameObject.name}' �������� ���");
    }

    public void RemoveDrill()
    {
        if (currentDrill != null)
        {
            Destroy(currentDrill);
            currentDrill = null;
        }
    }
}