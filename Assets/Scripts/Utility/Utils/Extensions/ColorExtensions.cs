namespace LemonGames
{
    using UnityEngine;

    public static class ColorExtensions
    {
        /// <summary>
        /// Change one or more channels by using named parameters
        /// </summary>
        public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            if (r.HasValue) color.r = r.Value;
            if (g.HasValue) color.g = g.Value;
            if (b.HasValue) color.b = b.Value;
            if (a.HasValue) color.a = a.Value;
            return color;
        }
    }
}
