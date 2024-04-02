// Useable Items are the most important Item Type.
// They are one-time-use Items that have various in-combat effects like healing the player, or damaging enemies.
// Some are useable within the pause menu.

using System;

[Serializable]
public class UseableItem : Item
{
    // how many of this item the player currently owns
    public int _owned;

    // what function the item has
    public enum Function { life, energy, life_and_energy, support, battle }
    public Function _function;

    // the numerical effect of the item
    // ex: for gain '5' life, '5' is the effect
    public int _effect;

    public Action.Side _side;
    public Action.Target _target;

    // how likely the item will appear during a random interaction in battle
    public int _weight;

    public UseableItem(string name, int owned = 1, int effect = 0, Function function = Function.battle,
        Action.Side side = Action.Side.player, Action.Target target = Action.Target.any, int weight = 1) : base(name)
    {
        _owned = owned;
        _effect = effect;
        _function = function;
        _side = side;
        _target = target;
        _weight = weight;
    }

    public UseableItem(string name, UseableItem item) : base(name)
    {
        _owned = item._owned;
        _effect = item._effect;
        _function = item._function;

        _side = item._side;
        _target = item._target;
        _weight = item._weight;
    }

    public void Add(int by)
    {
        _owned = Math.Min(_owned + by, 99);
    }
}
