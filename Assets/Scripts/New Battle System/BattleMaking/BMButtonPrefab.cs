using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMButtonPrefab : MonoBehaviour
{
    public enum Cell
    {
        Cell1,
        Cell2,
        Cell3,
        Cell4,
        Cell5,
        Cell6
    }

    public Cell cell;

    public Keys key;

    public Sprite buttonSprite;

    public bool setted;
    


    void Start()
    {
        if (!transform.parent)
        {
            if (!setted)
            {
                var cellDivisor = FindObjectOfType<CellDivisor>();
                if(cellDivisor) cellDivisor.SetParent();
            }

            var manager = FindObjectOfType<BMManager>();
            manager.CreateButton(this);
        }
        else
        {
            if (Enum.TryParse(transform.parent.name, out cell))
            {
                var manager = FindObjectOfType<BMManager>();
                manager.CreateButton(this);
            }
            else
            {
                Debug.LogError("La stringa non corrisponde ad alcun elemento di enum.");
            }
        }
    }
}