using System.Threading;
using UnityEngine.EventSystems;

public class PlacingState : IPlacerState
{
    private readonly BasePlacer ctx;
    public PlacingState(BasePlacer context) { ctx = context; }

    public void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (InputManager.Instance.IsInputHeld())
        {
            InputManager.Instance.UpdateDrag();
            ctx.OnTouchDragUpdate();
        }

        if (InputManager.Instance.IsInputUp())
        {
            if (ctx.RequiresPreview)
            {
                // 첫 드래그 끝났을 때: 시작점 저장
                ctx.OnInitialDragEnd();
                ctx.TransitionTo(PlacementState.Preview);
            }
            else if (!ctx.RequiresPreview)
            {
                // 기본 배치(건물 등)
                ctx.SetPlacementButtonsActive(true);
            }
        }
    }

    public void UpdateLogic()
    {
        ctx.UpdatePlacement();
    }
}
