using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin.InteractableListeners;

/// <summary>
/// Searches for and broadcasts interactables to track using methods that are
/// only available to the server.
/// </summary>
public class ServerInteractableListener: NetworkBehaviour, IInteractableListener
{
    public void Awake()
    {
        if(!isServer)
        {
            Log.Info("Running on client, not registering interactables.");
            return;
        }
        Log.Debug("ServerInteractableListener loaded");
        RegisterInteractableType("GoldChest", new RegisteredInteractableType{ objectiveToken = "GOLD_CHEST_OBJECTIVE"});
        RegisterInteractableType("iscLockbox", new RegisteredInteractableType{ objectiveToken = "LOCKBOX_OBJECTIVE"});
        RegisterInteractableType("iscLockboxVoid", new RegisteredInteractableType{ objectiveToken = "LOCKBOX_VOID_OBJECTIVE"});
        RegisterInteractableType("iscFreeChest", new RegisteredInteractableType{ objectiveToken = "FREE_CHEST_OBJECTIVE"});
    }

    public void OnEnable()
    {
        if(!isServer)
        {
            return;
        }
        // These events only trigger server-side.
        Log.Debug("Subscribing to stage initialization events");
        SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
        SpawnCard.onSpawnedServerGlobal += OnSpawnCardSpawned;
    }

    public void OnDisable()
    {
        Log.Debug("Unsubscribing to scene initialization events");
        SceneDirector.onPrePopulateSceneServer -= OnPrePopulateSceneServer;
        SpawnCard.onSpawnedServerGlobal -= OnSpawnCardSpawned;
    }

    public RegisteredInteractableType GetRegisteredInteractableType(string interactableIdentifier)
    {
        return registeredInteractables[interactableIdentifier];
    }

    [SyncEvent]
    public event EventHandler<InteractableAddedEventArgs> EventInteractableAdded;

    /// <summary>
    /// Registers an interactable to be tracked.
    /// </summary>
    /// <param name="spawncardName">The name of the spawncard asset or
    /// gameobject name. For rusty
    /// lockboxes this is "iscLockbox", for encrusted caches this is
    /// "iscLockboxVoid"</param>
    /// <param name="objectiveToken">The name of the objective token (for
    /// retrieving localized objective string)</param>
    private void RegisterInteractableType(string spawncardName, RegisteredInteractableType registeredInteractableType)
    {
        Log.Debug($"Registering interactable {spawncardName}");
        registeredInteractables.Add(spawncardName, registeredInteractableType);
    }

    /******EVENT HANDLERS*********/
    private void OnPrePopulateSceneServer(SceneDirector director)
    {
        Log.Info("ServerInteractableListener: New stage loaded, searching for gold chest.");
        GameObject foundGoldChest = GameObject.Find("GoldChest");
        if(foundGoldChest != null)
        {
            Log.Debug("Firing EventInteractableAdded");
            EventInteractableAdded?.Invoke(this, new InteractableAddedEventArgs("GoldChest", foundGoldChest));
        }
    }

    private void OnSpawnCardSpawned(SpawnCard.SpawnResult result)
    {
        string spawncardName = result.spawnRequest.spawnCard.name;
        if(registeredInteractables.ContainsKey(spawncardName))
        {
            Log.Info($"ServerInteractableListener: Registered interactable {spawncardName} found.");
            GameObject interactable = result.spawnedInstance;
            EventInteractableAdded?.Invoke(this, new InteractableAddedEventArgs(spawncardName, interactable));
        }
    }

    // Interactables that have been registered. The key is the SpawnCard name:
    // "iscLockbox" for rusty lockboxes, "iscLockboxVoid" for encrusted caches,
    // etc.
    private Dictionary<string, RegisteredInteractableType> registeredInteractables = new();

}
