using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    public Vector3 initPosition;
    public float initFOV;
    public Vector3 pos1;
    public float FOV1;

    private IEnumerator Start()
    {
        virtualCamera.transform.position = initPosition;
        virtualCamera.m_Lens.FieldOfView = initFOV;

        yield return new WaitForSeconds(3f);

        
        DOTween.To(() => virtualCamera.m_Lens.FieldOfView, x => virtualCamera.m_Lens.FieldOfView = x, FOV1, 8).SetEase(Ease.InQuad);
        virtualCamera.transform.DOMove(pos1, 8).SetEase(Ease.InQuad);
    }
}