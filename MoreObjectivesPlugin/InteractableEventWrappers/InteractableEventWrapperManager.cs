using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin.InteractableEventWrappers;

public class InteractableEventWrapperManager: NetworkBehaviour
{
    [ClientRpc]
    public IInteractableEventWrapper TrackInteractable(GameObject interactable)
    {
        IInteractableEventWrapper eventWrapper = InteractableEventWrapperFactory.CreateInteractableEventWrapper(gameObject, interactable);
        eventWrapper.MonitorInteractable(interactable);
        eventWrappers.Add(eventWrapper);
        return eventWrapper;
    }

    [ClientRpc]
    public void RemoveEventWrapper(IInteractableEventWrapper eventWrapper)
    {
        eventWrappers.Remove(eventWrapper);
        eventWrapper.Destroy();
    }

    public void RemoveAllEventWrappers()
    {
        foreach(IInteractableEventWrapper eventWrapper in eventWrappers)
        {
            eventWrapper.Destroy();
        }
        eventWrappers = new();
    }

    private List<IInteractableEventWrapper> eventWrappers = new();
}
