using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DialogueResolver
{
    public static DialogueResolution Resolve(GuestProfile guest)
    {
        if (guest == null || !guest.IsAlive || !guest.IsAccepted)
        {
            return new DialogueResolution
            {
                BeatOrTemplateId = "invalid",
                Outcome = DialogueOutcome.Refused,
                Lines = new[] { "Bu kisi su an konusacak durumda degil." },
                ConversationEnded = true
            };
        }

        NpcCharacterDefinition template = NpcCatalog.GetById(guest.CatalogId);
        GuestReliabilityTier tier = template != null
            ? NpcDialogueCatalog.GetReliabilityTier(template)
            : GuestReliabilityTier.Normal;

        WitnessStatementRecord pendingWitness = FactJournal.Instance?.GetPendingWitnessStatement(guest.Id);
        if (pendingWitness != null)
            return ResolveWitnessStatement(guest, template, tier, pendingWitness);

        CharacterDialogueFile dialogue = template != null
            ? NpcDialogueCatalog.GetCharacterDialogue(template, guest.IsVampire)
            : null;

        DialogueBeat beat = PickBeat(guest, dialogue);
        if (beat != null)
        {
            FactJournal.Instance?.MarkBeatPlayed(guest.Id, beat.id);
            if (!string.IsNullOrEmpty(beat.id))
                FactJournal.Instance?.SetFlag($"beat_{guest.CatalogId}_{beat.id}");

            string[] lines = beat.lines?
                .Where(line => line != null && !string.IsNullOrEmpty(line.text))
                .Select(line => line.text)
                .ToArray() ?? Array.Empty<string>();

            FactJournal.Instance?.IncrementTalkCount(guest.Id);
            return new DialogueResolution
            {
                BeatOrTemplateId = beat.id,
                Outcome = DialogueOutcome.Truth,
                Lines = lines.Length > 0 ? lines : new[] { "..." },
                ConversationEnded = false
            };
        }

        GenericSmalltalkEntry smalltalk = NpcDialogueCatalog.GetRandomSmalltalk();
        FactJournal.Instance?.IncrementTalkCount(guest.Id);
        return new DialogueResolution
        {
            BeatOrTemplateId = smalltalk?.id ?? "fallback",
            Outcome = DialogueOutcome.Truth,
            Lines = new[] { smalltalk?.text ?? "Bugun hava sakin. Kervansarayiniz guzel." },
            ConversationEnded = false
        };
    }

    private static DialogueResolution ResolveWitnessStatement(
        GuestProfile guest,
        NpcCharacterDefinition template,
        GuestReliabilityTier tier,
        WitnessStatementRecord witness)
    {
        NightMurderRecord murder = FactJournal.Instance?.GetLatestMurder();
        DialogueOutcome outcome = DialogueReliability.RollOutcome(guest, tier, hasWitnessStatement: true);
        DialogueTemplateDefinition templateDef = PickWitnessTemplate(witness, outcome);
        GuestProfile distortSubject = outcome == DialogueOutcome.Distorted
            ? PickDistortSubject(guest, murder)
            : null;

        Dictionary<string, string> slots = DialogueTemplateInterpolator.BuildWitnessSlots(
            witness, murder, distortSubject);

        string line = DialogueTemplateInterpolator.Apply(templateDef?.template ?? DefaultWitnessLine(outcome), slots);
        FactJournal.Instance?.MarkWitnessStatementDelivered(guest.Id, outcome);
        FactJournal.Instance?.IncrementTalkCount(guest.Id);
        FactJournal.Instance?.SetFlag($"witness_{guest.Id}_night_{murder?.Night}");

        if (outcome == DialogueOutcome.Refused)
        {
            return new DialogueResolution
            {
                BeatOrTemplateId = templateDef?.id ?? "witness_refuse",
                Outcome = outcome,
                Lines = new[] { line },
                ConversationEnded = true
            };
        }

        return new DialogueResolution
        {
            BeatOrTemplateId = templateDef?.id ?? "witness_truth",
            Outcome = outcome,
            Lines = new[] { line },
            ConversationEnded = false
        };
    }

    private static DialogueTemplateDefinition PickWitnessTemplate(
        WitnessStatementRecord witness,
        DialogueOutcome outcome)
    {
        if (outcome == DialogueOutcome.Refused)
            return NpcDialogueCatalog.FindTemplate("witness_refuse_talk")
                   ?? NpcDialogueCatalog.FindTemplate("witness_lie_sleeping");

        if (outcome == DialogueOutcome.Lie)
            return NpcDialogueCatalog.FindTemplate("witness_lie_sleeping")
                   ?? NpcDialogueCatalog.FindTemplate("witness_lie_was_elsewhere");

        if (outcome == DialogueOutcome.Distorted)
            return NpcDialogueCatalog.FindTemplate("witness_misremember")
                   ?? NpcDialogueCatalog.FindWitnessTemplate(witness.Type, DialogueOutcome.Truth);

        return NpcDialogueCatalog.FindWitnessTemplate(witness.Type, DialogueOutcome.Truth)
               ?? NpcDialogueCatalog.FindTemplate("witness_nothing");
    }

    private static GuestProfile PickDistortSubject(GuestProfile witnessGuest, NightMurderRecord murder)
    {
        if (GuestManager.Instance == null)
            return null;

        List<GuestProfile> candidates = GuestManager.Instance.GetStayingGuests()
            .Where(guest => guest.IsAlive && guest.Id != witnessGuest.Id)
            .ToList();

        if (candidates.Count == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private static DialogueBeat PickBeat(GuestProfile guest, CharacterDialogueFile dialogue)
    {
        if (dialogue?.beats == null || dialogue.beats.Length == 0 || FactJournal.Instance == null)
            return null;

        int talkCount = FactJournal.Instance.GetTalkCount(guest.Id);
        IEnumerable<DialogueBeat> ordered = dialogue.beats
            .Where(beat => beat != null)
            .OrderByDescending(beat => beat.priority);

        foreach (DialogueBeat beat in ordered)
        {
            if (beat.once && FactJournal.Instance.HasPlayedBeat(guest.Id, beat.id))
                continue;

            if (!MeetsConditions(guest, talkCount, beat.conditions))
                continue;

            return beat;
        }

        return null;
    }

    private static bool MeetsConditions(GuestProfile guest, int talkCount, DialogueBeatConditions conditions)
    {
        if (conditions == null)
            return true;

        if (conditions.min_kalinan_gun >= 0 && guest.KalinanGun < conditions.min_kalinan_gun)
            return false;

        if (conditions.max_kalinan_gun >= 0 && guest.KalinanGun > conditions.max_kalinan_gun)
            return false;

        if (conditions.min_talk_count >= 0 && talkCount < conditions.min_talk_count)
            return false;

        if (conditions.max_talk_count >= 0 && talkCount > conditions.max_talk_count)
            return false;

        if (!string.IsNullOrEmpty(conditions.requires_flag) &&
            FactJournal.Instance != null &&
            !FactJournal.Instance.HasFlag(conditions.requires_flag))
            return false;

        return true;
    }

    private static string DefaultWitnessLine(DialogueOutcome outcome)
    {
        switch (outcome)
        {
            case DialogueOutcome.Refused:
                return "Bu konuyu simdi konusmak istemiyorum.";
            case DialogueOutcome.Lie:
                return "Gecen gece uyuyordum. Hicbir sey duymadim.";
            case DialogueOutcome.Distorted:
                return "{victim_room} tarafinda bir ses duydum sanirim ama emin degilim.";
            default:
                return "Gecen gece {victim_room} tarafinda ses duydum.";
        }
    }
}
