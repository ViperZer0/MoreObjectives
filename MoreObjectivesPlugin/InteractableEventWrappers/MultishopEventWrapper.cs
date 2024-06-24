using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin.InteractableEventWrappers;

public class MultishopEventWrapper: NetworkBehaviour, IInteractableEventWrapper
{
    public event EventHandler<InteractableInteractedWithArgs> EventInteractableInteractedWith;

    public void OnEnable()
    {
        // Have to hook into MultiShopController.OnPurchase() method.
        On.RoR2.MultiShopController.OnPurchase += OnPurchase;
    }

    public void MonitorInteractable(GameObject interactable)
    {
        MultiShopController multiShopController = interactable.GetComponent<MultiShopController>();
        if(multiShopController == null)
        {
            Log.Error("Attempted to add an interactable that didn't have a MultiShopController component to the MultishopEventWrapper.");
            return;
        }
        trackedMultishop = interactable;
    }

    public void Destroy()
    {
        Destroy(this);
        return;
    }

    // Hook into MultiShopController.OnPurchase() method to register when a
    // multishop terminal has been purchased from one.
    private void OnPurchase(On.RoR2.MultiShopController.orig_OnPurchase orig, MultiShopController self, Interactor interactor, PurchaseInteraction purchaseInteraction)
    {
        orig(self, interactor, purchaseInteraction);
        // OnPurchase fires for any multishop terminal.
        // We only want to handle the purchase if it's the one we're tracking.
        if(trackedMultishop == self.gameObject)
        {
            Log.Info("MultiShopController purchase!");
            EventInteractableInteractedWith?.Invoke(this, new InteractableInteractedWithArgs(trackedMultishop));
        }
    }

    private GameObject trackedMultishop;

}
