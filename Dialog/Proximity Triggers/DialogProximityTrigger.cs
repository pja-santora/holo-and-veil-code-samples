// Proximity Trigger for Dialog Events
// Attached to many NPCs within the game.

// Same functionality as ProximityTrigger (the base class), but communicates with other classes 
// like DialogSystem while dialog is happening, or ShopMenu while the player is on a shop menu.

using System.Collections;
using UnityEngine;

public class DialogProximityTrigger : ProximityTrigger
{
    public GameObject new_dialog_icon;
    public DialogSystem dialog_system;
    public string _name;

    protected int _index;
    private float base_rotation;

    protected override void Start()
    {
        base.Start();
        CheckForNewDialog();

        base_rotation = transform.rotation.y;
    }

    protected override void TriggerEvent()
    {
        base.TriggerEvent();
        AdjustSpriteRotation();
        StartCoroutine(Conversation());
    }

    protected virtual IEnumerator Conversation()
    {
        _index = DialogTracker.Database[_name];
        BeforeDialog();

        int adjusted_index = Mathf.Min(_index, NPCDialog.Dialogs[_name].Count - 1);

        dialog_system.EnableNPCDialog(_name, adjusted_index, transform.position.x);
        while (dialog_system.InConversation) { yield return null; }

        AfterDialog();
        while (ShopMenuUI.On) { yield return null; }

        AfterShop();
        while (dialog_system.InConversation) { yield return null; }

        DialogTracker.Database[_name] = adjusted_index + 1;
        _icon.SetActive(true);
    }

    protected virtual void BeforeDialog() { }

    protected virtual void AfterDialog() { }

    protected virtual void AfterShop() { }

    protected override void Enter()
    {
        base.Enter();
        new_dialog_icon.SetActive(false);
    }

    protected override void Exit()
    {
        base.Exit();
        CheckForNewDialog();
        ResetSpriteRotation();
    }

    protected bool NPCHasNewDialog()
    {
        return DialogTracker.Database[_name] < NPCDialog.Dialogs[_name].Count;
    }

    protected bool DuringThePrelude()
    {
        return GameState.GAME_STATE == GameState.State.prelude_1 || GameState.GAME_STATE == GameState.State.prelude_2;
    }

    private void CheckForNewDialog()
    {
        if (NPCHasNewDialog())
        {
            new_dialog_icon.SetActive(true);
        }
    }

    private void AdjustSpriteRotation() 
    {
        if (player_movement.transform.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (player_movement.transform.position.x < transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void ResetSpriteRotation()
    {
        transform.rotation = Quaternion.Euler(0, base_rotation * 180f, 0);
    }
}
