// Constructs Dialog (a conversation) using a series (or array) of Proses.

using System;
using UnityEngine;

[Serializable]
public class Dialog
{
    public Prose[] Proses;

    [HideInInspector] public int index = 0;

    [HideInInspector] public GameState.State remove_state;

    public Dialog(Prose[] proses, GameState.State remove = GameState.State._start)
    {
        Proses = new Prose[proses.Length];
        for (int i = 0; i < proses.Length; ++i)
        {
            Proses[i] = new Prose(proses[i]);
        }
        remove_state = remove;
    }

    public Dialog(string[] description)
    {
        Proses = new Prose[description.Length];
        for (int i = 0; i < description.Length; ++i)
        {
            Proses[i] = new Prose(description[i]);
        }
    }

    public void Set(Dialog dialog)
    {
        Proses = new Prose[dialog.Proses.Length];
        for (int i = 0; i < dialog.Proses.Length; ++i)
        {
            Proses[i] = new Prose(dialog.Proses[i]);
        }
        remove_state = dialog.remove_state;
    }

    public void SetEmpty()
    {
        Proses = new Prose[0];
    }
}
