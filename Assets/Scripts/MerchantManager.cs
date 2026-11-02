using System.Collections.Generic;
using UnityEngine;

public class TradeQuote
{
    public int OfferValue;
    public int TakeValue;
    public int OfferedGold;
    public int GoldDelta;
    public bool HasSelection;
    public bool CanConfirm;
}

public class MerchantManager : MonoBehaviour
{
    public static MerchantManager Instance { get; private set; }

    private static readonly (string Id, int Count)[] DayStock =
    {
        ("elma", 8),
        ("bugday", 6),
        ("sarimsak", 3),
        ("kaya_tuzu", 3),
        ("tesbih", 2),
        ("parsomen", 4),
        ("moon_herb", 3),
        ("wolf_bane", 3),
        ("hesap_kitabi", 1)
    };

    private readonly Dictionary<string, int> stock = new Dictionary<string, int>();

    public event System.Action OnStockChanged;
    public event System.Action OnPresenceChanged;

    public bool IsPresent { get; private set; } = true;

    public void Depart()
    {
        if (!IsPresent)
            return;

        IsPresent = false;
        if (MerchantTradeUI.Instance != null && MerchantTradeUI.Instance.IsVisible)
            MerchantTradeUI.Instance.Hide();
        OnPresenceChanged?.Invoke();
    }

    public void Return()
    {
        if (IsPresent)
            return;

        IsPresent = true;
        OnPresenceChanged?.Invoke();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        Restock();
    }

    public int GetStock(string itemId) =>
        stock.TryGetValue(itemId, out int count) ? count : 0;

    public IReadOnlyDictionary<string, int> Stock => stock;

    public void Restock()
    {
        stock.Clear();
        for (int i = 0; i < DayStock.Length; i++)
            stock[DayStock[i].Id] = DayStock[i].Count;

        OnStockChanged?.Invoke();
    }

    public TradeQuote Quote(Dictionary<string, int> offer, Dictionary<string, int> take, int offeredGold = 0)
    {
        bool offerValid = Sum(offer, false, out int offerValue, out bool offerAny);
        bool takeValid = Sum(take, true, out int takeValue, out bool takeAny);
        int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        if (offeredGold < 0)
            offeredGold = 0;

        int delta = offerValue - takeValue - offeredGold;
        int netCargo = UnitCount(take) - UnitCount(offer);
        bool fits = InventoryManager.Instance == null || netCargo <= InventoryManager.Instance.DepotRoom;

        return new TradeQuote
        {
            OfferValue = offerValue,
            TakeValue = takeValue,
            OfferedGold = offeredGold,
            GoldDelta = delta,
            HasSelection = offerAny || takeAny,
            CanConfirm = offerValid && takeValid && (offerAny || takeAny) && gold + delta >= 0 && fits
        };
    }

    private static int UnitCount(Dictionary<string, int> selection)
    {
        if (selection == null)
            return 0;

        int total = 0;
        foreach (KeyValuePair<string, int> pair in selection)
        {
            if (pair.Value > 0)
                total += pair.Value;
        }

        return total;
    }

    public bool TryExecute(Dictionary<string, int> offer, Dictionary<string, int> take, int offeredGold = 0)
    {
        TradeQuote quote = Quote(offer, take, offeredGold);
        if (!quote.CanConfirm || InventoryManager.Instance == null || GameManager.Instance == null)
            return false;

        ApplyPlayerSide(offer, true);
        ApplyPlayerSide(take, false);
        ApplyMerchantSide(offer, true);
        ApplyMerchantSide(take, false);

        if (quote.GoldDelta > 0)
            GameManager.Instance.AddGold(quote.GoldDelta);
        else if (quote.GoldDelta < 0)
            GameManager.Instance.SpendGold(-quote.GoldDelta);

        OnStockChanged?.Invoke();
        return true;
    }

    private bool Sum(Dictionary<string, int> selection, bool fromMerchant, out int value, out bool any)
    {
        value = 0;
        any = false;
        if (selection == null)
            return true;

        foreach (KeyValuePair<string, int> pair in selection)
        {
            if (pair.Value <= 0)
                continue;

            any = true;
            ItemDefinition item = ItemCatalog.Get(pair.Key);
            if (item == null || !item.Tradeable)
                return false;

            int owned = fromMerchant
                ? GetStock(item.Id)
                : InventoryManager.Instance != null ? InventoryManager.Instance.GetTradeCount(item) : 0;

            if (pair.Value > owned)
                return false;

            int unit = fromMerchant ? ItemCatalog.MerchantPrice(item) : ItemCatalog.BasePrice(item);
            value += unit * pair.Value;
        }

        return true;
    }

    private static void ApplyPlayerSide(Dictionary<string, int> selection, bool removing)
    {
        if (selection == null || InventoryManager.Instance == null)
            return;

        foreach (KeyValuePair<string, int> pair in selection)
        {
            if (pair.Value <= 0)
                continue;

            ItemDefinition item = ItemCatalog.Get(pair.Key);
            if (item == null)
                continue;

            if (item.Kind == ItemKind.Iksir)
            {
                if (removing)
                    InventoryManager.Instance.TryRemovePotion(item.Id, pair.Value);
                else
                    InventoryManager.Instance.AddPotion(item.Id, pair.Value);
            }
            else if (removing)
            {
                InventoryManager.Instance.TryRemoveItem(item.Id, pair.Value);
            }
            else
            {
                InventoryManager.Instance.AddItem(item.Id, pair.Value);
            }
        }
    }

    private void ApplyMerchantSide(Dictionary<string, int> selection, bool adding)
    {
        if (selection == null)
            return;

        foreach (KeyValuePair<string, int> pair in selection)
        {
            if (pair.Value <= 0)
                continue;

            int next = GetStock(pair.Key) + (adding ? pair.Value : -pair.Value);
            if (next <= 0)
                stock.Remove(pair.Key);
            else
                stock[pair.Key] = next;
        }
    }
}
