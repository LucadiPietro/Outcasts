using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapExtensions 
{
    
    public static TileBase GetTile(this Tilemap tilemap, int x, int y)
    {
        return tilemap.GetTile(new Vector3Int(x, y, 0));
    }

}
