using AdventurersHaven;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomTileData
{
    public bool Occupied { get; private set; } // 사용 여부
    public Construction Construction { get; private set; }

    public void SetData(bool occupied = false)
    {
        this.Construction = null;
        this.Occupied = occupied;
    }

    public void SetData(Construction construction, bool occupied = false, string tag = null)
    {
        this.Construction = construction;
        this.Occupied = occupied;
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

    [SerializeField] private GameObject baseRoadPrefab;
    [SerializeField] private ConstructionType conType;
    [SerializeField] private string baseRoadTag;
    private Construction baseRoadCon;

    protected override void Awake()
    {
        base.Awake();
        BuildingTilemap = buildingTilemap;
        ElementTilemap = elementTilemap;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        baseRoadCon = baseRoadPrefab.GetComponent<Construction>();
        baseRoadCon.SetData(DataManager.Instance.GetConstructionData(conType, baseRoadTag));
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
                    if (tilemap == ElementTilemap)
                    {
                        tileDict[pos].SetData(baseRoadCon, true);
                    }
                    else tileDict[pos].SetData(true);
                }
            }
        }
    }

    public bool CanPlaceBuilding(Vector3Int origin, Vector2Int size, Construction construction)
    {
        if (construction.IsDemolish())
        {
            Vector3Int pos = Vector3Int.right + Vector3Int.up + origin;
            return buildTileDict.ContainsKey(pos) && elementTileDict.ContainsKey(pos);
        }

        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                // 건물일 때 -> 빌딩, 도로 있으면 배치 불가
                if(construction.Type == ConstructionType.Build)
                {
                    if (!buildTileDict.ContainsKey(pos)) return false;
                    if (buildTileDict.ContainsKey(pos) && buildTileDict[pos].Occupied) return false;
                    if (!elementTileDict.ContainsKey(pos)) return false;
                    if (elementTileDict.ContainsKey(pos) && elementTileDict[pos].Occupied) return false;
                }
                // 도로일 때 -> 건물 있으면 배치 불가, 도로는 배치 가능
                else if(construction.Type == ConstructionType.Element) 
                {
                    if (!buildTileDict.ContainsKey(pos)) return false;
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

                tileDict[pos].SetData(construction, true);
            }
        }
    }

    public void RemoveBuildingArea(Vector3Int origin, Vector2Int size, Construction construction)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        List<Construction> removedConstructions = new List<Construction>();

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                // 빌딩 타일맵
                if (buildTileDict.ContainsKey(pos))
                {
                    var realConstruction = buildTileDict[pos].Construction;
                    if (realConstruction != null)
                    {
                        buildingTilemap.SetTile(pos, null);
                        buildTileDict[pos].SetData(false);

                        // 한 번만 ReturnToPool 하기 위해
                        if (!removedConstructions.Contains(realConstruction))
                        {
                            PoolManager.Instance.ReturnToPool<Construction>(realConstruction.Tag, realConstruction);
                            removedConstructions.Add(realConstruction);
                        }
                    }
                }

                // 엘리먼트 타일맵 (도로 등)
                if (elementTileDict.ContainsKey(pos))
                {
                    var realConstruction = elementTileDict[pos].Construction;
                    if (realConstruction != null)
                    {
                        elementTilemap.SetTile(pos, null);
                        elementTileDict[pos].SetData(false);

                        if (!removedConstructions.Contains(realConstruction))
                        {
                            PoolManager.Instance.ReturnToPool<Construction>(realConstruction.Tag, realConstruction);
                            removedConstructions.Add(realConstruction);
                        }
                    }
                }
            }
        }

        PoolManager.Instance.ReturnToPool<Construction>(construction.Tag, construction);
    }

    // 현재 위치에 같은 도로가 있는지 확인 (같은 도로 배치 불가)
    public bool IsSameRoadData(Vector3Int origin, Vector2Int size, string tag)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        int count = 0;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;
                if (!elementTileDict.ContainsKey(pos) || elementTileDict[pos].Construction == null) continue;
                if (elementTileDict[pos].Construction.Tag == tag) count++;
            }
        }
        
        return size.x * size.y == count ? true : false;
    }

    public Vector2Int GetBuildingAre(Vector3Int pos)
    {
        if(buildTileDict.ContainsKey(pos) && buildTileDict[pos].Construction != null)
        {
            return buildTileDict[pos].Construction.Size;
        }
        
        if (elementTileDict.ContainsKey(pos) && elementTileDict[pos].Construction != null)
        {
            return elementTileDict[pos].Construction.Size;
        }

        return Vector2Int.one;
    }
}
