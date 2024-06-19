using System.Collections.Generic;
using BepInEx.Configuration;
using MoreObjectivesPlugin.Configuration;
using RoR2;
using UnityEngine;

namespace MoreObjectivesPlugin;

/// <summary>
/// Not all interactables are spawned in via SpawnCard. Notably, the legendary
/// chest on stage 4 comes with the stage, so we have to do a hardcoded search
/// for the object with that name after the stage is loaded. 
/// </summary>
public class StageInteractableManager: MonoBehaviour
{
    // Tracks registered interactables.
    private Dictionary<string, RegisteredInteractable> registeredInteractables = new();

    private struct RegisteredInteractable {
        public string objectiveToken;
        public ConfigEntry<bool> objectiveConfiguration;

        /// <summary>
        /// Dynamically get the value of the configuration option associated
        /// with this entry (via configurationEntryName)
        /// </summary>
        /// <returns>True if this objective should be tracked, false
        /// otherwise</returns>
        public bool GetConfigValue()
        {
            return objectiveConfiguration.Value;
        }
    }

    // Maps string object names to the controller that monitors them.
    private Dictionary<string, InteractableObjectiveController> trackedInteractables = new();

    public void RegisterInteractable(string gameobjectName, string objectiveToken, ConfigEntry<bool> objectiveConfiguration)
    {
        Log.Debug($"Registering interactable {gameobjectName}");
        registeredInteractables.Add(gameobjectName, new RegisteredInteractable{
                objectiveToken = objectiveToken,
                objectiveConfiguration = objectiveConfiguration,
                });
    }

    public void OnEnable()
    {
        Log.Info("StageInteractableManager loaded");
        Stage.onStageStartGlobal += OnStageLoaded;
    }

    /// <summary>
    /// Adds a tracked interactable to the controller monitoring it. 
    /// </summary>
    /// <param name="objectName">The object name. Doesn't serve any purpose
    /// other than as a key to group objects together.</param>
    /// <param name="gameObject">The object to monitor and add an objective for</param>
    private void AddTrackedInteractable(string objectName, GameObject gameObject)
    {
        Log.Info($"Adding tracked interactable: {objectName}");
        Log.Debug($"Tracking object: {gameObject}");
        InteractableObjectiveController controller = trackedInteractables.ContainsKey(objectName) ? trackedInteractables[objectName] : ScriptableObject.CreateInstance<InteractableObjectiveController>();
        controller.AddInteractable(gameObject);
        controller.SetObjectiveToken(registeredInteractables[objectName].objectiveToken);
        trackedInteractables[objectName] = controller;
    }

    /// <summary>
    /// Resets all monitoring for interactables (i.e when the stage changes)
    /// </summary>
    private void ResetAllInteractables()
    {
        foreach(InteractableObjectiveController controller in trackedInteractables.Values)
        {
            controller.Destroy();
        }
        trackedInteractables = new();
    }

    /************EVENTS****************/
    private void OnStageLoaded(Stage stage)
    {
        Log.Info("StageInteractableManager: New stage, resetting all tracked interactables");
        ResetAllInteractables();
        foreach(KeyValuePair<string, RegisteredInteractable> kvp in registeredInteractables)
        {
            string objectName = kvp.Key;
            RegisteredInteractable interactable = kvp.Value;
            // Ignore objectives we are not tracking. The configuration can be
            // changed mid-game, so between each stage we want to check to see
            // if its' changed and we should stop tracking an objective.
            if(!interactable.GetConfigValue())
            {
                break;
            }
            // Note that GameObject.Find() will only ever return one object, so
            // any objectives generated should only ever have one tracked
            // interacatble. It's still recommended to include a _PLURAL
            // language token when localizing however, just in case. 
            GameObject foundObject = GameObject.Find(objectName);
            if(foundObject != null)
            {
                AddTrackedInteractable(objectName, foundObject);
            }
        }
    }
}
