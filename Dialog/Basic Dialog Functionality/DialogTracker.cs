using System.Collections.Generic;

public class DialogTracker
{ 
    public static Dictionary<string, int> Database = new Dictionary<string, int> {
        { "Gemini", 0 },
        { "Jemyni", 0 },

        { "Lucian", 0 },
        { "Donna", 0 },
        { "Sinch", 0 },

        { "Nina", 0 },

        { "Saul",  0 },
        { "Tobello", 0 },
        { "Vaylo", 0 },
        { "Taylo", 0 },
    };

    public static void LowerFreeDialogIndex(string npc, int read_conversations_removed)
    {
        Database[npc] -= read_conversations_removed;
    }
}
