using System.Collections;

namespace Adv76.JsonMergePatch;

internal class JsonMergePatchOperation
{
    private readonly bool _dictOp;

    private readonly object _target;
    private readonly object _key;
    private readonly object? _value;
    private readonly Action<object, object?> _setter;

    public JsonMergePatchOperation(object target, object? value, Action<object, object?> setter)
    {
        _dictOp = false;
        
        _target = target;
        _key = null!;
        _value = value;
        _setter = setter;
    }

    public JsonMergePatchOperation(IDictionary dict, object key, object? value)
    {
        _dictOp = true;

        _target = dict;
        _key = key;
        _value = value;
        _setter = null!;
    }

    public void Apply()
    {
        if (_dictOp)
        {
            if (_value is not null)
            {
                ((IDictionary)_target)[_key] = _value;
            }
            else
            {
                ((IDictionary)_target).Remove(_key);
            }
        }
        else
        {
            _setter(_target, _value);
        }
    }
}