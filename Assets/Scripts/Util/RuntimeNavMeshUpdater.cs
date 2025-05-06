using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using NavMeshPlus.Components;

public class RuntimeNavMeshUpdater : MonoBehaviour
{
    private NavMeshSurface surface;
    private List<NavMeshBuildSource> cachedSources = new List<NavMeshBuildSource>();
    private Bounds cachedBounds;

    private void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;

        // navMeshData가 없으면 새로 생성 후 등록
        if (surface.navMeshData == null)
        {
            surface.navMeshData = new NavMeshData();
            NavMesh.AddNavMeshData(surface.navMeshData);
        }
    }

    /// <summary>
    /// 초기 NavMesh Bake
    /// </summary>
    public void InitNavMesh()
    {
        Physics2D.SyncTransforms();

        // 소스 수집
        cachedSources.Clear();
        NavMeshBuilder.CollectSources(
            surface.transform,
            surface.layerMask,
            surface.useGeometry,
            surface.defaultArea,
            new List<NavMeshBuildMarkup>(),
            cachedSources
        );

        // Bounds 계산 (타일맵 기준)
        var cellBounds = MapManager.Instance.ElementTilemap.cellBounds;
        Vector3 center = new Vector3(cellBounds.center.x, 0, cellBounds.center.y);
        Vector3 size = new Vector3(cellBounds.size.x, 1, cellBounds.size.y);
        cachedBounds = new Bounds(center, size);

        // NavMesh 비동기 생성
        NavMeshBuilder.UpdateNavMeshDataAsync(
            surface.navMeshData,
            surface.GetBuildSettings(),
            cachedSources,
            cachedBounds
        );

        Debug.Log("[Init] NavMesh 생성됨");
    }

    /// <summary>
    /// 런타임 중 NavMesh 갱신 (예: 타일 추가/제거 후 호출)
    /// </summary>
    public void UpdateNavMeshFromChanges()
    {
        Physics2D.SyncTransforms();

        cachedSources.Clear();
        NavMeshBuilder.CollectSources(
            surface.transform,
            surface.layerMask,
            surface.useGeometry,
            surface.defaultArea,
            new List<NavMeshBuildMarkup>(),
            cachedSources
        );

        cachedBounds = surface.navMeshData.sourceBounds;

        NavMeshBuilder.UpdateNavMeshDataAsync(
            surface.navMeshData,
            surface.GetBuildSettings(),
            cachedSources,
            cachedBounds
        );

        Debug.Log("[Update] NavMesh 갱신됨");
    }
}
