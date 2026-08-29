namespace Adv76.JsonMergePatch;

internal class ErrorDictionary
{
    private readonly Dictionary<string, List<string>> _dictionary = [];

    public int Count => _dictionary.Count;

    public void Add(string key, string message)
    {
        if (_dictionary.TryGetValue(key, out var errors))
        {
            errors.Add(message);
        }
        else
        {
            _dictionary[key] = [message];
        }
    }
    
    public Dictionary<string, string[]> ToArrayDictionary() 
        => _dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
}