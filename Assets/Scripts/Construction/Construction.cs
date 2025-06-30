using System;
using Unity.VisualScripting;
using UnityEngine;

public class Construction : MonoBehaviour
{
    public Vector2Int Size { get; private set; } // 건물 크기
    public string Tag { get; private set; }
    public ConstructionType Type { get; private set; }
    public string SubType { get; private set; }
    public BuildType? BuildType { get; private set; }
    public ElementType? ElementType { get; private set; }
    public PatternType PatternType { get; private set; }

    private NotRoadSign notRoadSign;
    public Action OnPlace;
    public Action OnCancel;

    public void Init(ConstructionType type, string typeID)
    {
        var data = DataManager.Instance.Construction.GetDeepConstructionData(type, typeID);
        var subType = DataManager.Instance.Construction.GetMiddleSubType(type, typeID);

        if (data == null || subType == null) return;

        Size = Vector2Int.right * data.BlockSize[0] + Vector2Int.up * data.BlockSize[1];
        Tag = data.Tag;
        Type = type;
        SubType = subType;
        SetSubType();
        SetPatternType();

        if (Type != ConstructionType.Build) return;
        SetBuild();
    }
    
    private void SetBuild()
    {
        OnPlace = null;
        OnPlace += ChangeNotRoadSign;
        OnCancel = null;
        OnCancel += ReturnNotRoadSign;
    }

    private void ChangeNotRoadSign()
    {
        var outRoadCells = RoadPathfinder.Instance.GetConnectedRoads(this);
        notRoadSign = PoolManager.Instance.SpawnFromPool<NotRoadSign>("NotRoadSign");
        notRoadSign.transform.position = transform.position;

        if (outRoadCells == null || outRoadCells.Count == 0)
        {
            notRoadSign.gameObject.SetActive(true);
        }
        else
        {
            notRoadSign.gameObject.SetActive(false);
            foreach (var cell in outRoadCells)
            {
                Debug.Log($"ChangeNotRoadSign Pos {cell}");
            }
        }
    }

    private void ReturnNotRoadSign()
    {
        if (notRoadSign == null) return;
        PoolManager.Instance.ReturnToPool("NotRoadSign", notRoadSign);
        notRoadSign = null;
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
