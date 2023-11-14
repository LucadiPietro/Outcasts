using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public Transform target;

    public bool startMove;

    public bool run;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitUntil(() => startMove);
            
            GetComponent<EnemyMovement>().Movement(target.position,run);
            startMove = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
