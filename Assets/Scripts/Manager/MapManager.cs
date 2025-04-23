using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomTileData
{
    public bool Occupied { get; private set; } // 사용 여부
    public string Tag { get; private set; }

    public void SetData(bool occupied = false, string tag = null)
    {
        this.Occupied = occupied;
        this.Tag = tag;
    }
}

public class MapManager : SingletonBase<MapManager>
{
    [SerializeField] private Tilemap buildingTilemap;
    public Tilemap BuildingTilemap { get; private set; }

    [SerializeField] private Tilemap elementTilemap;
    public Tilemap ElementTilemap { get; private set; }

    private Dictionary<Vector3Int, CustomTileData> buildTileDict = new();
    private Dictionary<Vector3Int, CustomTileData> elementTileDict = new();

    protected override void Awake()
    {
        base.Awake();
        BuildingTilemap = buildingTilemap;
        ElementTilemap = elementTilemap;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetTIleDict(BuildingTilemap, buildTileDict);
        SetTIleDict(ElementTilemap, elementTileDict);
    }

    private void SetTIleDict(Tilemap tilemap, Dictionary<Vector3Int, CustomTileData> tileDict)
    {
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.x; x < bounds.xMax; x++)
        {
            for (int y = bounds.y; y < bounds.yMax; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y;

                if (!tileDict.ContainsKey(pos))
                    tileDict[pos] = new CustomTileData();

                if (tilemap.HasTile(pos))
                {
                    var tileTag = tilemap == elementTilemap ? "WhiteRockRoad" : null;
                    tileDict[pos].SetData(true, tileTag);
                }
                else
                {
                    tileDict[pos].SetData(false);
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

    public void SetBuildingArea(Vector3Int origin, Vector2Int size, Construction construction)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        var tileDict = construction.Type == ConstructionType.Build ? buildTileDict : elementTileDict;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                if (!tileDict.ContainsKey(pos))
                    tileDict[pos] = new CustomTileData();

                tileDict[pos].SetData(true, construction.Tag);
            }
        }
    }

    // 현재 위치에 같은 도로가 있는지 확인 (같은 도로 배치 불가)
    public bool IsSameRoadData(Vector3Int origin, Vector2Int size, string objName)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        int count = 0;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;
                if (!elementTileDict.ContainsKey(pos)) continue;
                if (elementTileDict[pos].Tag == objName) count++;
            }
        }
        
        return size.x * size.y == count ? true : false;
    }
}
