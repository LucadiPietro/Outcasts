namespace LemonGames
{
    using UnityEngine;

    public static class VectorExtensions
    {
        /// <summary>
        /// Change one or more coordinates by using named parameters
        /// </summary>
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue) vector.x = x.Value;
            if (y.HasValue) vector.y = y.Value;
            if (z.HasValue) vector.z = z.Value;
            return vector;
        }
        /// <summary>
        /// Change one or more coordinates by using named parameters
        /// </summary>
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            if (x.HasValue) vector.x = x.Value;
            if (y.HasValue) vector.y = y.Value;
            return vector;
        }
    }
}
