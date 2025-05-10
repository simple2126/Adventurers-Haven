using UnityEngine.EventSystems;

// PreviewState.cs
/// 영역/라인 드래그(Preview) 로직 처리 상태
public class PreviewState : IPlacerState
{
    private readonly BasePlacer ctx;
    public PreviewState(BasePlacer context) { ctx = context; }

    public void Enter()
    {
        // Preview 진입 시 초기화(예: roadStartPos 저장)
    }

    public void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (InputManager.Instance.IsInputHeld())
        {
            ctx.OnLineDragUpdate(ctx.GetGridPos());
        }

        if (InputManager.Instance.IsInputUp())
        {
            InputManager.Instance.EndDrag();
            ctx.SetPlacementButtonsActive(true);
        }
    }

    public void UpdateLogic() { /* 필요시 추가 로직 */ }
    public void Exit() { /* 정리 */ }
}