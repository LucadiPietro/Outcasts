using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BMManager : MonoBehaviour
{
    public SDictionary<string, GameObject> cells;
    public List<GameObject> bars;


    public abstract void CreateButton(BMButtonPrefab prefab);
}