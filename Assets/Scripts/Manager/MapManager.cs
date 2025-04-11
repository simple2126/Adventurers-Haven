using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    public bool Occupied { get; private set; }
    public GameObject Building { get; private set; }

    public TileData(bool occupied = false, GameObject building = null)
    {
        this.Occupied = occupied;
        this.Building = building;
    }
}

public class MapManager : SingletonBase<MapManager>
{
    public Tilemap BuildingTilemap;

    private Dictionary<Vector3Int, TileData> tileDict = new();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetTIleDict();
    }

    private void SetTIleDict()
    {
        BoundsInt bounds = BuildingTilemap.cellBounds;
        for (int x = bounds.x; x < bounds.xMax; x++)
        {
            for (int y = bounds.y; y < bounds.yMax; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y;
                if (BuildingTilemap.HasTile(pos))
                {
                    tileDict[pos] = new TileData(true);
                }
                else
                {
                    tileDict[pos] = new TileData(false);
                }
            }
        }
    }

    public bool CanPlaceBuilding(Vector3Int origin, Vector2Int size)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;
                
                if (tileDict.ContainsKey(pos) && tileDict[pos].Occupied) return false;
            }
        }
        return true;
    }

    public void SetBuildingArea(Vector3Int origin, Vector2Int size, GameObject building)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                if (!tileDict.ContainsKey(pos)) tileDict[pos] = new TileData();
                tileDict[pos] = new TileData(true, building);
            }
        }
    }
}
