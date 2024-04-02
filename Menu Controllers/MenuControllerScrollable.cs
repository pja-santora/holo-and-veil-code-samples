using UnityEngine;

public class MenuControllerScrollable : MenuControllerAdvanced
{
    protected float _time = 0;
    protected float start_time = 0;

    protected int scroll_scale = 0;

    protected override void Update()
    {
        CheckScroll();

        if (tracker == MenuTracker.on)
        {
            // if moving up or down
            if (Command.Up_() || Command.Down_() || scroll_scale < -1 || scroll_scale > 1)
            {
                start_time += Time.unscaledDeltaTime;

                if (start_time >= 0.5f)
                {
                    _time += Time.unscaledDeltaTime;
                    if (_time >= 0.1f)
                    {
                        // if moving down
                        if (Command.Down_())
                        {
                            if (index < max_index)
                                index += 1;
                            else
                                index = 0;

                        }// if moving up
                        else if (Command.Up_())
                        {
                            if (index > 0)
                                index -= 1;
                            else
                                index = max_index;
                        }
                        SoundEffectSystem.Play(SFX.menu_move_1);
                        _time = 0;
                    }
                }
                else
                {
                    // stops continuous movement
                    if (!KeyDown)
                    {
                        // if moving down
                        if (Command.Down_() || scroll_scale < -1)
                        {
                            if (index < max_index)
                                index += 1;
                            else
                                index = 0;

                        }// if moving up
                        else if (Command.Up_() || scroll_scale > 1)
                        {
                            if (index > 0)
                                index -= 1;
                            else
                                index = max_index;
                        }
                        KeyDown = true;
                        SoundEffectSystem.Play(SFX.menu_move_1);

                        scroll_scale = 0;
                    }
                }
            }
            else
            {
                KeyDown = false;
                start_time = 0;
            }
                
        }
    }

    protected void CheckScroll()
    {
        if (Command.ScrollUp())
        {
            if (scroll_scale < 0)
            {
                scroll_scale = 0;
            }
            scroll_scale += 1;
        }
        else if (Command.ScrollDown())
        {
            if (scroll_scale > 0)
            {
                scroll_scale = 0;
            }
            scroll_scale -= 1;
        }
    }
}