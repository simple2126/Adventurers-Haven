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