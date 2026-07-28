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

    [Header("From Next Scene")]
    [SerializeField] Transform broganPos2;
    [SerializeField] Transform malphaytPos2;
    [SerializeField] Transform lysanderPos2;
    public UnityEvent FromNext;

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

            case "Warp Scene 2": //Back from the next scene
                FromNext.Invoke();
                brogan.position = broganPos2.position;
                malphayt.position = malphaytPos2.position;
                lysander.position = lysanderPos2.position;
                break;
        }
    }
}