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

        // 마우스/터치 Down 은 두 번째 드래그(Preview 시작) 신호로만 쓸 거면 여기선 생략 가능
        if (InputManager.Instance.IsInputDown())
        {
            InputManager.Instance.BeginDrag();
        }

        if (InputManager.Instance.IsInputHeld())
        {
            InputManager.Instance.UpdateDrag();
            ctx.OnTouchDragUpdate();
        }

        if (InputManager.Instance.IsInputUp())
        {
            InputManager.Instance.EndDrag();

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
