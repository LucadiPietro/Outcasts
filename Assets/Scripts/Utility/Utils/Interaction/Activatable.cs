using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Activatable : MonoBehaviour
{

    [SerializeField] UnityEvent onActivation;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && active)
        {
            onActivation.Invoke();
        }
    }


    bool active = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            active = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            active = false;
        }
    }


}
