using UnityEngine;

/// <summary>
/// Büyücü odası sahne geçişine ekle. Oda kilitliyken SceneTransition devre dışı kalır.
/// </summary>
[RequireComponent(typeof(SceneTransition))]
public class WizardRoomEnterGate : MonoBehaviour
{
    private SceneTransition sceneTransition;

    private void Awake()
    {
        sceneTransition = GetComponent<SceneTransition>();
        UpdateGate();
    }

    private void Update()
    {
        UpdateGate();
    }

    private void UpdateGate()
    {
        if (sceneTransition == null)
            return;

        bool canEnter = WizardRoomManager.Instance != null && WizardRoomManager.Instance.IsUnlocked;
        sceneTransition.enabled = canEnter;
    }
}
