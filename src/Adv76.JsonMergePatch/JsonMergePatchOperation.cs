namespace Adv76.JsonMergePatch;

internal class JsonMergePatchOperation(Action<object> action, object target)
{
    private readonly Action<object> _action = action;
    private readonly object _target = target;
    
    public void Apply() => _action(_target);
}