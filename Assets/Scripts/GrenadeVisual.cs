using UnityEngine;

public class GrenadeVisual : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int segments = 20;
    public float arcHeight = 1.5f;

    [Header("Настройки линии")]
    public Color lineColor = Color.yellow;
    public float lineWidth = 0.1f;

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

    public void ShowTrajectory(Vector3 start, Vector3 target)
    {
        lineRenderer.enabled = true;

        float distance = Vector3.Distance(start, target);
        float arcHeight = Mathf.Min(2f, distance * 0.3f); // Высота дуги

        Vector3[] positions = CalculateArc(start, target, segments, arcHeight);

        for (int i = 0; i <= segments; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }
    }


    

    public void HideTrajectory()
    {
        lineRenderer.enabled = false;
    }

    // Теперь принимает 4 аргумента
    private Vector3[] CalculateArc(Vector3 start, Vector3 end, int segments, float height)
    {
        Vector3[] points = new Vector3[segments + 1];

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