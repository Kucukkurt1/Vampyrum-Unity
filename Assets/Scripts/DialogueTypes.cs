using System;

public enum WitnessType
{
    None,
    HeardNoise,
    SawPersonLeave
}

public enum DialogueOutcome
{
    Truth,
    Lie,
    Distorted,
    Refused
}

public enum GuestReliabilityTier
{
    Normal,
    Unreliable
}

[Serializable]
public class DialogueLine
{
    public string speaker = "npc";
    public string text;
}

[Serializable]
public class DialogueBeatConditions
{
    public int min_kalinan_gun = -1;
    public int max_kalinan_gun = -1;
    public int min_talk_count = -1;
    public int max_talk_count = -1;
    public string requires_flag;
}

[Serializable]
public class DialogueBeat
{
    public string id;
    public bool once = true;
    public int priority;
    public DialogueBeatConditions conditions;
    public DialogueLine[] lines;
}

[Serializable]
public class DialogueFallback
{
    public string[] pool;
}

[Serializable]
public class CharacterDialogueFile
{
    public int karakter_id;
    public string dosya;
    public string tur;
    public DialogueBeat[] beats;
    public DialogueFallback fallback;
}

[Serializable]
public class CharacterDialogueMeta
{
    public int karakter_id;
    public string dosya;
    public string guvenilirlik = "normal";
}

[Serializable]
public class DialogueTemplateDefinition
{
    public string id;
    public string template;
    public string[] slots;
    public DialogueTemplateConditions conditions;
    public int priority;
}

[Serializable]
public class DialogueTemplateConditions
{
    public string witness_type;
    public string outcome;
}

[Serializable]
public class GenericSmalltalkFile
{
    public GenericSmalltalkEntry[] entries;
}

[Serializable]
public class GenericSmalltalkEntry
{
    public string id;
    public string text;
}

[Serializable]
public class WitnessStatementRecord
{
    public string WitnessGuestId;
    public WitnessType Type;
    public string SubjectGuestId;
    public string SubjectName;
    public string VictimRoom;
    public bool PendingPlayerTalk;
    public DialogueOutcome DeliveredOutcome;
}

[Serializable]
public class NightMurderRecord
{
    public int Night;
    public string VictimGuestId;
    public string VictimName;
    public string VictimRoom;
    public string AttackerGuestId;
    public WitnessStatementRecord[] Witnesses;
}

public class DialogueResolution
{
    public string BeatOrTemplateId;
    public DialogueOutcome Outcome;
    public string[] Lines;
    public bool ConversationEnded;
}
