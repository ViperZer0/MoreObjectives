using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin.InteractableEventWrappers;

/// <summary>
/// Monitors a chest interactable object and fires an event when it is
/// interacted with.
/// </summary>
public class ChestEventWrapper : NetworkBehaviour, IInteractableEventWrapper
{
    // SyncEvent allows us to listen to this event client-side
    [SyncEvent]
    public event EventHandler<InteractableInteractedWithArgs> EventInteractableInteractedWith;

    public void MonitorInteractable(GameObject interactable)
    {
        PurchaseInteraction interaction = interactable.GetComponent<PurchaseInteraction>();
        if (interaction == null)
        {
            Log.Error($"Couldn't get PurchaseInteraction component of GameObject {interaction}");
            return;
        }
        this.interactable = interactable;
        interaction.onPurchase.AddListener(OnPurchase);
    }

    public void Destroy()
    {
        Log.Info("Destroying ChestEventWrapper");
        if(interactable != null)
        {
            PurchaseInteraction interaction = interactable.GetComponent<PurchaseInteraction>();
            interaction.onPurchase.RemoveListener(OnPurchase);
        }
        Destroy(this);
    }

    private void OnPurchase(Interactor interactor)
    {
        Log.Info("Tracked interactable interacted with!");
        EventInteractableInteractedWith?.Invoke(this, new InteractableInteractedWithArgs(interactable));
    }

    private GameObject interactable;
}
