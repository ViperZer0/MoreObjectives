using System.Collections.Generic;
using MoreObjectivesPlugin.Configuration;
using MoreObjectivesPlugin.InteractableListeners;
using MoreObjectivesPlugin.InteractableObjectiveControllers;
using RoR2;
using UnityEngine;

namespace MoreObjectivesPlugin;

public class ObjectiveTracker: MonoBehaviour
{
    public void Awake()
    {
        Log.Debug("ObjectiveTracker awake!");
        this.interactableListener = gameObject.GetComponent<IInteractableListener>();
    }

    public void OnEnable()
    {
        Log.Debug("Subscribing to interactable listener");
        interactableListener.EventInteractableAdded += OnInteractableAdded;
        // Called on both client and server.
        Stage.onStageStartGlobal += OnNewStageLoaded;
    }

    public void OnDisable()
    {
        if(interactableListener != null)
        {
            interactableListener.EventInteractableAdded -= OnInteractableAdded;
        }
        Stage.onStageStartGlobal -= OnNewStageLoaded;
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

    private void OnNewStageLoaded(Stage stage)
    {
        // This is SUPER hacky but because the Stage.onStageStartGlobal event
        // fires after we load all the interactables, we basically need to
        // offset it so that the 
        ResetTrackedInteractables();
    }
    
    private void AddTrackedInteractable(string interactableName, GameObject interactable)
    {
        Log.Debug($"Adding tracked interactable {interactableName}");
        IInteractableObjectiveController controller = newTrackedInteractables.ContainsKey(interactableName) ? newTrackedInteractables[interactableName] : InteractableObjectiveControllerFactory.CreateInteractableObjectiveController(gameObject, interactable);
        controller.AddInteractable(interactable);
        controller.SetObjectiveToken(interactableListener.GetRegisteredInteractableType(interactableName).objectiveToken);
        newTrackedInteractables[interactableName] = controller;
    }

    private void ResetTrackedInteractables()
    {
        Log.Debug("Resetting tracked interactables");
        foreach(IInteractableObjectiveController controller in trackedInteractables.Values)
        {
            controller.Destroy();
        }
        // Load the new interactables that we've been caching.
        trackedInteractables = newTrackedInteractables;
        newTrackedInteractables = new();
    }

    private IInteractableListener? interactableListener = null;

    private Dictionary<string, IInteractableObjectiveController> trackedInteractables = new();
    // Basically because the Stage.onStageStartGlobal event fires after
    // the OnInteractableAdded event, we want to pre-load new interactables into
    // a separate dictionary, delete the old interactables, then replace them
    // with the new ones.
    private Dictionary<string, IInteractableObjectiveController> newTrackedInteractables = new();
}
