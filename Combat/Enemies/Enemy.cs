// The main Enemy class that imbues functionality into enemies during combat.
// Each enemy type is constant, thus parameters are set in the inspector.
// Handles various tasks like adjusting stats (life, atk, def) and applying status effects.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // basic stats
    public string _name;
    public int _lv;

    public int _life;
    public int _max_life;
    public int second_state_start_life;
    
    public int base_atk;
    public int _atk;
    public int second_state_atk_modifier;

    public int base_def;
    public int _def;
    public int second_state_def_modifier;
    
    // vertical position of the enemy in battle
    public enum Quality { ground, air, ceiling }
    public Quality _quality;
    public Quality first_state_quality;
    public Quality second_state_quality;

    public enum State { first, second }
    public State _state;

    public enum StateChangeType { none, hit, health }
    public StateChangeType state_change_type;

    public enum BaseSkillPoints { normal = 5, mini = 25, boss = 50 }
    public BaseSkillPoints _base_sp;

    public Status.SubType[] Immunities;

    public EnemyHUD _hud; // UI Offset 211.7686
    public Animator _animator;
    public SpriteWrangler _wangler;

    public float consult_offsetX;
    public float consult_offsetY;
    public float mid_pointY;

    [System.Serializable] public struct Action
    {
        public string _name;

        // order by ascending weight
        // ( 0 to 9 ) for Actions
        // ( 10 to 19 ) for LimitedActions, can be used at low life
        // ( 20 to 29 ) for SecondStateActions, can be used while in second state
        public int _weight;

        public enum Type { attack, tactic }
        public Type _type;

        public enum Side { player, enemy }
        public Side _side;

        public enum Range { front, any, all, self }
        public Range _range;

        public enum Position { outside, during, projectile }
        public Position _position;
        
        public int _parts;
        public int _modifier;

        public bool Direct;

        public bool HasStatus;
        public Status _status;

        // if empty, no next action
        public string next_action;

        // player gaurd timing, timing = 0 for tactics
        public float _timing;
        public float lag_time; // before moving back
        public float _end_time;

        // action starting positions when targeting player characters, ( [0] = front, [1] = back )
        public float[] _Xposition;
    }
    public Action[] Actions;

    // can use limited actions when at or below this life total, 0 if no limited actions
    public int limited_life;

    // upper bound for limited or second state actions
    public int rng_limit;

    // how many turns the enemy stays in the second state, -1 if in second state indefinitely
    public int second_state_turn_limit = -1;

    // true if the enemy has actions only usable during the second state
    public bool HasOtherActions = false;

    // true if the enemy has a move animation
    public bool HasMove = false;

    public enum Type { normal = 1, elite = 2 }
    [HideInInspector] public Type _type;

    // how many turns the enemy has been in the second state
    [HideInInspector] public int second_state_turns;

    [HideInInspector] public List<Status> Statuses = new List<Status>();
    [HideInInspector] public List<GameObject> VisualStatusEffects = new List<GameObject>();

    // enemy physical location in battle
    [HideInInspector] public int _placement;

    // ----- FUNCTIONS ------------------------------------------------------ //
    public bool TakeDamage(int damage)
    {
        int adjusted_damage = Mathf.Max(damage, 0);
        if (InStatuses(Status.SubType.poison))
        {
            adjusted_damage *= 2;
        }

        if (adjusted_damage > 0)
        {
            _animator.Play("_hit");
        }
        _hud.ShowDamageDelt(adjusted_damage);

        // if the enemy dies
        if (_life - adjusted_damage <= 0)
        {
            _life = 0;
            return true;
        }

        _life -= adjusted_damage;
        return false;
    }

    public void GainLife(int by)
    {
        _life = Mathf.Min(_life + by, _max_life);
    }

    public void AddStatus(Status.Type type, Status.SubType sub, int turns = -1, int value = 0, string name = "")
    {
        int index = GetIndexInStatuses(sub);
        int burn_index = GetIndexInStatuses(Status.SubType.burn);
        int inferno_index = GetIndexInStatuses(Status.SubType.inferno);

        // if the character has the same type of status already. Taunt, Regeneration, [ATK], and [DEF] stack
        if (type == Status.Type.status && index != -1 && sub != Status.SubType.taunt &&
            sub != Status.SubType.regeneration)
        {
            // if the new status being applied has a higher turn count, replace the old status
            if (turns > Statuses[index].turn_count && Statuses[index].turn_count != -1)
            {
                Statuses[index].turn_count = turns;
            }
        }
        // if the character has Inferno and the applied status is Burn
        else if (sub == Status.SubType.burn && inferno_index != -1) { }
        else
        {
            // apply other affects and animations
            if (sub == Status.SubType.atk)
            {
                AddAtkBuff(value);
            }
            else if (sub == Status.SubType.def)
            {
                AddDefBuff(value);
            }
            else if (sub == Status.SubType.burn)
            {
                // start burn animation
                GameObject effect = (GameObject)Instantiate(Resources.Load("Battle/Effects/Status/burn_effect"), transform);
                effect.name = "burn_effect";
                VisualStatusEffects.Add(effect);
            }
            else if (sub == Status.SubType.inferno)
            {
                // remove Burn if applied status is Inferno
                if (burn_index != -1)
                {
                    RemoveStatus(burn_index);
                }

                // start inferno animation
                GameObject effect = (GameObject)Instantiate(Resources.Load("Battle/Effects/Status/inferno_effect"), transform);
                effect.name = "inferno_effect";
                VisualStatusEffects.Add(effect);
            }
            else if (sub == Status.SubType.poison)
            {
                // start poison animation
                GameObject effect = (GameObject)Instantiate(Resources.Load("Battle/Effects/Status/poison_effect"), transform);
                effect.name = "poison_effect";
                VisualStatusEffects.Add(effect);
            }
            else if (sub == Status.SubType.invisible)
            {
                _wangler.SetMaterial("invisible");
            }
            else if (sub == Status.SubType.dizzy)
            {
                _animator.SetBool("Dizzy", true);
            }
            else if (sub == Status.SubType.blind)
            {
                // (TODO) start blind animation
            }

            Statuses.Add(new Status(type, sub, turns, value, name));
            SortStatuses();
        }
    }

    public void AddStatus(Status status)
    {
        AddStatus(status._type, status.sub_type, status.turn_count, status._value, status._name);
    }

    public void AddAtkBuff(int by)
    {
        _atk += by;
    }

    public void AddDefBuff(int by)
    {
        _def += by;
    }

    public void LowerStatusTurns()
    {
        for (int i = 0; i < Statuses.Count; ++i)
        {
            if (Statuses[i].turn_count > 0)
            {
                Statuses[i].turn_count -= 1;
                if (Statuses[i].turn_count == 0)
                {
                    RemoveStatus(i);
                    i -= 1;
                }
            }
        }
    }

    public void RemoveStatus(int i)
    {
        if (Statuses[i].sub_type == Status.SubType.atk)
        {
            AddAtkBuff(-Statuses[i]._value);
        }
        else if (Statuses[i].sub_type == Status.SubType.def)
        {
            AddDefBuff(-Statuses[i]._value);
        }
        else if (Statuses[i].sub_type == Status.SubType.burn)
        {
            // stop burn animation
            RemoveVisualEffect("burn_effect");
        }
        else if (Statuses[i].sub_type == Status.SubType.inferno)
        {
            // stop inferno animation
            RemoveVisualEffect("inferno_effect");
        }
        else if (Statuses[i].sub_type == Status.SubType.poison)
        {
            // stop poison animation
            RemoveVisualEffect("poison_effect");
        }
        else if (Statuses[i].sub_type == Status.SubType.dizzy)
        {
            _animator.SetBool("Dizzy", false);
        }
        else if (Statuses[i].sub_type == Status.SubType.blind)
        {
            // (TODO) stop blind animation
        }
        else if (Statuses[i].sub_type == Status.SubType.invisible)
        {
            _wangler.SetMaterial();
        }

        Statuses.RemoveAt(i);
    }

    public void RemoveStatus(string name)
    {
        for (int i = 0; i < Statuses.Count; ++i)
        {
            if (Statuses[i]._name.Equals(name))
            {
                RemoveStatus(i);
                return;
            }
        }
    }

    public void SortStatuses()
    {
        List<Status> sorted_list = Statuses.OrderBy(x => x._type).ThenBy(y => y.sub_type).ThenBy(z => z.turn_count).ToList();
        Statuses = new List<Status>(sorted_list);
    }

    public bool InStatuses(Status.SubType type)
    {
        foreach (Status status in Statuses)
        {
            if (status.sub_type == type)
            {
                return true;
            }
        }
        return false;
    }

    public bool InImmunities(Status.SubType type)
    {
        foreach (Status.SubType status in Immunities)
        {
            if (status == type)
            {
                return true;
            }
        }
        return false;
    }

    public int GetIndexInStatuses(Status.SubType sub)
    {
        for (int i = 0; i < Statuses.Count; ++i)
        {
            if (Statuses[i].sub_type == sub)
            {
                return i;
            }
        }
        return -1;
    }

    public int[] GetIndicesInStatuses(Status.SubType sub)
    {
        List<int> indices = new List<int>();

        for (int i = 0; i < Statuses.Count; ++i)
        {
            if (Statuses[i].sub_type == sub)
            {
                indices.Add(i);
            }
        }
        return indices.ToArray();
    }

    public void PlayStatusAnimation(string type)
    {
        for (int i = 0; i < VisualStatusEffects.Count; ++i)
        {
            if (VisualStatusEffects[i].name.Equals(type))
            {
                VisualStatusEffects[i].GetComponent<Animator>().Play("status_inflict");
                return;
            }
        }
    }

    public void RemoveVisualEffect(string type)
    {
        for (int i = 0; i < VisualStatusEffects.Count; ++i)
        {
            if (VisualStatusEffects[i].name.Equals(type))
            {
                Destroy(VisualStatusEffects[i]);
                VisualStatusEffects.RemoveAt(i);
                return;
            }
        }
    }

    public void SetToElite()
    {
        _type = Type.elite;

        _life *= 2;
        _max_life *= 2;
        second_state_start_life *= 2;
        limited_life *= 2;

        _atk *= 2;
        base_atk *= 2;

        _def += 1;
        base_def += 1;

        if (!InImmunities(Status.SubType.dizzy))
        {
            Immunities = Immunities.Concat(new Status.SubType[] { Status.SubType.dizzy }).ToArray();
        }
    }
}
