public class IdleState : IPlacerState
{
    private readonly BasePlacer ctx;
    public IdleState(BasePlacer context) { ctx = context; }

    public void HandleInput()
    {
        if (InputManager.Instance.IsInputDown())
        {
            ctx.TransitionTo(PlacementState.Placing);
        }
    }
    public void UpdateLogic() { }
}
