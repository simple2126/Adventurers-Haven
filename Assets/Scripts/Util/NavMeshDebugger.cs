using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugger : MonoBehaviour
{
    [Header("NavMesh 테스트 포인트")]
    public Transform[] testPoints;

    [Header("Gizmo 설정")]
    public float sampleRadius = 0.5f;
    public float gizmoSphereRadius = 0.2f;
    public bool drawAlways = false; // true면 Edit 모드에서도 그림

    private void OnDrawGizmos()
    {
        if (!drawAlways || Application.isPlaying) return;

        DrawNavMeshGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (drawAlways || Application.isPlaying)
        {
            DrawNavMeshGizmos();
        }
    }

    private void DrawNavMeshGizmos()
    {
        if (testPoints == null) return;

        foreach (var point in testPoints)
        {
            if (point == null) continue;

            Vector3 pos = point.position;
            NavMeshHit hit;
            bool found = NavMesh.SamplePosition(pos, out hit, sampleRadius, NavMesh.AllAreas);

            Gizmos.color = found ? Color.green : Color.red;
            Gizmos.DrawSphere(pos, gizmoSphereRadius);

            if (found)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, hit.position);
                Gizmos.DrawSphere(hit.position, gizmoSphereRadius * 0.8f);
            }
        }
    }
}
