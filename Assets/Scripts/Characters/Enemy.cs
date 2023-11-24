using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PlayableCharacters
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        FixedMovement,
        Warning
    }

    public EnemyState enemyState;
}
