using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Grenade : MonoBehaviour
{
    public enum GrenadeType
    {
        Frag,
        Smoke,
        Flash,
        Incendiary
    }

    [Header("Настройки гранаты")]
    public GrenadeType type = GrenadeType.Frag;
    [Header("Графика")]
    public Sprite sprite; // Для 2D
    public GameObject model; // Для 3D модели

    public float weight = 2f;
    public float baseDamage = 50f;
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

    private Color originalColor = Color.white;


    public void SetupStats()
    {
        switch (type)
        {
            case GrenadeType.Frag:
                baseDamage = 500f;
                explosionRadius = 11f;
                weight = 2f;
                break;

            case GrenadeType.Smoke:
                baseDamage = 0f;
                explosionRadius = 5f;
                weight = 1.5f;
                break;

            case GrenadeType.Flash:
                baseDamage = 0f;
                explosionRadius = 4f;
                weight = 1.2f;
                break;

            case GrenadeType.Incendiary:
                baseDamage = 60f;
                explosionRadius = 2.5f;
                weight = 2.8f;
                break;
        }
    }






    void Start()
    {
        SetupVisual();
        SetupStats();

        // Подготавливаем LineRenderer для отрисовки радиуса взрыва
        CreateExplosionCircle();
    }

    private void CreateExplosionCircle()
    {
        GameObject lineGO = new GameObject("ExplosionCircle");
        explosionLineRenderer = lineGO.AddComponent<LineRenderer>();
        explosionLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        explosionLineRenderer.positionCount = 36;
        explosionLineRenderer.useWorldSpace = true;
        explosionLineRenderer.loop = true;
        explosionLineRenderer.startWidth = 0.1f;
        explosionLineRenderer.enabled = false;
    }
    private void SetupVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;

        if (model != null && transform.childCount == 0)
        {
            GameObject modelGO = Instantiate(model, transform.position, Quaternion.identity, transform);
            meshRenderer = modelGO.GetComponent<MeshRenderer>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
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
        isFlashing = false;
        // Уничтожаем всё после взрыва
        yield return new WaitForSeconds(0.5f); // Пауза после взрыва
        Destroy(gameObject);
    }

    private void DrawExplosionRadius(float radius)
    {
        explosionLineRenderer.enabled = true;
        explosionLineRenderer.startColor = GetExplosionColor(type);
        explosionLineRenderer.endColor = GetExplosionColor(type);

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
                Vector3 directionToTarget = col.transform.position - transform.position;
                float distance = directionToTarget.magnitude;

                // Чем дальше от центра → тем меньше урона
                float damageMultiplier = Mathf.InverseLerp(explosionRadius, 0f, distance);
                float finalDamage = baseDamage * damageMultiplier;

                // Игнорируем 50% защиты
                float armorIgnore = 0.5f; // 50%
                float effectiveDefense = health.defense * (1f - armorIgnore);

                // Вычисляем урон с учётом защиты и игнорирования
                float damageToApply = Mathf.Max(finalDamage - effectiveDefense, finalDamage * 0.2f); // Минимум 20% урона

                health.TakeDamage(damageToApply);

            }
        }
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

    private Color GetExplosionColor(GrenadeType grenadeType)
    {
        switch (grenadeType)
        {
            case GrenadeType.Frag: return Color.red;
            case GrenadeType.Smoke: return Color.gray;
            case GrenadeType.Flash: return Color.yellow;
            case GrenadeType.Incendiary: return Color.blue;
            default: return Color.white;
        }
    }
    public void SetGrenadeType(GrenadeType newType)
    {
        type = newType;
        SetupStats();
    }
}