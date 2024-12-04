namespace StreamActions.Actions.TimedActions;

public abstract class TimedAction : StreamAction
{
    public virtual void OnUpdate() { }
    public virtual void OnEnd() { }

    public bool IsOngoing { get; set; } = false;

    public int Duration { get; protected set; } = -1;
}