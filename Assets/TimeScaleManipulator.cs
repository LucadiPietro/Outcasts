using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManipulator : MonoBehaviour
{
    public void ToggleTimeScale(float scale)
    {
        if(Time.timeScale == 1)
        {
            Time.timeScale = scale;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void EditTimeScale(Single value)
    {
        Time.timeScale = value;
    }
}
