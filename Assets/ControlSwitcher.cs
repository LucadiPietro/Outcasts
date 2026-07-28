using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControlSwitcher : MonoBehaviour
{
    [SerializeField] UnityEvent FromFreeToCutscene;
    [SerializeField] UnityEvent FromCutsceneToFree;

    public void SwitchToCutscene()
    {
        FromFreeToCutscene.Invoke();
    }

    public void SwitchToFree()
    {
        FromCutsceneToFree.Invoke();
    }
}
