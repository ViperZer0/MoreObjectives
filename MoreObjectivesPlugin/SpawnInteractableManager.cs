using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using System;

namespace MoreObjectivesPlugin;

/// <summary>
/// Monitors interactables that spawn via SpawnCard.
/// Captures interactables that are registered and generates objectives for
/// them.
/// </summary>
public class SpawnInteractableManager: MonoBehaviour
{
    // Interactables that have been registered. The key is the SpawnCard name:
    // "iscLockbox" for rusty lockboxes, "iscLockboxVoid" for encrusted caches,
    private Dictionary<string, RegisteredInteractable> registeredInteractables = new();

    // Interactables discovered when each stage is populated. The key is the
    // SpawnCard name (see above).
    private Dictionary<string, InteractableObjectiveController> interactableControllers = new();

    // Maps each tracked interactable to the spawncard name that created it. 
    private Dictionary<GameObject, string> trackedInteractables = new();


    /// <summary>
    /// Holds information about a registered interactable.
    /// </summary>
    private struct RegisteredInteractable
    {
        // Localization token name used to get the objective string
        public string objectiveToken;
    }

    public void Awake()
    {
        Log.Info("SpawnInteractableManager loaded");
    }

    public void OnEnable()
    {
        SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
        SpawnCard.onSpawnedServerGlobal += OnSpawnCardSpawned;
    }

    /// <summary>
    /// Registers an interactable to be tracked.
    /// This will not work if the object the user interacts with is different
    /// than the object that is spawned (e.g multishop terminals). This also
    /// won't work for objects that are not created via SpawnCard (e.g Legendary
    /// Chests). 
    /// for legendary chests 
    /// </summary>
    /// <param name="spawncardName">The name of the spawncard asset. For rusty
    /// lockboxes this is "iscLockbox", for encrusted caches this is
    /// "iscLockboxVoid"</param>
    /// <param name="objectiveToken">The name of the objective token (for
    /// retrieving localized objective string)</param>
    public void RegisterInteractable(string spawncardName, string objectiveToken)
    {
        registeredInteractables.Add(spawncardName, new RegisteredInteractable
                {
                    objectiveToken = objectiveToken,
                });
    }

    /// <summary>
    /// Called when a new stage is loaded. We want to clear the tracked
    /// interactables.
    /// </summary>
    private void ResetAllInteractables()
    {
        trackedInteractables = new();
        interactableControllers = new();
    }

    /// <summary>
    /// Add a tracked interactable. Note that this function doesn't check 
    /// to see if it's an object we're actually tracking, it's assumed that that
    /// is done already.
    /// </summary>
    /// <param name="interactableName">Get the name of the interactable. Mostly
    /// meaningless except for use as a key.</param>
    /// <param name="interactableObject">The object to track interactions with.</param>
    private void AddTrackedInteractable(string interactableName, GameObject interactableObject)
    {
        Log.Info($"Adding tracked interactable: {interactableName}");
        Log.Debug($"Tracking object: {interactableObject}");
        InteractableObjectiveController controller = interactableControllers.ContainsKey(interactableName) ? interactableControllers[interactableName] : ScriptableObject.CreateInstance<InteractableObjectiveController>();
        controller.AddInteractable(interactableObject);
        string objectiveToken = registeredInteractables[interactableName].objectiveToken;
        controller.SetObjectiveToken(objectiveToken);
        trackedInteractables.Add(interactableObject, interactableName);
    }

    /// <summary>
    /// Remove a tracked interactable (because it's been interacted with).
    /// Note that this function doesn't check to see if this is a tracked
    /// interactable, it's assumed that that check has been done and the object
    /// we're passing in is a tracked one.
    /// </summary>
    /// <param name="interactedObject">The object that was interacted with.</param>
    private void RemoveTrackedInteractable(GameObject interactedObject)
    {
        Log.Info($"Removing tracked interactable: {interactedObject}");
        trackedInteractables.Remove(interactedObject);
    }

    /******EVENT HANDLERS*********/
    private void OnPrePopulateSceneServer(SceneDirector director)
    {
        Log.Info("Resetting all tracked interactables");
        ResetAllInteractables();
    }

    private void OnSpawnCardSpawned(SpawnCard.SpawnResult result)
    {
        string spawncardName = result.spawnRequest.spawnCard.name;
        Log.Debug($"Spawned: {spawncardName}");
        // if the spawned object is one we are tracking, add it.
        if(registeredInteractables.ContainsKey(spawncardName))
        {
            GameObject interactable = result.spawnedInstance;
            AddTrackedInteractable(spawncardName, interactable);
        }
    }
}
