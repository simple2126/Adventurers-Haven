using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacingState : IPlacerState
{
    private readonly BasePlacer ctx;
    public PlacingState(BasePlacer context) { ctx = context; }

    public void Enter() { /* 진입 초기화 필요시 */ }

    public void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

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
            ctx.SetPlacementButtonsActive(true);
        }
    }

    public void UpdateLogic()
    {
        // DefaultPlacer 의 경우 즉시 위치/색상 갱신
        if (!ctx.RequiresPreview)
            ctx.UpdatePlacement();
    }

    public void Exit() { /* 상태 종료 정리 시 필요시 */ }
}