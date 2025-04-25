using AdventurersHaven;
using UnityEngine;

public class Construction : MonoBehaviour
{
    // 게임 오브젝트 이름
    public string Tag { get; private set; }
    public ConstructionType Type { get; private set; }
    public string SubType { get; private set; }
    public BuildType? BuildType { get; private set; }
    public ElementType? ElementType { get; private set; }

    public void SetData(Construction_Data data)
    {
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
}
