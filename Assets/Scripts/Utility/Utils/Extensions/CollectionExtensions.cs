using UnityEngine;
using System.Collections.Generic;

public static class CollectionExtensions 
{
    
    public static void AddAll<T>(this List<T> first, List<T> second)
    {
        foreach(T t in second)
        {
            first.Add(t);
        }
    }

    public static void AddAllNoClones<T>(this List<T> first, List<T> second)
    {
        foreach (T t in second)
        {
            if(!first.Contains(t))
                first.Add(t);
        }
    }

    public static bool AddNoClones<T>(this List<T> first, T second)
    {
        if (!first.Contains(second))
        {
            first.Add(second);
            return true;
        }
        return false;
    }

    public static List<T> Sublist<T>(this List<T> list, int startIndex, int endIndex)
    {
        List<T> res = new List<T>();
        for(int x = startIndex; x < endIndex; x++)
        {
            if(x < list.Count )
                res.Add(list[x]);
        }
        return res;
    }

    public static T GetRandom<T>(this List<T> list)
    {
        int i = Random.Range(0, list.Count);
        return list[i];
    }

    public static T PopRandom<T>(this List<T> list)
    {
        T elem = list.GetRandom();
        list.Remove(elem);
        return elem;
    }

    public static bool TrueForAny<T>(this List<T> list, System.Predicate<T> match)
    {
        foreach(T elem in list)
        {
            if (match.Invoke(elem)) return true;
        }
        return false;
    }

}
