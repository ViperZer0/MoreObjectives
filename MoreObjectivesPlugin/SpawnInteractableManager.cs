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
public class SpawnInteractableManager: MonoBehaviour, IObjectiveCollectable
{
    // Interactables that have been registered. The key is the SpawnCard name:
    // "iscLockbox" for rusty lockboxes, "iscLockboxVoid" for encrusted caches,
    private Dictionary<string, RegisteredInteractable> registeredInteractables = new();

    // Interactables discovered when each stage is populated. The key is the
    // SpawnCard name (see above).
    private Dictionary<string, DiscoveredInteractables> trackedInteractableData = new();

    // Maps each tracked interactable to the spawncard name that created it. 
    private Dictionary<GameObject, string> trackedInteractables = new();


    /// <summary>
    /// Holds information about a registered interactable.
    /// </summary>
    private struct RegisteredInteractable
    {
        // Localization token name used to get the objective string
        public string objectiveToken;

        // Function used to extract the interactable game object from a captured
        // SpawnResult. For most things like chests this is just
        // spawnResult.spawnedInstance but for things like multishop terminals
        // it is not.
        public Func<SpawnCard.SpawnResult,GameObject> getInteractableObject;
    }

    /// <summary>
    /// When a new interactable we're tracking is discovered, this holds the
    /// reference to it as well as the total count of interactables that have
    /// been added (of this type) and how many of them have been interacted with 
    /// </summary>
    private class DiscoveredInteractables: UnityEngine.Object
    {
        // When an interactable has been activated, this is incremented. 
        // When it equals totalInteractables, the objective is considered
        // complete.
        public int interactablesActivated = 0;
        // Total number of interactables. This should be equal to
        // spawnedInteractables.Count
        public int totalInteractables = 0;
        // The list of interactables discovered.
        public List<GameObject> spawnedInteractables = new();
    }

    public void Awake()
    {
        Log.Info("SpawnInteractableManager loaded");
    }

    public void OnEnable()
    {
        SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
        SpawnCard.onSpawnedServerGlobal += OnSpawnCardSpawned;
        GlobalEventManager.OnInteractionsGlobal += OnGlobalInteraction;
    }

    /// <summary>
    /// Registers an interactable to be tracked using some default parameters. 
    /// This will not work if the object the user interacts with is different
    /// than the object that is spawned (e.g multishop terminals). This also
    /// won't work for objects that are not created via SpawnCard (e.g Legendary
    /// Chests). For multishop terminals <see cref="RegisterUniqueInteractable"/>,
    /// for legendary chests 
    /// </summary>
    /// <param name="spawncardName">The name of the spawncard asset. For rusty
    /// lockboxes this is "iscLockbox", for encrusted caches this is
    /// "iscLockboxVoid"</param>
    /// <param name="objectiveToken">The name of the objective token (for
    /// retrieving localized objective string)</param>
    public void RegisterDefaultInteractable(string spawncardName, string objectiveToken)
    {
        RegisterUniqueInteractable(spawncardName, objectiveToken, (result) => result.spawnedInstance);
    }

    /// <summary>
    /// Registers an interactable with a custom method for getting the
    /// interactable object. This is useful if the interactable object is different
    /// than the one spawned by SpawnCard, which is the case with multishop
    /// terminals. However, this class still only expects *one* interactable
    /// object. If you want to have groups of interactables, any of which can
    /// trigger an objective, <see cref="MultipleInteractableManager/>
    /// </summary>
    /// <param name="spawncardName">The name of the spawncard asset.</param>
    /// <param name="objectiveToken">The name of the objective token.</param>
    /// <param name="getInteractableObject">The function to extract the
    /// interactable GameObject from the SpawnCard.SpawnResult.</param>
    public void RegisterUniqueInteractable(string spawncardName, string objectiveToken, Func<SpawnCard.SpawnResult, GameObject> getInteractableObject)
    {
        Log.Info($"Registering interactable {spawncardName}");
        registeredInteractables.Add(spawncardName, new RegisteredInteractable
                {
                    objectiveToken = objectiveToken,
                    getInteractableObject = getInteractableObject,
                });
    }

    /// <summary>
    /// Called when a new stage is loaded. We want to clear the tracked
    /// interactables.
    /// </summary>
    private void ResetAllInteractables()
    {
        trackedInteractables = new();
        trackedInteractableData = new();
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
        DiscoveredInteractables data = trackedInteractableData.ContainsKey(interactableName) ? trackedInteractableData[interactableName] : new DiscoveredInteractables();
        data.totalInteractables += 1;
        data.spawnedInteractables.Add(interactableObject);
        trackedInteractableData[interactableName] = data;
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
        string objectName = trackedInteractables[interactedObject];
        DiscoveredInteractables interactableData = trackedInteractableData[objectName];
        interactableData.interactablesActivated++;
        interactableData.spawnedInteractables.Remove(interactedObject);
    }

    /// <summary>
    /// Checks whether an objective is complete
    /// </summary>
    /// <param name="data">The interactable data</param>
    /// <returns>True if the objective is complete, false otherwise.</returns>
    private bool ObjectiveComplete(DiscoveredInteractables data)
    {
        return data.interactablesActivated == data.totalInteractables;
    }

    /// <summary>
    /// Checks if the legendary chest is on stage. We have to manually search for the
    /// legendary chest by name.
    /// </summary>
    /// <returns>True if the legendary chest is on stage, false otherwise.</returns>
    private bool IsGoldChestOnStage()
    {
        GameObject goldChest = GameObject.Find("GoldChest");
        return goldChest != null;
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
            GameObject interactable = registeredInteractables[spawncardName].getInteractableObject(result);
            AddTrackedInteractable(spawncardName, interactable);
        }
    }

    private void OnGlobalInteraction(Interactor interactor, IInteractable interactable, GameObject interactableObject)
    {
        // If the object interacted with is one we're tracking, register that
        // interaction.
        if(trackedInteractables.ContainsKey(interactableObject))
        {
            RemoveTrackedInteractable(interactableObject);
        }
    }

    public IEnumerable<ObjectiveManager.InteractableObjectiveSource> GetObjectives()
    {
        return trackedInteractableData.Keys.Where((key) => !ObjectiveComplete(trackedInteractableData[key]))
            .Select((key) => {
                    RegisteredInteractable registration = registeredInteractables[key];
                    DiscoveredInteractables interactableData = trackedInteractableData[key];
                    return new ObjectiveManager.InteractableObjectiveSource {
                        sourceId = key,
                        interactablesActivated = interactableData.interactablesActivated,
                        totalInteractables = interactableData.totalInteractables,
                        objectiveToken = registration.objectiveToken
                    };
                });
    }
}
