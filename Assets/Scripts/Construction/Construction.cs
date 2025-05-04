using AdventurersHaven;
using UnityEngine;

public class Construction : MonoBehaviour
{
    // 게임 오브젝트 이름
    public Vector2Int Size { get; private set; } // 건물 크기
    public string Tag { get; private set; }
    public ConstructionType Type { get; private set; }
    public string SubType { get; private set; }
    public BuildType? BuildType { get; private set; }
    public ElementType? ElementType { get; private set; }

    public void Init(Construction_Data data)
    {
        Size = Vector2Int.right * data.blockSize[0] + Vector2Int.up * data.blockSize[1];
        Tag = data.tag;
        Type = data.constructionType;
        SubType = data.subType;
        SetSubType();
    }

    private void SetSubType()
    {
        switch (Type)
        {
            case ConstructionType.Build:
                BuildType = (BuildType)System.Enum.Parse(typeof(BuildType), SubType);
                break;
            case ConstructionType.Element:
                ElementType = (ElementType)System.Enum.Parse(typeof(ElementType), SubType);
                break;
        }
    }

    public bool IsRoad()
    {
        return Type == ConstructionType.Element &&
            this.ElementType == global::ElementType.Road;
    }

    public bool IsDemolish()
    {
        return Type == ConstructionType.Element &&
            this.ElementType == global::ElementType.Demolish;
    }
}
