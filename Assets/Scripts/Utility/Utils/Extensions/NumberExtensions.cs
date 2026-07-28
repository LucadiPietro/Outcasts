namespace LemonGames
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Returns true if the value is between <paramref name="rangeMin"/> and <paramref name="rangeMax"/>
        /// </summary>
        public static bool IsInRange(this float value, float rangeMin, float rangeMax) => value >= rangeMin && value <= rangeMax;
        /// <summary>
        /// Returns true if the value is between <paramref name="rangeMin"/> and <paramref name="rangeMax"/>
        /// </summary>
        public static bool IsInRange(this double value, double rangeMin, double rangeMax) => value >= rangeMin && value <= rangeMax;
        /// <summary>
        /// Returns true if the value is between <paramref name="rangeMin"/> and <paramref name="rangeMax"/>
        /// </summary>
        public static bool IsInRange(this int value, int rangeMin, int rangeMax) => value >= rangeMin && value <= rangeMax;
        /// <summary>
        /// Transforms the current value mapping min -> newMin and max -> newMax.
        /// </summary>
        public static float Remap(this float value, float min, float max, float newMin, float newMax) => (value - min) / (max - min) * (newMax - newMax) + newMin;
    }
}
