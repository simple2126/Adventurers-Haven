using AdventurersHaven;
using UnityEngine;
using UnityEngine.AI;

public class Adventurer : MonoBehaviour
{
    private NavMeshAgent agent;

    public string Tag { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.enabled = false;
    }

    public void Init(Vector3 destination)
    {
        Adventurer_Data data = DataManager.Instance.GetAdventurerData(AdventurerType.Archer, "Archer");
        Tag = data.tag;

        // 먼저 z값 보정
        Vector3 fixedPos = transform.position;
        fixedPos.z = 0f;
        transform.position = fixedPos;

        // NavMesh에서 가장 가까운 지점 찾기
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;

            // NavMesh 위로 옮긴 후에 agent 활성화
            agent.enabled = true;
            agent.SetDestination(destination);
            Debug.Log("✅ Agent enabled and destination set.");
        }
        else
        {
            Debug.LogError("❌ Spawn 위치가 NavMesh 위가 아님: " + transform.position);
        }
    }
}
