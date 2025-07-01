using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    private const string LogTag = "[Adventurer]";
    [SerializeField] private float moveSpeed = 2f; // ← 이동 속도

    [SerializeField] private List<Vector3> path;          // ← 도로 경로(월드좌표)
    [SerializeField] private Vector3 buildCenterPos;      // ← 건물 중심(월드좌표)

    private int pathIdx;
    private bool isMoving = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        gameObject.SetActive(false); // Adventurer는 비활성화 상태로 시작
        Debug.Log($"[Adventurer] Current Animator State: {anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")}");
    }

    public void InitRandomBuildPath()
    {
        StartCoroutine(SearchRoutine());
    }

    private IEnumerator SearchRoutine()
    {
        while (true)
        {
            Vector3Int startCell = MapManager.Instance.ElementTilemap.WorldToCell(transform.position);
            //Debug.Log($"{LogTag} Searching path from {startCell}");

            bool success = RoadPathfinder.Instance.TryFindPathToBuild(
                startCell, out buildCenterPos, out path);
            
            if(!success)
            {
                PoolManager.Instance.ReturnToPool<Adventurer>(this.gameObject.name, this);
            }
            
            //Debug.Log($"{LogTag} Path search result: {success}, buildCenterPos={buildCenterPos}");

            if (success && path.Count > 0)
            {
                //Debug.Log($"{LogTag} Found path {path.Count} → {buildCenterPos}");
                anim.SetBool("Walk", true);
                Debug.Log($"[Adventurer] Current Animator State: {anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")}");
                StartCoroutine(WalkRoutine());
                yield break; // 일단 걷기 시작하면 루프 종료
            }

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
            sprite.flipX = target.x < transform.position.x;
            while ((transform.position - target).sqrMagnitude > 0.001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
            pathIdx++;
        }

        isMoving = false;
        anim.SetBool("Walk", false);
        //Debug.Log($"{LogTag} Arrived at build {buildCenterPos}");
    }


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
        if(!collision.TryGetComponent<Construction>(out var construction)) return;

        if (construction.Type == ConstructionType.Build)
        {
            gameObject.SetActive(false); // 건물 안에 들어가면 Adventurer 비활성화
            StopAllCoroutines();
            path.Clear();
        }
    }
}
