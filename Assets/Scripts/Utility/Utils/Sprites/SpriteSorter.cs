using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSorter : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] bool isStatic = false;

    private void Awake()
    {
        if(!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
        Sort();
    }

    private void FixedUpdate()
    {
        if (isStatic) return;
        Sort();
    }

    void Sort()
    {
        float dec = transform.position.y % 1f;
        int p1 = (int)transform.position.y * -100;
        int p2 = (int)(100 * dec);
        spriteRenderer.sortingOrder = p1 - p2;
    }

}
