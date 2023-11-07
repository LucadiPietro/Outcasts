using UnityEngine;

class LayerMaskExtensions
{
    public static bool Contains( LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
