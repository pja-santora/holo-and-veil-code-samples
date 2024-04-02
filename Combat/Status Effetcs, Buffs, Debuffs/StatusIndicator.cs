// Visual representation of status effects during combat
// Instantiated and set when a status effect is applied to an actor (character or enemy)

using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour
{   
    public GameObject _buff;
    public GameObject _debuff;

    public Image _icon;
    public Text _turns;

    public GameObject _background;
    public GameObject _background_turns;

    [HideInInspector] public Status.Type _type;
    [HideInInspector] public Status.SubType sub_type;
    [HideInInspector] public int turn_count;
    [HideInInspector] public string _name;

    public void Set(Status.Type type, Status.SubType sub, int turns = -1, string name = "")
    {
        _background.SetActive(true);

        if (type == Status.Type.buff)
        {
            _buff.SetActive(true);
        }
        else if (type == Status.Type.debuff)
        {
            _debuff.SetActive(true);
        }

        var buff_prefab = (GameObject)Resources.Load("Battle/UI/Status/" + sub.ToString() + "_status");
        _icon.sprite = buff_prefab.GetComponent<Image>().sprite;
        _icon.SetNativeSize();
        
        if (turns > 0)
        {
            _background_turns.SetActive(true);
            _turns.text = "" + turns;
        }
        else
        {
            _background_turns.SetActive(false);
            _turns.text = "";
        }

        _type = type;
        sub_type = sub;
        turn_count = turns;
        _name = name;
    }

    public void LowerTurnCount()
    {
        turn_count -= 1;
        _turns.text = "" + turn_count;
    }

    public void HideTurns()
    {
        _background_turns.SetActive(false);
        _turns.text = "";
    }
}
