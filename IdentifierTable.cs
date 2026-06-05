using System.Collections.Generic;

class IdentifierTable
{
    private Dictionary<string, string> _variables;

    public IdentifierTable()
    {
        _variables = new Dictionary<string, string>();
    }

    public bool AddVariable(string name, string type)
    {
        string key = name.ToLower();
        if (_variables.ContainsKey(key))
        {
            return false;
        }
        _variables[key] = type.ToLower();
        return true;
    }

    public string GetVariableType(string name)
    {
        string key = name.ToLower();
        if (_variables.ContainsKey(key))
        {
            return _variables[key];
        }
        return null;
    }
}