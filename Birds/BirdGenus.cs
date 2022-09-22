using System;

namespace Birds;

public class BirdGenus : IComparable<BirdGenus>
{
    private string _key;
    private string _name;
    private int _familyKey;

    public string Key { get => _key; set => _key = value; }
    public string Name { get => _name; set => _name = value; }
    public int FamilyKey { get => _familyKey; set => _familyKey = value; }

    public BirdGenus()
    {
        _key = "";
        _name = "";
        _familyKey = -1;
    }

    public BirdGenus(string genusName, int familyKey)
    {
        _key = genusName;
        _name = genusName;
        _familyKey = familyKey;
    }

    public int CompareTo(BirdGenus other)
    {
        return _name.CompareTo(other._name);
    }
}