using Common.Cutscenes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePlayer : MonoBehaviour
{
    public void PlayCutscene(SequentialCutscene cutscene)
    {
        cutscene.Play();
    }
}
