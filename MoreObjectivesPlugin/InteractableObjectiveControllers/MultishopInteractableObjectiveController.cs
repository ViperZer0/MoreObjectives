using System;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace MoreObjectivesPlugin;

public class MultishopInteractableObjectiveController: MonoBehaviour, IInteractableObjectiveController
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
        Log.Info("MultishopInteractableObjectiveController awake!");
    }

    public void OnEnable()
    {
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
        // Have to hook into MultiShopController.OnPurchase() method.
        On.RoR2.MultiShopController.OnPurchase += OnPurchase;
    }

    public void Destroy()
    {
        Log.Info("Destroying MultishopInteractableObjectiveController");
        ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
        Destroy(this);
    }

    public void AddInteractable(GameObject multishop)
    {
        MultiShopController multiShopController = multishop.GetComponent<MultiShopController>();
        if(multiShopController == null)
        {
            Log.Error("Attempted to add an interactable that didn't have a MultiShopController component to the MultishopInteractableObjectiveController.");
            return;
        }
        totalInteractables++;
        interactables.Add(multiShopController);
    }

    public void SetObjectiveToken(string objectiveToken)
    {
        if(this.objectiveToken == null)
        {
            this.objectiveToken = objectiveToken;
        }
    }

    // Hook into MultiShopController.OnPurchase() method to register when a
    // multishop terminal has been purchased from one. Then filter it down to
    // only terminals we are tracking.
    private void OnPurchase(On.RoR2.MultiShopController.orig_OnPurchase orig, MultiShopController self, Interactor interactor, PurchaseInteraction purchaseInteraction)
    {
        orig(self, interactor, purchaseInteraction);
        if(interactables.Contains(self))
        {
            Log.Info("MultiShopController purchase!");
            interactablesActivated++;
            interactables.Remove(self);
        }
    }

    private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
    {
        if(!ObjectiveComplete)
        {
            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                    {
                        source = this,
                        objectiveType = typeof(MultishopInteractableObjectiveTracker),
                        master = master
                    });
        }
    }

    private class MultishopInteractableObjectiveTracker: ObjectivePanelController.ObjectiveTracker
    {
        private int interactablesActivated = -1;

        public override string GenerateString()
        {
            MultishopInteractableObjectiveController interactableObjectiveController = (MultishopInteractableObjectiveController)sourceDescriptor.source;
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
            return ((MultishopInteractableObjectiveController)sourceDescriptor.source).InteractablesActivated != interactablesActivated;
        }
    }

    /*****STATE*******/
    private int interactablesActivated = 0;
    private int totalInteractables = 0;
    private string objectiveToken;
    private List<MultiShopController> interactables = new();
}
