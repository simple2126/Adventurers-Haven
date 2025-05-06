using UnityEngine;
using UnityEngine.UI;

public class DefaultPlacer : BasePlacer
{
    public DefaultPlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    : base(camera, check, cancel, notPlaceable)
    {
    }

    protected override void UpdatePlacement()
    {
        bool canPlace = CanPlaceAtCurrentPosition();
        ChangeColor(canPlace);
        notPlaceableIndicator.SetActive(!canPlace);

        if (InputManager.Instance.IsInputDown() && canPlace && state == PlacementState.Placing)
        {
            state = PlacementState.Confirming;
            SetPlacementButtonsActive(true);
        }
    }
}