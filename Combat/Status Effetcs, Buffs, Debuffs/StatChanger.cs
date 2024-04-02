using UnityEngine;
using UnityEngine.UI;

public class StatChanger : MonoBehaviour
{
    public Text _amount;
    public Image _icon;
    public Animator _animator;
    public GameObject _effects;

    public enum Type { life, energy, buff, debuff }
    [HideInInspector] public Type _type;

    public enum SubType { atk, def }
    [HideInInspector] public SubType _subtype;

    public void Set(int amount, Type type, SubType sub = SubType.def, Enemy.Quality enemy_state = Enemy.Quality.ground)
    {
        _type = type;
        _subtype = sub;

        GameObject tag_prefab;

        // set icons
        if (_type == Type.life)
        {
            tag_prefab = (GameObject)Resources.Load("Battle/UI/Stat Change/" + "life_icon");
            _amount.color = Color.white;
        }
        else if (_type == Type.energy)
        {
            tag_prefab = (GameObject)Resources.Load("Battle/UI/Stat Change/" + "energy_icon");
            _amount.color = Color.white;
        }
        else
        {
            if (_subtype == SubType.atk)
            {
                tag_prefab = (GameObject)Resources.Load("Battle/UI/Status/" + "atk_status");
            }
            else
            {
                tag_prefab = (GameObject)Resources.Load("Battle/UI/Status/" + "def_status");
            }
        }
        _icon.sprite = tag_prefab.GetComponent<Image>().sprite;
        _icon.SetNativeSize();

        PlayEffect(type);

        // set position
        if (enemy_state == Enemy.Quality.ceiling)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + 1296.0f);
        }
        if (enemy_state == Enemy.Quality.air)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + 648.0f);
        }
        else
        {
            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y);
        }

        // set amount
        if (amount > 0)
        {
            _amount.text = "+" + amount;
            _animator.SetBool("Increase", true);
        }
        else
        {
            _amount.color = Color.grey;
            _amount.text = "" + amount;
            _animator.SetBool("Decrease", true);
        }
    }

    private void PlayEffect(Type type)
    {
        string prefab_name;

        if (type == Type.life)
        {
            prefab_name = "life_circle";
        }
        else if (type == Type.energy)
        {
            prefab_name = "energy_circle";
        }
        else if (type == Type.buff)
        {
            prefab_name = "up_arrow";
        }
        else
        {
            prefab_name = "down_arrow";
        }

        Instantiate(Resources.Load("Battle/UI/Stat Change/" + prefab_name), _effects.transform);
    }
}
