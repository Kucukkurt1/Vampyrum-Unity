using System.Collections.Generic;

using System.Linq;

using System.Text;

using UnityEngine;



public class NightSimulationResult

{

    public bool MurderOccurred;

    public string VictimGuestId;

    public string VictimName;

    public string VictimRoom;

    public string AttackerGuestId;

    public int ActiveVampireCount;

    public readonly List<string> EventLines = new List<string>();

}



public static class NightSimulator

{

    private const float WitnessHeardChance = 0.45f;

    private const float WitnessSawLeaveChance = 0.3f;



    public static NightSimulationResult Simulate(float attackChance)

    {

        var result = new NightSimulationResult();



        if (GuestManager.Instance == null)

        {

            result.EventLines.Add("Gece sessiz gecti. (Misafir kaydi yok)");

            return result;

        }



        GuestManager guestManager = GuestManager.Instance;

        List<GuestProfile> attackers = guestManager.GetAttackersTonight();

        List<GuestProfile> victims = guestManager.GetPotentialVictimsTonight();



        result.ActiveVampireCount = guestManager.CountActiveVampires();



        if (attackers.Count == 0)

        {

            result.EventLines.Add("Gece sessiz gecti.");

            return result;

        }



        if (victims.Count == 0)

        {

            result.EventLines.Add("Gece gergin gecti ama kimse olmedi.");

            return result;

        }



        bool attackHappened = Random.value <= attackChance;

        if (!attackHappened)

        {

            result.EventLines.Add("Gece bir saldiri girisimini engellediniz veya saldirgan vazgecti.");

            return result;

        }



        GuestProfile attacker = attackers[Random.Range(0, attackers.Count)];

        GuestProfile victim = victims[Random.Range(0, victims.Count)];

        guestManager.KillGuest(victim.Id);



        result.MurderOccurred = true;

        result.VictimGuestId = victim.Id;

        result.VictimName = victim.DisplayName;

        result.VictimRoom = victim.RoomName;

        result.AttackerGuestId = attacker.Id;

        result.ActiveVampireCount = guestManager.CountActiveVampires();



        result.EventLines.Add($"CINAYET: {victim.DisplayName} ({victim.RoomName}) gece oldu.");

        result.EventLines.Add("Sabah ceset bulundu. Sorusturma gerekli.");



        RecordMurderInJournal(attacker, victim);



        return result;

    }



    private static void RecordMurderInJournal(GuestProfile attacker, GuestProfile victim)

    {

        if (FactJournal.Instance == null || attacker == null || victim == null)

            return;



        int night = GameManager.Instance != null ? GameManager.Instance.CurrentDay : 0;

        var witnesses = GenerateWitnesses(attacker, victim);



        var record = new NightMurderRecord

        {

            Night = night,

            VictimGuestId = victim.Id,

            VictimName = victim.DisplayName,

            VictimRoom = victim.RoomName,

            AttackerGuestId = attacker.Id,

            Witnesses = witnesses.ToArray()

        };



        FactJournal.Instance.RecordMurder(record);

    }



    private static List<WitnessStatementRecord> GenerateWitnesses(GuestProfile attacker, GuestProfile victim)

    {

        var witnesses = new List<WitnessStatementRecord>();

        if (GuestManager.Instance == null)

            return witnesses;



        List<GuestProfile> candidates = GuestManager.Instance.GetStayingGuests()

            .Where(guest =>

                guest.IsAlive &&

                guest.Id != victim.Id &&

                guest.Id != attacker.Id)

            .ToList();



        foreach (GuestProfile candidate in candidates)

        {

            WitnessType type = RollWitnessType(candidate, victim);

            if (type == WitnessType.None)

                continue;



            witnesses.Add(new WitnessStatementRecord

            {

                WitnessGuestId = candidate.Id,

                Type = type,

                SubjectGuestId = type == WitnessType.SawPersonLeave ? attacker.Id : string.Empty,

                SubjectName = type == WitnessType.SawPersonLeave ? attacker.DisplayName : string.Empty,

                VictimRoom = victim.RoomName,

                PendingPlayerTalk = true,

                DeliveredOutcome = DialogueOutcome.Truth

            });

        }



        return witnesses;

    }



    private static WitnessType RollWitnessType(GuestProfile candidate, GuestProfile victim)

    {

        bool sameRoomArea = !string.IsNullOrEmpty(candidate.RoomName) &&

                            candidate.RoomName == victim.RoomName;



        if (sameRoomArea && Random.value <= WitnessSawLeaveChance)

            return WitnessType.SawPersonLeave;



        if (Random.value <= WitnessHeardChance)

            return WitnessType.HeardNoise;



        if (Random.value <= WitnessSawLeaveChance * 0.5f)

            return WitnessType.SawPersonLeave;



        return WitnessType.None;

    }



    public static string FormatNightSection(NightSimulationResult result)

    {

        var section = new StringBuilder();

        section.AppendLine("--- Gece Olaylari ---");



        foreach (string line in result.EventLines)

            section.AppendLine(line);



        return section.ToString().TrimEnd();

    }

}


