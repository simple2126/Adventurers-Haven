using System.Collections.Generic;
using UnityEngine;

public class RoadPathfinder
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

    readonly Vector2Int roadSize = new(2, 2);
    static readonly Vector3Int[] Dir4 = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

    public bool TryFindPathToRandomBuild(Vector3Int start, out Vector3 buildCenter, out List<Vector3> worldPath)
    {
        buildCenter = default; worldPath = null;
        if (!elemDict.TryGetValue(start, out var s) || s.Construction == null || !s.Construction.IsRoad()) return false;

        var buildCells = GetRandomBuildCells(out var buildCon);
        if (buildCells == null || buildCon == null) return false;

        var neighborRoads = GetOuterRoadCells(buildCells);

        List<Vector3Int> bestPath = null;
        int bestLen = int.MaxValue;
        foreach (var goal in neighborRoads)
        {
            if (AStar(start, goal, out var path) && path.Count < bestLen)
            {
                bestPath = path;
                bestLen = path.Count;
            }
        }
        if (bestPath != null)
        {
            worldPath = PathToWorld(bestPath);
            buildCenter = buildCon.transform.position;
            return true;
        }
        return false;
    }

    Vector3 GetRoadCenterWorld(Vector3Int anchorCell)
    {
        var tilemap = MapManager.Instance.ElementTilemap;
        Vector3 anchorWorld = tilemap.GetCellCenterWorld(anchorCell);
        if (!elemDict.TryGetValue(anchorCell, out var data) || data.Construction == null)
            return anchorWorld;
        var roadSize = data.Construction.Size;
        Vector3 offset = new Vector3(
            -(roadSize.x - 1) * tilemap.cellSize.x / 2f,
            -(roadSize.y - 1) * tilemap.cellSize.y / 2f,
            0f
        );
        return anchorWorld + offset;
    }

    List<Vector3> PathToWorld(List<Vector3Int> path)
    {
        var tilemap = MapManager.Instance.ElementTilemap;
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
        open.Push(startNode); openDict[start] = startNode;

        while (open.Count > 0)
        {
            var curr = open.Pop(); openDict.Remove(curr.Pos);
            if (curr.Pos == goal) { result = Reconstruct(curr); return true; }
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
                    open.Push(node); openDict[n] = node;
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
            if (elemDict.TryGetValue(nc, out var neighborData) &&
                neighborData.Construction != null && neighborData.Construction.IsRoad())
            {
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
        con = cons[Random.Range(0, cons.Count)];
        return map[con];
    }

    List<Vector3Int> GetOuterRoadCells(List<Vector3Int> buildCells)
    {
        var set = new HashSet<Vector3Int>(buildCells);
        var outerRoads = new List<Vector3Int>();
        foreach (var cell in buildCells)
        {
            foreach (var dir in Dir4)
            {
                var neighbor = cell + dir;
                if (!set.Contains(neighbor) &&
                    elemDict.TryGetValue(neighbor, out var data) &&
                    data.Construction != null && data.Construction.IsRoad())
                {
                    outerRoads.Add(neighbor);
                }
            }
        }
        return outerRoads;
    }
}
