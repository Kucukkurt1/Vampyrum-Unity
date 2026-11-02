using System;

[Serializable]
public class NpcCharacterDefinition
{
    public int id;
    public string ad;
    public string soyad;
    public int yas;
    public string cinsiyet;
    public string koken;
    public string meslek;
    public string aciklama;
    public int model_id;
    public string rol;
    public string vampir_atama;
    public NpcIntRange kalis_gunu = NpcIntRange.Fixed(5);
    public NpcIntRange ucret_gunluk = NpcIntRange.Fixed(3);
    public NpcIntRange maliyet_gunluk = NpcIntRange.Fixed(2);
    public bool aktif;

    public string DisplayName => string.IsNullOrEmpty(soyad) ? ad : $"{ad} {soyad}";

    public string FileSlug =>
        string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad)
            ? $"npc_{id}"
            : $"{ad}_{soyad}".ToLowerInvariant();

    public bool IsAlwaysVampire =>
        string.Equals(vampir_atama, "her_zaman", StringComparison.OrdinalIgnoreCase);

    public bool IsVampireBlocked =>
        string.Equals(vampir_atama, "asla", StringComparison.OrdinalIgnoreCase);

    public bool IsRandomVampireCandidate =>
        string.Equals(vampir_atama, "rastgele", StringComparison.OrdinalIgnoreCase);

    public void RollEconomy(Random random, out int planlananGun, out int rolledUcret, out int rolledMaliyet)
    {
        planlananGun = kalis_gunu.Roll(random);
        rolledUcret = ucret_gunluk.Roll(random);
        rolledMaliyet = maliyet_gunluk.Roll(random);
    }
}
