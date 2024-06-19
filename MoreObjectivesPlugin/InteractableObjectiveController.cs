using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace MoreObjectivesPlugin;

/// <summary>
/// Monitors one or more interactable objects and generates an objective for
/// them until the objects are all interacted with. 
/// </summary>
public class InteractableObjectiveController: ScriptableObject, IInteractableObjectiveController
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

    public string ObjectiveToken {
        get => objectiveToken;
    }

    public void Awake()
    {
        Log.Info("InteractableObjectiveController awake");
    }

    public void OnEnable()
    {
        //GlobalEventManager.OnInteractionsGlobal += OnGlobalInteraction;
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
    }

    public void Destroy()
    {
        Log.Info("Destroying InteractableObjectiveController");
        //GlobalEventManager.OnInteractionsGlobal -= OnGlobalInteraction;
        ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
        foreach(PurchaseInteraction interaction in interactables)
        {
            interaction.onPurchase.RemoveListener(OnPurchase);
        }
        Destroy(this);
    }

    public void AddInteractable(GameObject gameObject)
    {
        Log.Debug("Adding interactable to InteractableObjectiveController");
        PurchaseInteraction interaction = gameObject.GetComponent<PurchaseInteraction>();
        if(interaction == null)
        {
            Log.Error($"Couldn't get PurchaseInteraction component of GameObject {interaction}");
            return;
        }
        totalInteractables++;
        interactables.Add(interaction);
        interaction.onPurchase.AddListener(OnPurchase);
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

    private void OnPurchase(Interactor interactor)
    {
        Log.Info("Tracked purchase interacted with!");
        interactablesActivated++;
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
                return Language.GetString(interactableObjectiveController.ObjectiveToken);
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
    private List<PurchaseInteraction> interactables = new();
    private string objectiveToken;
}
