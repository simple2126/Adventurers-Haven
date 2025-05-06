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
        IsOccupied = true;
        Construction = construction;
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

    private RuntimeNavMeshUpdater runtimeNavMeshUpdater;

    protected override void Awake()
    {
        base.Awake();
        BuildingTilemap = buildingTilemap;
        ElementTilemap = elementTilemap;
        runtimeNavMeshUpdater = GetComponentInChildren<RuntimeNavMeshUpdater>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PoolManager.Instance.AddPools<Construction>(poolConfig);
        baseRoadCon = poolConfig.Prefab.GetComponent<Construction>();
        baseRoadCon.Init(DataManager.Instance.GetConstructionData(conType, baseRoadTag));
        SetTileDict(BuildingTilemap, buildTileDict);
        SetTileDict(ElementTilemap, elementTileDict);
        runtimeNavMeshUpdater.InitNavMesh();
    }

    private void SetTileDict(Tilemap tilemap, Dictionary<Vector3Int, CustomTileData> tileDict)
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3 cellSize = tilemap.cellSize;
        var conData = DataManager.Instance.GetConstructionData(conType, baseRoadTag);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new(x, y, 0);
                if (!tileDict.ContainsKey(pos)) tileDict[pos] = new CustomTileData();

                if (!tilemap.HasTile(pos)) continue;

                if (tilemap == ElementTilemap)
                {
                    int offsetX = x - bounds.xMin;
                    int offsetY = y - bounds.yMin;

                    if ((offsetX % baseRoadCon.Size.x == 0) && (offsetY % baseRoadCon.Size.y == 0))
                    {
                        Vector3 worldPos = tilemap.GetCellCenterWorld(pos) + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
                        var con = PoolManager.Instance.SpawnFromPool<Construction>(baseRoadTag, worldPos, Quaternion.identity);
                        con.Init(conData);
                        SetBuildingAreaLeftBottom(pos, baseRoadCon.Size, con);
                    }
                }
                else
                {
                    tileDict[pos].SetTileData(null);
                }
            }
        }
    }

    public void SetBuildingAreaLeftBottom(Vector3Int origin, Vector2Int size, Construction construction)
    {
        var tileDict = construction.Type == ConstructionType.Build ? buildTileDict : elementTileDict;
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

    public bool CanPlaceBuilding(Vector3Int origin, Vector2Int size, Construction construction)
    {
        if (construction.IsDemolish())
        {
            Vector3Int pos = Vector3Int.right + Vector3Int.up + origin;
            return buildTileDict.ContainsKey(pos) || elementTileDict.ContainsKey(pos);
        }

        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetY; y < size.y - offsetY; y++)
            {
                Vector3Int pos = Vector3Int.right * x + Vector3Int.up * y + origin;

                if (construction.Type == ConstructionType.Build)
                {
                    if (!buildTileDict.ContainsKey(pos) || buildTileDict[pos].IsOccupied) return false;
                    if (!elementTileDict.ContainsKey(pos) || elementTileDict[pos].IsOccupied) return false;
                }
                else if (construction.Type == ConstructionType.Element)
                {
                    if (!buildTileDict.ContainsKey(pos) || buildTileDict[pos].IsOccupied) return false;
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
                tileDict[pos].SetTileData(construction);
            }
        }
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

            if (targetCon.Type == ConstructionType.Build && buildTileDict.TryGetValue(current, out var buildTileData))
            {
                if(buildTileData == null) continue;
                Construction buildCon = buildTileData.Construction;
                if (buildTileData != null && buildCon.gameObject == targetCon.gameObject)
                {
                    buildingTilemap.SetTile(current, null);
                    buildingTilemap.RefreshTile(current);
                    buildTileDict[current].ClearTileData();

                    if (!removedConstructions.Contains(buildCon))
                    {
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

            if (targetCon.Type == ConstructionType.Element && elementTileDict.TryGetValue(current, out var elementTIleData))
            {
                if (elementTIleData == null) continue;
                Construction elementCon = elementTIleData.Construction;
                if (elementTIleData != null && elementCon.gameObject == targetCon.gameObject)
                {
                    elementTilemap.SetTile(current, null);
                    elementTilemap.RefreshTile(current);
                    elementTileDict[current].ClearTileData();

                    if (!removedConstructions.Contains(elementCon))
                    {
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

        PoolManager.Instance.ReturnToPool<Construction>(construction.Tag, construction);
    }

    private Construction GetCurrentConstruction(Vector3Int pos)
    {
        if (buildTileDict.TryGetValue(pos, out var buildTileData) && buildTileData != null)
        {
            return buildTileData.Construction;
        }
        if (elementTileDict.TryGetValue(pos, out var elementTileData) && elementTileData != null)
        {
            return elementTileData.Construction;
        }
        return null;
    }

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

        return size.x * size.y == count;
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
                if (buildTileDict.ContainsKey(pos) && buildTileDict[pos] != null)
                {
                    if (buildTileDict[pos].Construction.gameObject == con.gameObject) buildCount++;
                }

                if (elementTileDict.ContainsKey(pos) && elementTileDict[pos] != null)
                {
                    if (elementTileDict[pos].Construction.gameObject == con.gameObject) elementCount++;
                }
            }
        }

        int maxCount = size.x * size.y;
        Debug.Log($"maxCount {maxCount} buildCount {buildCount} elementCount {elementCount}");
        return maxCount == buildCount || maxCount == elementCount;
    }

    public void ShowOrHideTileDict(bool isShow)
    {
        foreach (var tile in buildTileDict.Values)
        {
            if (tile != null)
            {
                tile.Construction.gameObject.SetActive(isShow);
            }
        }
        foreach (var tile in elementTileDict.Values)
        {
            if (tile != null)
            {
                tile.Construction.gameObject.SetActive(isShow);
            }
        }
    }
}