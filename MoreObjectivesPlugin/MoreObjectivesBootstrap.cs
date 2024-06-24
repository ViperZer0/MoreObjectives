using MoreObjectivesPlugin.InteractableEventWrappers;
using MoreObjectivesPlugin.InteractableListeners;
using MoreObjectivesPlugin.Networking;
using UnityEngine;

namespace MoreObjectivesPlugin;

public class MoreObjectivesBootstrap: MonoBehaviour
{
    public void Awake()
    {
        InteractableListenerFactory.CreateInteractableListener(gameObject);
        gameObject.AddComponent<ObjectiveTracker>();
        if(NetworkCheck.ServerExists.Value)
        {
            gameObject.AddComponent<InteractableEventWrapperManager>();
        }
    }
}
