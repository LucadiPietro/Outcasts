using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThroneRoomLevelManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] Transform brogan;
    [SerializeField] Transform malphayt;
    [SerializeField] Transform lysander;
    [SerializeField] Transform santiago;

    [Header("From Battle")]
    [SerializeField] Transform broganPos;
    [SerializeField] Transform malphaytPos;
    [SerializeField] Transform lysanderPos;
    [SerializeField] Transform santiagoPos;
    public UnityEvent FromBattle;

    private void Start()
    {
        //Debug.Log(SessionManager.Instance.LastScene);

        switch (SessionManager.Instance.LastScene)
        {
            case null: //First load (PER ADESSO)
                break;

            case "Warp Scene": //Back from the battle
                FromBattle.Invoke();
                brogan.position = broganPos.position;
                malphayt.position = malphaytPos.position;
                lysander.position = lysanderPos.position;
                santiago.position = santiagoPos.position;
                break;
        }
    }
}
