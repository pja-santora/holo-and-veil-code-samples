using System;

[Serializable]
public class KeyItem : Item
{
    // does the player keep the item for the rest of the game
    public enum Type { temporary, permanent }
    public Type _type;

    public KeyItem(string name, Type type = Type.permanent) : base(name)
    {
        _type = type;
    }

    public KeyItem(string name, KeyItem item) : base(name)
    {
        _type = item._type;
    }
}
