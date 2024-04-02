using System;

[Serializable]
public class Artifact : Item
{
    // how many of this item the player currently owns
    public int _owned;

    // the currency value of the artifact
    public enum Type { dull = 20, clear = 50, glint = 100, lucent = 1000, }
    public Type _type;

    public Artifact(string name, Type type, int owned = 1) : base(name)
    {
        _type = type;
        _owned = owned;
    }

    public void Add(int by)
    {
        _owned = Math.Min(_owned + by, 99);
    }
}
