using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPathfinder : SingletonBase<RoadPathfinder>
{
    private class Node
    {
        public Vector3Int Pos;
        public int G, H;
        public int F => G + H;
        public Node Parent;
        public Node(Vector3Int pos, int g, int h, Node parent) { Pos = pos; G = g; H = h; Parent = parent; }
    }

    private class MinHeap
    {
        private readonly List<Node> data = new();
        public int Count => data.Count;
        public void Push(Node n) { data.Add(n); Up(data.Count - 1); }
        public Node Pop()
        {
            var root = data[0];
            data[0] = data[^1];
            data.RemoveAt(data.Count - 1);
            Down(0);
            return root;
        }
        public void DecreaseKey(Node n) { Up(data.IndexOf(n)); }
        void Up(int i) { while (i > 0 && Compare(i, (i - 1) / 2)) { Swap(i, (i - 1) / 2); i = (i - 1) / 2; } }
        void Down(int i)
        {
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, s = i;
                if (l < data.Count && Compare(l, s)) s = l;
                if (r < data.Count && Compare(r, s)) s = r;
                if (s == i) break;
                Swap(i, s); i = s;
            }
        }
        bool Compare(int a, int b) => data[a].F < data[b].F || (data[a].F == data[b].F && data[a].H < data[b].H);
        void Swap(int a, int b) { var t = data[a]; data[a] = data[b]; data[b] = t; }
    }

    private Dictionary<Vector3Int, CustomTileData> elemDict => MapManager.Instance.ElementTileDict;
    private Dictionary<Vector3Int, CustomTileData> buildDict => MapManager.Instance.BuildTileDict;
    private Tilemap elementTilemap => MapManager.Instance.ElementTilemap;
    private Tilemap buildingTilemap => MapManager.Instance.BuildingTilemap;

    readonly Vector2Int roadSize = new(2, 2);
    static readonly Vector3Int[] Dir4 = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

    public bool TryFindPathToRandomBuild(Vector3Int start, out Vector3 buildCenter, out List<Vector3> worldPath)
    {
        buildCenter = default; worldPath = null;
        if (!elemDict.TryGetValue(start, out var s) || s.Construction == null || !s.Construction.IsRoad())
        {
            Debug.LogWarning($"Start position {start} is not a valid road tile.");
            return false;
        }

        var buildCells = GetRandomBuildCells(out var buildCon);
        if (buildCells == null || buildCon == null)
        {
            Debug.Log("No valid buildings found for pathfinding.");
            return false;
        }

        var neighborRoads = GetOuterRoadCells(buildCon, buildCells);

        List<Vector3Int> bestPath = null;
        int bestLen = int.MaxValue;
        foreach (var goal in neighborRoads)
        {
            if (!elemDict.ContainsKey(goal)) continue;

            if (AStar(start, goal, out var path) && path.Count < bestLen)
            {
                bestPath = path;
                bestLen = path.Count;
            }
        }

        Debug.Log($"Found {neighborRoads.Count} outer roads, best path length: {bestLen}");
        if (bestPath != null)
        {
            worldPath = PathToWorld(bestPath);
            buildCenter = buildCon.transform.position;

            var lastRoadCenter = worldPath[^1];
            var dir = (buildCenter - lastRoadCenter).normalized;

            // 타일 크기 정보
            var elementTilemapSize = elementTilemap.cellSize;
            var buildTilemapSize = buildingTilemap.cellSize;

            // 건물 크기 (타일 수 단위)
            var conSize = buildCon.Size;

            // 방향 보정 (직교 방향으로)
            Vector3 fixedDir;
            float roadHalf, buildHalf;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) // 가로 방향
            {
                fixedDir = Vector3.right * Mathf.Sign(dir.x);
                roadHalf = (roadSize.x * elementTilemapSize.x) / 2f;
                buildHalf = (conSize.x * buildTilemapSize.x) / 2f;
            }
            else // 세로 방향
            {
                fixedDir = Vector3.up * Mathf.Sign(dir.y);
                roadHalf = (roadSize.y * elementTilemapSize.y) / 2f;
                buildHalf = (conSize.y * buildTilemapSize.y) / 2f;
            }

            float totalOffset = roadHalf + buildHalf;

            // 건물 기준으로 한 칸 앞 위치 계산
            var stopBeforeBuild = buildCenter - fixedDir * totalOffset;

            if (Vector3.Dot(dir, fixedDir) > 0f)
                worldPath.RemoveAt(worldPath.Count - 1);

            // 경로에 추가
            worldPath.Add(stopBeforeBuild);
            worldPath.Add(buildCenter);

            return true;
        }
        return false;
    }

    Vector3 GetRoadCenterWorld(Vector3Int anchorCell)
    {
        Vector3 anchorWorld = elementTilemap.GetCellCenterWorld(anchorCell);

        if (!elemDict.TryGetValue(anchorCell, out var data) || data.Construction == null)
            return anchorWorld;
        
        var roadSize = data.Construction.Size;
        Vector3 offset = new Vector3(
            -(roadSize.x - 1) * elementTilemap.cellSize.x / 2f,
            -(roadSize.y - 1) * elementTilemap.cellSize.y / 2f,
            0f
        );
        return anchorWorld + offset;
    }

    List<Vector3> PathToWorld(List<Vector3Int> path)
    {
        var result = new List<Vector3>();
        foreach (var cell in path)
            result.Add(GetRoadCenterWorld(cell));
        return result;
    }

    bool AStar(Vector3Int start, Vector3Int goal, out List<Vector3Int> result)
    {
        result = null;
        if (!elemDict.TryGetValue(start, out var s) || s.Construction == null || !s.Construction.IsRoad()) return false;
        if (!elemDict.TryGetValue(goal, out var g) || g.Construction == null || !g.Construction.IsRoad()) return false;

        var open = new MinHeap();
        var openDict = new Dictionary<Vector3Int, Node>();
        var closed = new HashSet<Vector3Int>();
        var startNode = new Node(start, 0, Heur(start, goal), null);
        
        open.Push(startNode); 
        openDict[start] = startNode;

        while (open.Count > 0)
        {
            var curr = open.Pop(); 
            openDict.Remove(curr.Pos);
            
            if (curr.Pos == goal) 
            { 
                result = Reconstruct(curr); 
                return true; 
            }

            closed.Add(curr.Pos);
            foreach (var n in Neighbors(curr.Pos))
            {
                if (closed.Contains(n)) continue;
                
                int gScore = curr.G + CalcStepCost(curr.Pos, n);
                if (openDict.TryGetValue(n, out var ex))
                {
                    if (gScore < ex.G) { ex.G = gScore; ex.Parent = curr; open.DecreaseKey(ex); }
                }
                else
                {
                    var node = new Node(n, gScore, Heur(n, goal), curr);
                    open.Push(node); 
                    openDict[n] = node;
                }
            }
        }
        return false;
    }

    int Heur(Vector3Int a, Vector3Int b)
    {
        // a, b 각각의 도로 크기를 가져옴
        elemDict.TryGetValue(a, out var aData);
        elemDict.TryGetValue(b, out var bData);
        var sizeA = (aData?.Construction != null) ? aData.Construction.Size : Vector2Int.one;
        var sizeB = (bData?.Construction != null) ? bData.Construction.Size : Vector2Int.one;

        // 두 좌표간 중심점끼리의 맨해튼 거리로 변경
        int dx = Mathf.Abs(a.x - b.x) / sizeA.x;
        int dy = Mathf.Abs(a.y - b.y) / sizeA.y;
        return dx + dy;
    }

    List<Vector3Int> Neighbors(Vector3Int c)
    {
        var n = new List<Vector3Int>();
        if (!elemDict.TryGetValue(c, out var data) || data.Construction == null) return n;
        
        var size = data.Construction.Size;
        foreach (var d in Dir4)
        {
            var step = new Vector3Int(d.x * size.x, d.y * size.y, 0);
            var nc = c + step;
            //Debug.Log($"Checking neighbor of {c}: {nc}");

            if (n.Contains(nc)) continue; // 중복 방지
            if (elemDict.TryGetValue(nc, out var neighborData) &&
                neighborData.Construction != null && neighborData.Construction.IsRoad())
            {
                //Debug.Log($"  => Valid neighbor road at {nc}");
                n.Add(nc);
            }
        }
        return n;
    }

    int CalcStepCost(Vector3Int from, Vector3Int to)
    {
        if (!elemDict.TryGetValue(from, out var data) || data.Construction == null)
            return 1;
        var size = data.Construction.Size;
        // 방향에 따라 x 또는 y 사용
        if (from.x != to.x) return size.x;
        if (from.y != to.y) return size.y;
        return 1;
    }

    List<Vector3Int> Reconstruct(Node n)
    {
        var rev = new List<Vector3Int>();
        while (n != null) { rev.Add(n.Pos); n = n.Parent; }
        rev.Reverse(); return rev;
    }

    List<Vector3Int> GetRandomBuildCells(out Construction con)
    {
        var map = new Dictionary<Construction, List<Vector3Int>>();
        
        foreach (var kv in buildDict)
            if (kv.Value?.IsOccupied == true && kv.Value.Construction != null)
                (map.TryGetValue(kv.Value.Construction, out var l) ? l : map[kv.Value.Construction] = new List<Vector3Int>()).Add(kv.Key);

        if (map.Count == 0) { con = null; return null; }
        
        var cons = new List<Construction>(map.Keys);
        var rand = Random.Range(0, cons.Count);
        Debug.Log($"Random Con {rand}");
        con = cons[rand];
        return map[con];
    }

    Vector3Int GetAnchorCell(Vector3Int cell)
    {
        if (!elemDict.TryGetValue(cell, out var data) || data.Construction == null)
            return cell;

        var worldPos = data.Construction.transform.position;
        var anchorCell = elementTilemap.WorldToCell(worldPos);

        return anchorCell;
    }

    public List<Vector3Int> GetOuterRoadCells(Construction builcon, Vector3Int buildCell)
    {
        return GetOuterRoadCells(builcon, new List<Vector3Int> { buildCell });
    }

    public List<Vector3Int> GetOuterRoadCells(Construction buildCon, List<Vector3Int> buildCells)
    {
        var set = new HashSet<Vector3Int>(buildCells);

        // 건물 중심 월드 위치
        Vector3 buildCenter = buildCon.transform.position;

        // 건물 크기 (타일 단위)
        Vector2Int buildSize = buildCon.Size;

        // 도로 크기 (타일 단위)
        Vector2Int roadSize = new Vector2Int(2, 2); // 필요하면 인자로 받거나 멤버변수 사용

        // 타일 크기 (월드 단위)
        Vector3 cellSizeElement = elementTilemap.cellSize;
        Vector3 cellSizeBuild = buildingTilemap.cellSize;

        // 건물 기준 타일 위치 (기준점은 건물 위치 그대로 사용)
        Vector3Int buildCenterCell = elementTilemap.WorldToCell(buildCenter);

        // 기준 위치를 도로 방향으로 보정할 변수 (도로가 붙는 방향을 임시로 상하좌우 중 찾기)
        // 여기선 예시로 "건물 중심과 도로 셀 중 가장 가까운 도로 방향"을 찾기 위해 주변 도로 확인
        // 실제 상황에 따라 다르게 구할 수도 있음

        // 주변 4방향 체크용
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        // 인접한 도로 셀 방향 찾기 (가장 가까운 방향 하나)
        Vector3Int? roadDir = null;

        foreach (var dir in dirs)
        {
            var checkCell = buildCenterCell + new Vector3Int(dir.x * (buildSize.x / 2 + roadSize.x / 2), dir.y * (buildSize.y / 2 + roadSize.y / 2), 0);
            if (elemDict.TryGetValue(checkCell, out var data) && data.Construction != null && data.Construction.IsRoad())
            {
                roadDir = dir;
                break;
            }
        }

        if (roadDir == null)
        {
            // 도로 방향을 못 찾으면 기존처럼 주변 전부 체크
            roadDir = null;
        }

        // 기준점 보정 (도로 방향이 있을 때만)
        Vector3Int baseCell = buildCenterCell;
        if (roadDir.HasValue)
        {
            Vector3Int dir = roadDir.Value;

            // 보정 거리 (도로 절반 + 건물 절반)
            float offsetX = ((roadSize.x * cellSizeElement.x) / 2f) + ((buildSize.x * cellSizeBuild.x) / 2f);
            float offsetY = ((roadSize.y * cellSizeElement.y) / 2f) + ((buildSize.y * cellSizeBuild.y) / 2f);

            Vector3 offsetWorld = new Vector3(dir.x * offsetX, dir.y * offsetY, 0f);
            Vector3 correctedPos = buildCenter - offsetWorld;

            baseCell = elementTilemap.WorldToCell(correctedPos);
        }

        var dirToNeighbors = new Dictionary<Vector3Int, List<Vector3Int>>()
    {
        { Vector3Int.up, new List<Vector3Int>() },
        { Vector3Int.down, new List<Vector3Int>() },
        { Vector3Int.left, new List<Vector3Int>() },
        { Vector3Int.right, new List<Vector3Int>() }
    };

        // buildCells 주변 도로 셀 수집 (기존대로)
        foreach (var cell in buildCells)
        {
            foreach (var dir in Dir4)
            {
                var neighbor = cell + dir;
                if (!set.Contains(neighbor) &&
                    elemDict.TryGetValue(neighbor, out var data) &&
                    data.Construction != null && data.Construction.IsRoad())
                {
                    dirToNeighbors[dir].Add(GetAnchorCell(neighbor));
                }
            }
        }

        var outerRoads = new List<Vector3Int>();

        foreach (var pair in dirToNeighbors)
        {
            var list = pair.Value.Distinct().ToList();
            if (list.Count == 0) continue;

            // 기준 위치 보정 추가: 중심 대신 보정된 baseCell 사용
            list.Sort((a, b) =>
                Vector3Int.Distance(a, baseCell).CompareTo(Vector3Int.Distance(b, baseCell)));

            outerRoads.Add(list[0]);
        }

        return outerRoads;
    }
}
