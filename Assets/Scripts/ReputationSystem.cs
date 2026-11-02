using UnityEngine;

public class ReputationSystem : MonoBehaviour
{
    public static ReputationSystem Instance { get; private set; }

    [SerializeField] private int reputation = 70;
    [SerializeField] private int minReputation = 0;
    [SerializeField] private int maxReputation = 100;

    public int Reputation => reputation;
    public bool IsLowReputation => reputation < 40;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void AdjustReputation(int delta)
    {
        reputation = Mathf.Clamp(reputation + delta, minReputation, maxReputation);
    }

    public static int ScaleGuestPay(int basePay)
    {
        if (Instance == null || basePay <= 0)
            return basePay;

        int standing = Instance.reputation;
        float multiplier = standing >= 90 ? 1.4f : standing >= 80 ? 1.2f : standing < 40 ? 0.8f : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(basePay * multiplier));
    }

    public float GetTruthChanceMultiplier(GuestReliabilityTier tier)
    {
        float baseMultiplier = tier == GuestReliabilityTier.Unreliable ? 0.55f : 0.9f;
        if (IsLowReputation)
            baseMultiplier *= 0.65f;

        return baseMultiplier;
    }
}
