using System.Collections.Generic;
using RoR2;
using UnityEngine;

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
        foreach(InteractableObjectiveController controller in interactableControllers.Values)
        {
            controller.Destroy();
        }
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
        // If a controller doesn't already exist, create one and set its
        // objective token.
        if(!interactableControllers.ContainsKey(interactableName))
        {
            interactableControllers[interactableName] = ScriptableObject.CreateInstance<InteractableObjectiveController>();
            interactableControllers[interactableName].SetObjectiveToken(registeredInteractables[interactableName].objectiveToken);
        }

        InteractableObjectiveController controller = interactableControllers[interactableName];
        controller.AddInteractable(interactableObject);
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
        if(registeredInteractables.ContainsKey(spawncardName))
        {
            GameObject interactable = result.spawnedInstance;
            AddTrackedInteractable(spawncardName, interactable);
        }
    }
}
