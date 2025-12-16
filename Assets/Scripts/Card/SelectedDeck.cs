using System.Collections.Generic;

public static class SelectedDeckManager
{
    public static Deck SelectedDeck { get; private set; }

    public static void SetSelectedDeck(Deck deck)
    {
        SelectedDeck = deck;
    }

    public static void Clear()
    {
        SelectedDeck = null;
    }

    public static List<int> GetSelectedDeckIds()
    {
        return SelectedDeck?.cards;
    }

    public static string GetSelectedDeckCsv()
    {
        List<int> ids = GetSelectedDeckIds();
        return ids.Count == 0 ? string.Empty : string.Join(",", ids);
    }

    public static List<int> ParseCsv(string csv)
    {
        List<int> list = new();
        if (string.IsNullOrWhiteSpace(csv)) return list;
        var parts = csv.Split(',');
        foreach (string p in parts)
        {
            if (int.TryParse(p, out var id)) list.Add(id);
        }
        return list;
    }
}
