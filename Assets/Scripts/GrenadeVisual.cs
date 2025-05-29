using UnityEngine;

public class GrenadeVisual : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int segments = 20;

    [Header("Настройки линии")]
    public Color normalColor = Color.yellow;
    public Color tooFarColor = Color.red;
    public float lineWidth = 0.1f;
    public float maxThrowDistance = 60f;


    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }


    // Метод обновления траектории
    public bool UpdateTrajectory(Vector3 start, Vector3 target)
    {
        float distance = Vector3.Distance(start, target);

        // Устанавливаем цвет линии
        lineRenderer.startColor = distance > maxThrowDistance ? tooFarColor : normalColor;
        lineRenderer.endColor = lineRenderer.startColor;

        // Рассчитываем дугу
        Vector3[] positions = CalculateArc(start, target, segments);
        for (int i = 0; i <= segments; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }

        lineRenderer.enabled = true;

        return distance <= maxThrowDistance;
    }
   
    public bool ShowTrajectory(Vector3 start, Vector3 target)
    {
        float distance = Vector3.Distance(start, target);

        // Устанавливаем цвет линии
        lineRenderer.startColor = distance > maxThrowDistance ? tooFarColor : normalColor;
        lineRenderer.endColor = lineRenderer.startColor;

        // Рассчитываем дугу
        Vector3[] positions = CalculateArc(start, target, segments);
        for (int i = 0; i <= segments; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }

        lineRenderer.enabled = true;

        return distance <= maxThrowDistance;
    }



    public void HideTrajectory()
    {
        lineRenderer.enabled = false;
    }

    private Vector3[] CalculateArc(Vector3 start, Vector3 end, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];
        float height = Mathf.Min(2f, Vector3.Distance(start, end) * 0.3f);
        
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += Mathf.Sin(Mathf.PI * t) * height;
            points[i] = pos;
        }

        return points;
    }
}