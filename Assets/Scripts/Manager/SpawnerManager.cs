using UnityEngine;

public class SpawnerManager : SingletonBase<SpawnerManager>
{
    [SerializeField] private PoolManager.PoolConfig poolConfigs;
    [SerializeField] private Transform[] spawnPositions;

    private float spawnInterval = 0f;
    private float spawnTime = 1f;
    private int spawnCount = 0;

    protected override void Awake()
    {
        base.Awake();
        PoolManager.Instance.AddPools<Adventurer>(poolConfigs);
        gameObject.SetActive(false); // 스폰 매니저는 비활성화 상태로 시작
    }

    private void Update()
    {
        spawnInterval += Time.deltaTime;
        if(spawnCount < 1 && spawnInterval > spawnTime)
        {
            if (!SpawnerManager.Instance.gameObject.activeSelf) return;
            spawnInterval = 0f;
            int rand = Random.Range(0, spawnPositions.Length);
            var obj = PoolManager.Instance.SpawnFromPool<Adventurer>(poolConfigs.Tag, spawnPositions[rand].position, Quaternion.identity);
            obj.InitRandomBuildPath();
            spawnCount++;
            Debug.Log($"Spawne");
        }
    }
}
