using UnityEngine;
using UnityEngine.UI;

public class DefaultPlacer : BasePlacer
{
    public override bool RequiresPreview => false;

    public DefaultPlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    : base(camera, check, cancel, notPlaceable)
    {
    }

    public override void UpdatePlacement()
    {
        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction);
        ChangeColor(canPlace);
        notPlaceableIndicator.SetActive(!canPlace);
    }
}