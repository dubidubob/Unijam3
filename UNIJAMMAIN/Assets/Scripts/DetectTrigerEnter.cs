using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DetectTrigerEnter : MonoBehaviour
{
    [SerializeField] Color color = Color.red;
    private SpriteRenderer spriteRenderer;
    private Color origin;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        origin = spriteRenderer.color;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        spriteRenderer.color = color;
        Invoke("Back", 0.5f);
    }

    private void Back()
    {
        spriteRenderer.color = origin;
    }
}
