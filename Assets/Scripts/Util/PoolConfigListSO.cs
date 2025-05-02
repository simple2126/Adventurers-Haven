using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pooling/PoolConfigList")]
public class PoolConfigListSO : ScriptableObject
{
    public List<PoolConfigSO> poolConfigs;
}