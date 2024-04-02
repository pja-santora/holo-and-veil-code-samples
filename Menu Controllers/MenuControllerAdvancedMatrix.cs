using UnityEngine;

public class MenuControllerAdvancedMatrix : MenuControllerScrollable
{
    public int row_width;

    protected override void Update()
    {
        CheckScroll();

        if (tracker == MenuTracker.on)
        {
            // if moving left or right
            if (Command.Left_() || Command.Right_())
            {
                start_time += Time.unscaledDeltaTime;
                if (start_time >= 0.5f)
                {
                    _time += Time.unscaledDeltaTime;
                    if (_time >= 0.1f)
                    {
                        MoveLeftOrRight();
                        SoundEffectSystem.Play(SFX.menu_move_1);
                        _time = 0;
                    }
                }
                // stops continuous movement
                else if (!KeyDown)
                {
                    MoveLeftOrRight();
                    SoundEffectSystem.Play(SFX.menu_move_1);
                    KeyDown = true;
                }
            }
            // if moving up or down
            else if (Command.Up_() || Command.Down_() || scroll_scale < -1 || scroll_scale > 1)
            {
                start_time += Time.unscaledDeltaTime;
                if (start_time >= 0.5f)
                {
                    _time += Time.unscaledDeltaTime;
                    if (_time >= 0.1f)
                    {
                        MoveUpOrDown();
                        SoundEffectSystem.Play(SFX.menu_move_1);
                        _time = 0;
                    }
                }
                else
                {
                    // stops continuous movement
                    if (!KeyDown)
                    {
                        MoveUpOrDown();
                        SoundEffectSystem.Play(SFX.menu_move_1);
                        KeyDown = true;

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

    private void MoveLeftOrRight()
    {
        // if moving right
        if (Command.Right_())
        {
            if (index == max_index)
                index = GetRowStart();
            else if (index < GetRowStart() + row_width - 1)
                index += 1;
            else
                index = GetRowStart();

        }
        // if moving left
        else if (Command.Left_())
        {
            if (index > GetRowStart())
                index -= 1;
            else
            {
                if (GetRowStart() + row_width - 1 > max_index)
                    index = max_index;
                else
                    index = GetRowStart() + row_width - 1;
            }  
        }
    }

    private void MoveUpOrDown()
    {
        // if moving down
        if (Command.Down_() || scroll_scale < -1)
        {
            if (index < GetMaxRow() * row_width)
            {
                if (index + row_width > max_index)
                    index = max_index;
                else
                    index += row_width;
            }
            else
                index -= GetMaxRow() * row_width;

        }
        // if moving up
        else if (Command.Up_() || scroll_scale > 1)
        {
            if (index / row_width > 0)
                index -= row_width;
            else
            {
                if (index + GetMaxRow() * row_width > max_index)
                    index = max_index;
                else
                    index += GetMaxRow() * row_width;
            }
        }
    }

    private int GetRowStart()
    {
        return index / row_width * row_width;
    }

    public int GetMaxRow()
    {
        return max_index / row_width;
    }
}
