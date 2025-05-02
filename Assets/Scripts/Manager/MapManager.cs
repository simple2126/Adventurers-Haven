using AdventurersHaven;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CustomTileData
{
    public bool Occupied { get; private set; } // 사용 여부
    public Construction Construction { get; private set; }

    public void SetData(bool occupied = false)
    {
        this.Construction = null;
        this.Occupied = occupied;
    }

    public void SetData(Construction construction, bool occupied = false)
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

    [SerializeField] private PoolManager.PoolConfig poolConfig;
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
        PoolManager.Instance.AddPools<Construction>(poolConfig);
        baseRoadCon = poolConfig.Prefab.GetComponent<Construction>();
        baseRoadCon.SetData(DataManager.Instance.GetConstructionData(conType, baseRoadTag));
        SetTIleDict(BuildingTilemap, buildTileDict);
        SetTIleDict(ElementTilemap, elementTileDict);
    }

    private void SetTIleDict(Tilemap tilemap, Dictionary<Vector3Int, CustomTileData> tileDict)
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3 cellSize = tilemap.cellSize;
        var conData = DataManager.Instance.GetConstructionData(conType, baseRoadTag);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y;

                if (!tileDict.ContainsKey(pos))
                    tileDict[pos] = new CustomTileData();

                if (tilemap.HasTile(pos))
                {
                    if (tilemap == ElementTilemap)
                    {
                        Debug.Log($"ElementTilemap Has Tile at {pos}");
                        int offsetX = x - bounds.xMin;
                        int offsetY = y - bounds.yMin;

                        // 가로 (왼 -> 오), 세로(아래 -> 위)
                        if ((offsetX % baseRoadCon.Size.x == 0) && (offsetY % baseRoadCon.Size.y == 0))
                        {
                            // 월드 좌표 계산 후 객체 생성
                            Vector3 worldPos = tilemap.GetCellCenterWorld(pos) + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
                            var con = PoolManager.Instance.SpawnFromPool<Construction>(baseRoadTag, worldPos, Quaternion.identity);
                            con.SetData(conData);
                            SetBuildingAreaLeftBottom(pos, con.Size, con); // 타일맵에 도로 배치
                        }
                    }
                    else tileDict[pos].SetData(true);
                }
            }
        }
    }

    // 초기화 때만 사용 -> 좌하단 기준
    public void SetBuildingAreaLeftBottom(Vector3Int origin, Vector2Int size, Construction construction)
    {
        var tileDict = construction.Type == ConstructionType.Build ? buildTileDict : elementTileDict;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                if (!tileDict.ContainsKey(pos))
                    tileDict[pos] = new CustomTileData();

                tileDict[pos].SetData(construction, true);
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

        // 타겟 건축물을 가져옴
        Construction targetCon = GetCurrentConstruction(origin);
        if (targetCon == null) return;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                // 빌딩 타일맵에서 동일 오브젝트 삭제
                if (targetCon.Type == ConstructionType.Build && buildTileDict.ContainsKey(pos))
                {
                    var realConstruction = buildTileDict[pos].Construction;
                    if (realConstruction != null && realConstruction == targetCon) // targetCon과 비교
                    {
                        buildingTilemap.SetTile(pos, null);
                        buildingTilemap.RefreshTile(pos);
                        buildTileDict[pos].SetData(false);

                        Debug.Log($"GetData {buildingTilemap.GetTile(pos) == null}");

                        // 한 번만 ReturnToPool 하기 위해
                        if (!removedConstructions.Contains(realConstruction))
                        {
                            PoolManager.Instance.ReturnToPool<Construction>(realConstruction.Tag, realConstruction);
                            removedConstructions.Add(realConstruction);
                        }
                    }
                }

                // 엘리먼트 타일맵에서 동일 오브젝트 삭제
                if (targetCon.Type == ConstructionType.Element && elementTileDict.ContainsKey(pos))
                {
                    var realConstruction = elementTileDict[pos].Construction;
                    if (realConstruction != null && realConstruction == targetCon) // targetCon과 비교
                    {
                        elementTilemap.SetTile(pos, null);
                        elementTilemap.RefreshTile(pos);
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

    private Construction GetCurrentConstruction(Vector3Int pos)
    {
        if (buildTileDict.ContainsKey(pos) && buildTileDict[pos].Construction != null)
        {
            return buildTileDict[pos].Construction;
        }
        else if (elementTileDict.ContainsKey(pos) && elementTileDict[pos].Construction != null)
        {
            return elementTileDict[pos].Construction;
        }
        return null;
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

    public Vector2Int GetConstructionSize(Vector3Int pos)
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

    public bool CurrentSizeInOneObject(Vector3Int origin, Vector2Int size)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        int buildCount = 0;
        int elementCount = 0;

        Construction con = GetCurrentConstruction(origin);

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;
                if (buildTileDict.ContainsKey(pos) && buildTileDict[pos].Construction != null)
                {
                    if (buildTileDict[pos].Construction.gameObject == con.gameObject) buildCount++;
                }

                if (elementTileDict.ContainsKey(pos) && elementTileDict[pos].Construction != null)
                {
                    if (elementTileDict[pos].Construction.gameObject == con.gameObject) elementCount++;
                }
            }
        }

        int maxCount = size.x * size.y;
        Debug.Log($"maxCount {maxCount} buildCount {buildCount} elementCount {elementCount}");
        return maxCount == buildCount || maxCount == elementCount ? true : false;
    }

    public void ShowOrHideTileDict(bool isShow)
    {
        foreach(var tile in buildTileDict.Values)
        {
            if (tile.Construction != null)
            {
                tile.Construction.gameObject.SetActive(isShow);
            }
        }
        foreach (var tile in elementTileDict.Values)
        {
            if (tile.Construction != null)
            {
                tile.Construction.gameObject.SetActive(isShow);
            }
        }
    }
}
