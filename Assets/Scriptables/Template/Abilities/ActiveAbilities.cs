using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Active Abilities/Active Abilities", fileName = "Active Abilities")]
[System.Serializable]

public class ActiveAbilities : ScriptableObject
{
    public Sprite abilityPortrait;
    public string description;
}
