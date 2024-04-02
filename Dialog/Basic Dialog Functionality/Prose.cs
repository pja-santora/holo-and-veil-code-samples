// Basic Dialog representation.
// A "Prose" is one section or screen of dialog

using System;
using UnityEngine;

[Serializable]
public class Prose
{
    // the name of the character speaking
    public string _character;

    // what they say
    [TextArea(2, 10)] public string _line;

    // what side thier sprite is on
    public enum Side { left, right }
    public Side _side;

    // what pose their sprite is standing in
    public string _pose;

    // what characters needs to be in the party for this prose to appear
    public string[] RequiredCharacters;

    public Prose()
    {
        _character = "";
        _line = "";

        _side = Side.left;
        _pose = "";

        RequiredCharacters = new string[0];
    }

    public Prose(string character, string line, Side side, string pose, string[] requiredCharacters = null)
    {
        _character = character;
        _line = line;

        _side = side;
        _pose = pose;

        if (requiredCharacters == null || requiredCharacters.Length == 0)
        {
            RequiredCharacters = new string[0];
        }
        else
        {
            RequiredCharacters = new string[requiredCharacters.Length];
            Array.Copy(requiredCharacters, RequiredCharacters, requiredCharacters.Length);
        }
    }

    public Prose(Prose prose)
    {
        _character = prose._character;
        _line = prose._line;

        _side = prose._side;
        _pose = prose._pose;

        if (prose.RequiredCharacters == null || prose.RequiredCharacters.Length == 0)
        {
            RequiredCharacters = new string[0];
        }
        else
        {
            RequiredCharacters = new string[prose.RequiredCharacters.Length];
            Array.Copy(prose.RequiredCharacters, RequiredCharacters, prose.RequiredCharacters.Length);
        }
    }

    public Prose(string description)
    {
        _line = description;
    }
}
