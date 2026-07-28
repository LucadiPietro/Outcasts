using UnityEngine;
class EightDirRC
{
    static float[] res = new float[8];
    public static float[] DistancesToLayerMask(Vector2 p, LayerMask lm, float dist)
    {
        for (int a = 0; a < 8; a++)
        {
            Vector2 dir = Vector2.up.Rotate(2 * Mathf.PI * a / 8);
            RaycastHit2D hit = Physics2D.Raycast(p, dir, dist, lm);
            if (hit.collider != null)
            {
                Debug.DrawLine(p, p + dir, Color.red, Time.deltaTime);
                res[a] = hit.collider.transform.Distance(p);
            }
            else
            {
                res[a] = -1f;
            }
        }
        return res;
    }

    public static float[] DistancesToLayerMask(Vector2 p, LayerMask lm)
    {
        return DistancesToLayerMask(p, lm, Mathf.Infinity);
    }

}
