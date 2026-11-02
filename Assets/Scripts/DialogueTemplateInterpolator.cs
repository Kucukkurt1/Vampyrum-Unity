using System.Collections.Generic;

public static class DialogueTemplateInterpolator
{
    public static string Apply(string template, Dictionary<string, string> slots)
    {
        if (string.IsNullOrEmpty(template) || slots == null)
            return template ?? string.Empty;

        string result = template;
        foreach (KeyValuePair<string, string> slot in slots)
            result = result.Replace("{" + slot.Key + "}", slot.Value ?? string.Empty);

        return result;
    }

    public static Dictionary<string, string> BuildWitnessSlots(
        WitnessStatementRecord witness,
        NightMurderRecord murder,
        GuestProfile distortSubject = null)
    {
        var slots = new Dictionary<string, string>
        {
            ["victim_name"] = murder?.VictimName ?? "bilinmeyen",
            ["victim_room"] = murder?.VictimRoom ?? witness?.VictimRoom ?? "???",
            ["subject_name"] = distortSubject != null
                ? distortSubject.DisplayName
                : witness?.SubjectName ?? "bir yolcu"
        };

        return slots;
    }
}
