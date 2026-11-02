using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class NpcCatalog
{
    private static List<NpcCharacterDefinition> cachedActiveCharacters;
    private static List<NpcCharacterDefinition> fallbackCharacters;

    public static IReadOnlyList<NpcCharacterDefinition> GetActiveCharacters()
    {
        if (cachedActiveCharacters != null)
            return cachedActiveCharacters;

        cachedActiveCharacters = LoadFromDocs();
        if (cachedActiveCharacters.Count > 0)
            return cachedActiveCharacters;

        cachedActiveCharacters = GetFallbackCharacters();
        Debug.LogWarning("NpcCatalog: docs/npc/characters okunamadi, yerlesik liste kullaniliyor.");
        return cachedActiveCharacters;
    }

    public static NpcCharacterDefinition GetById(int catalogId)
    {
        foreach (NpcCharacterDefinition character in GetActiveCharacters())
        {
            if (character.id == catalogId)
                return character;
        }

        return null;
    }

    private static List<NpcCharacterDefinition> LoadFromDocs()
    {
        var loaded = new List<NpcCharacterDefinition>();
        string charactersPath = GetCharactersDirectoryPath();
        if (!Directory.Exists(charactersPath))
            return loaded;

        foreach (string filePath in Directory.GetFiles(charactersPath, "*.json"))
        {
            string fileName = Path.GetFileName(filePath);
            if (fileName.StartsWith("_", StringComparison.Ordinal))
                continue;

            try
            {
                string json = File.ReadAllText(filePath);
                var definition = JsonUtility.FromJson<NpcCharacterDefinition>(json);
                if (definition == null || !definition.aktif)
                    continue;

                loaded.Add(definition);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"NpcCatalog: {fileName} okunamadi: {exception.Message}");
            }
        }

        loaded.Sort((a, b) => a.id.CompareTo(b.id));
        return loaded;
    }

    private static string GetCharactersDirectoryPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
            return string.Empty;

        return Path.Combine(projectRoot, "docs", "npc", "characters");
    }

    private static List<NpcCharacterDefinition> GetFallbackCharacters()
    {
        if (fallbackCharacters != null)
            return fallbackCharacters;

        fallbackCharacters = new List<NpcCharacterDefinition>
        {
            CreateFallback(2, "Nikolai", "Vasilev", "Fırtınadan içeri sığınan bir yolcu.")
        };

        return fallbackCharacters;
    }

    private static NpcCharacterDefinition CreateFallback(int id, string ad, string soyad, string aciklama)
    {
        return new NpcCharacterDefinition
        {
            id = id,
            ad = ad,
            soyad = soyad,
            aciklama = aciklama,
            rol = "misafir",
            vampir_atama = "rastgele",
            kalis_gunu = NpcIntRange.Fixed(5),
            ucret_gunluk = NpcIntRange.Fixed(3),
            maliyet_gunluk = NpcIntRange.Fixed(2),
            aktif = true
        };
    }
}
