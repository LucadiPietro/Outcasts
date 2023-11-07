using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrderMofidier : MonoBehaviour
{
    public SpriteRenderer _renderer;
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayableMovement playerMovement = other.gameObject.GetComponent<PlayableMovement>();
        if (playerMovement)
        {
            _renderer.sortingOrder = 3;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayableMovement playerMovement = other.gameObject.GetComponent<PlayableMovement>();
        if (playerMovement)
        {
            _renderer.sortingOrder = 1;
        }
    }
}