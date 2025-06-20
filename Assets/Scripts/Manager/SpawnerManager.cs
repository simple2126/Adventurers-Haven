using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : SingletonBase<SpawnerManager>
{
    [SerializeField] private PoolManager.PoolConfig[] poolConfigs;
    [SerializeField] private Transform[] spawnPositions;

    private List<Adventurer> adventurerList = new List<Adventurer>();
    private float spawnInterval = 0f;
    private float spawnTime = 1f;
    private int spawnCount = 0;

    protected override void Awake()
    {
        base.Awake();
        PoolManager.Instance.AddPools<Adventurer>(poolConfigs);
        gameObject.SetActive(false); // 스폰 매니저는 비활성화 상태로 시작
    }

    //private void Update()
    //{
    //    spawnInterval += Time.deltaTime;
    //    if(spawnCount < 1 && spawnInterval > spawnTime)
    //    {
    //        if (!SpawnerManager.Instance.gameObject.activeSelf) return;
    //        spawnInterval = 0f;
    //        int rand = Random.Range(0, spawnPositions.Length);
    //        int randAdventurer = Random.Range(0, poolConfigs.Length);
    //        var obj = PoolManager.Instance.SpawnFromPool<Adventurer>(poolConfigs[randAdventurer].Tag, spawnPositions[rand].position, Quaternion.identity);
    //        adventurerList.Add(obj);
    //        obj.InitRandomBuildPath();
    //        spawnCount++;
    //        Debug.Log($"Spawn");
    //    }
    //}

    public void SearchPathAllAdventurer()
    {
        if (adventurerList.Count == 0) return;

        for (int i = 0; i < adventurerList.Count; i++)
        {
            var adventurer = adventurerList[i];
            if (adventurer != null && !adventurer.gameObject.activeSelf)
            {
                adventurer.InitRandomBuildPath();
            }
        }
    }

    public void Spawn()
    {
        Debug.Log($"[SpawnerManager] Spawn Adventurer");
        int rand = Random.Range(0, spawnPositions.Length);
        int randAdventurer = Random.Range(0, poolConfigs.Length);
        var obj = PoolManager.Instance.SpawnFromPool<Adventurer>(poolConfigs[randAdventurer].Tag, spawnPositions[rand].position, Quaternion.identity);
        obj.InitRandomBuildPath();
    }
}
