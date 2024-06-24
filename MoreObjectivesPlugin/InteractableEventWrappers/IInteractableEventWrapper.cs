using System;
using UnityEngine;

namespace MoreObjectivesPlugin.InteractableEventWrappers;

public class InteractableInteractedWithArgs : EventArgs
{
    public GameObject interactable;
    
    public InteractableInteractedWithArgs(GameObject interactable)
    {
        this.interactable = interactable;
    }
}

public interface IInteractableEventWrapper
{
    public void MonitorInteractable(GameObject gameObject);
    public void Destroy();
    public event EventHandler<InteractableInteractedWithArgs> EventInteractableInteractedWith;
}
