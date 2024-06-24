using RoR2;
using UnityEngine;

namespace MoreObjectivesPlugin.InteractableEventWrappers;

/// <summary>
/// Factory for creating interactable objective controllers based on what kind
/// of interactable it is and the network condition. 
/// </summary>
public static class InteractableEventWrapperFactory
{
    public static IInteractableEventWrapper CreateInteractableEventWrapper(GameObject componentOwner, GameObject interactableObject)
    {
        // Multishop terminals have to be handled differently.
        if(interactableObject.TryGetComponent<MultiShopController>(out _))
        {
            return componentOwner.AddComponent<MultishopEventWrapper>();
        }
        else
        {
            return componentOwner.AddComponent<ChestEventWrapper>();
        }
    }
}
