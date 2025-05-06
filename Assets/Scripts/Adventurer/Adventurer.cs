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

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas))
        {
            transform.position = hit.position; // ✅ NavMesh 위로 위치 스냅
            agent.enabled = true;
            agent.SetDestination(destination);
        }
        else
        {
            Debug.LogWarning($"❌ 배치 실패: NavMesh 근처에 없음 - {transform.position} hit Position {hit.position}");
        }
    }
}
