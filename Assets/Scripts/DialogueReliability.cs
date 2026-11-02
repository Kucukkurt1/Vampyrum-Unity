using UnityEngine;

public static class DialogueReliability
{
    public static DialogueOutcome RollOutcome(GuestProfile guest, GuestReliabilityTier tier, bool hasWitnessStatement)
    {
        if (guest == null)
            return DialogueOutcome.Truth;

        if (guest.IsVampire)
            return RollVampireOutcome(hasWitnessStatement);

        float truthChance = ReputationSystem.Instance != null
            ? ReputationSystem.Instance.GetTruthChanceMultiplier(tier)
            : tier == GuestReliabilityTier.Unreliable ? 0.5f : 0.9f;

        float roll = Random.value;
        if (roll <= truthChance)
            return DialogueOutcome.Truth;

        if (tier == GuestReliabilityTier.Unreliable)
            return DialogueOutcome.Distorted;

        return DialogueOutcome.Lie;
    }

    private static DialogueOutcome RollVampireOutcome(bool hasWitnessStatement)
    {
        if (!hasWitnessStatement)
            return Random.value <= 0.25f ? DialogueOutcome.Truth : DialogueOutcome.Lie;

        float roll = Random.value;
        if (roll <= 0.1f)
            return DialogueOutcome.Truth;

        if (roll <= 0.35f)
            return DialogueOutcome.Refused;

        return DialogueOutcome.Lie;
    }
}
