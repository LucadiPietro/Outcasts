using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoint : MonoBehaviour
{
    [SerializeField] Transform m_Target;
    [SerializeField] PlayableMovement m_Movement;
    [SerializeField] float m_DelayTime;     // Delay in seconds (e.g., 2 seconds)
    private Queue<Vector3> positionsQueue;  // Queue to store past positions
    private float timeInterval;   // Time interval between each recorded position

    void Start()
    {
        positionsQueue = new Queue<Vector3>();
        timeInterval = Time.fixedDeltaTime;  // The interval at which physics updates happen
    }

    void FixedUpdate()
    {
        if (!m_Movement.IsMoving)
            return;

        // Store the current target position in the queue
        positionsQueue.Enqueue(m_Target.position);

        // If more than "delay" seconds worth of positions are stored, remove the oldest one
        if (positionsQueue.Count > Mathf.RoundToInt(m_DelayTime / timeInterval))
        {
            Vector3 pastPosition = positionsQueue.Dequeue();

            // Move the current object to that past position
            transform.position = pastPosition;
        }
    }

    private void LateUpdate()
    {
        
    }
}
