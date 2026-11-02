using System;
using UnityEngine;

public enum GuestDecision
{
    Pending,
    Accepted,
    Rejected
}

[Serializable]
public class GuestProfile
{
    public string Id;
    public int CatalogId;
    public string DisplayName;
    public string Description;
    public string RoomName;
    public string RoomSlotId;

    public int UcretGunluk = 3;
    public int MaliyetGunluk = 2;
    public int PlanlananGun = 5;
    public int KalinanGun;
    public int OnOdemeAlindi;

    public bool IsVampire;
    public GuestDecision Decision = GuestDecision.Pending;
    public bool IsStaying;
    public bool IsInDungeon;
    public bool IsProtectedTonight;
    public bool IsAlive = true;

    public bool IsPending => Decision == GuestDecision.Pending;
    public bool IsAccepted => Decision == GuestDecision.Accepted;

    public int KalanGun => Mathf.Max(0, PlanlananGun - KalinanGun);

    public int PrepaymentTotal => UcretGunluk * PlanlananGun;

    public int CalculateRefund() => KalanGun * UcretGunluk;

    public bool CanBeKilledTonight =>
        IsAlive && IsAccepted && IsStaying && !IsInDungeon && !IsProtectedTonight && !IsVampire;

    public bool CanAttackTonight =>
        IsAlive && IsVampire && IsAccepted && IsStaying && !IsInDungeon;
}
