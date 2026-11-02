using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly Dictionary<string, int> items = new Dictionary<string, int>();
    private readonly Dictionary<string, int> potions = new Dictionary<string, int>();

    public int DepotCapacity { get; private set; } = 50;
    public int DepotExpandStep => 25;
    public int DepotExpandCost => 40 + depotExpansions * 20;
    public int DepotUsed => Sum(items) + Sum(potions);
    public int DepotRoom => Mathf.Max(0, DepotCapacity - DepotUsed);

    private int depotExpansions;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        SeedStarterMaterials();
    }

    private void SeedStarterMaterials()
    {
        items["moon_herb"] = 3;
        items["silver_dust"] = 2;
        items["holy_water"] = 1;
        items["wolf_bane"] = 2;
        items["elma"] = 4;
        items["bugday"] = 3;
        items["sarimsak"] = 2;
        items["zumrut"] = 1;
        items["elmas"] = 1;
        items["aytasi"] = 1;
    }

    public int GetItemCount(string itemId) =>
        items.TryGetValue(itemId, out int count) ? count : 0;

    public int GetMaterialCount(string materialId) => GetItemCount(materialId);

    public int GetTradeCount(ItemDefinition item)
    {
        if (item == null)
            return 0;

        if (item.Kind == ItemKind.Iksir)
            return GetPotionCount(item.Id);

        return GetItemCount(item.Id);
    }

    public void AddItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        int fit = Mathf.Min(amount, DepotRoom);
        if (fit <= 0)
            return;

        items[itemId] = GetItemCount(itemId) + fit;
        OnInventoryChanged?.Invoke();
    }

    public bool TryExpandDepot()
    {
        if (GameManager.Instance == null || !GameManager.Instance.SpendGold(DepotExpandCost))
            return false;

        DepotCapacity += DepotExpandStep;
        depotExpansions++;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool TryRemoveItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0 || GetItemCount(itemId) < amount)
            return false;

        int left = GetItemCount(itemId) - amount;
        if (left == 0)
            items.Remove(itemId);
        else
            items[itemId] = left;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetPotionCount(string potionId) =>
        potions.TryGetValue(potionId, out int count) ? count : 0;

    public bool HasMaterials(PotionDefinition potion)
    {
        if (potion?.Materials == null)
            return true;

        foreach (MaterialRequirement req in potion.Materials)
        {
            if (GetMaterialCount(req.MaterialId) < req.Amount)
                return false;
        }

        return true;
    }

    public bool TrySpendMaterials(PotionDefinition potion)
    {
        if (!HasMaterials(potion))
            return false;

        foreach (MaterialRequirement req in potion.Materials)
        {
            int left = GetMaterialCount(req.MaterialId) - req.Amount;
            if (left <= 0)
                items.Remove(req.MaterialId);
            else
                items[req.MaterialId] = left;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void AddPotion(string potionId, int amount = 1)
    {
        if (string.IsNullOrEmpty(potionId) || amount <= 0)
            return;

        int fit = Mathf.Min(amount, DepotRoom);
        if (fit <= 0)
            return;

        potions[potionId] = GetPotionCount(potionId) + fit;
        OnInventoryChanged?.Invoke();
    }

    public bool TryUsePotion(string potionId)
    {
        if (GetPotionCount(potionId) <= 0)
            return false;

        int left = GetPotionCount(potionId) - 1;
        if (left <= 0)
            potions.Remove(potionId);
        else
            potions[potionId] = left;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool TryRemovePotion(string potionId, int amount)
    {
        if (string.IsNullOrEmpty(potionId) || amount <= 0 || GetPotionCount(potionId) < amount)
            return false;

        int left = GetPotionCount(potionId) - amount;
        if (left <= 0)
            potions.Remove(potionId);
        else
            potions[potionId] = left;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public string FormatDepotListing()
    {
        var owned = new List<ItemDefinition>();
        IReadOnlyList<ItemDefinition> all = ItemCatalog.All;
        for (int i = 0; i < all.Count; i++)
        {
            if (GetTradeCount(all[i]) > 0)
                owned.Add(all[i]);
        }

        owned.Sort(CompareDepotItems);
        var text = new StringBuilder();
        text.Append("Depo ");
        text.Append(DepotUsed);
        text.Append('/');
        text.Append(DepotCapacity);
        text.Append(". Genişletme ");
        text.Append(DepotExpandCost);
        text.Append(" altın, +");
        text.Append(DepotExpandStep);
        text.Append(" yer.\n\n");
        if (owned.Count == 0)
        {
            text.Append("Raflarda sayılacak eşya yok.");
            return text.ToString();
        }

        for (int i = 0; i < owned.Count; i++)
        {
            ItemDefinition item = owned[i];
            text.Append(item.DisplayName);
            text.Append("  —  ");
            text.Append(GetTradeCount(item));
            if (i < owned.Count - 1)
                text.Append('\n');
        }

        return text.ToString();
    }

    private static int Sum(Dictionary<string, int> counts)
    {
        int total = 0;
        foreach (KeyValuePair<string, int> pair in counts)
            total += pair.Value;
        return total;
    }

    private static int CompareDepotItems(ItemDefinition a, ItemDefinition b)
    {
        int rarity = a.Rarity.CompareTo(b.Rarity);
        if (rarity != 0)
            return rarity;

        return string.CompareOrdinal(a.DisplayName, b.DisplayName);
    }

    public string FormatMaterialCounts(PotionDefinition potion)
    {
        if (potion?.Materials == null || potion.Materials.Length == 0)
            return "Hammadde gerekmez.";

        var lines = new List<string>();
        foreach (MaterialRequirement req in potion.Materials)
        {
            int have = GetMaterialCount(req.MaterialId);
            lines.Add($"- {req.DisplayName}: {have}/{req.Amount}");
        }

        return string.Join("\n", lines);
    }
}
