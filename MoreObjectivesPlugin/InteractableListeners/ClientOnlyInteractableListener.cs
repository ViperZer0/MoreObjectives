using System.Collections.Generic;
using RoR2;
using UnityEngine;
using System;

namespace MoreObjectivesPlugin.InteractableListeners;

public class ClientOnlyInteractableListener: MonoBehaviour, IInteractableListener
{
    
    /*****Unity Messages*******/
    public void Awake()
    {
        // Register predefined traps we want to keep track of.
        RegisterInteractableType("GoldChest", new RegisteredInteractableType{ objectiveToken = "GOLD_CHEST_OBJECTIVE"});
        RegisterInteractableType("Lockbox(Clone)", new RegisteredInteractableType{ objectiveToken = "LOCKBOX_OBJECTIVE"});
        RegisterInteractableType("LockboxVoid(Clone)", new RegisteredInteractableType{ objectiveToken = "LOCKBOX_VOID_OBJECTIVE"});
        RegisterInteractableType("FreeChest(Clone)", new RegisteredInteractableType{ objectiveToken = "FREE_CHEST_OBJECTIVE"});
    }

    public void OnEnable()
    {
        Stage.onStageStartGlobal += OnStageLoaded;
    }

    public void OnDisable()
    {
        Stage.onStageStartGlobal -= OnStageLoaded;
    }

    // Fires when an object we are tracking is found/added and should be tracked.
    public event EventHandler<InteractableAddedEventArgs> EventInteractableAdded;

    public RegisteredInteractableType GetRegisteredInteractableType(string interactableName)
    {
        return registeredInteractables[interactableName];
    }
    /********Internal methods******/
    private void RegisterInteractableType(string interactableName, RegisteredInteractableType registeredInteractableType)
    {
        Log.Debug($"Registering interactable {interactableName}");
        registeredInteractables.Add(interactableName, registeredInteractableType);
    }

    /********Event Handlers********/
    private void OnStageLoaded(Stage stage)
    {
        Log.Info("ClientOnlyInteractableListener: New stage, searching for tracked interactables");
        foreach(KeyValuePair<string, RegisteredInteractableType> kvp in registeredInteractables)
        {
            string interactableName = kvp.Key;
            RegisteredInteractableType interactableType = kvp.Value;
            // Note that GameObject.Find() will only ever return one object, so
            // any objectives generated should only ever have one tracked
            // interacatble. It's still recommended to include a _PLURAL
            // language token when localizing however, just in case. 
            GameObject foundObject = GameObject.Find(interactableName);
            if(foundObject != null)
            {
                EventInteractableAdded?.Invoke(this, new InteractableAddedEventArgs(interactableName, foundObject));
            }
        }
    }

    // Tracks registered interactables.
    private Dictionary<string, RegisteredInteractableType> registeredInteractables = new();
}
