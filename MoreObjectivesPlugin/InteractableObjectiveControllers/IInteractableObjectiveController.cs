using UnityEngine;

namespace MoreObjectivesPlugin;

public interface IInteractableObjectiveController
{
    public void AddInteractable(GameObject gameObject);
    public void Destroy();
    public void SetObjectiveToken(string objectiveToken);
}
