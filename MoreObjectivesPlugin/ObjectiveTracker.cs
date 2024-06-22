using System.Collections.Generic;
using MoreObjectivesPlugin.Configuration;
using MoreObjectivesPlugin.InteractableListeners;
using MoreObjectivesPlugin.InteractableObjectiveControllers;
using UnityEngine;

namespace MoreObjectivesPlugin;

public class ObjectiveTracker: MonoBehaviour
{
    public void Awake()
    {
        Log.Debug("ObjectiveTracker awake!");
    }

    public void OnEnable()
    {
        SubscribeToInteractableListener(gameObject.GetComponent<MoreObjectivesPlugin>().InteractableListener);
    }

    public void OnDisable()
    {
        if(interactableListener != null)
        {
            interactableListener.EventInteractableAdded -= OnInteractableAdded;
        }
    }

    public void SubscribeToInteractableListener(IInteractableListener interactableListener)
    {
        Log.Debug("Subscribing to interactable listener");
        this.interactableListener = interactableListener;
        interactableListener.EventInteractableAdded += OnInteractableAdded;
    }

    private void OnInteractableAdded(object sender, InteractableAddedEventArgs args)
    {
        // Only add interactable if user has it enabled
        if(!ConfigurationManager.GetConfigValueByInteractableName(args.interactableIdentifier))
        {
            return;
        }
        
        AddTrackedInteractable(args.interactableIdentifier, args.interactable);
    }
    
    private void AddTrackedInteractable(string interactableName, GameObject interactable)
    {
        IInteractableObjectiveController controller = trackedInteractables.ContainsKey(interactableName) ? trackedInteractables[interactableName] : InteractableObjectiveControllerFactory.CreateInteractableObjectiveController(gameObject, interactable);
        controller.AddInteractable(interactable);
        controller.SetObjectiveToken(interactableListener.GetRegisteredInteractableType(interactableName).objectiveToken);
        trackedInteractables[interactableName] = controller;
    }

    private void ResetTrackedInteractables()
    {
        foreach(IInteractableObjectiveController controller in trackedInteractables.Values)
        {
            controller.Destroy();
        }
        trackedInteractables = new();
    }

    private IInteractableListener? interactableListener = null;

    private Dictionary<string, IInteractableObjectiveController> trackedInteractables = new();
}
