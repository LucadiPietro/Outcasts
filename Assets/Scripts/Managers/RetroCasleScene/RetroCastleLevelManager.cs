using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RetroCastleLevelManager : MonoBehaviour
{
    #region --------------------------------------------Configuration---------------------------------------------------

    public RetroCastleNightCameraManager virtualCameraManager;
    public List<PlayableMovement> players;
    public List<EnemyMovement> patrolsGuard;
    public List<Enemy> santiagoAndGuards;
    public RetroCastleDialogueManager dialogueManager;

    #endregion
    
    #region --------------------------------------------First Cut Scene-------------------------------------------------

    [Header("First Cut Scene Camera Configuration")]
    public List<RetroCastleNightConfig> cameraConfig;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartCamera());
        StartCoroutine(StartingCharMovement());
    }

    private IEnumerator StartCamera()
    {
        virtualCameraManager.SetCameraPosition(cameraConfig[0].position, cameraConfig[0].FOV);

        for (int i = 1; i < cameraConfig.Count; i++)
        {
            var ele = cameraConfig[i];
            yield return StartCoroutine(virtualCameraManager.MoveCameraRoutine(ele.position, ele.FOV, ele.restTime, ele.duration));
        }
    }

    private IEnumerator StartingCharMovement()
    {
        Coroutine coroutine = null;
        foreach (var pla in players)
        {
           coroutine =  StartCoroutine(pla.FirstMovemnt());
        }

        yield return coroutine;
        yield return new WaitForSeconds(1);

        dialogueManager.StartDialague(0);
        /*foreach (var ene in santiagoAndGuards)
        {
            StartCoroutine(ene.GetComponent<EnemyMovement>().FixedMovemnt());
        }*/
    }
}
