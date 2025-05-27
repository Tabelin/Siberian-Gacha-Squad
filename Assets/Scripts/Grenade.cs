using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionRadius = 3f;
    public float damage = 50f;
    public float throwSpeed = 20f;
    public float explosionDelay = 2f;

    private Transform targetPoint;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("У гранаты нет компонента Rigidbody!");
        }

        Destroy(gameObject, 5f); // Уничтожаем через 5 секунд
    }

    public void LaunchTo(Vector3 target)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody не назначен — бросок отменён");
            return;
        }
        Vector3 direction = (target - transform.position).normalized;
        rb.linearVelocity = direction * throwSpeed + Vector3.up * 5f; // Подбрасываем вверх

        StartCoroutine(ExplodeAfterDelay(explosionDelay));
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask);
        foreach (Collider enemy in enemies)
        {
            HealthSystem health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"💥 Граната нанесла {damage} урона по цели");
            }
        }

        Debug.Log("💥 Взрыв гранаты!");

        // Можно добавить VFX и звук
        Destroy(gameObject);
    }

    public LayerMask enemyLayerMask;
}