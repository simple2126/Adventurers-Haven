using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomTileData
{
    public bool IsOccupied { get; private set; }
    public Construction Construction { get; private set; }

    public void ClearTileData()
    {
        IsOccupied = false;
        Construction = null;
    }

    public void SetTileData(Construction construction)
    {
        IsOccupied = construction != null;
        Construction = construction;
    }

    public void SetOccupied()
    {
        IsOccupied = true;
        Construction = null;
    }
}

public class MapManager : SingletonBase<MapManager>
{
    [SerializeField] private Tilemap buildingTilemap;
    public Tilemap BuildingTilemap { get; private set; }

    [SerializeField] private Tilemap elementTilemap;
    public Tilemap ElementTilemap { get; private set; }

    public Dictionary<Vector3Int, CustomTileData> BuildTileDict { get; private set; } = new();
    public Dictionary<Vector3Int, CustomTileData> ElementTileDict { get; private set; } = new();

    [SerializeField] private PoolManager.PoolConfig poolConfig;
    [SerializeField] private ConstructionType conType;
    [SerializeField] private string baseRoadTag;
    [SerializeField] private string typeID;
    private Construction baseRoadCon;

    private TilemapPainter tilemapPainter;

    protected override void Awake()
    {
        base.Awake();
        BuildingTilemap = buildingTilemap;
        ElementTilemap = elementTilemap;
        tilemapPainter = GetComponent<TilemapPainter>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PoolManager.Instance.AddPool<Construction>(poolConfig);
        baseRoadCon = poolConfig.Prefab.GetComponent<Construction>();
        baseRoadCon.Init(conType, typeID);
        SetTileDict(BuildingTilemap, BuildTileDict);
        SetTileDict(ElementTilemap, ElementTileDict);

        SpawnerManager.Instance.gameObject.SetActive(true);
    }

    private void SetTileDict(Tilemap tilemap, Dictionary<Vector3Int, CustomTileData> tileDict)
    {
        tilemap.CompressBounds(); // 타일맵의 경계 압축
        BoundsInt bounds = tilemap.cellBounds;
        Vector3 cellSize = tilemap.cellSize;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y;
                if (!tileDict.ContainsKey(pos))
                {
                    tileDict[pos] = new CustomTileData();
                    tileDict[pos].ClearTileData();
                }
                if (!tilemap.HasTile(pos)) continue;

                if (tilemap == ElementTilemap)
                {
                    int offsetX = x - bounds.xMin;
                    int offsetY = y - bounds.yMin;

                    // 오프셋 계산 및 타일 설정
                    if ((offsetX % baseRoadCon.Size.x == 0) && (offsetY % baseRoadCon.Size.y == 0))
                    {
                        if (HasTilesInArea(tilemap, pos, baseRoadCon.Size))
                        {
                            Vector3 worldPos = tilemap.GetCellCenterWorld(pos) + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
                            var con = PoolManager.Instance.SpawnFromPool<Construction>(baseRoadTag, worldPos, Quaternion.identity);
                            con.Init(conType, typeID);
                            tileDict[pos].SetTileData(con);
                            SetBuildingAreaLeftBottom(pos, con.Size, con);
                            tilemapPainter.PlaceTiles(tilemap, pos, con.Size, con.PatternType, false);
                        }
                    }
                }
                else
                {
                    tileDict[pos].SetOccupied();
                }
            }
        }

    }

    private bool HasTilesInArea(Tilemap tilemap, Vector3Int origin, Vector2Int size)
    {
        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector3Int checkPos = origin + new Vector3Int(dx, dy, 0);
                if (!tilemap.HasTile(checkPos))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void SetBuildingAreaLeftBottom(Vector3Int origin, Vector2Int size, Construction construction)
    {
        var tileDict = construction.Type == ConstructionType.Build ? BuildTileDict : ElementTileDict;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;
                if (!tileDict.ContainsKey(pos)) tileDict[pos] = new CustomTileData();
                tileDict[pos].SetTileData(construction);
            }
        }
    }

    /// 지정된 origin/size 영역의 셀 좌표를 열거
    public IEnumerable<Vector3Int> GetCells(Vector3Int origin, Vector2Int size)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;
        for (int x = -offsetX; x < size.x - offsetX; x++)
            for (int y = -offsetY; y < size.y - offsetY; y++)
                yield return origin + new Vector3Int(x, y, 0);
    }

    /// 공통 체크 루틴: 테스트 함수를 넘겨주면 false 반환 지점이 있으면 바로 빠져나옴
    private bool CheckArea(Vector3Int origin, Vector2Int size, Construction construction,
                           Func<Vector3Int, bool> cellTest)
    {
        foreach (var pos in GetCells(origin, size))
        {
            // 철거 모드면 있는지만 보면 되고
            if (construction.IsDemolish())
            {
                if (BuildTileDict.ContainsKey(pos) || ElementTileDict.ContainsKey(pos))
                    return true;
                continue;
            }

            // 일반 배치/요소 모드면 각 셀마다 테스트
            if (!cellTest(pos))
                return false;
        }

        // 철거인데 한 번도 발견 못했으면 false
        return !construction.IsDemolish();
    }

    /// 영역 안에 타일(오브젝트)이 있는지만 볼 때
    public bool InBounds(Vector3Int origin, Vector2Int size, Construction construction)
    {
        return CheckArea(origin, size, construction, pos =>
        {
            if (construction.Type == ConstructionType.Build)
                return BuildTileDict.ContainsKey(pos) && ElementTileDict.ContainsKey(pos);
            else // Element 타입
                return BuildTileDict.ContainsKey(pos);
        });
    }

    /// 실제 배치 가능 여부 (존재 + 비어있음) 체크
    public bool CanPlaceBuilding(Vector3Int origin, Vector2Int size, Construction construction)
    {
        return CheckArea(origin, size, construction, pos =>
        {
            if (construction.Type == ConstructionType.Build)
            {
                var bOk = BuildTileDict.TryGetValue(pos, out var b) && !b.IsOccupied;
                var eOk = ElementTileDict.TryGetValue(pos, out var e) && !e.IsOccupied;
                return bOk && eOk;
            }
            else // Element 타입
            {
                return BuildTileDict.TryGetValue(pos, out var b) && !b.IsOccupied;
            }
        });
    }

    public void SetBuildingArea(Vector3Int origin, Vector2Int size, Construction construction)
    {
        var tileDict = construction.Type == ConstructionType.Build
                       ? BuildTileDict
                       : ElementTileDict;

        if (construction.IsRoad())
        {
            //Debug.Log($"SetBuildingArea {construction.Tag} {construction.Type} {construction.SubType} {construction.GetPattern()}");
            tilemapPainter.PlaceTiles(elementTilemap, origin, size, construction.PatternType);
        }

        var list = new List<Vector3Int>();
        foreach (var pos in GetCells(origin, size))
        {
            // 사전에 무조건 key가 있다고 가정(이미 CanPlaceBuilding 으로 검증됨)
            //Debug.Log($"SetBuildingArea {pos} {construction.Tag} {construction.Type} {construction.SubType}");
            tileDict[pos].SetTileData(construction);
            //Debug.Log($"SetBuildingArea {pos} {tileDict[pos].IsOccupied} {tileDict[pos].Construction?.Tag}");
            list.Add(pos);
        }

        if (SpawnerManager.Instance.gameObject.activeSelf && construction.Type == ConstructionType.Build)
        {
            SpawnerManager.Instance.Spawn();
        }

        //Debug.Log("SetBuildingArea");
        //foreach (var pos in list)
        //{
        //    Debug.Log($"SetBuildingArea {pos} {tileDict[pos].IsOccupied} {tileDict[pos].Construction?.Tag}");
        //}
    }

    public void RemoveBuildingArea(Vector3Int origin, Construction construction)
    {
        Construction targetCon = GetCurrentConstruction(origin);
        if (targetCon == null) return;

        List<Construction> removedConstructions = new List<Construction>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        Vector3Int[] directions = 
        {
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.up,
        };

        queue.Enqueue(origin);
        visited.Add(origin);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (targetCon.Type == ConstructionType.Build && BuildTileDict.TryGetValue(current, out var buildTileData))
            {
                if (buildTileData == null) continue;
                Construction buildCon = buildTileData.Construction;
                if (buildCon != null && buildCon.gameObject == targetCon.gameObject)
                {
                    //buildingTilemap.SetTile(current, null);
                    //buildingTilemap.RefreshTile(current);
                    BuildTileDict[current].ClearTileData();

                    if (!removedConstructions.Contains(buildCon))
                    {
                        buildCon.OnCancel?.Invoke();
                        PoolManager.Instance.ReturnToPool<Construction>(buildCon.Tag, buildCon);
                        removedConstructions.Add(buildCon);
                    }

                    foreach (var dir in directions)
                    {
                        Vector3Int next = current + dir;
                        if (!visited.Contains(next))
                        {
                            visited.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            if (targetCon.Type == ConstructionType.Element && ElementTileDict.TryGetValue(current, out var elementTIleData))
            {
                if (elementTIleData == null) continue;
                Construction elementCon = elementTIleData.Construction;
                if (elementCon != null && elementCon.gameObject == targetCon.gameObject)
                {
                    elementTilemap.SetTile(current, null);
                    elementTilemap.RefreshTile(current);
                    ElementTileDict[current].ClearTileData();

                    if (!removedConstructions.Contains(elementCon))
                    {
                        if(!elementCon.IsRoad()) PoolManager.Instance.ReturnToPool<Construction>(elementCon.Tag, elementCon);
                        PoolManager.Instance.ReturnToPool<Construction>(elementCon.Tag, elementCon);
                        removedConstructions.Add(elementCon);
                    }

                    foreach (var dir in directions)
                    {
                        Vector3Int next = current + dir;
                        if (!visited.Contains(next))
                        {
                            visited.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }
        }
    }

    private Construction GetCurrentConstruction(Vector3Int pos)
    {
        if (BuildTileDict.TryGetValue(pos, out var buildTileData) && buildTileData.Construction != null)
        {
            return buildTileData.Construction;
        }
        if (ElementTileDict.TryGetValue(pos, out var elementTileData) && elementTileData.Construction != null)
        {
            return elementTileData.Construction;
        }
        return null;
    }

    public bool IsSameRoadData(Vector3Int origin, Vector2Int size, string tag)
    {
        foreach (var pos in GetCells(origin, size))
        {
            if (!ElementTileDict.TryGetValue(pos, out var tile)
                || !tile.IsOccupied
                || tile.Construction.Tag != tag)
            {
                return false;
            }
        }
        return true;
    }

    public Vector2Int GetConstructionSize(Vector3Int pos)
    {
        Construction con = GetCurrentConstruction(pos);
        return con == null ? Vector2Int.zero : con.Size;
    }

    public bool CurrentSizeInOneObject(Vector3Int origin, Vector2Int size)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        int buildCount = 0;
        int elementCount = 0;

        Construction con = GetCurrentConstruction(origin);
        if (con == null) return false;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;
                if (BuildTileDict.ContainsKey(pos) && BuildTileDict[pos].IsOccupied)
                {
                    if (BuildTileDict[pos].Construction.gameObject == con.gameObject) buildCount++;
                }

                if (ElementTileDict.ContainsKey(pos) && ElementTileDict[pos].IsOccupied)
                {
                    if (ElementTileDict[pos].Construction.gameObject == con.gameObject) elementCount++;
                }
            }
        }

        int maxCount = size.x * size.y;
        //Debug.Log($"maxCount {maxCount} buildCount {buildCount} elementCount {elementCount}");
        return maxCount == buildCount || maxCount == elementCount;
    }

    public void ShowOrHideTileDict(bool isShow)
    {
        foreach (var tile in BuildTileDict.Values)
        {
            if (tile.Construction != null)
            {
                tile.Construction.gameObject.SetActive(isShow);
            }
        }
        foreach (var tile in ElementTileDict.Values)
        {
            if (tile.Construction != null)
            {
                tile.Construction.gameObject.SetActive(isShow);
            }
        }
    }

    public List<Vector3Int> GetBuildCells(Construction con)
    {
        if (con == null) return new List<Vector3Int>();

        var cells = new List<Vector3Int>();
        foreach (var kv in BuildTileDict)
        {
            if (kv.Value?.IsOccupied == true && kv.Value.Construction == con)
                cells.Add(kv.Key);
        }
        return cells;
    }
}