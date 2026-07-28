using UnityEngine;

[CreateAssetMenu(menuName = "BattleSystem/New Dropdown Items", fileName = "Dropdown Items")]
public class DropdownItems : ScriptableObject
{
    [System.Serializable]
    public class Item
    {
        public string itemName;
        public Sprite itemImage;
        public Keys key;
    }

    public Item[] items;
}