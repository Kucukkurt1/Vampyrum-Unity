using UnityEngine.EventSystems;

/// <summary>
/// UI tiklamalari Time.timeScale = 0 iken de calissin.
/// </summary>
public class UnscaledStandaloneInputModule : StandaloneInputModule
{
    public override void Process()
    {
        if (eventSystem != null && !eventSystem.isActiveAndEnabled)
            return;

        base.Process();
    }
}
