using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class NpcDialogueCatalog
{
    private static Dictionary<string, CharacterDialogueFile> cachedCharacterDialogues;
    private static Dictionary<string, CharacterDialogueMeta> cachedCharacterMeta;
    private static Dictionary<string, DialogueTemplateDefinition> cachedTemplates;
    private static GenericSmalltalkFile cachedSmalltalk;

    public static string ResolveDialoguePath(NpcCharacterDefinition template, bool isVampireThisRun)
    {
        if (template == null)
            return string.Empty;

        string slug = template.FileSlug;
        string dialogueRoot = GetDialogueDirectoryPath();
        if (string.IsNullOrEmpty(dialogueRoot))
            return string.Empty;

        if (template.IsAlwaysVampire && isVampireThisRun)
            return Path.Combine(dialogueRoot, slug, "vampir.json");

        return Path.Combine(dialogueRoot, slug, "insan.json");
    }

    public static CharacterDialogueFile GetCharacterDialogue(NpcCharacterDefinition template, bool isVampireThisRun)
    {
        EnsureCharacterCache();
        string path = ResolveDialoguePath(template, isVampireThisRun);
        if (string.IsNullOrEmpty(path))
            return null;

        if (cachedCharacterDialogues.TryGetValue(path, out CharacterDialogueFile cached))
            return cached;

        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            var dialogue = JsonUtility.FromJson<CharacterDialogueFile>(json);
            cachedCharacterDialogues[path] = dialogue;
            return dialogue;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"NpcDialogueCatalog: {path} okunamadi: {exception.Message}");
            return null;
        }
    }

    public static CharacterDialogueMeta GetCharacterMeta(NpcCharacterDefinition template)
    {
        EnsureCharacterCache();
        if (template == null)
            return null;

        string metaPath = Path.Combine(GetDialogueDirectoryPath(), template.FileSlug, "meta.json");
        if (cachedCharacterMeta.TryGetValue(metaPath, out CharacterDialogueMeta cached))
            return cached;

        if (!File.Exists(metaPath))
            return new CharacterDialogueMeta { karakter_id = template.id, dosya = template.FileSlug };

        try
        {
            string json = File.ReadAllText(metaPath);
            var meta = JsonUtility.FromJson<CharacterDialogueMeta>(json);
            cachedCharacterMeta[metaPath] = meta;
            return meta;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"NpcDialogueCatalog: {metaPath} okunamadi: {exception.Message}");
            return new CharacterDialogueMeta { karakter_id = template.id, dosya = template.FileSlug };
        }
    }

    public static GuestReliabilityTier GetReliabilityTier(NpcCharacterDefinition template)
    {
        CharacterDialogueMeta meta = GetCharacterMeta(template);
        if (meta != null && string.Equals(meta.guvenilirlik, "unreliable", StringComparison.OrdinalIgnoreCase))
            return GuestReliabilityTier.Unreliable;

        return GuestReliabilityTier.Normal;
    }

    public static IReadOnlyDictionary<string, DialogueTemplateDefinition> GetTemplates()
    {
        EnsureTemplateCache();
        return cachedTemplates;
    }

    public static DialogueTemplateDefinition FindTemplate(string templateId)
    {
        EnsureTemplateCache();
        cachedTemplates.TryGetValue(templateId, out DialogueTemplateDefinition template);
        return template;
    }

    public static DialogueTemplateDefinition FindWitnessTemplate(WitnessType witnessType, DialogueOutcome outcome)
    {
        EnsureTemplateCache();
        DialogueTemplateDefinition best = null;
        int bestPriority = int.MinValue;

        foreach (DialogueTemplateDefinition template in cachedTemplates.Values)
        {
            if (template.conditions == null)
                continue;

            if (!string.IsNullOrEmpty(template.conditions.outcome) &&
                !string.Equals(template.conditions.outcome, outcome.ToString(), StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrEmpty(template.conditions.witness_type) &&
                !string.Equals(template.conditions.witness_type, witnessType.ToString(), StringComparison.OrdinalIgnoreCase))
                continue;

            int priority = template.priority;
            if (priority >= bestPriority)
            {
                bestPriority = priority;
                best = template;
            }
        }

        return best;
    }

    public static GenericSmalltalkEntry GetRandomSmalltalk()
    {
        EnsureSmalltalkCache();
        if (cachedSmalltalk?.entries == null || cachedSmalltalk.entries.Length == 0)
            return null;

        return cachedSmalltalk.entries[UnityEngine.Random.Range(0, cachedSmalltalk.entries.Length)];
    }

    public static string GetSharedModifiersPath()
    {
        string dialogueRoot = GetDialogueDirectoryPath();
        return string.IsNullOrEmpty(dialogueRoot)
            ? string.Empty
            : Path.Combine(dialogueRoot, "_shared", "vampir_modifiers.json");
    }

    private static void EnsureCharacterCache()
    {
        if (cachedCharacterDialogues != null)
            return;

        cachedCharacterDialogues = new Dictionary<string, CharacterDialogueFile>();
        cachedCharacterMeta = new Dictionary<string, CharacterDialogueMeta>();
    }

    private static void EnsureTemplateCache()
    {
        if (cachedTemplates != null)
            return;

        cachedTemplates = new Dictionary<string, DialogueTemplateDefinition>();
        string templatesPath = Path.Combine(GetDialogueDirectoryPath(), "_shared", "templates");
        if (!Directory.Exists(templatesPath))
            return;

        foreach (string filePath in Directory.GetFiles(templatesPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var template = JsonUtility.FromJson<DialogueTemplateDefinition>(json);
                if (template == null || string.IsNullOrEmpty(template.id))
                    continue;

                cachedTemplates[template.id] = template;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"NpcDialogueCatalog template: {filePath} okunamadi: {exception.Message}");
            }
        }
    }

    private static void EnsureSmalltalkCache()
    {
        if (cachedSmalltalk != null)
            return;

        string path = Path.Combine(GetDialogueDirectoryPath(), "_shared", "generic_smalltalk.json");
        if (!File.Exists(path))
        {
            cachedSmalltalk = new GenericSmalltalkFile();
            return;
        }

        try
        {
            cachedSmalltalk = JsonUtility.FromJson<GenericSmalltalkFile>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"NpcDialogueCatalog smalltalk okunamadi: {exception.Message}");
            cachedSmalltalk = new GenericSmalltalkFile();
        }
    }

    private static string GetDialogueDirectoryPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
            return string.Empty;

        return Path.Combine(projectRoot, "docs", "npc", "dialogue");
    }
}
