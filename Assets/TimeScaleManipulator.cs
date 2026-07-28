using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManipulator : MonoBehaviour
{
    public float scaleOnStart = 1.5f;

    private void Start()
    {
        Time.timeScale = scaleOnStart;
    }

    public void ToggleTimeScale(float scale)
    {
        if(Time.timeScale == scaleOnStart)
        {
            Time.timeScale = scale;
        }
        else
        {
            Time.timeScale = scaleOnStart;
        }
    }

    public void EditTimeScale(Single value)
    {
        Time.timeScale = value;
    }
}
