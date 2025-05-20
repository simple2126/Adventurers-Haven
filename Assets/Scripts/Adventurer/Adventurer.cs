using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// =======================================================================
// Adventurer : 경로 따라 이동 후 건물 중앙으로 입장
// public class Adventurer : MonoBehaviour

public class Adventurer : MonoBehaviour
{
    private const string LogTag = "[Adventurer]";
    [SerializeField] private float stepInterval = 0.25f;
    [SerializeField] private float retryInterval = 0.5f;
    [SerializeField] private Vector2Int blockSize = new(2, 2);

    private RoadPathfinder pathfinder;
    private List<Vector3Int> path;
    private Vector3Int targetBuildCell;
    private int pathIdx;
    private Tilemap roadTM; private Tilemap buildTM;
    private bool isMoving;

    public void InitRandomBuildPath()
    {
        roadTM = MapManager.Instance.ElementTilemap;
        buildTM = MapManager.Instance.BuildingTilemap;
        pathfinder = new RoadPathfinder(MapManager.Instance.ElementTileDict, MapManager.Instance.BuildTileDict);
        StartCoroutine(SearchRoutine());
    }

    private IEnumerator SearchRoutine()
    {
        int attempt = 0; const int maxAttempts = 100;
        while (attempt < maxAttempts)
        {
            attempt++;
            Vector3Int startCell = roadTM.WorldToCell(transform.position);
            Debug.Log($"{LogTag} Search attempt {attempt} from {startCell}");
            path = pathfinder.FindPathToRandomBuild(startCell, out targetBuildCell);
            if (path != null) { Debug.Log($"{LogTag} Path len {path.Count} to build {targetBuildCell}"); StartCoroutine(WalkRoutine()); yield break; }
            yield return new WaitForSeconds(retryInterval);
        }
        Debug.LogError($"{LogTag} Failed to find path after {maxAttempts} attempts");
    }

    private IEnumerator WalkRoutine()
    {
        if (path == null || path.Count == 0) { Debug.LogError($"{LogTag} path null"); yield break; }
        isMoving = true; pathIdx = 0;
        Vector3 blockHalf = new(roadTM.cellSize.x * (blockSize.x / 2f), roadTM.cellSize.y * (blockSize.y / 2f), 0);
        while (pathIdx < path.Count)
        {
            Vector3 target = roadTM.GetCellCenterWorld(path[pathIdx]) + blockHalf;
            Vector3 start = transform.position; float t = 0;
            while (t < 1f) { t += Time.deltaTime / stepInterval; transform.position = Vector3.Lerp(start, target, t); yield return null; }
            transform.position = target; pathIdx++;
        }
        // 건물 중앙 이동
        Vector3 center = buildTM.GetCellCenterWorld(targetBuildCell);
        Vector3 s = transform.position; float tt = 0;
        while (tt < 1f) { tt += Time.deltaTime / stepInterval; transform.position = Vector3.Lerp(s, center, tt); yield return null; }
        transform.position = center; isMoving = false;
        Debug.Log($"{LogTag} Arrived at build {targetBuildCell}");
    }

    public bool IsMoving() => isMoving;

    private void OnDrawGizmos()
    {
        if (path == null || roadTM == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.Count - 1; i++) Gizmos.DrawLine(roadTM.GetCellCenterWorld(path[i]), roadTM.GetCellCenterWorld(path[i + 1]));
        if (pathIdx < path.Count) { Gizmos.color = Color.green; Gizmos.DrawSphere(roadTM.GetCellCenterWorld(path[pathIdx]), 0.2f); }
        if (buildTM != null && targetBuildCell != Vector3Int.zero) { Gizmos.color = Color.red; Gizmos.DrawSphere(buildTM.GetCellCenterWorld(targetBuildCell), 0.3f); }
    }
}
