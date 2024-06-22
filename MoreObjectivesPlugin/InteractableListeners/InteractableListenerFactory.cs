using MoreObjectivesPlugin.Networking;
using UnityEngine;

namespace MoreObjectivesPlugin.InteractableListeners;

public static class InteractableListenerFactory
{
    public static IInteractableListener CreateInteractableListener(GameObject componentOwner)
    {
        if(NetworkCheck.ServerExists.Value)
        {
            return componentOwner.AddComponent<ServerInteractableListener>();
        }
        else
        {
            return componentOwner.AddComponent<ClientOnlyInteractableListener>();
        }
    }
}
