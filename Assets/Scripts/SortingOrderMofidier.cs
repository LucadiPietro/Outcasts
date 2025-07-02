using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrderMofidier : MonoBehaviour
{
    public SpriteRenderer _renderer;
    [SerializeField] int _sortOrderFront = 0, _sortOrderBack = 3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayableMovement playerMovement = other.gameObject.GetComponent<PlayableMovement>();
        if (playerMovement)
        {
            _renderer.sortingOrder = _sortOrderBack;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayableMovement playerMovement = other.gameObject.GetComponent<PlayableMovement>();
        if (playerMovement)
        {
            _renderer.sortingOrder = _sortOrderFront;
        }
    }
}