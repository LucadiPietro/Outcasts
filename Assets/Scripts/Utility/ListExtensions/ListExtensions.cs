namespace Outcasts
{
    using System;
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static (int insertionIndex, bool existing) BinarySearch<T>(this IList<T> list, T valueToFind, IComparer<T> comparer)
        {
            int lowerIndex = 0;
            int higherIndex = list.Count - 1;

            while (lowerIndex <= higherIndex)
            {
                int middleIndex = lowerIndex + ((higherIndex - lowerIndex) / 2);
                T middleElement = list[middleIndex];
                int comparison = comparer.Compare(middleElement, valueToFind);

                // If they are equal
                if (comparison == 0) return (middleIndex, true);
                // If value to find is to the right, increase lowerIndex
                else if (comparison < 0) lowerIndex = middleIndex + 1;
                // If value to find is to the left, decrease higherIndex
                else higherIndex = middleIndex - 1;
            }

            return (lowerIndex, false); // Insertion index
        }
        public static (int insertionIndex, bool existing) BinarySearch<T>(this IList<T> list, T valueToFind)
            where T : IComparable<T>
        {
            int lowerIndex = 0;
            int higherIndex = list.Count - 1;

            while (lowerIndex <= higherIndex)
            {
                int middleIndex = lowerIndex + ((higherIndex - lowerIndex) / 2);
                T middleElement = list[middleIndex];
                int comparison = middleElement.CompareTo(valueToFind);

                // If they are equal
                if (comparison == 0) return (middleIndex, true);
                // If value to find is to the right, increase lowerIndex
                else if (comparison < 0) lowerIndex = middleIndex + 1;
                // If value to find is to the left, decrease higherIndex
                else higherIndex = middleIndex - 1;
            }

            return (lowerIndex, false); // Insertion index
        }
    }
}
