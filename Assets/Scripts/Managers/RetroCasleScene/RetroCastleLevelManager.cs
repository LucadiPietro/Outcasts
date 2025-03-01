using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RetroCastleLevelManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform brogan;
    [SerializeField] Transform malphayt;
    [SerializeField] Transform lysander;

    [Header("From Battle")]
    [SerializeField] Transform broganPos;
    [SerializeField] Transform malphaytPos;
    [SerializeField] Transform lysanderPos;
    public UnityEvent FromBattle;
    
    private void Start()
    {
        switch (SessionManager.Instance.LastScene)
        {
            case null: //First load (PER ADESSO)
                break;

            case "Warp Scene": //Back from the battle
                FromBattle.Invoke();
                brogan.position = broganPos.position;
                malphayt.position = malphaytPos.position;
                lysander.position = lysanderPos.position;
                break;

            case "Scene Two": //Back from the next scene
                break;
        }
    }
}