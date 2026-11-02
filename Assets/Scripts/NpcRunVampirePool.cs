using System;
using System.Collections.Generic;
using System.Linq;

public static class NpcRunVampirePool
{
    private static HashSet<int> randomVampireCatalogIds = new HashSet<int>();
    private static bool initialized;

    public static void Initialize(int runSeed, int targetRandomVampireCount)
    {
        randomVampireCatalogIds.Clear();

        List<int> candidates = NpcCatalog.GetActiveCharacters()
            .Where(character => character.IsRandomVampireCandidate)
            .Select(character => character.id)
            .ToList();

        if (candidates.Count == 0 || targetRandomVampireCount <= 0)
        {
            initialized = true;
            return;
        }

        var random = new Random(runSeed);
        candidates = candidates.OrderBy(_ => random.Next()).ToList();

        int pickCount = Math.Min(targetRandomVampireCount, candidates.Count);
        for (int i = 0; i < pickCount; i++)
            randomVampireCatalogIds.Add(candidates[i]);

        initialized = true;
    }

    public static bool IsVampireThisRun(NpcCharacterDefinition template)
    {
        if (template == null)
            return false;

        if (template.IsAlwaysVampire)
            return true;

        if (template.IsVampireBlocked)
            return false;

        if (!initialized)
            return false;

        return randomVampireCatalogIds.Contains(template.id);
    }

    public static IReadOnlyCollection<int> GetRandomVampireCatalogIds() => randomVampireCatalogIds;
}
