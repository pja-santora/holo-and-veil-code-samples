using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    public string _name;

    public enum State { ground, air }
    public State _state;
    public State base_state;

    public Animator _animator;
    public SpriteWrangler _wangler;
    public GameObject _effects;

    // stats
    [HideInInspector] public int _lv;
    [HideInInspector] public int _life;
    [HideInInspector] public int _max_life;

    [HideInInspector] public int _energy;
    [HideInInspector] public int _max_energy;

    [HideInInspector] public int _resistance;

    [HideInInspector] public int _atk;
    [HideInInspector] public int _def;

    [HideInInspector] public int _dodge;
    [HideInInspector] public int _luck;

    [HideInInspector] public int _sp;

    // actions
    [HideInInspector] public Attack[] Attacks;
    [HideInInspector] public string[] Duets;
    [HideInInspector] public Gambit[] Gambits;

    [HideInInspector] public Tag[] Tags;
    [HideInInspector] public List<Status> Statuses = new List<Status>();
    [HideInInspector] public Status.SubType[] Immunities;
    [HideInInspector] public List<GameObject> VisualStatusEffects = new List<GameObject>();

    [HideInInspector] public bool HasActed;
    [HideInInspector] public int _index;

    public void Set(int position)
    {
        _lv = CharacterDictionary.GetLv[_name]();
        _sp = CharacterDictionary.GetSP[_name]();

        _life = CharacterDictionary.GetLife[_name]();
        _max_life = CharacterDictionary.GetMaxLife[_name]();

        _energy = CharacterDictionary.GetEnergy[_name]();
        _max_energy = CharacterDictionary.GetMaxEnergy[_name]();

        _resistance = CharacterDictionary.GetResistance[_name]();

        _atk = CharacterDictionary.GetAtk[_name]();
        _def = CharacterDictionary.GetDef[_name]();

        _dodge = CharacterDictionary.GetDodge[_name]();
        _luck = CharacterDictionary.GetLuck[_name]();

        Attacks = new Attack[CharacterDictionary.GetAttacks[_name]().Count];
        CharacterDictionary.GetAttacks[_name]().CopyTo(Attacks);

        Duets = new string[CharacterDictionary.GetDuets[_name]().Count];
        CharacterDictionary.GetDuets[_name]().CopyTo(Duets);

        Gambits = new Gambit[CharacterDictionary.GetGambits[_name]().Count];
        CharacterDictionary.GetGambits[_name]().CopyTo(Gambits);

        Tags = new Tag[CharacterDictionary.GetEquippedTags[_name]().Count];
        CharacterDictionary.GetEquippedTags[_name]().CopyTo(Tags);

        _index = position;
    }

    public int TakeDamage(int damage, bool piercing = false)
    {
        int adjusted_damage = damage;
        if (!piercing)
        {
            adjusted_damage = Mathf.Max(damage - _def, 0);
        }

        if (InStatuses(Status.SubType.poison))
        {
            adjusted_damage *= 2;
        }

        // if the player dies
        if (_life - adjusted_damage <= 0)
        {
            _animator.SetBool("PassOut", true);
            _life = 0;
        }
        else
        {
            _life -= adjusted_damage;
        }
        return adjusted_damage;
    }

    public void GainLife(int by)
    {
        _life = Mathf.Min(_life + by, _max_life);
    }

    public void GainEnergy(int by)
    {
        _energy = Mathf.Min(_energy + by, _max_energy);
    }

    public bool AddUpSP()
    {
        _sp += 1;
        if (_sp == 100)
        {
            _sp = 0;
            return true;
        }
        return false;
    }

    public void MatchLevelUp()
    {
        _lv = CharacterDictionary.GetLv[_name]();

        _life = CharacterDictionary.GetLife[_name]();
        _max_life = CharacterDictionary.GetMaxLife[_name]();

        _energy = CharacterDictionary.GetEnergy[_name]();
        _max_energy = CharacterDictionary.GetMaxEnergy[_name]();
    }

    public void SetOverworldStats()
    {
        CharacterDictionary.SetLife[_name](Mathf.Max(_life, 1));
        CharacterDictionary.SetEnergy[_name](_energy);
        CharacterDictionary.SetSP[_name](_sp);
    }

    public void AddStatus(Status.Type type, Status.SubType sub, int turns = -1, int value = 0, string name = "")
    {
        int index = GetIndexInStatuses(sub);
        int burn_index = GetIndexInStatuses(Status.SubType.burn);
        int inferno_index = GetIndexInStatuses(Status.SubType.inferno);

        // if the character has the same type of status already. Taunt, Regeneration, Restoration, [ATK], and [DEF] stack
        if (type == Status.Type.status && index != -1 && sub != Status.SubType.taunt &&
            sub != Status.SubType.regeneration && sub != Status.SubType.restoration)
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
            // apply other effects and animations
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
            else if (sub == Status.SubType.aura)
            {
                // (TODO) start aura animation
            }
            else if (sub == Status.SubType.counter)
            {
                // (TODO) start counter animation
            }
            else if (sub == Status.SubType.taunt)
            {
                // start taunt animation
                GameObject effect = (GameObject)Instantiate(Resources.Load("Battle/Effects/Status/taunt_effect"), transform);
                effect.name = "taunt_effect";
                VisualStatusEffects.Add(effect);
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
        else if (Statuses[i].sub_type == Status.SubType.aura)
        {
            // (TODO) stop aura animation
        }
        else if (Statuses[i].sub_type == Status.SubType.counter)
        {
            // (TODO) stop counter animation
        }
        else if (Statuses[i].sub_type == Status.SubType.taunt)
        {
            // stop taunt animation
            RemoveVisualEffect("taunt_effect");
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

    public bool InStatuses(Status.SubType sub)
    {
        foreach (Status status in Statuses)
        {
            if (status.sub_type == sub)
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

    public int TauntCount()
    {
        int taunt_count = 0;
        foreach (Status status in Statuses)
        {
            if (status.sub_type == Status.SubType.taunt)
            {
                taunt_count += 1;
            }
        }
        return taunt_count;
    }
}
