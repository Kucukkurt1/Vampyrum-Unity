public enum ItemRarity
{
    Siradan,
    Ender,
    Nadir,
    Efsanevi
}

public enum ItemKind
{
    Tas,
    Erzak,
    Takas,
    Hammadde,
    Iksir,
    Kitap,
    BuyuKitabi,
    Koleksiyon
}

public class ItemDefinition
{
    public string Id;
    public string DisplayName;
    public ItemRarity Rarity;
    public ItemKind Kind;
    public bool Tradeable;
}
