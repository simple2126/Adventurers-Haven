using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Global : SingletonBase<Global>
{
    public Dictionary<Vector3Int, CustomTileData> ElemDict => MapManager.Instance.ElementTileDict;
    public Dictionary<Vector3Int, CustomTileData> BuildDict => MapManager.Instance.BuildTileDict;
    public Tilemap ElementTilemap => MapManager.Instance.ElementTilemap;
    public Tilemap BuildingTilemap => MapManager.Instance.BuildingTilemap;

    public static readonly Vector2Int RoadSize = new(2, 2);

    public static readonly Vector3Int[] Dir4 = {
        Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down
    };
}

