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
    public List<CameraConfigCollections> cameraConfigCollectionsList;

    public bool cameraCanMove;
    public bool canMove;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartCamera());
        StartCoroutine(StartingCharMovement());
    }

    private IEnumerator StartCamera()
    {
        var cameraConfig = cameraConfigCollectionsList[0].cameraConfigs;
        virtualCameraManager.SetCameraPosition(cameraConfig[0].position, cameraConfig[0].FOV);

        for (int i = 1; i < cameraConfig.Count; i++)
        {
            var ele = cameraConfig[i];
            yield return StartCoroutine(virtualCameraManager.MoveCameraRoutine(ele.position, ele.FOV, ele.restTime, ele.duration));
        }
        
        yield return new WaitUntil((() => cameraCanMove));

        cameraCanMove = false;
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
        
        foreach (var ene in santiagoAndGuards)
        {
            ene.gameObject.SetActive(true);
        }

        yield return new WaitUntil((() => DialogueController.instance.notDialogue));
        yield return new WaitForSeconds(0.3f);
        cameraCanMove = true;
        
        yield return new WaitUntil((() => canMove));

        canMove = false;
        
        foreach (var ene in santiagoAndGuards)
        {
            ene.gameObject.SetActive(true);
            StartCoroutine(ene.GetComponent<EnemyMovement>().FixedMovemnt());
        }
    }
}
