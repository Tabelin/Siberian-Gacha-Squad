using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Outline : MonoBehaviour
{
    public Color OutlineColor = Color.yellow;
    public float OutlineWidth = 0.2f;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogError("Outline требует компонент SpriteRenderer!");
            Destroy(this);
            return;
        }

        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (_spriteRenderer != null && _spriteRenderer.material != null)
        {
            _spriteRenderer.material.SetFloat("_OutlineWidth", OutlineWidth);
            _spriteRenderer.material.SetColor("_OutlineColor", OutlineColor);
        }
    }
}