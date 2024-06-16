using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;
using R2API;
using BepInEx;

namespace MoreObjectivesPlugin;

public class InteractableManager: MonoBehaviour
{
    private Dictionary<string, InteractableData> trackedInteractableData = new();

    private Dictionary<GameObject, string> trackedInteractables = new();

    const string PLURAL_SUFFIX = "_PLURAL";

    public class InteractableData: Object
    {
        public int interactablesActivated = 0;
        public int totalInteractables = 0;
        public string objectiveToken;
        public List<GameObject> spawnedInteractables = new();

        public InteractableData(string objectiveToken)
        {
            this.objectiveToken = objectiveToken;
        }

        public void Reset()
        {
            this.interactablesActivated = 0;
            this.totalInteractables = 0;
            spawnedInteractables = new();
        }
    }

    public void Awake()
    {
        Log.Info("InteractableManager loaded");
        SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
        SpawnCard.onSpawnedServerGlobal += OnSpawnCardSpawned;
        GlobalEventManager.OnInteractionsGlobal += OnGlobalInteraction;
    }

    public void OnEnable()
    {
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
    }

    public void RegisterInteractable(string spawncardName, string objectiveToken)
    {
        Log.Info($"Registering interactable {spawncardName}");
        trackedInteractableData.Add(spawncardName, new InteractableData(objectiveToken));
    }

    private void OnPrePopulateSceneServer(SceneDirector director)
    {
        Log.Info("Resetting all tracked interactables");
        ResetAllInteractables();
    }

    private void OnSpawnCardSpawned(SpawnCard.SpawnResult result)
    {
        Log.Debug($"Spawned: {result.spawnRequest.spawnCard.name}");
        // if the spawned object is one we are tracking, add it.
        if(trackedInteractableData.ContainsKey(result.spawnRequest.spawnCard.name))
        {
            AddTrackedInteractable(result);
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

    private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
    {
        foreach(InteractableData interactableData in trackedInteractableData.Values)
        {
            if(!ObjectiveComplete(interactableData))
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                        {
                            source = interactableData,
                            master = master,
                            objectiveType = typeof(InteractableObjectiveTracker),
                        });
            }
        }
    }

    // When we load a new stage we want to reset all the interactables.
    private void ResetAllInteractables()
    {
        trackedInteractables = new();
        foreach(InteractableData data in trackedInteractableData.Values)
        {
            data.Reset();
        }
    }

    private void AddTrackedInteractable(SpawnCard.SpawnResult result)
    {
        Log.Info($"Adding tracked interactable: {result.spawnRequest.spawnCard.name}");
        Log.Debug($"Tracking object: {result.spawnedInstance}");
        InteractableData data = trackedInteractableData[result.spawnRequest.spawnCard.name];
        data.totalInteractables += 1;
        data.spawnedInteractables.Add(result.spawnedInstance);
        trackedInteractableData[result.spawnRequest.spawnCard.name] = data;
        trackedInteractables.Add(result.spawnedInstance, result.spawnRequest.spawnCard.name);
    }

    private void RemoveTrackedInteractable(GameObject interactedObject)
    {
        Log.Info($"Removing tracked interactable: {interactedObject}");
        string objectName = trackedInteractables[interactedObject];
        InteractableData interactableData = trackedInteractableData[objectName];
        interactableData.interactablesActivated++;
        interactableData.spawnedInteractables.Remove(interactedObject);
    }

    private bool ObjectiveComplete(InteractableData data)
    {
        return data.interactablesActivated == data.totalInteractables;
    }

    private class InteractableObjectiveTracker: ObjectivePanelController.ObjectiveTracker
    {
        public override string GenerateString()
        {
            // We need to downcast the source descriptor since a regular
            // ObjectiveSourceDescriptor doesn't include enough information for
            // us to generate the objective string.
            InteractableData interactableData = (InteractableData)sourceDescriptor.source;
            // Pluralize objective, and pass in the number of activated
            // interactables vs the total needed.
            if(interactableData.totalInteractables > 1)
            {
                string objectiveToken = $"{interactableData.objectiveToken}{PLURAL_SUFFIX}";
                return Language.GetStringFormatted(objectiveToken, interactableData.interactablesActivated, interactableData.totalInteractables);
            }
            // Otherwise we just use the singular objective string.
            else
            {
                return Language.GetString(interactableData.objectiveToken);
            }
        }
        
        public override bool IsDirty()
        {
            return true;
        }
    }

}
