using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private PoolManager.PoolConfig poolConfigs;
    [SerializeField] private Transform[] spawnPositions;

    private float spawnInterval = 0f;
    private float spawnTime = 1f;
    private int spawnCount = 0;

    private void Awake()
    {
        PoolManager.Instance.AddPools<Adventurer>(poolConfigs);
    }

    private void Update()
    {
        spawnInterval += Time.deltaTime;
        if(spawnCount < 1 && spawnInterval > spawnTime)
        {
            spawnInterval = 0f;
            int rand = Random.Range(0, spawnPositions.Length);
            var obj = PoolManager.Instance.SpawnFromPool<Adventurer>(poolConfigs.Tag, spawnPositions[rand].position, Quaternion.identity);
            int other = rand == 0 ? 1 : 0;
            obj.Init(spawnPositions[other].position);
            spawnCount++;
        }
    }
}
