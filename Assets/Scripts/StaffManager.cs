using System;
using UnityEngine;

public enum StaffRole
{
    Wizard,
    Watch,
    Cook,
    Runner
}

public class StaffManager : MonoBehaviour
{
    public static StaffManager Instance { get; private set; }

    private readonly int[] levels = new int[4];

    public bool HasWizardStaff => Level(StaffRole.Wizard) > 0;
    public int WizardHireCost => HirePrice(StaffRole.Wizard, 1);
    public string LastWatchReport { get; private set; } = "";
    public string LastCraftMessage { get; private set; } = "";

    public event Action OnStaffChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public int Level(StaffRole role) => levels[(int)role];

    public int NightlyWage
    {
        get
        {
            int total = 0;
            foreach (StaffRole role in Enum.GetValues(typeof(StaffRole)))
                total += Wage(role);
            return total;
        }
    }

    public int Wage(StaffRole role)
    {
        switch (Level(role))
        {
            case 1: return 2;
            case 2: return 4;
            case 3: return 7;
            default: return 0;
        }
    }

    public int HirePrice(StaffRole role, int level)
    {
        switch (role)
        {
            case StaffRole.Wizard:
                return level == 2 ? 70 : level == 3 ? 130 : 30;
            case StaffRole.Watch:
                return level == 2 ? 56 : level == 3 ? 104 : 24;
            case StaffRole.Cook:
                return level == 2 ? 48 : level == 3 ? 88 : 20;
            default:
                return level == 2 ? 84 : level == 3 ? 154 : 36;
        }
    }

    public string Title(StaffRole role)
    {
        switch (role)
        {
            case StaffRole.Wizard: return "Büyücü";
            case StaffRole.Watch: return "Nöbetçi";
            case StaffRole.Cook: return "Aşçı";
            default: return "Seferci";
        }
    }

    public string Describe(StaffRole role, int level)
    {
        string text;
        switch (role)
        {
            case StaffRole.Wizard:
                text = WizardText(level);
                break;
            case StaffRole.Watch:
                text = WatchText(level);
                if (Level(role) == level && !string.IsNullOrEmpty(LastWatchReport))
                    text += "\n\nSon rapor: " + LastWatchReport;
                break;
            case StaffRole.Cook:
                text = CookText(level);
                break;
            default:
                text = RunnerText(level);
                break;
        }

        int employed = Level(role);
        if (employed == level)
            text += "\n\nBu kişi şu an çalışıyor.";
        else if (employed > 0)
            text += "\n\nŞu an seviye " + employed + " çalışıyor. Bunu alırsan o aynı gün gider. Ödenen altın geri gelmez.";

        return text;
    }

    private static string WizardText(int level)
    {
        switch (level)
        {
            case 1:
                return "Çırağı yeni. Kazanı kurar ama eli titrer. Büyüyü on denemeden dördünde tutturur. "
                    + "Tutmayınca ay otu, gümüş tozu ve diğer malzeme yine yanar. Şişe boş kalır, iksir çıkmaz. "
                    + "Gece iki altın ister. Kapıda bir büyücü durur. Başkasını alırsan bu gider.";
            case 2:
                return "İşi öğrenmiş bir kaynatıcı. Büyüyü on denemeden yedisinde tutturur. "
                    + "Tutmayan büyü yine malzemeyi yer, şişeyi doldurmaz. "
                    + "Gece dört altın ister. Hanın tek büyücüsü olur. Daha ustasını ya da çırağı alırsan bu aynı gün ayrılır.";
            default:
                return "Usta. Büyüyü on denemeden dokuzunda tutturur. Boşa giden kaynama seyrektir. "
                    + "Giden yine malzemedir. Şişe ancak büyü tutunca dolur. "
                    + "Gece yedi altın ister. Bunu alırsan daha ucuz büyücü aynı akşam kapıdan çıkar.";
        }
    }

    private static string WatchText(int level)
    {
        switch (level)
        {
            case 1:
                return "Koridoru tek mumla yürür. Gece ölme ihtimali yüzde yirmi. Ölürse kadro boşalır ve o sabah rapor da kesilir. "
                    + "Sağ kalırsa tek satır yazar: koridorda ses vardı, ya da gece sakindi. Oda adı yoktur, kapıdan çıkanı da söylemez. "
                    + "Gece iki altın ister.";
            case 2:
                return "Daha temkinli yürür. Ölme ihtimali yüzde sekiz. Cinayet gecesi hangi odanın kapısının açıldığını yazar. "
                    + "Kimseyi adıyla söylemez. Sessiz gecede kapıların kapalı kaldığını yazar. "
                    + "Ölürse yer boş kalır. Gece dört altın ister.";
            default:
                return "Eski nöbetçi. Ölme ihtimali yüzde üç. Rapor odanın adını ve kapıdan birinin çıktığını yazar. "
                    + "Yüzü vermez. Katili sen ayırırsın. Sessiz gecede koridorda kimse olmadığını da yazar. "
                    + "Gece yedi altın ister. Sağ kaldığı sabah rapor bu sayfada durur.";
        }
    }

    private static string CookText(int level)
    {
        string pay = "İtibar kırkın altındayken misafir yüzde yirmi az öder. Seksen olunca yüzde yirmi, doksan olunca yüzde kırk fazla öder. Han yetmiş itibarla açılır. ";
        switch (level)
        {
            case 1:
                return "Sade yemek çıkarır. Her gece itibar bir artar. " + pay
                    + "Bu aşçıyla seksen birkaç gece sürer. Gece iki altın ister. İkinci bir aşçı tutulmaz.";
            case 2:
                return "Sofrayı düzgün kurar. Her gece itibar iki artar. " + pay
                    + "Gece dört altın ister. Bunu alırsan diğer aşçı aynı gün ayrılır.";
            default:
                return "Misafirin andığı yemek bu mutfaktan çıkar. Her gece itibar dört artar. " + pay
                    + "Birkaç günde ödeme yükselir. Gece yedi altın ister. Daha ucuz aşçı aynı akşam gider.";
        }
    }

    private static string RunnerText(int level)
    {
        switch (level)
        {
            case 1:
                return "Sefer masasından dışarı çıkar. Dönüşü kesin değildir. Ölme ihtimali yüzde yirmi beş. "
                    + "Sağ dönerse tek eşya getirir ve o eşya sıradandır. Ölürse kadro boşalır, yük gelmez. "
                    + "Gece iki altın ister. Masa henüz ayrı kurulacak. Kimi tuttuğun bu sayfada belli olur.";
            case 2:
                return "Yolu bilir. Ölme ihtimali yüzde on. İki eşya getirir. Arada ender parça çıkar, çoğu yine sıradandır. "
                    + "Ölürse ne kendisi ne yük döner. Gece dört altın ister. Sefer, masa açılınca bu kişinin seviyesine göre yürür.";
            default:
                return "Eski kervancı. Ölme ihtimali yüzde üç. Üç eşya getirir. Nadir parça çıkabilir. "
                    + "Yine de her dönüş şölen değildir. Boş bir gece de olabilir, ölümü seyrektir. "
                    + "Gece yedi altın ister. Bunu alırsan acemi seferci aynı gün ayrılır.";
        }
    }

    public bool TryHireWizard()
    {
        if (HasWizardStaff)
            return false;

        return TryHire(StaffRole.Wizard, 1);
    }

    public bool TryHire(StaffRole role, int level)
    {
        if (level < 1 || level > 3 || GameManager.Instance == null)
            return false;

        if (Level(role) == level)
            return false;

        if (!GameManager.Instance.SpendGold(HirePrice(role, level)))
            return false;

        if (role == StaffRole.Watch)
            LastWatchReport = "";

        levels[(int)role] = level;
        OnStaffChanged?.Invoke();
        return true;
    }

    public bool RollCraftSuccess()
    {
        int level = Level(StaffRole.Wizard);
        if (level <= 0)
        {
            LastCraftMessage = "Büyücü yok.";
            return false;
        }

        bool success = UnityEngine.Random.value <= CraftPercent(level) / 100f;
        LastCraftMessage = success ? "Büyü tuttu." : "Büyü tutmadı. Malzeme gitti.";
        return success;
    }

    public int CraftPercent(int level)
    {
        switch (level)
        {
            case 1: return 40;
            case 2: return 70;
            case 3: return 90;
            default: return 0;
        }
    }

    public void ResolveNight(NightSimulationResult night)
    {
        int cook = Level(StaffRole.Cook);
        if (cook > 0 && ReputationSystem.Instance != null)
            ReputationSystem.Instance.AdjustReputation(CookReputation(cook));

        int watch = Level(StaffRole.Watch);
        if (watch <= 0)
        {
            OnStaffChanged?.Invoke();
            return;
        }

        if (UnityEngine.Random.value < WatchDeathChance(watch))
        {
            levels[(int)StaffRole.Watch] = 0;
            LastWatchReport = "Nöbetçi gece öldü.";
            night?.EventLines.Add(LastWatchReport);
            OnStaffChanged?.Invoke();
            return;
        }

        LastWatchReport = BuildWatchReport(watch, night);
        night?.EventLines.Add("Nöbetçi: " + LastWatchReport);
        OnStaffChanged?.Invoke();
    }

    public float RunnerDeathChanceForCurrent() => RunnerDeathChance(Level(StaffRole.Runner));

    public int RunnerHaulForCurrent() => RunnerHaul(Level(StaffRole.Runner));

    public int RunnerRarityCapForCurrent()
    {
        switch (Level(StaffRole.Runner))
        {
            case 1: return 0;
            case 2: return 1;
            case 3: return 2;
            default: return -1;
        }
    }

    private static int CookReputation(int level)
    {
        switch (level)
        {
            case 1: return 1;
            case 2: return 2;
            case 3: return 4;
            default: return 0;
        }
    }

    private static float WatchDeathChance(int level)
    {
        switch (level)
        {
            case 1: return 0.20f;
            case 2: return 0.08f;
            case 3: return 0.03f;
            default: return 0f;
        }
    }

    private static float RunnerDeathChance(int level)
    {
        switch (level)
        {
            case 1: return 0.25f;
            case 2: return 0.10f;
            case 3: return 0.03f;
            default: return 0f;
        }
    }

    private static int RunnerHaul(int level)
    {
        switch (level)
        {
            case 1: return 1;
            case 2: return 2;
            case 3: return 3;
            default: return 0;
        }
    }

    private static string RunnerRarity(int level)
    {
        switch (level)
        {
            case 1: return "Getirdikleri sıradan.";
            case 2: return "Ender eşya çıkabilir.";
            default: return "Nadir eşya çıkabilir.";
        }
    }

    private static string BuildWatchReport(int level, NightSimulationResult night)
    {
        bool murder = night != null && night.MurderOccurred;
        if (!murder)
        {
            if (level >= 3)
                return "Bütün kapılar sürgülüydü. Koridorda kimse yoktu.";
            if (level == 2)
                return "Kapılar kapalı kaldı.";
            return "Koridor sakindi.";
        }

        string room = string.IsNullOrEmpty(night.VictimRoom) ? "bir oda" : night.VictimRoom;
        if (level >= 3)
            return room + " kapısı açıldı. İçeriden biri çıktı.";
        if (level == 2)
            return room + " kapısı açıldı.";
        return "Koridorda ses vardı.";
    }
}
