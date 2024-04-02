// Base Class for most Menu Controllers present within UIs.
// Checks for Commands/Input from the player and adjusted the menu item/option the player is selecting.

// Communicates with many Menu architechtures and UIs within the game.

using UnityEngine;

public enum MenuTracker
{
    on,
    off,
}

public class MenuControllerAdvanced : MonoBehaviour
{
    // indexes
    public int index;
    public int max_index;

    // if the player is pressing a button
    protected bool KeyDown;

    // whether the controller is on or off
    public MenuTracker tracker;

    protected virtual void Update()
    {
        if (tracker == MenuTracker.on)
        {
            // if moving up or down
            if (Command.Up_() || Command.Down_())
            {
                // stops continuous movement
                if (!KeyDown)
                {
                    // if moving down
                    if (Command.Down_())
                    {
                        if (index < max_index)
                            index += 1;
                        else
                            index = 0;

                    }
                    // if moving up
                    else if (Command.Up_())
                    {
                        if (index > 0)
                            index -= 1;
                        else
                            index = max_index;
                    }
                    KeyDown = true;
                    SoundEffectSystem.Play(SFX.menu_move_1);
                }
            }
            else
                KeyDown = false;
        }
    }
}
