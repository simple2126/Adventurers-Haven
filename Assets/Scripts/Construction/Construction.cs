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
    public PatternType PatternType { get; private set; }

    public void Init(ConstructionType type, string typeID)
    {
        Debug.Log($"Init 시작: type = {type}, subTypeID = {typeID}");

        var data = DataManager.Instance.GetDeepConstructionData(type, typeID);
        var subType = DataManager.Instance.GetMiddleSubType(type, typeID);

        if (data == null)
        {
            Debug.LogError($"[Init 오류] 데이터를 찾지 못했습니다. Type: {type}, subTypeID: {typeID}");
            return;
        }

        if (data == null || subType == null) return;

        Size = Vector2Int.right * data.BlockSize[0] + Vector2Int.up * data.BlockSize[1];
        Debug.Log($"Size {Size.x} x {Size.y}");
        Tag = data.Tag;
        Type = type;
        SubType = subType;
        SetSubType();
        SetPatternType();
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

    private void SetPatternType()
    {
        if (IsRoad())
        {
            // Tag WhiteRockRoad, GrayRockRoad
            int count = 0;
            string pattern = null;
            for (int i = 0; i < Tag.Length; i++)
            {
                if (char.IsUpper(Tag[i]))
                {
                    count++;
                    if (count == 2)
                    {
                        pattern = Tag.Substring(0, i);
                    }
                }
            }

            if (pattern == "White")
            {
                PatternType = PatternType.White;
            }
            else if (pattern == "Gray")
            {
                PatternType = PatternType.Gray;
            }
            else
            {
                PatternType = PatternType.White; // 기본값
            }
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
