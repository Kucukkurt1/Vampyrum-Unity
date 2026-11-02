using System.Collections.Generic;

public static class PotionCatalog
{
    private static readonly List<PotionDefinition> potions = new List<PotionDefinition>
    {
        new PotionDefinition
        {
            Id = "diagnosis",
            DisplayName = "Vampir Teshis Iksiri",
            Description = "Suphelinin kanini test eder. Kesin sonuc vermez; yuzdelik ipucu verir.",
            GoldCost = 40,
            Materials = new[]
            {
                new MaterialRequirement { MaterialId = "moon_herb", DisplayName = "Ay Otu", Amount = 2 },
                new MaterialRequirement { MaterialId = "silver_dust", DisplayName = "Gumus Tozu", Amount = 1 }
            }
        },
        new PotionDefinition
        {
            Id = "protection",
            DisplayName = "Koruma Iksiri",
            Description = "Secilen kisi bir geceligine vampir saldirisina karsi korunur.",
            GoldCost = 30,
            Materials = new[]
            {
                new MaterialRequirement { MaterialId = "holy_water", DisplayName = "Kutsal Su", Amount = 1 },
                new MaterialRequirement { MaterialId = "wolf_bane", DisplayName = "Kurtbocegi", Amount = 2 }
            }
        }
    };

    public static IReadOnlyList<PotionDefinition> All => potions;

    public static PotionDefinition GetById(string id) =>
        potions.Find(p => p.Id == id);
}
