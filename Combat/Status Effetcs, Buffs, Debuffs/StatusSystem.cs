using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    public GameObject[] character_indicator_positions;
    public GameObject[] enemy_indicator_positions;

    [HideInInspector] public List<List<StatusIndicator>> CharacterStatusIndicators = new List<List<StatusIndicator>>();
    [HideInInspector] public List<List<StatusIndicator>> EnemyStatusIndicators = new List<List<StatusIndicator>>();

    [HideInInspector] public List<int> EnemyPositionReferences = new List<int>();

    private readonly int[] _CONTAINERS = new int[2] { 647, -647 };
    private const int _OFFSET = 180;

    public void Set(int character_count, int enemy_count)
    {
        for (int i = 0; i < character_count; ++i)
        {
            CharacterStatusIndicators.Add(new List<StatusIndicator>());
        }

        for (int i = 0; i < enemy_count; ++i)
        {
            EnemyStatusIndicators.Add(new List<StatusIndicator>());
            EnemyPositionReferences.Add(i);
        }
    }

    public void AddCharacterStatusIndicator(int c, Status.Type type, Status.SubType sub, int turns = -1, string name = "")
    {
        int index = InCharacterStatus(c, sub);
        int burn_index = InCharacterStatus(c, Status.SubType.burn);
        int inferno_index = InCharacterStatus(c, Status.SubType.inferno);

        // if the character has the same type of status already. Taunt, Regeneration, Restoration, [ATK], and [DEF] stack
        if (type == Status.Type.status && index != -1 && sub != Status.SubType.taunt && 
            sub != Status.SubType.regeneration && sub != Status.SubType.restoration)
        {
            // if the new status being applied has a higher turn count, replace the old status
            if (turns > CharacterStatusIndicators[c][index].turn_count && CharacterStatusIndicators[c][index].turn_count != -1)
            {
                CharacterStatusIndicators[c][index].turn_count = turns;
                if (turns == -1)
                {
                    CharacterStatusIndicators[c][index].HideTurns();
                }
                else
                {
                    CharacterStatusIndicators[c][index]._turns.text = "" + turns;
                }
            }
        }
        // if the character has Inferno and the applied status is Burn
        else if (sub == Status.SubType.burn && inferno_index != -1) { }
        else
        {
            // if the character has Burn and the applied status is Inferno
            if (sub == Status.SubType.inferno && burn_index != -1)
            {
                RemoveCharacterIndicator(c, burn_index);
            }

            // instantiate
            GameObject indicator_prefab = (GameObject)Instantiate(Resources.Load("Battle/UI/buff_indicator"),
                character_indicator_positions[c].transform);

            // set
            StatusIndicator buff_indicator = indicator_prefab.GetComponent<StatusIndicator>();
            buff_indicator.Set(type, sub, turns, name);

            CharacterStatusIndicators[c].Add(buff_indicator);
            SortCharacterList(c);
            AdjustCharacterListPositions(c);
        }
    }

    public void AddCharacterStatusIndicator(int c, Status status)
    {
        AddCharacterStatusIndicator(c, status._type, status.sub_type, status.turn_count, status._name);
    }

    public void AddEnemyStatusIndicator(int e, Status.Type type, Status.SubType sub, int turns, string name = "")
    {
        int index = InEnemyStatus(e, sub);
        int burn_index = InEnemyStatus(e, Status.SubType.burn);
        int inferno_index = InEnemyStatus(e, Status.SubType.inferno);

        // if the enemy has the same type of status already. Taunt, Regeneration, [ATK], and [DEF] stack
        if (type == Status.Type.status && index != -1 && sub != Status.SubType.taunt &&
            sub != Status.SubType.regeneration)
        {
            // if the new status being applied has a higher turn count, replace the old status
            if (turns > EnemyStatusIndicators[e][index].turn_count && EnemyStatusIndicators[e][index].turn_count != -1)
            {
                EnemyStatusIndicators[e][index].turn_count = turns;
                if (turns == -1)
                {
                    EnemyStatusIndicators[e][index].HideTurns();
                }
                else
                {
                    EnemyStatusIndicators[e][index]._turns.text = "" + turns;
                }
            }
        }
        // if the enemy has Inferno and the applied status is Burn
        else if (sub == Status.SubType.burn && inferno_index != -1) { }
        else
        {
            // if the enemy has Burn and the applied status is Inferno
            if (sub == Status.SubType.inferno && burn_index != -1)
            {
                RemoveEnemyIndicator(e, burn_index);
            }

            // instantiate
            GameObject indicator_prefab = (GameObject)Instantiate(Resources.Load("Battle/UI/buff_indicator"),
                enemy_indicator_positions[EnemyPositionReferences[e]].transform);

            // set
            StatusIndicator buff_indicator = indicator_prefab.GetComponent<StatusIndicator>();
            buff_indicator.Set(type, sub, turns, name);

            EnemyStatusIndicators[e].Add(buff_indicator);
            SortEnemyList(e);
            AdjustEnemyListPositions(e);
        }
    }

    public void AddEnemyStatusIndicator(int e, Status status)
    {
        AddEnemyStatusIndicator(e, status._type, status.sub_type, status.turn_count, status._name);
    }

    public void LowerCharacterIndicatorTurns(int c)
    {
        for (int i = 0; i < CharacterStatusIndicators[c].Count; ++i)
        {
            if (CharacterStatusIndicators[c][i].turn_count > 0)
            {
                CharacterStatusIndicators[c][i].LowerTurnCount();
                if (CharacterStatusIndicators[c][i].turn_count == 0)
                {
                    RemoveCharacterIndicator(c, i);
                    i -= 1;
                }
            }
        }
    }

    public void LowerEnemyIndicatorTurns(int e)
    {
        for (int i = 0; i < EnemyStatusIndicators[e].Count; ++i)
        {
            if (EnemyStatusIndicators[e][i].turn_count > 0)
            {
                EnemyStatusIndicators[e][i].LowerTurnCount();
                if (EnemyStatusIndicators[e][i].turn_count == 0)
                {
                    RemoveEnemyIndicator(e, i);
                    i -= 1;
                }
            }
        }
    }

    public void RemoveCharacterIndicator(int c, int index)
    {
        Destroy(CharacterStatusIndicators[c][index].gameObject);
        CharacterStatusIndicators[c].RemoveAt(index);
        AdjustCharacterListPositions(c);
    }

    public void RemoveCharacterIndicator(int c, string name)
    {
        for (int i = 0; i < CharacterStatusIndicators[c].Count; ++i)
        {
            if (CharacterStatusIndicators[c][i]._name.Equals(name))
            {
                RemoveCharacterIndicator(c, i);
                return;
            }
        }
    }

    public void ClearCharacterStatus(int c)
    {
        for (int i = CharacterStatusIndicators[c].Count - 1; i >= 0; --i)
        {
            Destroy(CharacterStatusIndicators[c][i].gameObject);
            CharacterStatusIndicators[c].RemoveAt(i);
        }
    }

    public void RemoveEnemyIndicator(int e, int index)
    {
        Destroy(EnemyStatusIndicators[e][index].gameObject);
        EnemyStatusIndicators[e].RemoveAt(index);
        AdjustEnemyListPositions(e);
    }

    public void RemoveEnemyIndicator(int e, string name)
    {
        for (int i = 0; i < EnemyStatusIndicators[e].Count; ++i)
        {
            if (EnemyStatusIndicators[e][i]._name.Equals(name))
            {
                RemoveEnemyIndicator(e, i);
                return;
            }
        }
    }

    public void ClearEnemyStatus(int e)
    {
        for (int i = EnemyStatusIndicators[e].Count - 1; i >= 0; --i)
        {
            Destroy(EnemyStatusIndicators[e][i].gameObject);
            EnemyStatusIndicators[e].RemoveAt(i);
        }
    }

    public void SortCharacterList(int c)
    {
        List<StatusIndicator> sorted_list = CharacterStatusIndicators[c].OrderBy(x => x._type).ThenBy(y => y.sub_type).ThenBy(z => z.turn_count).ToList();
        CharacterStatusIndicators[c] = new List<StatusIndicator>(sorted_list);
    }

    public void SortEnemyList(int e)
    {
        List<StatusIndicator> sorted_list = EnemyStatusIndicators[e].OrderBy(x => x._type).ThenBy(y => y.sub_type).ThenBy(z => z.turn_count).ToList();
        EnemyStatusIndicators[e] = new List<StatusIndicator>(sorted_list);
    }

    public void SwapCharacterListPositions(int c)
    {
        for (int i = 0; i < CharacterStatusIndicators[c].Count; ++i)
        {
            CharacterStatusIndicators[c][i].transform.localPosition = new Vector2(_CONTAINERS[(c + 1) % 2], _OFFSET * i);
        }
    }

    public void AdjustCharacterListPositions(int c)
    {
        for (int i = 0; i < CharacterStatusIndicators[c].Count; ++i)
        {
            CharacterStatusIndicators[c][i].transform.localPosition = new Vector2(0, _OFFSET * i);
        }
    }

    public void AdjustEnemyListPositions(int e)
    {
        for (int i = 0; i < EnemyStatusIndicators[e].Count; ++i)
        {
            EnemyStatusIndicators[e][i].transform.localPosition = new Vector2(0, _OFFSET * i);
        }
    }

    private int InCharacterStatus(int c, Status.SubType sub)
    {
        for (int i = 0; i < CharacterStatusIndicators[c].Count; ++i)
        {
            if (CharacterStatusIndicators[c][i].sub_type == sub)
            {
                return i;
            }
        }
        return -1;
    }

    private int InEnemyStatus(int e, Status.SubType sub)
    {
        for (int i = 0; i < EnemyStatusIndicators[e].Count; ++i)
        {
            if (EnemyStatusIndicators[e][i].sub_type == sub)
            {
                return i;
            }
        }
        return -1;
    }
}
