using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RetroCastleLevelManager : MonoBehaviour
{
    #region --------------------------------------------Configuration---------------------------------------------------

    public RetroCastleNightCameraManager virtualCameraManager;
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
        //StartCoroutine(StartingCharMovement());
    }

    private IEnumerator StartCamera()
    {
        var cameraConfig = cameraConfigCollectionsList[0].cameraConfigs;
        virtualCameraManager.SetCameraPosition(cameraConfig[0].position, cameraConfig[0].FOV);

        for (int i = 1; i < cameraConfig.Count; i++)
        {
            var ele = cameraConfig[i];
            yield return StartCoroutine(
                virtualCameraManager.MoveCameraRoutine(ele.position, ele.FOV, ele.restTime, ele.duration));
        }

        yield return new WaitUntil((() => cameraCanMove));

        cameraCanMove = false;


        cameraConfig = cameraConfigCollectionsList[1].cameraConfigs;
        virtualCameraManager.SetCameraPosition(cameraConfig[0].position, cameraConfig[0].FOV);

        for (int i = 1; i < cameraConfig.Count; i++)
        {
            var ele = cameraConfig[i];
            yield return StartCoroutine(
                virtualCameraManager.MoveCameraRoutine(ele.position, ele.FOV, ele.restTime, ele.duration));
        }

        canMove = true;

        yield return new WaitUntil((() => cameraCanMove));
        
        cameraConfig = cameraConfigCollectionsList[2].cameraConfigs;
        virtualCameraManager.SetCameraPosition(cameraConfig[0].position, cameraConfig[0].FOV);

        for (int i = 1; i < cameraConfig.Count; i++)
        {
            var ele = cameraConfig[i];
            yield return StartCoroutine(
                virtualCameraManager.MoveCameraRoutine(ele.position, ele.FOV, ele.restTime, ele.duration));
        }

        canMove = true;

        yield return new WaitUntil((() => cameraCanMove));
    }
}