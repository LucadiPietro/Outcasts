using System;
using Unity.Collections;
using UnityEngine;

public class PlayableCharacters : Character
{
    [Space(20)] [Header("Health Panel")] [Space(10)]
    public int startingHealth;

    public int actualHealth;


    [Space(10)] public int abilityPoint;

    [Space(20)] [Header("Battle Panel")] [Space(10)]
    public int vitality;

    public float attack;
    public float attackBuff = 1;
    public float defence;
    public float defenceBuff = 1;

    public float damageConstant = 1;

    public BMButtonPrefab.Cell cell;

    [Space(20)] [Header("READONLY PANEL")] [Space(10)]
    public int additionalAttack;

    public int additionalDefence;
    [Space(10)] public int additionalHealth;
    public int totalHealth;

    private void Start()
    {
        totalHealth = startingHealth;
        actualHealth = startingHealth;
    }


    protected void retrieveCommonParameters(out int totHealth, out int actHealt, out int abPoint,
        out Sprite actPortrait)
    {
        actHealt = actualHealth;

        totHealth = startingHealth + additionalHealth;

        actPortrait = portrait;

        abPoint = abilityPoint;
    }

    public void GetHit(float damage)
    {
        actualHealth -= Mathf.RoundToInt(damage);

        if (actualHealth <= 0)
        {
            actualHealth = 0;
        }
    }
}