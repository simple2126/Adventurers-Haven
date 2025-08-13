using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPathfinder : SingletonBase<RoadPathfinder>
{
    private AStar aStar;
    private IReadOnlyDictionary<Vector3Int, CustomTileData> buildDict => Global.Instance.BuildDict;
    private IReadOnlyDictionary<Vector3Int, CustomTileData> elementDict => Global.Instance.ElemDict;
    private Tilemap buildingTilemap => Global.Instance.BuildingTilemap;
    private Tilemap elementTilemap => Global.Instance.ElementTilemap;

    private Transform[] spawnPositions => Global.Instance.SpawnPositions;
    private Vector2Int roadSize => Global.RoadSize;
    private Vector3Int[] dir4 => Global.Dir4;
    private Vector3Int start = Vector3Int.zero;

    protected override void Awake()
    {
        base.Awake();
        aStar = new AStar(Heur, Neighbors, CalcStepCost, IsValidRoad);
    }

    public bool TryFindPathToRandomBuild(Vector3Int start, out Vector3 buildCenter, out List<Vector3> worldPath)
    {
        buildCenter = default;
        worldPath = null;

        if (!IsValidRoad(start))
        {
            Debug.Log($"Start is Not Road");
            return false;
        }

        var validConstructions = GetValidConnectedConstructions(start, out var pathDict);
        if (validConstructions.Count == 0)
            return false;

        var targetBuild = validConstructions[Random.Range(0, validConstructions.Count)];
        var bestPath = pathDict[targetBuild];

        worldPath = PathToWorld(bestPath);

        if (TryGetNearestEntrance(worldPath[worldPath.Count - 1], targetBuild, out var entrancePos))
        {
            worldPath.Add(entrancePos);
            Debug.Log($"Entrance Pos: {entrancePos}");
        }

        buildCenter = targetBuild.transform.position;
        worldPath.Add(buildCenter);
        return true;
    }

    private List<Construction> GetValidConnectedConstructions(
        Vector3Int start, out Dictionary<Construction, List<Vector3Int>> paths)
    {
        paths = new Dictionary<Construction, List<Vector3Int>>();
        var result = new List<Construction>();

        foreach (var pair in buildDict)
        {
            var data = pair.Value;
            if (data == null || !data.IsOccupied || data.Construction == null)
                continue;

            var con = data.Construction;
            if (result.Contains(con)) continue;

            if (!AreAllSpawnsConnectedToBuild(con, out var edgeRoads, false, start))
                continue;

            if (GetBestGoalFromEdges(start, edgeRoads, out var path))
            {
                paths[con] = path;
                result.Add(con);
            }
        }

        return result;
    }

    private bool TryGetNearestEntrance(Vector3 lastPath, Construction targetBuild, out Vector3 entrancePos)
    {
        entrancePos = default;

        if (targetBuild == null)
            return false;

        Vector3 buildCenter = targetBuild.transform.position;
        Vector2Int buildSize = targetBuild.Size;       
        Vector3 tileSize = elementTilemap.cellSize;

        Vector3 delta = lastPath - buildCenter;
        Vector3Int dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0 ? Vector3Int.right : Vector3Int.left)
                : (delta.y > 0 ? Vector3Int.up : Vector3Int.down);

        if (!IsFullRoadLineInDirection(targetBuild, dir))
            return false;

        float offsetX = (buildSize.x / 2f + roadSize.x / 2f) * tileSize.x;
        float offsetY = (buildSize.y / 2f + roadSize.y / 2f) * tileSize.y;

        entrancePos = buildCenter + new Vector3(dir.x * offsetX, dir.y * offsetY, 0f);
        return true;
    }

    private bool IsFullRoadLineInDirection(Construction targetBuild, Vector3Int direction)
    {
        var tilemap = elementTilemap;
        Vector2 buildCellSize = buildingTilemap.cellSize;
        Vector3 buildOffset = (Vector3)(new Vector2(targetBuild.Size.x * buildCellSize.x * 0.5f, targetBuild.Size.y * buildCellSize.y * 0.5f));
        Vector3Int buildBottomLeftCell = tilemap.WorldToCell(targetBuild.transform.position - buildOffset);
        Vector2Int buildSize = targetBuild.Size;

        int checkLength = (direction.x != 0) ? buildSize.y : buildSize.x;

        Debug.Log($"[IsFullRoadLineInDirection] Checking build: {targetBuild.name}, Size: {buildSize}, Dir: {direction}, CheckLength: {checkLength}, BL Cell (Calculated): {buildBottomLeftCell}");

        for (int i = 0; i < checkLength; i++)
        {
            Vector3Int checkCellOffset = Vector3Int.zero;

            if (direction == Vector3Int.up) 
            {
                checkCellOffset.x = i;
                checkCellOffset.y = buildSize.y;
            }
            else if (direction == Vector3Int.down)
            {
                checkCellOffset.x = i;
                checkCellOffset.y = -1;
            }
            else if (direction == Vector3Int.left)
            {
                checkCellOffset.x = -1;
                checkCellOffset.y = i;
            }
            else if (direction == Vector3Int.right)
            {
                checkCellOffset.x = buildSize.x;
                checkCellOffset.y = i;
            }

            Vector3Int checkCell = buildBottomLeftCell + checkCellOffset;
            if (!IsValidRoad(checkCell))
            {
                return false; // 하나라도 도로가 아니면 즉시 false 반환
            }
        }
        return true;
    }

    public bool AreAllSpawnsConnectedToBuild(
        Construction build, out List<Vector3Int> connectedRoads, 
        bool checkAllSpawns = true, Vector3Int? singleStartCell = null)
    {
        connectedRoads = new List<Vector3Int>();

        if (build == null)
            return false;

        // 1. 검사할 시작점 목록 만들기
        List<Vector3Int> startCells;
        if (checkAllSpawns)
        {
            var spawnTransforms = spawnPositions;
            if (spawnTransforms == null || spawnTransforms.Length == 0)
                return false;

            startCells = spawnTransforms
                .Select(t => elementTilemap.WorldToCell(t.position))
                .ToList();
        }
        else
        {
            if (!singleStartCell.HasValue)
                return false; // 단일 모드인데 startCell 안 줬으면 실패

            startCells = new List<Vector3Int> { singleStartCell.Value };
        }

        // 2. 경계 도로 찾기
        List<Vector3Int> edgeRoads = GetDominantEdgeRoadCells(build);
        if (edgeRoads.Count == 0)
            return false;

        // 3. 연결된 Construction 수집
        List<Construction> conList = new List<Construction>();
        foreach (var cell in edgeRoads)
        {
            if (elementDict.TryGetValue(cell, out var value) && value.Construction != null)
            {
                if (!conList.Contains(value.Construction))
                {
                    conList.Add(value.Construction);
                    var pos = value.Construction.transform.position;
                    connectedRoads.Add(elementTilemap.WorldToCell(pos));
                }
            }
        }

        // 4. 각 시작점에서 연결 여부 검사
        foreach (var startCell in startCells)
        {
            bool connected = false;

            foreach (var con in conList)
            {
                Vector3Int cell = elementTilemap.WorldToCell(con.transform.position);
                if (aStar.FindPath(startCell, cell, out _))
                {
                    connected = true;
                    break;
                }
            }

            if (!connected)
            {
                Debug.LogWarning($"Start {startCell} is NOT connected to {build.name}");
                return false;
            }
        }

        return true;
    }


    public bool IsConnectRoad(Construction con)
    {
        if (con == null)
            return false;

        return AreAllSpawnsConnectedToBuild(con, out _, true);
    }

    private bool GetBestGoalFromEdges(Vector3Int start, List<Vector3Int> edgeRoads, out List<Vector3Int> bestPath)
    {
        bestPath = null;
        int bestLength = int.MaxValue;

        foreach (var goal in edgeRoads)
        {
            if (!elementDict.ContainsKey(goal)) continue;

            if (aStar.FindPath(start, goal, out var path) && path.Count < bestLength)
            {
                bestPath = path;
                bestLength = path.Count;
            }
        }

        return bestPath != null;
    }

    private List<Vector3Int> GetDominantEdgeRoadCells(Construction con)
    {
        var result = new List<Vector3Int>();
        var conCell = elementTilemap.WorldToCell(con.transform.position);
        var cells = MapManager.Instance.GetCells(conCell, con.Size);

        // 경계 좌표 계산
        int minX = int.MaxValue, maxX = int.MinValue,
            minY = int.MaxValue, maxY = int.MinValue;

        foreach (var c in cells)
        {
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }

        // 경계 셀들에서 한 칸 바깥이 도로인지 확인
        foreach (var c in cells)
        {
            if (c.x == minX && IsValidRoad(c + Vector3Int.left)) result.Add(c + Vector3Int.left);
            if (c.x == maxX && IsValidRoad(c + Vector3Int.right)) result.Add(c + Vector3Int.right);
            if (c.y == minY && IsValidRoad(c + Vector3Int.down)) result.Add(c + Vector3Int.down);
            if (c.y == maxY && IsValidRoad(c + Vector3Int.up)) result.Add(c + Vector3Int.up);
        }

        foreach(var cell in result)
        {
            Debug.Log($"Out Road Cell {cell}");
        }

        return result;
    }

    private List<Vector3> PathToWorld(List<Vector3Int> path)
    {
        var result = new List<Vector3>();
        for (int i = 0; i < path.Count; i++)
            result.Add(GetRoadCenterWorld(path[i]));
        return result;
    }

    private Vector3 GetRoadCenterWorld(Vector3Int cell)
    {
        Vector3 center = elementTilemap.GetCellCenterWorld(cell);

        if (!elementDict.TryGetValue(cell, out var data) || data.Construction == null)
            return center;

        Vector2Int size = data.Construction.Size;
        Vector3 offset = new Vector3(
            -(size.x - 1) * elementTilemap.cellSize.x / 2f,
            -(size.y - 1) * elementTilemap.cellSize.y / 2f,
            0f
        );

        return center + offset;
    }

    #region AStar in Method

    private int Heur(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3Int> Neighbors(Vector3Int c)
    {
        var n = new List<Vector3Int>();
        if (!elementDict.TryGetValue(c, out var data) || data.Construction == null) return n;

        var size = data.Construction.Size;
        foreach (var d in dir4)
        {
            var step = new Vector3Int(d.x * size.x, d.y * size.y, 0);
            var nc = c + step;

            if (n.Contains(nc)) continue; // 중복 방지
            if (elementDict.TryGetValue(nc, out var neighborData) &&
                neighborData.Construction != null && neighborData.Construction.IsRoad())
            {
                n.Add(nc);
            }
        }
        return n;
    }

    private int CalcStepCost(Vector3Int from, Vector3Int to)
    {
        if (!elementDict.TryGetValue(from, out var data) || data.Construction == null)
            return 1;
        var size = data.Construction.Size;

        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        if (dx > 0) return size.x;
        if (dy > 0) return size.y;
        return 1;
    }

    private bool IsValidRoad(Vector3Int pos)
    {
        return elementDict.TryGetValue(pos, out var data) && data.Construction?.IsRoad() == true;
    }

    #endregion
}