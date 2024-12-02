namespace StreamActions;

public abstract class TimedAction : StreamAction
{
    public virtual void OnUpdate() { }
    public virtual void OnEnd() { }

    public bool IsOngoing { get; set; } = false;

    public abstract int Duration { get; }
}