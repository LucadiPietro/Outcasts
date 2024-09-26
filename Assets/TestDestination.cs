using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDestination : MonoBehaviour
{
    [SerializeField] List<Movable> movables;

    [SerializeField] List<Transform> targets;

    void Start()
    {

        for (int i = 0; i < movables.Count; i++)
        {
            movables[i].StartFollowing(targets[i]);
        }
        
    }

    void Update()
    {
        
    }
}
