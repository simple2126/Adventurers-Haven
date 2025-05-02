using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pooling/PoolConfig")]
public class PoolConfigSO : ScriptableObject
{
    public string tag;
    public GameObject prefab;
    public int size = 0;
}