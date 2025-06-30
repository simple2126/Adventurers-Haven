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

    public bool TryFindPathToBuild(Vector3Int start, out Vector3 buildCenter, out List<Vector3> worldPath)
    {
        buildCenter = default;
        worldPath = null;

        if (!elemDict.TryGetValue(start, out var s) || s.Construction == null || !s.Construction.IsRoad())
            return false;

        var candidates = new List<Construction>();
        foreach (var data in buildDict.Values)
        {
            if (data?.IsOccupied != true || data.Construction == null) continue;
            candidates.Add(data.Construction);
        }

        if (candidates.Count == 0) return false;

        var targetBuild = candidates[Random.Range(0, candidates.Count)];
        buildCenter = targetBuild.transform.position;

        if (!TryGetClosestConnectedRoadPath(start, targetBuild, out worldPath))
            return false;

        return true;
    }

    private bool TryGetClosestConnectedRoadPath(Vector3Int start, Construction build, out List<Vector3> worldPath)
    {
        worldPath = null;
        var roadCells = GetConnectedRoads(build);
        if (roadCells.Count == 0) return false;

        List<Vector3Int> bestPath = null;
        int bestLen = int.MaxValue;

        foreach (var goal in roadCells)
        {
            if (!elemDict.ContainsKey(goal)) continue;

            if (AStar(start, goal, out var path) && path.Count < bestLen)
            {
                bestPath = path;
                bestLen = path.Count;
            }
        }

        if (bestPath == null) return false;

        worldPath = PathToWorld(bestPath);

        var lastRoadCenter = worldPath[^1];
        var dir = (build.transform.position - lastRoadCenter).normalized;

        // 방향 보정
        Vector3 fixedDir;
        float roadHalf, buildHalf;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            fixedDir = Vector3.right * Mathf.Sign(dir.x);
            roadHalf = (roadSize.x * elementTilemap.cellSize.x) / 2f;
            buildHalf = (build.Size.x * buildingTilemap.cellSize.x) / 2f;
        }
        else
        {
            fixedDir = Vector3.up * Mathf.Sign(dir.y);
            roadHalf = (roadSize.y * elementTilemap.cellSize.y) / 2f;
            buildHalf = (build.Size.y * buildingTilemap.cellSize.y) / 2f;
        }

        float totalOffset = roadHalf + buildHalf;
        var stopBeforeBuild = build.transform.position - fixedDir * totalOffset;

        var lastWorld = worldPath[^1];
        var lastCell = elementTilemap.WorldToCell(lastWorld);

        if (Vector3.Dot(dir, fixedDir) > 0f)
        {
            worldPath.RemoveAt(worldPath.Count - 1);
        }

        worldPath.Add(stopBeforeBuild);
        worldPath.Add(build.transform.position);
        return true;
    }

    List<Vector3> PathToWorld(List<Vector3Int> path)
    {
        var result = new List<Vector3>();
        foreach (var cell in path)
            result.Add(GetRoadCenterWorld(cell));
        return result;
    }

    private Vector3 GetRoadCenterWorld(Vector3Int cell)
    {
        Vector3 anchorWorld = elementTilemap.GetCellCenterWorld(cell);

        if (!elemDict.TryGetValue(cell, out var data) || data.Construction == null)
            return anchorWorld;

        var roadSize = data.Construction.Size;
        Vector3 offset = new Vector3(
            -(roadSize.x - 1) * elementTilemap.cellSize.x / 2f,
            -(roadSize.y - 1) * elementTilemap.cellSize.y / 2f,
            0f
        );
        return anchorWorld + offset;
    }

    private bool AStar(Vector3Int start, Vector3Int goal, out List<Vector3Int> result)
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

    private int Heur(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3Int> Neighbors(Vector3Int c)
    {
        var n = new List<Vector3Int>();
        if (!elemDict.TryGetValue(c, out var data) || data.Construction == null) return n;

        var size = data.Construction.Size;
        foreach (var d in Dir4)
        {
            var step = new Vector3Int(d.x * size.x, d.y * size.y, 0);
            var nc = c + step;

            if (n.Contains(nc)) continue; // 중복 방지
            if (elemDict.TryGetValue(nc, out var neighborData) &&
                neighborData.Construction != null && neighborData.Construction.IsRoad())
            {
                n.Add(nc);
            }
        }
        return n;
    }

    private int CalcStepCost(Vector3Int from, Vector3Int to)
    {
        if (!elemDict.TryGetValue(from, out var data) || data.Construction == null)
            return 1;
        var size = data.Construction.Size;

        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        if (dx > 0) return size.x;
        if (dy > 0) return size.y;
        return 1;
    }

    private List<Vector3Int> Reconstruct(Node n)
    {
        var rev = new List<Vector3Int>();
        while (n != null) { rev.Add(n.Pos); n = n.Parent; }
        rev.Reverse(); return rev;
    }

    private bool SamePosition(Vector3 a, Vector3 b)
    {
        return elementTilemap.WorldToCell(a) == elementTilemap.WorldToCell(b);
    }

    public List<Vector3Int> GetConnectedRoads(Construction build)
    {
        var result = new HashSet<Vector3Int>();
        var buildPos = build.transform.position;
        var buildSize = build.Size;
        var buildTileSize = buildingTilemap.cellSize;
        var roadTileSize = elementTilemap.cellSize;

        foreach (var dir in Dir4)
        {
            Vector3 buildOffset = new Vector3(
                dir.x * (buildSize.x * buildTileSize.x / 2f),
                dir.y * (buildSize.y * buildTileSize.y / 2f),
                0f
            );

            Vector3 roadOffset = new Vector3(
                dir.x * (roadSize.x * roadTileSize.x / 2f),
                dir.y * (roadSize.y * roadTileSize.y / 2f),
                0f
            );

            Vector3 checkPos = buildPos + buildOffset + roadOffset;
            Vector3Int checkCell = elementTilemap.WorldToCell(checkPos);

            if (elemDict.TryGetValue(checkCell, out var roadData) && roadData.Construction?.IsRoad() == true)
            {
                var roadPos = roadData.Construction.transform.position;

                if (SamePosition(checkPos, roadPos))
                {
                    result.Add(checkCell);
                    continue;
                }

                AddClosestRoadCellAround(checkPos, checkCell, roadPos, result);
            }
        }

        return result.ToList();
    }

    private void AddClosestRoadCellAround(Vector3 checkPos, Vector3Int centerCell, Vector3 roadPos, HashSet<Vector3Int> result)
    {
        foreach (var offset in Dir4)
        {
            Vector3Int neighborCell = centerCell + offset;

            if (elemDict.TryGetValue(neighborCell, out var neighborData) && neighborData.Construction?.IsRoad() == true)
            {
                Vector3 neighborPos = neighborData.Construction.transform.position;

                Vector3 closerPos = Vector3.SqrMagnitude(checkPos - roadPos) < Vector3.SqrMagnitude(checkPos - neighborPos)
                    ? roadPos
                    : neighborPos;

                Vector3Int closerCell = elementTilemap.WorldToCell(closerPos);

                if (elemDict.TryGetValue(closerCell, out var closerData) &&
                    closerData.Construction?.IsRoad() == true &&
                    !result.Contains(closerCell))
                {
                    result.Add(closerCell);
                }
            }
        }
    }
}