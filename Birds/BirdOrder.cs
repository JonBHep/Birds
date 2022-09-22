using System;

namespace Birds;

public class BirdOrder : IComparable<BirdOrder>
{
    private int _key;
    private string _name;

    public int Key { get => _key; set => _key = value; }

    public string Name { get => _name; set => _name = value; }

    public int CompareTo(BirdOrder other)
    {
        return _name.CompareTo(other._name);
    }
}