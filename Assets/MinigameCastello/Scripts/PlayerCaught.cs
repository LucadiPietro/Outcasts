using UnityEngine;

public class PlayerCaught : MonoBehaviour
{
    public Transform player;
    public CanvasManager script;

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            script.isCaught = true;
        }
    }
}
