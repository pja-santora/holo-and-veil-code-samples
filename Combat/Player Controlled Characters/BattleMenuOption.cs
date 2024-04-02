// BattleMenuOption is used to represent each option availible in a character's battle menu.
// Stores relevant information used during combat.

// SetCanSelect() checks to make sure an action present in a character's BattleMenu is useable
// based on the character's energy (like MP, certain actions cost energy to use) and 
// the quality of the enemies present (for example, some attacks can only target "grounded" enemies. See Enemy.cs)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuOption : MonoBehaviour
{
    public Text _name;
    public Text _cost;
    public Animator _animator;

    public GameObject _e;

    // whether the player can select this option during battle
    [HideInInspector] public bool CanSelect;

    [HideInInspector] public Action.Side _side;
    [HideInInspector] public Action.Target _target;

    [HideInInspector] public int action_cost; 

    public void Set(string name, int cost, Action.Side side, Action.Target target)
    {
        _name.text = name;

        if (cost > 0)
        {
            _cost.text = "" + cost;
        }
        else
        {
            _e.SetActive(false);
            _cost.text = "";
        }
        action_cost = cost;

        _side = side;
        _target = target;
    }

    public void Check()
    {
        if (CanSelect && _animator.isActiveAndEnabled)
        {
            _animator.SetBool("Greyed", false);
        }
        else if (!CanSelect && _animator.isActiveAndEnabled)
        {
            _animator.SetBool("Greyed", true);
        }
    }

    public virtual void SetCanSelect(int energy, List<Enemy> Enemies)
    {
        if (energy < action_cost)
        {
            CanSelect = false;
            return;
        }

        if (_side == Action.Side.enemy)
        {
            bool all_invisible = true;
            foreach (Enemy enemy in Enemies)
            {
                if (!enemy.InStatuses(Status.SubType.invisible))
                {
                    all_invisible = false;
                    break;
                }
            }

            if (all_invisible)
            {
                CanSelect = false;
                return;
            }

            int e = 0;
            if (_target == Action.Target.first_non_ceiling || _target == Action.Target.any_non_ceiling || _target == Action.Target.all_non_ceiling)
            {
                // need to have at least 1 non-ceiling
                foreach (Enemy enemy in Enemies)
                {
                    if (enemy._quality != Enemy.Quality.ceiling && !enemy.InStatuses(Status.SubType.invisible))
                    {
                        e += 1;
                        break;
                    }
                }

                if (e == 0)
                {
                    CanSelect = false;
                    return;
                }
            }
            else if (_target == Action.Target.all_non_air)
            {
                // need to have at least 1 non-air
                foreach (Enemy enemy in Enemies)
                {
                    if (enemy._quality != Enemy.Quality.air && !enemy.InStatuses(Status.SubType.invisible))
                    {
                        e += 1;
                        break;
                    }
                }

                if (e == 0)
                {
                    CanSelect = false;
                    return;
                }
            }
        }

        if (_name.text.Equals("Run") && BattleTransition.battle_enemy_name[0] != 'c')
        {
            CanSelect = false;
            return;
        }

        CanSelect = true;
    }
}
