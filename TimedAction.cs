namespace StreamActions;

public abstract class TimedAction : StreamAction
{
    public virtual void OnUpdate() { }
    public virtual void OnEnd() { }

    public abstract int Duration { get; }
}