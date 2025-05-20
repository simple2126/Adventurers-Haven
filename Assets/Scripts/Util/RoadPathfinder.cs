using System.Collections.Generic;
using UnityEngine;

// =======================================================================
// RoadPathfinder : "도로 블록 → 무작위 건물 중앙" 경로 계산
//   • FindPathToRandomBuild(...) ⇒ A* 로 건물 외곽과 맞닿은 도로 블록까지 이동 경로 계산
// =======================================================================
public class RoadPathfinder
{
    private const string LogTag = "[RoadPathfinder]";

    // -------------------------------------------------------------------
    // 내부 노드 자료구조 (A*)
    // -------------------------------------------------------------------
    private class Node
    {
        public Vector3Int Pos;
        public int G;                 // 시작 ~ 현재 노드까지 실제 비용
        public int H;                 // 휴리스틱 (Manhattan)
        public int F => G + H;        // 총 비용
        public Node Parent;
        public Node(Vector3Int pos, int g, int h, Node parent)
        {
            Pos = pos;
            G = g;
            H = h;
            Parent = parent;
        }
    }

    // -------------------------------------------------------------------
    // 필드
    // -------------------------------------------------------------------
    private readonly Dictionary<Vector3Int, CustomTileData> elementTileDict;
    private readonly Dictionary<Vector3Int, CustomTileData> buildTileDict;

    // 도로/건물 Construction 캐싱용 리스트 (필요 시 확장)
    private readonly List<Construction> buildConList = new();
    private readonly List<Construction> roadConList = new();

    // 도로 블록 한 칸이 차지하는 셀 크기 (예: 2×2)
    private readonly Vector2Int roadSize = new(2, 2);

    // 이동 방향 (4방)
    private static readonly Vector3Int[] Dir4 =
    {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };

    // -------------------------------------------------------------------
    // 생성자
    // -------------------------------------------------------------------
    public RoadPathfinder(Dictionary<Vector3Int, CustomTileData> elementTileDict,
                          Dictionary<Vector3Int, CustomTileData> buildTileDict)
    {
        this.elementTileDict = elementTileDict;
        this.buildTileDict = buildTileDict;
    }

    // -------------------------------------------------------------------
    // 퍼블릭 : 시작 셀 → 무작위 건물 중앙 셀 경로 찾기
    // -------------------------------------------------------------------
    public List<Vector3Int> FindPathToRandomBuild(Vector3Int startCell, out Vector3Int buildCenterCell)
    {
        buildCenterCell = Vector3Int.zero;

        // 1) 무작위 건물 중심 셀 선택
        buildCenterCell = GetRandomBuildCenterCell();
        if (buildCenterCell == Vector3Int.zero)
        {
            Debug.LogError($"{LogTag} No build cell found");
            return new();
        }

        // 2) 건물과 맞닿아 있는 도로 셀 찾기 (목표)
        Vector3Int targetRoadCell = GetNeighborRoadCell(buildCenterCell);
        if (targetRoadCell == Vector3Int.zero)
        {
            Debug.LogError($"{LogTag} No road cell found");
            return new();
        }

        // 3) 도로 Construction 리스트 갱신 (옵션)
        SetConList(elementTileDict, roadConList);

        // 4) A* 경로 계산
        return AStar(startCell, targetRoadCell);
    }

    // ===================================================================
    // A* 알고리즘 구현
    // ===================================================================
    private List<Vector3Int> AStar(Vector3Int start, Vector3Int goal)
    {
        List<Vector3Int> empty = new();
        if (!buildTileDict[start].Construction.IsRoad() || !buildTileDict[goal].Construction.IsRoad())
        { 
            return empty; 
        }

        // 개방/폐쇄 집합
        List<Node> open = new();
        HashSet<Vector3Int> closed = new();

        open.Add(new Node(start, 0, Heuristic(start, goal), null));

        while (open.Count > 0)
        {
            // F(=G+H) 값이 가장 낮은 노드 선택
            Node current = GetNodeWithLowestF(open);

            // 목표 도달 → 경로 재구성
            if (current.Pos == goal)
                return ReconstructPath(current);

            open.Remove(current);
            closed.Add(current.Pos);

            foreach (Vector3Int neighbor in GetNeighbors(current.Pos))
            {
                if (closed.Contains(neighbor)) continue;

                int tentativeG = current.G + 1; // 4방 단일 코스트

                Node existing = open.Find(n => n.Pos == neighbor);
                if (existing == null)
                {
                    // 새 노드
                    open.Add(new Node(neighbor, tentativeG, Heuristic(neighbor, goal), current));
                }
                else if (tentativeG < existing.G)
                {
                    // 더 좋은 경로 발견 → 업데이트
                    existing.G = tentativeG;
                    existing.Parent = current;
                }
            }
        }

        // 경로 없음
        return empty;
    }

    private static Node GetNodeWithLowestF(List<Node> list)
    {
        Node best = list[0];
        for (int i = 1; i < list.Count; i++)
            if (list[i].F < best.F || (list[i].F == best.F && list[i].H < best.H))
                best = list[i];

        return best;
    }

    private static int Heuristic(Vector3Int a, Vector3Int b)
    {
        // Manhattan 거리 (블록 단위)
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        foreach (Vector3Int dir in Dir4)
        {
            // 도로 블록 크기만큼 이동 (ex: 2×2) → 중심 좌표만 사용
            Vector3Int step = new(dir.x * roadSize.x, dir.y * roadSize.y, 0);
            Vector3Int neighbor = cell + step;
            if (buildTileDict[neighbor].Construction.IsRoad())
                yield return neighbor;
        }
    }

    private static List<Vector3Int> ReconstructPath(Node node)
    {
        List<Vector3Int> rev = new();
        while (node != null)
        {
            rev.Add(node.Pos);
            node = node.Parent;
        }
        rev.Reverse();
        return rev;
    }

    // ===================================================================
    // 보조 메서드
    // ===================================================================
    private static void SetConList(Dictionary<Vector3Int, CustomTileData> conDict, List<Construction> conList)
    {
        conList.Clear();
        foreach (var kvp in conDict)
        {
            if (kvp.Value.Construction == null) continue;
            if (conList.Contains(kvp.Value.Construction)) continue;
            conList.Add(kvp.Value.Construction);
        }
    }

    private Vector3Int GetRandomBuildCenterCell()
    {
        SetConList(buildTileDict, buildConList);
        if (buildConList.Count == 0) return Vector3Int.zero;

        int randomIndex = Random.Range(0, buildConList.Count);
        Construction con = buildConList[randomIndex];
        return MapManager.Instance.BuildingTilemap.WorldToCell(con.transform.position);
    }

    // 건물 외곽 전방향을 roadSize 만큼 스캔
    private IEnumerable<Vector3Int> IterateDir4Area(Vector3Int origin, Vector2Int size)
    {
        foreach (Vector3Int dir in Dir4)
        {
            int maxStep = dir.x != 0 ? size.x : size.y;
            for (int s = 1; s <= maxStep; s++)
                yield return origin + dir * s;
        }
    }

    // 건물과 인접한 도로 셀 찾기
    private Vector3Int GetNeighborRoadCell(Vector3Int buildCenterCell)
    {
        Vector2Int size = buildTileDict[buildCenterCell].Construction.Size;
        foreach (Vector3Int offset in IterateDir4Area(buildCenterCell, size))
        {
            if (elementTileDict.ContainsKey(offset))
                return MapManager.Instance.ElementTilemap.WorldToCell(elementTileDict[offset].Construction.transform.position);
        }
        return Vector3Int.zero;
    }
}
