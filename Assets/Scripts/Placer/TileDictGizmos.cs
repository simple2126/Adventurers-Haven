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
/// • 노랑  : 도로(Element) 셀
/// • 하늘색: 건물(Build) 셀
/// 
/// MapManager 오브젝트에 붙여도 되고, 아무 곳이나 빈 오브젝트에 붙여도
/// <see cref="MapManager.Instance"/> 가 존재하기만 하면 동작한다.
/// </summary>
[ExecuteAlways]                       // 에디터 + 런타임 모두 호출
public class TileDictGizmos : MonoBehaviour
{
    [Header("Draw Options")]
    [Tooltip("플레이 모드에서만 표시할지 여부")]
    [SerializeField] private bool onlyPlayMode = true;

    [Tooltip("큐브 기즈모 한 변의 크기(유닛)")]
    [SerializeField] private float cubeSize = 0.15f;

    [Header("Gizmo Colors")]
    [SerializeField] private Color elementColor = Color.yellow;
    [SerializeField] private Color buildColor = new(0.3f, 0.8f, 1f);

    private void OnDrawGizmos()
    {
        if (onlyPlayMode && !Application.isPlaying) return;

        MapManager map = MapManager.Instance;
        if (map == null) return;                  // 아직 초기화 전

        // 1) 도로(Element) 타일 표시
        Gizmos.color = elementColor;
        DrawDict(map.ElementTileDict, map.ElementTilemap);

        // 2) 건물(Build) 타일 표시
        Gizmos.color = buildColor;
        DrawDict(map.BuildTileDict, map.BuildingTilemap);
    }

    /// <summary>
    /// 딕셔너리 하나를 순회하면서 셀 중앙에 큐브와 좌표 라벨을 그린다.
    /// </summary>
    private void DrawDict(Dictionary<Vector3Int, CustomTileData> dict, Tilemap tilemap)
    {
        foreach (var kvp in dict)
        {
            if (!kvp.Value.IsOccupied) continue;  // 빈 셀은 스킵

            Vector3 world = tilemap.GetCellCenterWorld(kvp.Key);
            Vector3 size = Vector3.one * cubeSize;
            Gizmos.DrawCube(world, size);

#if UNITY_EDITOR
            Handles.Label(
                world + Vector3.up * 0.1f,
                $"({kvp.Key.x},{kvp.Key.y})",
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
