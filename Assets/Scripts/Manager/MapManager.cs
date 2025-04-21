using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    public bool Occupied { get; private set; } // 사용 여부
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
    public Tilemap ElementTilemap;

    private Dictionary<Vector3Int, TileData> buildTileDict = new();
    private Dictionary<Vector3Int, TileData> elementTileDict = new();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetTIleDict(BuildingTilemap, buildTileDict);
        SetTIleDict(ElementTilemap, elementTileDict);
    }

    private void SetTIleDict(Tilemap tilemap, Dictionary<Vector3Int, TileData> tileDict)
    {
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.x; x < bounds.xMax; x++)
        {
            for (int y = bounds.y; y < bounds.yMax; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y;
                if (tilemap.HasTile(pos))
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

    public bool CanPlaceBuilding(Vector3Int origin, Vector2Int size, ConstructionType type)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                // 건물일 때 -> 빌딩, 도로 있으면 배치 불가
                if(type == ConstructionType.Build)
                {
                    if (buildTileDict.ContainsKey(pos) && buildTileDict[pos].Occupied) return false;
                    if (elementTileDict.ContainsKey(pos) && elementTileDict[pos].Occupied) return false;
                }
                // 도로일 때 -> 건물 있으면 배치 불가, 도로는 배치 가능
                else if(type == ConstructionType.Element) 
                {
                    if (buildTileDict.ContainsKey(pos) && buildTileDict[pos].Occupied) return false;
                }
            }
        }
        return true;
    }

    public void SetBuildingArea(Vector3Int origin, Vector2Int size, GameObject building, ConstructionType type)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        var tileDict = type == ConstructionType.Build ? buildTileDict : elementTileDict;

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
