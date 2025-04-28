using UnityEngine;
using UnityEngine.UI;

public class RemovePlacer : BasePlacer
{
    public RemovePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
        : base(camera, check, cancel, notPlaceable)
    {
    }

    private void ChangeScale()
    {
        Vector2Int size = MapManager.Instance.GetBuildingAre(gridPos);
        buildingSize = Vector2Int.one; // 초기화
        float xRatio = (float)size.x / buildingSize.x;
        float yRatio = (float)size.y / buildingSize.y;

        // 프리팹의 새로운 크기 계산
        previewConstruction.transform.localScale = Vector3.one; // 초기화
        Vector3 newScale = previewConstruction.transform.localScale;
        newScale.x *= xRatio;
        newScale.y *= yRatio;

        // 프리팹 크기 적용
        previewConstruction.transform.localScale = newScale;
        buildingSize = size;
    }

    protected override void UpdatePlacement()
    {
        bool canPlace = CanPlaceAtCurrentPosition();
        ChangeColor(canPlace);
        notPlaceableIndicator.SetActive(!canPlace);
        ChangeScale();

        if (Input.GetMouseButtonDown(0) && canPlace && state == PlacementState.Placing)
        {
            state = PlacementState.Confirming;
            SetPlacementButtonsActive(true);
        }
    }

    protected override void Place()
    {
        MapManager.Instance.RemoveBuildingArea(gridPos, buildingSize, previewConstruction);
    }
}
