using TMPro;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "BattleSystem/SaveData", fileName = "SaveData")]
public class SaveData : ScriptableObject
{
    public AudioClip audioClip;
    public float pitch = 100;
    [System.Serializable]
    public class Item
    {
        public string tagName;
        public float songTime;
        public TMP_Dropdown.OptionData dropdown1Selection;
        public TMP_Dropdown.OptionData dropdown2Selection;
    }

    public Item[] items;

}