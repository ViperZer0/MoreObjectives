using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;
using MoreObjectivesPlugin.InteractableEventWrappers;

namespace MoreObjectivesPlugin.InteractableObjectiveControllers;

/// <summary>
/// Monitors one or more interactable objects and generates an objective for
/// them until the objects are all interacted with. 
/// </summary>
public class InteractableObjectiveController: MonoBehaviour, IInteractableObjectiveController
{
    public int InteractablesActivated {
        get => interactablesActivated;
    }

    public int TotalInteractables {
        get => totalInteractables;
    }

    public bool ObjectiveComplete {
        get => InteractablesActivated == TotalInteractables;
    }

    public void OnEnable()
    {
        //GlobalEventManager.OnInteractionsGlobal += OnGlobalInteraction;
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
        InteractableEventWrapperManager manager = gameObject.GetComponent<InteractableEventWrapperManager>();
        if(manager == null)
        {
            Log.Error("Couldn't find InteractableEventWrapperManager in InteractableObjectiveController");
            return;
        }
        eventWrapperManager = manager;
    }

    public void Destroy()
    {
        ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
        foreach(IInteractableEventWrapper eventWrapper in eventWrappers)
        {
            eventWrapper.EventInteractableInteractedWith -= OnPurchase;
            eventWrapperManager.RemoveEventWrapper(eventWrapper);
        }
        eventWrappers = new();
    }

    public void AddInteractable(GameObject gameObject)
    {
        Log.Debug("Adding interactable to InteractableObjectiveController");
        IInteractableEventWrapper interactableWrapper = eventWrapperManager.TrackInteractable(gameObject);
        totalInteractables++;
        interactableWrapper.EventInteractableInteractedWith += OnPurchase;
        eventWrappers.Add(interactableWrapper);
    }

    /// <summary>
    /// Sets the objective token. Can be called multiple times but can only be
    /// set once.
    /// </summary>
    /// <param name="objectiveToken">The objective token</param>
    public void SetObjectiveToken(string objectiveToken)
    {
        if(this.objectiveToken == null)
        {
            this.objectiveToken = objectiveToken;
        }
    }

    private void OnPurchase(object sender, InteractableEventWrappers.InteractableInteractedWithArgs args)
    {
        if(sender is IInteractableEventWrapper eventWrapper)
        {
            Log.Info("Tracked purchase interacted with!");
            interactablesActivated++;
            eventWrapper.EventInteractableInteractedWith -= OnPurchase;
            eventWrappers.Remove(eventWrapper);
        }
    }

    private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
    {
        if(!ObjectiveComplete)
        {
            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                    {
                        source = this,
                        objectiveType = typeof(InteractableObjectiveTracker),
                        master = master
                    });
        }
    }

    private class InteractableObjectiveTracker: ObjectivePanelController.ObjectiveTracker
    {
        private int interactablesActivated = -1;

        public override string GenerateString()
        {
            InteractableObjectiveController interactableObjectiveController = (InteractableObjectiveController)sourceDescriptor.source;
            interactablesActivated = interactableObjectiveController.InteractablesActivated;
            if(interactableObjectiveController.TotalInteractables > 1)
            {
                string objectiveToken = $"{interactableObjectiveController.objectiveToken}{Global.PLURAL_SUFFIX}";
                return Language.GetStringFormatted(objectiveToken, interactablesActivated, interactableObjectiveController.TotalInteractables);
            }
            else
            {
                return Language.GetString(interactableObjectiveController.objectiveToken);
            }
        }

        public override bool IsDirty()
        {
            return ((InteractableObjectiveController)sourceDescriptor.source).InteractablesActivated != interactablesActivated;
        }
    }

    /****STATE*****/
    private int interactablesActivated = 0;
    private int totalInteractables = 0;
    private string objectiveToken;
    private List<IInteractableEventWrapper> eventWrappers = new();
    private InteractableEventWrapperManager eventWrapperManager;
}
