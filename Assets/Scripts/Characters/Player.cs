using System.Collections.Generic;
using UnityEngine;

public class Player : PlayableCharacters
{
    [Space(20)] [Header("Player Panel")] [Space(10)]
    public int experience;
    
    [Space(20)] [Header("Active Abilities")] [Space(10)]
    public List<ActiveAbilities> activeAbilities;
    
    [Space(20)] [Header("Position Multiplayer")] [Space(10)]
    public float positionMultiplayer;

    public float voteMultiplayer = 1;

    public enum PlayerState
    {
        CutScene,
        Playable
    }

    public PlayerState playerState;

    public void retrieveParameters(out int totHealth, out int actHealt, out int exp, out int abPoint,
        out Sprite actPortrait, out List<ActiveAbilities> actAbilities)
    {
        retrieveCommonParameters(out totHealth, out actHealt, out abPoint, out actPortrait);
        exp = experience;
        actAbilities = activeAbilities;
    }
}