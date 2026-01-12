using Common.Cutscenes;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationHelper : MonoBehaviour
{
    public static NavigationHelper Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopAllCharacters()
    {
        var chars = FindObjectsOfType<AIPath>();

        foreach (var agent in chars)
        {
            agent.destination = agent.position;
        }
    }

    public void ChangeDestination(AIPath agent, Transform newDestination)
    {
        agent.destination = newDestination.position;
    }

    public void ChangeSlowDownDistance(AIPath agent, float newDistance)
    {
        agent.slowdownDistance = newDistance;
    }
}
