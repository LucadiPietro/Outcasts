using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// This script resets Brogan's position after the BounceBack animation, canceling the offset 
    /// given by the animation's last frame
    /// </summary>
    
public class ResetBrogan : MonoBehaviour
{

    [SerializeField] Transform body;
    Transform bodyParent;
    [SerializeField] Animator animator;

    private void Start()
    {
        bodyParent = body.parent;
    }

    public void ResetPosition()
    {
        Vector3 globalPos = body.position;
        Vector3 localPos = body.localPosition;

        bodyParent.position = globalPos;

        animator.SetTrigger("Reset");
    }
}
