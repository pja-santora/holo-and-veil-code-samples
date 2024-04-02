// Base Class for all Item Types

using System;

[Serializable]
public class Item
{
    public string _name;

    public Item(string name)
    {
        _name = name;
    }
}
