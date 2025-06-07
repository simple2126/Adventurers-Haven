#if UNITY_EDITOR
using UnityEditor;     // Handles.Label 전용
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 씬 뷰에 <see cref="MapManager.BuildTileDict"/> 와 <see cref="MapManager.ElementTileDict"/>
/// 좌표를 항상 시각화해 주는 디버그 전용 스크립트.
///
/// • 노랑  : 도로(Element) 2×2 블록 (한 블록을 하나로 간주)  
/// • 하늘색: 건물(Build) 단일 셀  
///
/// MapManager 오브젝트에 붙여도 되고, 아무 곳이나 빈 오브젝트에 붙여도
/// <see cref="MapManager.Instance"/> 가 존재하기만 하면 동작합니다.
/// </summary>
[ExecuteAlways]                       // 에디터 + 런타임 모두 호출
public class TileDictGizmos : MonoBehaviour
{
    [Header("Draw Options")]
    [Tooltip("플레이 모드에서만 표시할지 여부")]
    [SerializeField] private bool onlyPlayMode = true;

    [Tooltip("큐브 기즈모 한 변의 크기(유닛, Z 방향 두께용)")]
    [SerializeField] private float cubeSize = 0.15f;

    [Header("Gizmo Colors")]
    [SerializeField] private Color elementColor = Color.yellow;
    [SerializeField] private Color buildColor = new(0.3f, 0.8f, 1f);

    // 4방향 (도로 블록끼리 연결선용)
    private static readonly Vector3Int[] Dir4 =
    {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };

    private void OnDrawGizmos()
    {
        if (onlyPlayMode && !Application.isPlaying) return;
        MapManager map = MapManager.Instance;
        if (map == null) return;

        // 1. 도로 셀 표시 및 연결선
        if (map.ElementTileDict != null && map.ElementTilemap != null)
        {
            Gizmos.color = elementColor;
            DrawRoadLines(map.ElementTileDict); // 도로 2x2 선만 (타일맵 불필요)
            DrawDict(map.ElementTileDict, map.ElementTilemap);      // 도로 칸에 큐브 (기존 방식)
        }
        // 2. 건물 셀 표시 (기존 그대로)
        if (map.BuildTileDict != null && map.BuildingTilemap != null)
        {
            Gizmos.color = buildColor;
            DrawDict(map.BuildTileDict, map.BuildingTilemap);
        }

        // (아무 영향 없음, 그냥 잔여 코드)
        // MapManager.Instance.ElementTilemap.GetCellCenterWorld(Vector3Int.zero);
    }

    /// <summary>
    /// Construction의 transform.position을 블록의 중심으로 사용해서  
    /// 도로(블록)끼리 연결선을 그림.
    /// </summary>
    private void DrawRoadLines(Dictionary<Vector3Int, CustomTileData> dict)
    {
        if (dict == null) return;

        // 각 도로 블록의 중심(Construction.transform.position)을 모은다.
        // 하나의 Construction이 여러 셀에 배정될 수 있으므로, 중복은 제거!
        var centerSet = new HashSet<Vector3>();
        var originToCenter = new Dictionary<Vector3Int, Vector3>();
        var originToSize = new Dictionary<Vector3Int, Vector2Int>();

        foreach (var kvp in dict)
        {
            var data = kvp.Value;
            if (!data.IsOccupied || data.Construction == null || !data.Construction.IsRoad()) continue;

            Vector3 center = data.Construction.transform.position;
            Vector3Int origin = kvp.Key;
            Vector2Int size = data.Construction.Size;

            // 이미 등록된 center면 skip (한 블록이 여러 셀에 등록된 경우)
            if (centerSet.Contains(center)) continue;

            centerSet.Add(center);
            originToCenter[origin] = center;
            originToSize[origin] = size;
        }

        // 2. 선 그리기 (오른쪽/위쪽 이웃)
        var drawnPairs = new HashSet<(Vector3, Vector3)>();
        foreach (var kvp in originToCenter)
        {
            var origin = kvp.Key;
            var center = kvp.Value;
            var size = originToSize[origin];

            // "내 사이즈만큼" 떨어진 곳이 이웃!
            Vector3Int[] dirs = { new(size.x, 0, 0), new(0, size.y, 0) };
            foreach (var dir in dirs)
            {
                var neighborOrigin = origin + dir;
                if (!originToCenter.TryGetValue(neighborOrigin, out var neighborCenter)) continue;

                var a = center;
                var b = neighborCenter;
                // (a, b) 순서로 이미 그린 경우 패스
                if (a.sqrMagnitude > b.sqrMagnitude)
                    (a, b) = (b, a);

                if (!drawnPairs.Contains((a, b)))
                {
                    Gizmos.DrawLine(a, b);
                    drawnPairs.Add((a, b));
                }
            }
        }
    }

    /// <summary>
    /// 일반적인 딕셔너리 하나를 순회하면서 셀 중앙에 큐브와 좌표 라벨을 그린다.
    /// (건물(Build)용)
    /// </summary>
    private void DrawDict(
        Dictionary<Vector3Int, CustomTileData> dict,
        Tilemap tilemap)
    {
        if (dict == null || tilemap == null) return;

        foreach (var kvp in dict)
        {
            Vector3Int cellPos = kvp.Key;
            CustomTileData data = kvp.Value;
            if (!data.IsOccupied) continue;  // 빈 셀은 스킵

            Vector3 world = tilemap.GetCellCenterWorld(cellPos);
            Vector3 size = Vector3.one * cubeSize;
            Gizmos.DrawCube(world, size);

#if UNITY_EDITOR
            Handles.Label(
                world + Vector3.up * 0.1f,
                $"({cellPos.x},{cellPos.y})",
                new GUIStyle
                {
                    fontSize = 10,
                    normal = { textColor = Gizmos.color },
                    alignment = TextAnchor.MiddleCenter,
                    richText = false
                });
#endif
        }
    }
}