using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileConsole : Singleton<MobileConsole>
{

    [SerializeField] int maxLines = 10;
    Text text;

    protected override void OnAwake()
    {
        base.OnAwake();
        text = transform.GetChild(0).GetComponent<Text>();
    }

    List<string> lines = new List<string>();
    public void Log(string s)
    {
        lines.Add(s);
        if(lines.Count > maxLines)
        {
            lines.RemoveAt(0);
        }
        text.text = "";
        foreach(string ss in lines)
        {
            text.text += ss + "\n";
        }
    }



}
