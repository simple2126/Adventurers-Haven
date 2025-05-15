using AdventurersHaven;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemovePlacer : BasePlacer
{
    public override bool RequiresPreview => false;

    public RemovePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
        : base(camera, check, cancel, notPlaceable)
    {
    }

    public override void StartPlacing(Construction_Data data, Construction construction)
    {
        base.StartPlacing(data, construction);
        notPlaceableIndicator.GetComponent<TextMeshProUGUI>().text = "제거불가!";
        UpdatePlacement();
    }

    public override void UpdatePlacement()
    {
        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction);
        bool sameObjectInArea = MapManager.Instance.CurrentSizeInOneObject(gridPos, buildingSize);

        // 두 조건을 모두 만족해야 함
        bool canRemove = canPlace && sameObjectInArea;

        ChangeColor(canRemove);
        notPlaceableIndicator.SetActive(!canRemove);

        if (InputManager.Instance.IsInputDown() && canRemove)
        {
            SetPlacementButtonsActive(true);
        }
    }

    public override void OnConfirm()
    {
        Place();
        PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
        Exit();
    }

    protected override void Place()
    {
        MapManager.Instance.RemoveBuildingArea(gridPos, previewConstruction);
    }
}
