using UnityEngine;
using System.Collections;

public static class VectorExtensions 
{
    
    public static Vector3 ToVector3(this Vector2Int v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector3 ToVector3(this Vector2 v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector2 ToVector2(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3[] ToVector3(this Vector2[] arr)
    {
        Vector3[] result = new Vector3[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            result[i] = arr[i].ToVector3();
        }
        return result;
    }

    public static Vector2 Rotate(this Vector2 v, float radians)
    {
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    public static float Distance(this Vector2 v, Vector2 to)
    {
        return Vector2.Distance(v, to);
    }

    public static float Distance(this Vector3 v, Vector3 to)
    {
        return Vector3.Distance(v, to);
    }

    public static Vector3 Add(this Vector3 v3, Vector2 v2)
    {
        return new Vector3(v3.x + v2.x, v3.y + v2.y, v3.z);
    }

    public static Vector2 Randomize(this Vector2 v)
    {
        v.x = Random.Range(-1F, 1F);
        v.y = Random.Range(-1F, 1F);
        return v.normalized;
    }

    public static Vector2 To(this Vector2 v, Vector2 to)
    {
        return to - v;
    }

    public static Vector3 To(this Vector3 v, Vector3 to)
    {
        return to - v;
    }

    public static Vector2 Dir(this Vector2 v, Vector2 target)
    {
        return v.To(target).normalized;
    }

    public static Vector3 Dir(this Vector3 v, Vector3 target)
    {
        return v.To(target).normalized;
    }

}
