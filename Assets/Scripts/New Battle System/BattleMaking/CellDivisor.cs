using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CellDivisor : MonoBehaviour
{
    public PlayableDirector playableDirector;

    public void SetParent()
    {
        var timeline = playableDirector.playableAsset as TimelineAsset;
        var list = timeline.GetOutputTracks().ToList();
        
        foreach (var ele in list)
        {
            
            switch (ele.name)
            {
                case var _ when ele.name.Contains("Cell1"):
                    foreach (var clip in ele.GetClips().ToList())
                    {
                        ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                        var obj = control.prefabGameObject.GetComponent<BMButtonPrefab>();
                        obj.cell = BMButtonPrefab.Cell.Cell1;
                        obj.setted = true;
                    }
                    break;
                case var _ when ele.name.Contains("Cell2"):
                    foreach (var clip in ele.GetClips().ToList())
                    {
                        ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                        var obj = control.prefabGameObject.GetComponent<BMButtonPrefab>();
                        obj.cell = BMButtonPrefab.Cell.Cell2;
                        obj.setted = true;
                    }
                    break;
                case var _ when ele.name.Contains("Cell3"):
                    foreach (var clip in ele.GetClips().ToList())
                    {
                        ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                        var obj = control.prefabGameObject.GetComponent<BMButtonPrefab>();
                        obj.cell = BMButtonPrefab.Cell.Cell3;
                        obj.setted = true;
                    }
                    break;
                case var _ when ele.name.Contains("Cell4"):
                    foreach (var clip in ele.GetClips().ToList())
                    {
                        ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                        var obj = control.prefabGameObject.GetComponent<BMButtonPrefab>();
                        obj.cell = BMButtonPrefab.Cell.Cell4;
                        obj.setted = true;
                    }
                    break;
                case var _ when ele.name.Contains("Cell5"):
                    foreach (var clip in ele.GetClips().ToList())
                    {
                        ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                        var obj = control.prefabGameObject.GetComponent<BMButtonPrefab>();
                        obj.cell = BMButtonPrefab.Cell.Cell5;
                        obj.setted = true;
                    }
                    break;
                case var _ when ele.name.Contains("Cell6"):
                    foreach (var clip in ele.GetClips().ToList())
                    {
                        ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                        var obj = control.prefabGameObject.GetComponent<BMButtonPrefab>();
                        obj.cell = BMButtonPrefab.Cell.Cell6;
                        obj.setted = true;
                    }
                    break;
            }
        }
    }
}
