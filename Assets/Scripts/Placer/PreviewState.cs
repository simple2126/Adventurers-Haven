using UnityEngine.EventSystems;

public class PreviewState : IPlacerState
{
    private readonly BasePlacer ctx;
    public PreviewState(BasePlacer context) { ctx = context; }

    public void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (InputManager.Instance.IsInputHeld())
        {
            InputManager.Instance.UpdateDrag();         // 누적 드래그량 계산
            ctx.OnTouchDragUpdate();                    // gridPos 업데이트 & 스냅 이동
            ctx.OnLineDragUpdate();                     // updated gridPos로 프리뷰
        }

        if (InputManager.Instance.IsInputUp())
        {
            InputManager.Instance.EndDrag();
            ctx.OnTouchDragUpdate();
            ctx.OnLineDragUpdate();
            ctx.SetPlacementButtonsActive(true);
        }
    }

    public void UpdateLogic() 
    {
        ctx.UpdatePlacement();
    }
}
