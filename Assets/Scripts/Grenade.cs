using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float damage = 500f;
    public float explosionRadius = 7f;
    public float speed = 15f;

    public LayerMask enemyLayerMask;

    private Vector3[] path;
    private int currentPoint = 0;
    private float timeSinceLaunch = 0f;
    private float totalFlightTime = 0f;
    private bool hasLaunched = false;




    void Update()
    {
        if (!hasLaunched || path == null || currentPoint >= path.Length)
            return;

        timeSinceLaunch += Time.deltaTime;

        if (currentPoint < path.Length - 1)
        {
            // Летим к следующей точке
            transform.position = Vector3.Lerp(path[currentPoint], path[currentPoint + 1],
                timeSinceLaunch / totalFlightTime * 10f); // Умножаем для плавности

            if (timeSinceLaunch >= totalFlightTime / 10f)
            {
                currentPoint++;
                timeSinceLaunch = 0f;
            }
        }
        else
        {
            // Долетели до конца → взрыв
            Explode();
            Destroy(gameObject);
        }
    }
    public void Launch(Vector3 target)
    {
        if (hasLaunched) return;

        Vector3 startPos = transform.position + Vector3.up * 1.5f;
        float distance = Vector3.Distance(startPos, target);
        float arcHeight = Mathf.Min(2f, distance * 0.3f);

        path = CalculateArc(startPos, target, segments: 20, height: arcHeight);
        totalFlightTime = distance / speed;

        hasLaunched = true;
        Debug.Log("Граната брошена!");
    }

    private IEnumerator SimulateFlight()
    {
        if (path == null || path.Length == 0)
            yield break;

        while (currentPoint < path.Length - 1)
        {
            transform.position = Vector3.Lerp(path[currentPoint], path[currentPoint + 1], timeSinceLaunch / totalFlightTime);
            timeSinceLaunch += Time.deltaTime;

            if (timeSinceLaunch >= totalFlightTime)
            {
                currentPoint++;
                timeSinceLaunch = 0f;

                if (currentPoint >= path.Length)
                {
                    Explode();
                    yield break;
                }
            }

            yield return null;
        }
    }

    Vector3[] CalculateArc(Vector3 start, Vector3 end, int segments, float height)
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

    private void Explode()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask);

        foreach (Collider enemy in enemies)
        {
            HealthSystem health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Debug.Log("💥 Граната взорвалась!");
        Destroy(gameObject);
    }
}