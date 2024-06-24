using MoreObjectivesPlugin.Networking;
using RoR2;
using UnityEngine;

namespace MoreObjectivesPlugin.InteractableObjectiveControllers;

/// <summary>
/// Factory for creating interactable objective controllers based on what kind
/// of interactable it is and the network condition. 
/// </summary>
public static class InteractableObjectiveControllerFactory
{
    public static IInteractableObjectiveController CreateInteractableObjectiveController(GameObject componentOwner, GameObject interactableObject)
    {
        // We can only use very limited features if we are running client-side
        // only.
        // Notably, we can't ever actually check objectives off. All we can do
        // is display them.
        if(!NetworkCheck.ServerExists.Value)
        {
            return componentOwner.AddComponent<NoOpInteractableObjectiveController>();
        }
        else
        {
            return componentOwner.AddComponent<InteractableObjectiveController>();
        }
    }
}
