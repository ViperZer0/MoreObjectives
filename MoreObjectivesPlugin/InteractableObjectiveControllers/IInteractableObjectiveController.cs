using UnityEngine;

namespace MoreObjectivesPlugin;

public interface IInteractableObjectiveController
{
    public void Destroy();
    public void AddInteractable(GameObject gameObject);
    public void SetObjectiveToken(string objectiveToken);
}
