using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class MorningReportViewBuilder
{
    public static void Populate(Transform contentRoot, GameManager gm)
    {
        ClearChildren(contentRoot);

        if (gm == null)
        {
            AddParagraph(contentRoot, "Rapor bilgisi yok.", CaravansaryUITheme.TextMuted);
            return;
        }

        AddSectionTitle(contentRoot, $"Gun {gm.CurrentDay} — Gece Ozeti");

        if (gm.LastNightResult != null)
        {
            bool murder = gm.LastNightResult.MurderOccurred;
            Color nightColor = murder ? CaravansaryUITheme.AccentRed : CaravansaryUITheme.TextPrimary;

            foreach (string line in gm.LastNightResult.EventLines)
                AddBullet(contentRoot, line, nightColor);

            if (murder)
            {
                AddSpacer(contentRoot, 4f);
                AddParagraph(contentRoot,
                    $"Kurban: {gm.LastNightResult.VictimName} ({gm.LastNightResult.VictimRoom})",
                    CaravansaryUITheme.AccentRed, FontStyle.Bold);
            }
        }

        AddSpacer(contentRoot, 8f);
        AddSectionTitle(contentRoot, "Gelirler");

        if (gm.LastDayIncome != null && gm.LastDayIncome.TotalRefunds > 0)
        {
            AddLineItem(contentRoot, "Iade", $"+{gm.LastDayIncome.TotalRefunds}", CaravansaryUITheme.AccentGreen);
        }
        else
        {
            AddParagraph(contentRoot, "Iade yok.", CaravansaryUITheme.TextMuted);
        }

        AddSpacer(contentRoot, 8f);
        AddSectionTitle(contentRoot, "Gece Giderleri");

        if (gm.LastDayIncome != null && gm.LastDayIncome.TotalHostingCost > 0)
        {
            AddLineItem(contentRoot, "Barindirma", $"-{gm.LastDayIncome.TotalHostingCost}", CaravansaryUITheme.AccentRed);
            AddParagraph(contentRoot,
                $"Kasa: {gm.LastGoldBeforeIncome} → {gm.LastGoldAfterIncome} altin",
                CaravansaryUITheme.TextSecondary);
        }
        else
        {
            AddParagraph(contentRoot, "Barindirma gideri yok.", CaravansaryUITheme.TextMuted);
        }

        AddSpacer(contentRoot, 8f);
        AddSectionTitle(contentRoot, "Sabit Giderler");

        int totalExpense = gm.LastMaintenanceCost + gm.LastTaxCost;
        AddLineItem(contentRoot, "Bakim", $"-{gm.LastMaintenanceCost}", CaravansaryUITheme.AccentRed);
        AddLineItem(contentRoot, "Vergi", $"-{gm.LastTaxCost}", CaravansaryUITheme.AccentRed);
        AddDivider(contentRoot);
        AddLineItem(contentRoot, "Toplam sabit gider", $"-{totalExpense}", CaravansaryUITheme.AccentRed, true);
        AddParagraph(contentRoot,
            $"Kasa: {gm.LastGoldAfterIncome} → {gm.Gold} altin",
            CaravansaryUITheme.TextSecondary);

        if (GuestManager.Instance != null)
        {
            AddSpacer(contentRoot, 8f);
            AddSectionTitle(contentRoot, "Misafirler");

            List<GuestProfile> staying = GuestManager.Instance.GetStayingGuests();
            if (staying.Count == 0)
                AddParagraph(contentRoot, "Konaklayan misafir yok.", CaravansaryUITheme.TextMuted);
            else
            {
                foreach (GuestProfile guest in staying)
                    AddBullet(contentRoot, $"{guest.DisplayName} — {guest.RoomName}", CaravansaryUITheme.TextPrimary);
            }

            foreach (GuestProfile guest in GuestManager.Instance.AllGuests)
            {
                if (!guest.IsAlive)
                    AddBullet(contentRoot, $"{guest.DisplayName}: OLDU", CaravansaryUITheme.AccentRed);
            }
        }

        AddSpacer(contentRoot, 12f);
        AddSummaryBar(contentRoot, gm.Gold);
    }

    public static void PopulateEmpty(Transform contentRoot)
    {
        ClearChildren(contentRoot);
        AddParagraph(contentRoot, "Henuz sabah raporu yok.", CaravansaryUITheme.TextMuted);
        AddParagraph(contentRoot, "Ilk geceyi tamamlamak icin yataga gidip E tusuna basin.", CaravansaryUITheme.TextSecondary);
    }

    private static void AddSectionTitle(Transform parent, string title)
    {
        var row = CreateRow(parent, 32f);
        CaravansaryUITheme.CreateAnchoredLabel(row.transform, title, 18, FontStyle.Bold, CaravansaryUITheme.AccentGold,
            Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(-4f, 28f),
            TextAnchor.MiddleLeft);
    }

    private static void AddParagraph(Transform parent, string text, Color color, FontStyle style = FontStyle.Normal)
    {
        var row = CreateRow(parent, 0f);
        var layout = row.GetComponent<LayoutElement>();
        layout.minHeight = 24f;
        layout.preferredHeight = -1f;

        var label = CaravansaryUITheme.CreateAnchoredLabel(row.transform, text, 16, style, color,
            Vector2.zero, Vector2.one, new Vector2(0f, 1f), new Vector2(8f, 0f), new Vector2(-8f, 0f),
            TextAnchor.UpperLeft);
        label.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private static void AddBullet(Transform parent, string text, Color color)
    {
        var row = CreateRow(parent, 0f);
        var layout = row.GetComponent<LayoutElement>();
        layout.minHeight = 22f;

        CaravansaryUITheme.CreateAnchoredLabel(row.transform, "•  " + text, 16, FontStyle.Normal, color,
            Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(-12f, 22f),
            TextAnchor.MiddleLeft);
    }

    private static void AddLineItem(Transform parent, string label, string value, Color valueColor, bool bold = false)
    {
        var row = CreateRow(parent, 28f);

        CaravansaryUITheme.CreateAnchoredLabel(row.transform, label, 16, FontStyle.Normal, CaravansaryUITheme.TextSecondary,
            new Vector2(0f, 0f), new Vector2(0.55f, 1f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), Vector2.zero,
            TextAnchor.MiddleLeft);

        CaravansaryUITheme.CreateAnchoredLabel(row.transform, value, 16, bold ? FontStyle.Bold : FontStyle.Normal, valueColor,
            new Vector2(0.55f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), Vector2.zero,
            TextAnchor.MiddleRight);
    }

    private static void AddDivider(Transform parent)
    {
        var row = CreateRow(parent, 10f);
        var line = new GameObject("Divider");
        line.transform.SetParent(row.transform, false);
        var rect = line.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.offsetMin = new Vector2(8f, -1f);
        rect.offsetMax = new Vector2(-8f, 1f);
        line.AddComponent<Image>().color = new Color(CaravansaryUITheme.PanelBorder.r, CaravansaryUITheme.PanelBorder.g,
            CaravansaryUITheme.PanelBorder.b, 0.35f);
    }

    private static void AddSummaryBar(Transform parent, int gold)
    {
        var row = CreateRow(parent, 44f);
        row.GetComponent<Image>().color = CaravansaryUITheme.StatBoxBg;

        CaravansaryUITheme.CreateAnchoredLabel(row.transform, "Guncel kasa", 14, FontStyle.Normal, CaravansaryUITheme.TextMuted,
            new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), Vector2.zero,
            TextAnchor.MiddleLeft);

        CaravansaryUITheme.CreateAnchoredLabel(row.transform, $"{gold} altin", 22, FontStyle.Bold, CaravansaryUITheme.AccentGold,
            new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), Vector2.zero,
            TextAnchor.MiddleRight);
    }

    private static void AddSpacer(Transform parent, float height)
    {
        var row = CreateRow(parent, height);
        row.GetComponent<Image>().color = Color.clear;
    }

    private static GameObject CreateRow(Transform parent, float height)
    {
        var row = new GameObject("Row");
        row.transform.SetParent(parent, false);

        var layout = row.AddComponent<LayoutElement>();
        if (height > 0f)
        {
            layout.minHeight = height;
            layout.preferredHeight = height;
        }

        row.AddComponent<Image>().color = Color.clear;
        return row;
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);
    }
}
