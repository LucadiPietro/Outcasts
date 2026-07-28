using Common.Cutscenes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldInteraction : MonoBehaviour
{
    [SerializeField] UnityEvent m_OnTriggerEntered;
    [SerializeField] UnityEvent m_OnTriggerExited;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            m_OnTriggerEntered.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_OnTriggerExited.Invoke();
        }
    }
}
