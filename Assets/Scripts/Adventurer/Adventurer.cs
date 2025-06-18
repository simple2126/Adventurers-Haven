using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =======================================================================
// Adventurer : 경로 따라 이동 후 건물 중앙으로 입장
// =======================================================================
public class Adventurer : MonoBehaviour
{
    private const string LogTag = "[Adventurer]";
    [SerializeField] private float stepInterval = 0.25f;
    [SerializeField] private float retryInterval = 0.2f;

    [SerializeField] private List<Vector3> path;          // ← 도로 경로(월드좌표)
    [SerializeField] private Vector3 buildCenterPos;      // ← 건물 중심(월드좌표)

    private RoadPathfinder pathfinder;
    private int pathIdx;
    private bool isMoving;

    public void InitRandomBuildPath()
    {
        pathfinder = new RoadPathfinder();
        StartCoroutine(SearchRoutine());
    }

    private IEnumerator SearchRoutine()
    {
        while (true)
        {
            Vector3Int startCell = MapManager.Instance.ElementTilemap.WorldToCell(transform.position);
            Debug.Log($"{LogTag} Searching path from {startCell}");

            bool success = pathfinder.TryFindPathToRandomBuild(
                startCell, out buildCenterPos, out path);

            Debug.Log($"{LogTag} Path search result: {success}, buildCenterPos={buildCenterPos}");

            if (success && path.Count > 0)
            {
                Debug.Log($"{LogTag} Found path {path.Count} → {buildCenterPos}");
                StartCoroutine(WalkRoutine());
                yield break; // 일단 걷기 시작하면 루프 종료
            }

            Debug.LogWarning($"{LogTag} Path not found. Retrying in {retryInterval} sec...");
            yield return null;
            // while 루프로 자동 반복됨
        }
    }

    private IEnumerator WalkRoutine()
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogError($"{LogTag} path null");
            yield break;
        }
        isMoving = true;
        pathIdx = 0;
        while (pathIdx < path.Count)
        {
            Vector3 target = path[pathIdx];
            Vector3 start = transform.position;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / stepInterval;
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
            transform.position = target;
            pathIdx++;
        }
        // 마지막으로 건물 중심까지 이동
        {
            Vector3 s = transform.position;
            float tt = 0;
            while (tt < 1f)
            {
                tt += Time.deltaTime / stepInterval;
                transform.position = Vector3.Lerp(s, buildCenterPos, tt);
                yield return null;
            }
            transform.position = buildCenterPos;
        }
        isMoving = false;
        Debug.Log($"{LogTag} Arrived at build {buildCenterPos}");
    }

    public bool IsMoving() => isMoving;

    private void OnDrawGizmos()
    {
        if (path == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.Count - 1; i++)
            Gizmos.DrawLine(path[i], path[i + 1]);
        if (pathIdx < path.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(path[pathIdx], 0.2f);
        }
        if (buildCenterPos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(buildCenterPos, 0.3f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.TryGetComponent<Construction>(out var construction);
        if (construction.Type == ConstructionType.Build)
        {
            gameObject.SetActive(false); // 건물 안에 들어가면 Adventurer 비활성화
            path = null; // 경로 초기화
        }
    }
}
