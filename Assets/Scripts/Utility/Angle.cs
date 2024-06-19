using UnityEngine;
namespace LemonGames
{
    [System.Serializable]
    /// <summary>
    /// An angle in degrees
    /// </summary>
    public struct Angle
    {
        public static readonly Angle NaN = new Angle(float.NaN);
        public Angle(float value)
        {
            this.value = value;
        }
        [SerializeField]
        float value;
        public float Degrees
        {
            get { return value; }
        }
        public float Radian
        {
            get { return value * Mathf.Deg2Rad; }
            set { this.value = value * Mathf.Rad2Deg; }
        }
        /// <summary>
        /// Remap value between 0 and 360
        /// </summary>
        public Angle Normalized
        {
            get { return Mathf.Repeat(value, 360); }
        }
        public bool IsNaN
        {
            get { return float.IsNaN(value); }
        }
        /// <summary>
        /// Length of the Chord relative to this angle on a unit circle
        /// </summary>
        public float ChordLenth
        {
            get { return 2 * Mathf.Sin(Radian / 2); }
        }
        /// <summary>
        /// The angle with the specified chord length on a unit circle
        /// </summary>
        public static Angle FromChordLenth(float chordLength)
        {
            return FromRadians(2 * Mathf.Asin(chordLength / 2));
        }
        public static Angle FromRadians(float radiansAngle)
        {
            return radiansAngle * Mathf.Rad2Deg;
        }
        public static Angle FromDegree(float degreeAngle)
        {
            return degreeAngle;
        }
        /// <summary>
        /// Calculates the angle corrisponding to the given position.
        /// </summary>
        /// <param name="pos">Position in the plane</param>
        /// <returns>The angle corrisponding to the position, in the [-180, 180] range</returns>
        public static Angle FromPosition(Vector2 pos)
        {
            return FromRadians(Mathf.Atan2(pos.y, pos.x));
        }
        /// <summary>
        /// Calculates the position corrisponding to the given angle and radius.
        /// </summary>
        /// <returns>The position corrisponding to the angle and radius</returns>
        public Vector2 ToPosition(float radius = 1.0f)
        {
            return new Vector2(radius * Mathf.Cos(Radian), radius * Mathf.Sin(Radian));
        }
        /// <summary>
        /// Calculates the position corrisponding to the given angle and radius.
        /// </summary>
        /// <returns>The position corrisponding to the angle and radius</returns>
        public static Vector2 ToPosition(Angle angle, float radius = 1.0f)
        {
            return new Vector2(radius * Mathf.Cos(angle.Radian), radius * Mathf.Sin(angle.Radian));
        }
        /// <summary>
        /// Brings the angle in the [min, min + 360] range
        /// </summary>
        /// <param name="min">Returned angle will be equal or greater than this value</param>
        /// <returns>An equivalent angle, just greater than min</returns>
        public Angle MakeBiggerThan(float min = 0)
        {
            return InRange(min, min + 360);
        }
        /// <summary>
        /// Brings the angle in the [max - 360, max] range
        /// </summary>
        /// <param name="max">Returned angle will be equal or less than this value</param>
        /// <returns>An equivalent angle, just less than max</returns>
        public Angle MakeSmallerThan(float max = 360)
        {
            return InRange(max - 360, max);
        }
        /// <summary>
        /// Brings the given angle in the [min, max] range
        /// If the angle is not in the given range, it will return NaN.
        /// </summary>
        /// <param name="min">Minimum endpoint of the desired interval</param>
        /// <param name="max">Maximum endpoint of the desired interval</param>
        /// <returns>The angle in the specified interval</returns>
        public Angle InRange(float min = 0, float max = 360)
        {
            Angle result = this;
            // whether we have to increase the value
            bool mustIncrease = result < min;
            // if result is less than min, we should get the next one
            while (result < min)
            {
                result += 360;
            }
            // whether we have to decrease the value
            bool mustDecrease = result > max;
            // we can't decrease it if we already had to increase it or it will get again smaller than min
            // the angle is not in the given range
            if (mustIncrease && mustDecrease) return NaN;
            // decrease result if we have to
            while (result > max)
            {
                result -= 360;
            }
            // if we are here, we either increased or decreased the result to the correct value
            return result;
        }
        /// <summary>
        /// Calculates the smallest of the two arc lengths between two given angles
        /// </summary>
        /// <returns>The smallest of the two arc lengths between the two angles (always positive)</returns>
        public static Angle SmallestArc(Angle first, Angle second)
        {
            Angle difference = second.Normalized.MakeBiggerThan(first.Normalized) - first.Normalized;
            return Mathf.Min(difference, 360 - difference);
        }
        /// <summary>
        /// Calculates the greatest of the two arc lengths between two given angles
        /// </summary>
        /// <returns>The greatest of the two arc lengths between the two angles (always positive)</returns>
        public static Angle GreatestArc(Angle first, Angle second)
        {
            return 360 - SmallestArc(first, second);
        }
        /// <summary>
        /// If an angle changes from first to second, tries to figure out (based on the smallest arc length)
        /// if the change was clockwise or counter-clockwise. In the first case returns the positive difference
        /// between the two, otherwise, the negative one.
        /// E.G.: first = 90; second = 330; If first moved counter-clockwise to second, it would have traveled 240 degrees.
        /// If clockwise, only 120; Thus the returned value is -120;
        /// </summary>
        /// <returns>The smallest of the two arc lengths between the two angles (always positive)</returns>
        public static Angle SmallestSignedDifference(Angle first, Angle second)
        {
            Angle difference = second.MakeBiggerThan(first) - first;
            if (difference == 0) return 0;
            if (difference < 180)
            {
                return difference;
            }
            else
            {
                return difference - 360;
            }
        }
        public static implicit operator float(Angle angle)
        {
            return angle.value;
        }
        public static implicit operator Angle(float value)
        {
            return new Angle(value);
        }
        public static bool operator ==(Angle first, Angle second)
        {
            return Mathf.Approximately(first, second);
        }
        public static bool operator !=(Angle first, Angle second)
        {
            return !(first == second);
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Angle)) return false;
            Angle a = (Angle)obj;
            return a == this;
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
