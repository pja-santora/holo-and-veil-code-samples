// Base Class for various status effects, buffs, and debuffs. (See SubType below) 

using UnityEngine;

[System.Serializable]
public class Status
{
    [HideInInspector] public string _name;

    public enum Type { status, buff, debuff }
    public Type _type;

    public enum SubType { 
        
        // buffs / debuffs
        atk, 
        def, 

        // statuses
        burn, // take 1 damage at the end of each turn
        inferno, // take 2 damage at the end of each turn
        poison, // all damage taken is doubled
        invisible, // can't be targeted or dealt damage by actions
        regeneration, // gain life at the end of each turn
        restoration, // player only; gain energy at the end of each turn

        dizzy, // enemy only; can't act
        blind, // enemy only; when attacking, accuracy -50%

        aura, // player only; attacker takes 1 damage when attacking directly
        counter, // player only; enemy takes damage = character's [ATK] when attacking directly
        taunt, // player only; chance to be targeted by enemy attacks +10%
    }
    public SubType sub_type;

    // how many turns does it last
    public int turn_count;

    // numerically representation of the effect
    public int _value;

    public Status()
    {
        _type = Type.status;
        sub_type = SubType.atk;
        turn_count = 0;
        _value = 0;
        _name = "";
    }

    public Status(Type type, SubType sub, int turns = -1, int value = 0, string name = "")
    {
        _type = type;
        sub_type = sub;
        turn_count = turns;
        _value = value;
        _name = name;
    }

    public Status(Status status)
    {
        _type = status._type;
        sub_type = status.sub_type;
        turn_count = status.turn_count;
        _value = status._value;
        _name = status._name;
    }
}
