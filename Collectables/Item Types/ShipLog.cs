using System;

[Serializable]
public class ShipLog : Item
{
    // order number for this log
    public int _number;
    public int _length;

    public ShipLog(string name, int number, int length) : base(name)
    {
        _number = number;
        _length = length;
    }
}
