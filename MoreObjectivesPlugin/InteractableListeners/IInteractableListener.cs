using System;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin.InteractableListeners;

public struct RegisteredInteractableType
{
    public string objectiveToken;
}

public class InteractableAddedEventArgs
{
    public GameObject interactable;
    public string interactableIdentifier;

    public InteractableAddedEventArgs(string interactableIdentifier, GameObject interactable)
    {
        this.interactableIdentifier = interactableIdentifier;
        this.interactable = interactable;
    }
}

public interface IInteractableListener
{
    public RegisteredInteractableType GetRegisteredInteractableType(string interactableIdentifier);
    public event EventHandler<InteractableAddedEventArgs> EventInteractableAdded;
}
