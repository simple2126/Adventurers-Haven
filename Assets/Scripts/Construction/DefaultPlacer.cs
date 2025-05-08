using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DefaultPlacer : BasePlacer
{
    public DefaultPlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    : base(camera, check, cancel, notPlaceable)
    {
    }

    protected override void UpdatePlacement()
    {
        if(state != PlacementState.Placing) return;

        Tilemap tilemap = previewConstruction.Type == ConstructionType.Build
            ? MapManager.Instance.BuildingTilemap
            : MapManager.Instance.ElementTilemap;

        previewConstruction.transform.position = GetSnappedPosition(tilemap, gridPos);
        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction);
        ChangeColor(canPlace);
        notPlaceableIndicator.SetActive(!canPlace);
    }
}