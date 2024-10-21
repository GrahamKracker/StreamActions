namespace StreamActions;

public abstract class DurationAction : StreamAction
{
    public virtual void OnUpdate() { }
    public virtual void OnEnd() { }

    public abstract float GetDurationInSeconds();
}