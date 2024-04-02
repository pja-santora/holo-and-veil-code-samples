// DialogSystem handles dialog events while dialog is happening.
// DialogSystem sets UI based on the current Prose (screen) the player is viewing within a Dialog (conversation) sequence.

// Update() waits for player input, then moves forward or backward within a Dialog sequence.

// Some Dialog sequences have special/hidden text that will only appear visually in the Dialog sequence if 
// the required characters are present in the player's party. (See AdjustIndexByRequirements(), AllRequiredCharactersAreInParty())

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSystem : MonoBehaviour
{
    public static bool On = false;

    public DialogUI dialog_ui;

    [HideInInspector] public Dialog _dialog;
    [HideInInspector] public bool InConversation;

    private enum Type { cutscene, npc, conversation }
    private Type _type;

    private PlayerPlatformerController player_movement;

    private string _name;
    private bool OnLeft;

    private void Awake()
    {
        player_movement = GameObject.Find(PlayerPlatformerController.player_collider).GetComponent<PlayerPlatformerController>();

        enabled = false;
        On = false;
    }

    public void EnableCutsceneDialog(string cutscene, int index = 0)
    {
        _type = Type.cutscene;
        _dialog.Set(CutsceneDialog.Database[cutscene][index]);
        StartCoroutine(EnableDialog(offset: 0));
    }

    public void EnableNPCDialog(string name, int index, float npc_Xposition)
    {
        _type = Type.npc;
        _dialog.Set(NPCDialog.Dialogs[name][index]);
        StartCoroutine(EnableDialog(name, npc_Xposition: npc_Xposition));
    }

    public void EnableSpecialDialog(string name, int index = 0, float offset = 1.5f, float npc_Xposition = 0)
    {
        _type = Type.npc;
        _dialog.Set(NPCDialog.Special[name][index]);
        StartCoroutine(EnableDialog(name, offset, npc_Xposition));
    }

    public void EnableConversationDialog(string pair1, string pair2, char lv)
    {
        _type = Type.conversation;
        _dialog.Set(ConversationDialog.Database[SupportSystem.Order(pair1, pair2) + " : " + lv]);
        StartCoroutine(EnableDialog(offset: 0));
    }

    public IEnumerator EnableDialog(string name = "", float offset = 1.5f, float npc_Xposition = 0f)
    {
        On = true;
        InConversation = true;
        dialog_ui.NotTyping = false;

        offset = Mathf.Abs(offset);
        float player_Xposition = player_movement.GetComponent<Transform>().position.x;
        PlayerPlatformerController.Halt();
        _name = name;

        // move player into position
        if (offset > 0)
        {
            if (player_Xposition > npc_Xposition)
            {
                if (!PlayerPlatformerController.FacingRight)
                {
                    player_movement.Flip();
                }

                for (float i = player_Xposition; i <= npc_Xposition + offset; i += 0.1f)
                {
                    player_movement.GetComponent<Transform>().position =
                        new Vector2(i, player_movement.GetComponent<Transform>().position.y);
                    yield return null;
                }

                player_movement.Flip(); 
                OnLeft = false;
            }
            else
            {
                if (PlayerPlatformerController.FacingRight)
                {
                    player_movement.Flip();
                }

                for (float i = player_Xposition; i >= npc_Xposition - offset; i -= 0.1f)
                {
                    player_movement.GetComponent<Transform>().position =
                        new Vector2(i, player_movement.GetComponent<Transform>().position.y);
                    yield return null;
                }

                player_movement.Flip();
                OnLeft = true;
            }
        }
        dialog_ui.Show();

        _dialog.index = 0;
        AdjustIndexByRequirements();
        SetDialog();
        enabled = true;
    }

    private void Update()
    {
        if (!PauseMenuToggle.On)
        {
            if (dialog_ui.NotTyping && _dialog.index < _dialog.Proses.Length - 1)
            {
                if (Command.Select())
                {
                    _dialog.index += 1;
                    if (AdjustIndexByRequirements()) return;

                    dialog_ui._passage.color = Color.white;
                    SetDialog();
                    SoundEffectSystem.Play(SFX.forward_dialog);
                }
                else if (Command.Back())
                {
                    if (_dialog.index != 0)
                    {
                        _dialog.index -= 1;
                        if (AdjustIndexByRequirements(false)) return;
                    }

                    dialog_ui._passage.color = Color.grey;
                    SetDialog();
                    SoundEffectSystem.Play(SFX.backward_dialog);
                }
            }
            else if (dialog_ui.NotTyping)
            {
                if (Command.Back())
                {
                    if (_dialog.index != 0)
                    {
                        _dialog.index -= 1;
                        if (AdjustIndexByRequirements(false)) return;
                    }

                    dialog_ui._passage.color = Color.grey;
                    SetDialog();
                    SoundEffectSystem.Play(SFX.backward_dialog);
                }
                else if (Command.Select())
                {
                    StartCoroutine(EndDialog());
                }
            }
            else
            {
                if (Command.Back() && dialog_ui._passage.color == Color.white)
                {
                    dialog_ui.StopAllCoroutines();
                    if (_dialog.index != 0)
                    {
                        _dialog.index -= 1;
                        if (AdjustIndexByRequirements(false)) return;
                    }

                    dialog_ui._passage.color = Color.grey;
                    SetDialog();
                    SoundEffectSystem.Play(SFX.backward_dialog);
                }
            }
        }

        if (CutsceneSkip.Skipping)
        {
            StartCoroutine(EndDialog());
        }
    }

    private bool AdjustIndexByRequirements(bool forward = true)
    {
        if (forward)
        {
            int index = _dialog.index;
            while (true)
            {
                // increase index until requirements are met or end of array is reached
                if (AllRequiredCharactersAreInParty(index))
                {
                    _dialog.index = index;
                    return false;
                }
                else
                {
                    index += 1;
                }

                // if end of array is reached
                if (index > _dialog.Proses.Length - 1)
                {
                    _dialog.index -= 1;
                    StartCoroutine(EndDialog());
                    return true;
                }
            }
        }
        else
        {
            int index = _dialog.index;
            while (true)
            {
                // decrease index until requirements are met or beginning of array is reached
                if (AllRequiredCharactersAreInParty(index))
                {
                    _dialog.index = index;
                    return false;
                }
                else
                {
                    index -= 1;
                }

                // if beginning of aray is reached
                if (index < 0)
                {
                    _dialog.index += 1;
                    return true;
                }
            }
        }
    }

    private bool AllRequiredCharactersAreInParty(int index)
    {
        for (int i = 0; i < _dialog.Proses[index].RequiredCharacters.Length; ++i)
        {
            if (!Player.InParty(_dialog.Proses[index].RequiredCharacters[i]))
            {
                return false;
            }
        }
        return true;
    }

    private void SetDialog()
    {
        if (_type == Type.npc)
        {
            dialog_ui.Set(_dialog.Proses[_dialog.index]._character, _dialog.Proses[_dialog.index]._line,
                GetSide(), _dialog.Proses[_dialog.index]._pose);
        }
        else // Other
        {
            dialog_ui.Set(_dialog.Proses[_dialog.index]._character, _dialog.Proses[_dialog.index]._line,
                _dialog.Proses[_dialog.index]._side, _dialog.Proses[_dialog.index]._pose);
        }
    }

    private Prose.Side GetSide()
    {
        if (OnLeft)
        {
            if (_name == _dialog.Proses[_dialog.index]._character)
            {
                return Prose.Side.right;
            }
            return Prose.Side.left;
        }
        else
        {
            if (_name == _dialog.Proses[_dialog.index]._character)
            {
                return Prose.Side.left;
            }
            return Prose.Side.right;
        }
    }

    private IEnumerator EndDialog()
    {
        SoundEffectSystem.Play(SFX.forward_dialog);

        enabled = false;
        dialog_ui.NotTyping = false;

        StartCoroutine(dialog_ui.Hide());
        yield return new WaitForSecondsRealtime(0.3f);

        InConversation = false;
        On = false;
    }
}
