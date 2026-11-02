using System;
using UnityEngine;

public class WizardRoomManager : MonoBehaviour
{
    public static WizardRoomManager Instance { get; private set; }

    [SerializeField] private int unlockCost = 50;

    public bool IsUnlocked { get; private set; }
    public int UnlockCost => unlockCost;

    public event Action OnRoomStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public bool TryUnlockRoom()
    {
        if (IsUnlocked || GameManager.Instance == null)
            return false;

        if (!GameManager.Instance.SpendGold(unlockCost))
            return false;

        IsUnlocked = true;
        OnRoomStateChanged?.Invoke();
        return true;
    }

    public bool CanUseGrimoirePurchase() =>
        IsUnlocked && StaffManager.Instance != null && StaffManager.Instance.HasWizardStaff;
}
