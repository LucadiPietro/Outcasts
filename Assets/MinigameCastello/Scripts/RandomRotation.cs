using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotationScript : MonoBehaviour
{
    public float rotationSpeed = 90.0f;
    private bool canRotate = true;


    void Update()
    {
        if (canRotate)
        {
            StartCoroutine(RotateObject(Vector3.forward * rotationSpeed, 3.0f));
        }
    }

    IEnumerator RotateObject(Vector3 axis, float delay)
    {
        canRotate = false;

        Quaternion targetRotation = Quaternion.AngleAxis(Random.Range(0, 4) * 90, Vector3.forward) * transform.rotation;

        float elapsedTime = 0.0f;
        while (elapsedTime < delay)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, (elapsedTime / delay));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canRotate = true;
    }
}
