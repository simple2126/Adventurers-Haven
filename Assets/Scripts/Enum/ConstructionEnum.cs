using GoogleSheet.Core.Type;

[UGS(typeof(ConstructionType))]

public enum ConstructionType
{
    Build,
    Element,
}

// ConstructionType 하위 타입

public enum BuildType
{
    Equipment,
    Restaurant,
}

public enum ElementType
{
    Road,
    Tree,
}

// Road 배치 상태 Enum
public enum RoadPlacementState
{
    None,
    Dragging,
    Confirm
}