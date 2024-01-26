using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class RetroCastleNightCameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    public IEnumerator MoveCameraRoutine(Vector3 pos1, float FOV1, float restTime, float duration)
    {
        yield return new WaitForSeconds(restTime);

        bool end = false;
        
        DOTween.To(() => virtualCamera.m_Lens.FieldOfView, x => virtualCamera.m_Lens.FieldOfView = x, FOV1, duration).SetEase(Ease.InQuad);
        virtualCamera.transform.DOMove(pos1, duration).SetEase(Ease.InQuad).OnComplete(() => end = true);

        yield return new WaitUntil(() => end);
    }

    public void SetCameraPosition(Vector3 initPosition, float initFOV)
    {
        virtualCamera.transform.position = initPosition;
        virtualCamera.m_Lens.FieldOfView = initFOV;
    }
}
