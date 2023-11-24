using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PlayableCharacters
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Warning
    }

    public EnemyState enemyState;
}
