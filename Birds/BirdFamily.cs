using System;

namespace Birds;

public class BirdFamily : IComparable<BirdFamily>
{
    private int _key;
    private string _name;
    private int _orderKey;

    public int Key { get => _key; set => _key = value; }
    public string Name { get => _name; set => _name = value; }
    public int OrderKey { get => _orderKey; set => _orderKey = value; }

    public int CompareTo(BirdFamily other)
    {
        if (other is BirdFamily autre)
        {
            return string.Compare(_name, other._name, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            throw new ArgumentNullException(nameof(other));
        }
    }
}