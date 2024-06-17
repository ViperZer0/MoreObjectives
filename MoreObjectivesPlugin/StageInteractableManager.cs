using System;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
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
    }

    // Maps string object names to the tracked interactable status.
    private Dictionary<string, TrackedInteractable> trackedInteractables = new();

    // The status of a tracked interactable.
    // Note that since GameObject.Find, the method on which this class relies,
    // only returns one interactable object, this class will never have multiple
    // interactables grouped together in an objective.
    private struct TrackedInteractable {
        public GameObject trackedObject;
        public bool interactedWith;

        public TrackedInteractable(GameObject gameObject)
        {
            trackedObject = gameObject;
            interactedWith = false;
        }
    }

    // Maps gameobjects to the string keys that define them.
    private Dictionary<GameObject, string> interactableLookup = new();

    public void RegisterInteractable(string gameobjectName, string objectiveToken)
    {
        registeredInteractables.Add(gameobjectName, new RegisteredInteractable{
                objectiveToken = objectiveToken,
                });
    }

    public void OnEnable()
    {
        SceneDirector.onPrePopulateSceneServer += OnStageLoaded;
        GlobalEventManager.OnInteractionsGlobal += OnGlobalInteraction;
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
    }

    private void AddTrackedInteractable(string objectName, GameObject gameObject)
    {
        trackedInteractables.Add(objectName, new TrackedInteractable(gameObject));
        interactableLookup.Add(gameObject, objectName);
    }

    private void RemoveTrackedInteractable(GameObject gameObject)
    {
        string objectName = interactableLookup[gameObject];
        TrackedInteractable interactableData = trackedInteractables[objectName];
        interactableData.interactedWith = true;
        trackedInteractables[objectName] = interactableData;
        interactableLookup.Remove(gameObject);
    }

    private void ResetAllInteractables()
    {
        interactableLookup = new();
        trackedInteractables = new();
    }

    private bool ObjectiveComplete(TrackedInteractable interactableData)
    {
        return interactableData.interactedWith;
    }

    /************EVENTS****************/
    private void OnStageLoaded(SceneDirector director)
    {
        Log.Info("Resetting all tracked interactables");
        ResetAllInteractables();
        foreach(string objectName in registeredInteractables.Keys)
        {
            GameObject foundObject = GameObject.Find(objectName);
            if(foundObject != null)
            {
                // We found a tracked object!
                AddTrackedInteractable(objectName, foundObject);
            }
        }
    }

    private void OnGlobalInteraction(Interactor interactor, IInteractable interactable, GameObject interactableObject)
    {
        if(interactableLookup.ContainsKey(interactableObject))
        {
            RemoveTrackedInteractable(interactableObject);
        }
    }

    /************OBJECTIVE GENERATION**************/
    private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
    {
        foreach(string key in trackedInteractables.Keys)
        {
            RegisteredInteractable registration = registeredInteractables[key];
            TrackedInteractable interactableData = trackedInteractables[key];
            if(!ObjectiveComplete(interactableData))
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                        {
                            source = new ObjectiveSourceDescriptorInteractableSource(registration, interactableData),
                            master = master,
                            objectiveType = typeof(InteractableObjectiveTracker),
                        });
            }
        }
    }

    /// <summary>
    /// Collects all necessary information into a context that is used to
    /// generate objective text. It's a little hacky but this is how RoR2 works.
    /// </summary>
    private class ObjectiveSourceDescriptorInteractableSource : UnityEngine.Object, IEquatable<ObjectiveSourceDescriptorInteractableSource>
    {
        public RegisteredInteractable registeredInteractableData;
        public TrackedInteractable discoveredInteractableData;

        public ObjectiveSourceDescriptorInteractableSource(RegisteredInteractable rid, TrackedInteractable did)
        {
            registeredInteractableData = rid;
            discoveredInteractableData = did;
        }

        public bool Equals(ObjectiveSourceDescriptorInteractableSource other)
        {
            return registeredInteractableData.Equals(other.registeredInteractableData) &&
                   discoveredInteractableData.Equals(other.discoveredInteractableData);
        }
    }

    /// <summary>
    /// Class used to generate objective text.
    /// </summary>
    private class InteractableObjectiveTracker: ObjectivePanelController.ObjectiveTracker
    {
        public override string GenerateString()
        {
            // We need to downcast the source descriptor since a regular
            // ObjectiveSourceDescriptor doesn't include enough information for
            // us to generate the objective string.
            ObjectiveSourceDescriptorInteractableSource context = (ObjectiveSourceDescriptorInteractableSource)sourceDescriptor.source;
            RegisteredInteractable registration = context.registeredInteractableData;
            TrackedInteractable interactableData = context.discoveredInteractableData;
            return Language.GetString(registration.objectiveToken);
        }
        
        public override bool IsDirty()
        {
            return true;
        }
    }


}
