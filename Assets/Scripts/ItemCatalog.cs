using System.Collections.Generic;

public static class ItemCatalog
{
    public const int MarkupNumerator = 13;
    public const int MarkupDenominator = 10;

    private static readonly List<ItemDefinition> items = new List<ItemDefinition>();
    private static readonly Dictionary<string, ItemDefinition> byId = new Dictionary<string, ItemDefinition>();

    static ItemCatalog()
    {
        Add("kantasi", "Kantasi", ItemRarity.Siradan, ItemKind.Tas);
        Add("obsidyen", "Obsidyen", ItemRarity.Siradan, ItemKind.Tas);
        Add("gumus_kulce", "Gumus Kulce", ItemRarity.Ender, ItemKind.Tas);
        Add("kehribar", "Kehribar", ItemRarity.Ender, ItemKind.Tas);
        Add("yesim", "Yesim", ItemRarity.Ender, ItemKind.Tas);
        Add("ametist", "Ametist", ItemRarity.Ender, ItemKind.Tas);
        Add("karanfil_tasi", "Karanfil Tasi", ItemRarity.Ender, ItemKind.Tas);
        Add("altin_kulce", "Altin Kulce", ItemRarity.Nadir, ItemKind.Tas);
        Add("zumrut", "Zumrut", ItemRarity.Nadir, ItemKind.Tas);
        Add("safir", "Safir", ItemRarity.Nadir, ItemKind.Tas);
        Add("opal", "Opal", ItemRarity.Nadir, ItemKind.Tas);
        Add("inci", "Inci", ItemRarity.Nadir, ItemKind.Tas);
        Add("elmas", "Elmas", ItemRarity.Efsanevi, ItemKind.Tas);
        Add("yakut", "Yakut", ItemRarity.Efsanevi, ItemKind.Tas);
        Add("aytasi", "Aytasi", ItemRarity.Efsanevi, ItemKind.Tas);
        Add("siyah_inci", "Siyah Inci", ItemRarity.Efsanevi, ItemKind.Tas);

        Add("dana_eti", "Dana Eti", ItemRarity.Siradan, ItemKind.Erzak);
        Add("domuz_eti", "Domuz Eti", ItemRarity.Siradan, ItemKind.Erzak);
        Add("geyik_eti", "Geyik Eti", ItemRarity.Siradan, ItemKind.Erzak);
        Add("sut", "Sut", ItemRarity.Siradan, ItemKind.Erzak);
        Add("peynir", "Peynir", ItemRarity.Siradan, ItemKind.Erzak);
        Add("yumurta", "Yumurta", ItemRarity.Siradan, ItemKind.Erzak);
        Add("balik", "Balik", ItemRarity.Siradan, ItemKind.Erzak);
        Add("tavuk", "Tavuk", ItemRarity.Siradan, ItemKind.Erzak);
        Add("un", "Un", ItemRarity.Siradan, ItemKind.Erzak);
        Add("bugday", "Bugday", ItemRarity.Siradan, ItemKind.Erzak);
        Add("elma", "Elma", ItemRarity.Siradan, ItemKind.Erzak);
        Add("armut", "Armut", ItemRarity.Siradan, ItemKind.Erzak);
        Add("visne", "Visne", ItemRarity.Siradan, ItemKind.Erzak);
        Add("kiraz", "Kiraz", ItemRarity.Siradan, ItemKind.Erzak);
        Add("erik", "Erik", ItemRarity.Siradan, ItemKind.Erzak);
        Add("patates", "Patates", ItemRarity.Siradan, ItemKind.Erzak);
        Add("cavdar", "Cavdar", ItemRarity.Siradan, ItemKind.Erzak);
        Add("arpa", "Arpa", ItemRarity.Siradan, ItemKind.Erzak);
        Add("yulaf", "Yulaf", ItemRarity.Siradan, ItemKind.Erzak);
        Add("mercimek", "Mercimek", ItemRarity.Siradan, ItemKind.Erzak);
        Add("bezelye", "Bezelye", ItemRarity.Siradan, ItemKind.Erzak);
        Add("fasulye", "Fasulye", ItemRarity.Siradan, ItemKind.Erzak);
        Add("havuc", "Havuc", ItemRarity.Siradan, ItemKind.Erzak);
        Add("pirasa", "Pirasa", ItemRarity.Siradan, ItemKind.Erzak);
        Add("lahana", "Lahana", ItemRarity.Siradan, ItemKind.Erzak);
        Add("sogan", "Sogan", ItemRarity.Siradan, ItemKind.Erzak);
        Add("bira", "Bira", ItemRarity.Siradan, ItemKind.Erzak);
        Add("bal", "Bal", ItemRarity.Ender, ItemKind.Erzak);
        Add("sarap", "Sarap", ItemRarity.Ender, ItemKind.Erzak);
        Add("likor", "Likor", ItemRarity.Ender, ItemKind.Erzak);
        Add("kuru_uzum", "Kuru Uzum", ItemRarity.Nadir, ItemKind.Erzak);
        Add("sarimsak", "Sarimsak", ItemRarity.Nadir, ItemKind.Erzak);
        Add("kaya_tuzu", "Kaya Tuzu", ItemRarity.Nadir, ItemKind.Erzak);
        Add("karabiber", "Karabiber", ItemRarity.Nadir, ItemKind.Erzak);
        Add("tarcin", "Tarcin", ItemRarity.Nadir, ItemKind.Erzak);

        Add("tesbih", "Tesbih", ItemRarity.Siradan, ItemKind.Takas);
        Add("kemik_tarak", "Kemik Tarak", ItemRarity.Siradan, ItemKind.Takas);
        Add("parsomen", "Parsomen", ItemRarity.Siradan, ItemKind.Takas);
        Add("muhur_mumu", "Muhur Mumu", ItemRarity.Siradan, ItemKind.Takas);
        Add("kursun_muhur", "Kursun Muhur", ItemRarity.Siradan, ItemKind.Takas);
        Add("murekkep_tasi", "Murekkep Tasi", ItemRarity.Siradan, ItemKind.Takas);
        Add("cakmaktasi", "Cakmaktasi", ItemRarity.Siradan, ItemKind.Takas);
        Add("kav_kutusu", "Kav Kutusu", ItemRarity.Siradan, ItemKind.Takas);
        Add("matara", "Matara", ItemRarity.Siradan, ItemKind.Takas);
        Add("pirinc_can", "Pirinc Can", ItemRarity.Siradan, ItemKind.Takas);
        Add("dikis_ignesi", "Demir Dikis Ignesi", ItemRarity.Siradan, ItemKind.Takas);
        Add("kemik_zar", "Kemik Zarlar", ItemRarity.Siradan, ItemKind.Takas);
        Add("ahsap_oyuncak", "Ahsap Oyuncak", ItemRarity.Siradan, ItemKind.Takas);
        Add("kaz_tuyu", "Kaz Tuyu", ItemRarity.Siradan, ItemKind.Takas);
        Add("tas_havan", "Tas Havan", ItemRarity.Siradan, ItemKind.Takas);
        Add("kerpeten", "Kerpeten", ItemRarity.Siradan, ItemKind.Takas);
        Add("kandil", "Kandil", ItemRarity.Siradan, ItemKind.Takas);
        Add("balmumu", "Balmumu", ItemRarity.Siradan, ItemKind.Takas);
        Add("gumus_hac", "Gumus Hac Kolye", ItemRarity.Ender, ItemKind.Takas);
        Add("pirinc_ayna", "Pirinc El Aynasi", ItemRarity.Ender, ItemKind.Takas);
        Add("gozluk", "Percinli Gozluk", ItemRarity.Ender, ItemKind.Takas);
        Add("gunes_saati", "Gunes Saati", ItemRarity.Ender, ItemKind.Takas);
        Add("ipek_mendil", "Ipek Mendil", ItemRarity.Ender, ItemKind.Takas);
        Add("kemer_tokasi", "Bronz Kemer Tokasi", ItemRarity.Ender, ItemKind.Takas);
        Add("satranc", "Ahsap Satranc", ItemRarity.Ender, ItemKind.Takas);
        Add("koku_kutusu", "Koku Kutusu", ItemRarity.Ender, ItemKind.Takas);
        Add("tutun_kesesi", "Tutun Kesesi", ItemRarity.Ender, ItemKind.Takas);
        Add("mercek", "Mercek", ItemRarity.Ender, ItemKind.Takas);
        Add("terazi", "Terazi", ItemRarity.Ender, ItemKind.Takas);
        Add("kum_saati", "Kum Saati", ItemRarity.Ender, ItemKind.Takas);
        Add("kehribar_tesbih", "Kehribar Tesbih", ItemRarity.Ender, ItemKind.Takas);
        Add("obsidyen_bilezik", "Obsidyen Bilezik", ItemRarity.Ender, ItemKind.Takas);
        Add("zumrut_yuzuk", "Zumrut Yuzuk", ItemRarity.Nadir, ItemKind.Takas);
        Add("gumus_hancer", "Gumus Hancer", ItemRarity.Nadir, ItemKind.Takas);
        Add("safir_yuzuk", "Safir Yuzuk", ItemRarity.Nadir, ItemKind.Takas);
        Add("inci_kupe", "Inci Kupeler", ItemRarity.Nadir, ItemKind.Takas);
        Add("altin_madalyon", "Altin Madalyon", ItemRarity.Nadir, ItemKind.Takas);
        Add("ametist_kadeh", "Ametist Tasli Kadeh", ItemRarity.Nadir, ItemKind.Takas);
        Add("turkuaz_kolye", "Turkuaz Kolye", ItemRarity.Nadir, ItemKind.Takas);
        Add("elmas_tac", "Elmas Islemeli Tac", ItemRarity.Efsanevi, ItemKind.Takas);
        Add("yakut_gerdanlik", "Yakut Gerdanlik", ItemRarity.Efsanevi, ItemKind.Takas);
        Add("yilan_bilezik", "Yilan Motifli Altin Bilezik", ItemRarity.Efsanevi, ItemKind.Takas);
        Add("usturlap", "Usturlap", ItemRarity.Efsanevi, ItemKind.Takas);

        Add("sari_kantaron", "Sari Kantaron", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("pelin_otu", "Pelin Otu", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("melekotu", "Melekotu", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("demirdiken", "Demirdiken", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("altinotu", "Altinotu", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("itbogan", "Itbogan", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("murekkep_mantari", "Murekkep Mantari", ItemRarity.Siradan, ItemKind.Hammadde);
        Add("moon_herb", "Ay Otu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("silver_dust", "Gumus Tozu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("holy_water", "Kutsal Su", ItemRarity.Ender, ItemKind.Hammadde);
        Add("wolf_bane", "Kurtbogan", ItemRarity.Ender, ItemKind.Hammadde);
        Add("adamotu", "Adamotu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("kuzgunotu", "Kuzgunotu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("kargaotu", "Kargaotu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("kurt_pencesi", "Kurt Pencesi", ItemRarity.Ender, ItemKind.Hammadde);
        Add("yilanotu", "Yilanotu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("seytan_otu", "Seytan Otu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("kurt_mantari", "Kurt Mantari", ItemRarity.Ender, ItemKind.Hammadde);
        Add("mavi_ejder_otu", "Mavi Ejder Otu", ItemRarity.Ender, ItemKind.Hammadde);
        Add("guzelavrat", "Guzelavrat Otu", ItemRarity.Nadir, ItemKind.Hammadde);
        Add("baldiran", "Baldiran", ItemRarity.Nadir, ItemKind.Hammadde);
        Add("kara_banotu", "Kara Banotu", ItemRarity.Nadir, ItemKind.Hammadde);
        Add("sinek_mantari", "Sinek Mantari", ItemRarity.Nadir, ItemKind.Hammadde);
        Add("cavdar_mahmuzu", "Cavdar Mahmuzu", ItemRarity.Nadir, ItemKind.Hammadde);
        Add("olum_melegi", "Olum Melegi Mantari", ItemRarity.Efsanevi, ItemKind.Hammadde);
        Add("ejderha_kani", "Ejderha Kani Recinesi", ItemRarity.Efsanevi, ItemKind.Hammadde);
        Add("sihirli_mantar", "Sihirli Mantar", ItemRarity.Efsanevi, ItemKind.Hammadde);
        Add("fosfor_mantari", "Karanlik Orman Fosfor Mantari", ItemRarity.Efsanevi, ItemKind.Hammadde);
        Add("seytanelmasi", "Seytanelmasi", ItemRarity.Efsanevi, ItemKind.Hammadde);

        Add("diagnosis", "Vampir Teshis Iksiri", ItemRarity.Nadir, ItemKind.Iksir);
        Add("kizil_teshis", "Kizil Teshis", ItemRarity.Efsanevi, ItemKind.Iksir);
        Add("protection", "Koruma Iksiri", ItemRarity.Nadir, ItemKind.Iksir);
        Add("esik_muhru", "Esik Muhru", ItemRarity.Efsanevi, ItemKind.Iksir);
        Add("uyku_iksiri", "Uyku Iksiri", ItemRarity.Ender, ItemKind.Iksir);
        Add("panzehir", "Panzehir", ItemRarity.Ender, ItemKind.Iksir);
        Add("yalanci_sahit", "Yalanci Sahit", ItemRarity.Nadir, ItemKind.Iksir);
        Add("gece_gozu", "Gece Gozu", ItemRarity.Ender, ItemKind.Iksir);
        Add("kan_sisesi", "Kan Sisesi", ItemRarity.Nadir, ItemKind.Iksir);
        Add("iz_silme", "Iz Silme", ItemRarity.Nadir, ItemKind.Iksir);
        Add("soluk_zehir", "Soluk Zehir", ItemRarity.Efsanevi, ItemKind.Iksir);
        Add("unutus", "Unutus", ItemRarity.Efsanevi, ItemKind.Iksir);

        Add("hesap_kitabi", "Hesap Kitabi", ItemRarity.Ender, ItemKind.Kitap);
        Add("hendese", "Hendese", ItemRarity.Ender, ItemKind.Kitap);
        Add("doga_felsefesi", "Doga Felsefesi", ItemRarity.Ender, ItemKind.Kitap);
        Add("simya_defteri", "Simya Defteri", ItemRarity.Ender, ItemKind.Kitap);
        Add("vakayiname", "Vakayiname", ItemRarity.Ender, ItemKind.Kitap);
        Add("tip_kanunu", "Tip Kanunu", ItemRarity.Ender, ItemKind.Kitap);
        Add("yildiz_ilmi", "Yildiz Ilmi", ItemRarity.Ender, ItemKind.Kitap);

        Add("kanli_dolunay", "Kanli Dolunay Kitabi", ItemRarity.Nadir, ItemKind.BuyuKitabi);
        Add("sifa_kitabi", "Sifa Kitabi", ItemRarity.Nadir, ItemKind.BuyuKitabi);
        Add("yilanin_dili", "Yilanin Dili", ItemRarity.Nadir, ItemKind.BuyuKitabi);
        Add("baykusun_gozu", "Baykusun Gozu", ItemRarity.Nadir, ItemKind.BuyuKitabi);
        Add("akrebin_ignesi", "Akrebin Ignesi", ItemRarity.Efsanevi, ItemKind.BuyuKitabi);

        Add("engizisyon_disi", "Engizisyon Yargicinin Kirik Disi", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("kul_sisesi", "Guneste Yanmis Ilk Kul Sisesi", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("kase_kirigi", "Kutsal Kase Kirigi", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("demir_kazik", "Kadim Demir Kazik Ucu", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("cam_goz", "Kor Kahinin Cam Gozu", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("veba_maskesi", "Veba Maskesi", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("erimis_sikke", "Erimis Altin Sikke", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("kusatma_plani", "Istanbul Kusatma Plani", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("gokturk_tasi", "Gokturk Yaziti", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("kedi_mumyasi", "Aforoz Edilmis Kedi Mumyasi", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("sahte_papa_muhru", "Sahte Papanin Muhuru", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("dans_pabucu", "Dans Eden Koylunun Pabucu", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
        Add("cennet_arsasi", "Cennetten Arsa Makbuzu", ItemRarity.Efsanevi, ItemKind.Koleksiyon, false);
    }

    public static IReadOnlyList<ItemDefinition> All => items;

    public static ItemDefinition Get(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        byId.TryGetValue(id, out ItemDefinition item);
        return item;
    }

    public static int BasePrice(ItemDefinition item)
    {
        if (item == null)
            return 0;

        switch (item.Rarity)
        {
            case ItemRarity.Ender:
                return 24;
            case ItemRarity.Nadir:
                return 60;
            case ItemRarity.Efsanevi:
                return 150;
            default:
                return 10;
        }
    }

    public static int MerchantPrice(ItemDefinition item)
    {
        int basePrice = BasePrice(item);
        if (basePrice <= 0)
            return 0;

        return (basePrice * MarkupNumerator + (MarkupDenominator - 1)) / MarkupDenominator;
    }

    private static void Add(string id, string displayName, ItemRarity rarity, ItemKind kind, bool tradeable = true)
    {
        var item = new ItemDefinition
        {
            Id = id,
            DisplayName = displayName,
            Rarity = rarity,
            Kind = kind,
            Tradeable = tradeable
        };

        items.Add(item);
        byId[id] = item;
    }
}
