// Base class representing Actions used by player controlled characters during combat.

using System;

[Serializable]
public class Action
{
    public string _name;

    public enum Side { player, enemy, all }
    public Side _side;

    // which actors the action affects
    public enum Target {
        /* player side */ self, both,
        /* both sides  */ any, 
        /* enemy side  */ first_non_ceiling, any_non_ceiling, all, all_non_ceiling, all_non_air
    }
    public Target _target;

    public enum Trait { none, piercing, reducing }
    public Trait _trait;

    public int _cost;

    public bool HasStatus;
    public Status _status;

    public Action(string name, Side side, Target target, Trait trait = Trait.none, int cost = 0, bool has_status = false, Status status = null)
    {
        _name = name;
        _side = side;
        _target = target;
        _trait = trait;
        _cost = cost;

        HasStatus = has_status;
        if (status == null)
            _status = new Status();
        else
            _status = new Status(status);
    }

    public Action(Action action)
    {
        _name = action._name;
        _side = action._side;
        _target = action._target;
        _trait = action._trait;
        _cost = action._cost;

        HasStatus = action.HasStatus;
        _status = new Status(action._status);
    }
}
