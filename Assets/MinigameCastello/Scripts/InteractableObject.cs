using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public GameObject popupUI;
    public GameObject popupText;
    public GameObject player;
    public CanvasManager gameManager;

    private bool canOpen;

    void Start()
    {
        popupUI.SetActive(false);
        popupText.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            canOpen = true;
            gameManager.canPause = false;
            popupText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            canOpen = false;
            popupUI.SetActive(false);
            gameManager.canPause = true;
        }
    }

    void Update()
    {
        if (canOpen && Input.GetKeyDown(KeyCode.Space))
        {
            InteractionMenuOn();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            popupUI.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void InteractionMenuOn()
    {
        popupUI.SetActive(true);
        popupText.SetActive(false);
        Time.timeScale = 0f;
    }
}
