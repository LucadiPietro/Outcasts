using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBoxManipulator : MonoBehaviour
{
    Vector3 originalPos;
    [SerializeField] Vector3 differentPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    public void SetDifferentPos()
    {
        transform.localPosition = differentPos;  
    }

    public void ResetPos()
    {
        transform.localPosition = originalPos;
    }
}
