using AdventurersHaven;
using UnityEngine;

public class Construction : MonoBehaviour
{
    public ConstructionType Type { get; private set; }

    public void SetData(Construction_Data data)
    {
        Type = data.ConstructionType;
    }
}
