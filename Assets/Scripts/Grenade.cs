using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Grenade : MonoBehaviour
{
    public float damage = 50f;
    public float explosionRadius = 3f;
    public float throwSpeed = 10f;
    public float explosionDelay = 2f;

    public LayerMask damageLayerMask; // Включает "Characters", "Enemies" и т.д.

    private Vector3[] path;
    private int currentPoint = 0;
    private float timeSinceLaunch = 0f;
    private float totalFlightTime = 0f;

    private LineRenderer explosionLineRenderer;
    private MeshRenderer meshRenderer;
    private SpriteRenderer spriteRenderer;

    private bool isFlashing = false;
    private float flashTimer = 0f;
    private float flashInterval = 0.2f;
    private Color originalColor = Color.white;

    void Start()
    {
        // Подготавливаем LineRenderer для отрисовки радиуса взрыва
        GameObject lineGO = new GameObject("ExplosionCircle");
        explosionLineRenderer = lineGO.AddComponent<LineRenderer>();
        explosionLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        explosionLineRenderer.positionCount = 36;
        explosionLineRenderer.useWorldSpace = true;
        explosionLineRenderer.loop = true;
        explosionLineRenderer.startWidth = 0.1f;
        explosionLineRenderer.enabled = false;

        // Находим рендереры
        meshRenderer = GetComponent<MeshRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
        else if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            Debug.LogWarning("Нет Renderer компонентов для мигания");

        explosionLineRenderer.enabled = false;
    }

    void Update()
    {
        if (path == null || isFlashing)
            return;

        timeSinceLaunch += Time.deltaTime;

        if (currentPoint < path.Length - 1)
        {
            transform.position = Vector3.Lerp(path[currentPoint], path[currentPoint + 1],
                timeSinceLaunch / (totalFlightTime / 10f));

            if (timeSinceLaunch >= totalFlightTime / 10f)
            {
                currentPoint++;
                timeSinceLaunch = 0f;
            }
        }
        else
        {
            transform.position = path[path.Length - 1];
            StartCoroutine(HandleExplosion());
        }
    }

    public void Launch(Vector3 target)
    {
        Vector3 startPos = transform.position + Vector3.up * 1.5f;
        float distance = Vector3.Distance(startPos, target);
        float arcHeight = Mathf.Min(2f, distance * 0.3f);

        path = CalculateArc(startPos, target, segments: 20, height: arcHeight);
        totalFlightTime = distance / throwSpeed;
    }

    private IEnumerator HandleExplosion()
    {
        isFlashing = true;
        DrawExplosionRadius(explosionRadius);

        float elapsed = 0f;
        float flashSpeed = 0.2f;

        while (elapsed < explosionDelay)
        {
            ToggleRenderers(Color.red);
            yield return new WaitForSeconds(flashSpeed);

            ToggleRenderers(originalColor);
            yield return new WaitForSeconds(flashSpeed);

            elapsed += flashSpeed * 2f;

            // Ускоряем мигание ближе к концу
            if (elapsed > explosionDelay * 0.8f)
            {
                flashSpeed *= 0.3f;
            }
        }

        // Наносим урон
        Explode();

        // 🚫 Скрываем или удаляем LineRenderer
        explosionLineRenderer.enabled = false;
        Destroy(explosionLineRenderer.gameObject, 0.5f); // Можно через полсекунды

        // Уничтожаем всё после взрыва
        yield return new WaitForSeconds(0.5f); // Пауза после взрыва
        Destroy(gameObject);
    }

    private void DrawExplosionRadius(float radius)
    {
        explosionLineRenderer.enabled = true;

        for (int i = 0; i < 36; i++)
        {
            float angle = i * Mathf.PI * 2 / 36;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            explosionLineRenderer.SetPosition(i, pos);
        }
    }

    private void Explode()
    {
        Collider[] affected = Physics.OverlapSphere(transform.position, explosionRadius, damageLayerMask);

        foreach (Collider col in affected)
        {
            HealthSystem health = col.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"💥 Нанесено {damage} урона по {col.name}");
            }
        }

        Debug.Log("💥 Граната взорвалась");

        // Можно добавить VFX или звук здесь
    }

    private void ToggleRenderers(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

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