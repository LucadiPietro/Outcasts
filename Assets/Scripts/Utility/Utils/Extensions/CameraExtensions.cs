using UnityEngine;
using System.Collections;

public static class CameraExtensions 
{

    public static Vector3 GetMouseWorldPosition(this Camera cam)
    {
        Vector3 mp = Input.mousePosition;
        mp.z = -10f;
        Vector3 pos = cam.ScreenToWorldPoint(mp);
        return pos;
    }

    public static Vector3 GetTouchWorldPosition(this Camera cam)
    {
        Vector3 mp = Input.GetTouch(0).position;
        mp.z = -10f;
        Vector3 pos = cam.ScreenToWorldPoint(mp);
        return pos;
    }

}
