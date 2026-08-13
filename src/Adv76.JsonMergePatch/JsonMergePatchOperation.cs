namespace Adv76.JsonMergePatch;

internal class JsonMergePatchOperation(Action<object> action, object target)
{
    public Action<object> Action { get; } = action;
    public object Target { get; } = target;
    
    public void Apply() => Action(Target);
}