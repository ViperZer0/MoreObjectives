using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace MoreObjectivesPlugin.InteractableObjectiveControllers;

/// <summary>
/// Generates objectives using only methods available client-side.
/// This means that objectives can't be removed, and there also isn't a good way
/// to have more than one interactable at the same time.
/// </summary>
public class NoOpInteractableObjectiveController : MonoBehaviour, IInteractableObjectiveController
{
    public void Awake()
    {
        Log.Debug("NoOpInteractableObjectiveController awake");
    }

    public void OnEnable()
    {
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
    }

    public void AddInteractable(GameObject gameObject)
    {
        // No need to track interactables, since we can't actually monitor them
        // at all.
        return;
    }

    public void Destroy()
    {
        return;
    }

    public void SetObjectiveToken(string objectiveToken)
    {
        Log.Debug($"Setting objective token: {objectiveToken}");
        if(this.objectiveToken == null)
        {
            this.objectiveToken = objectiveToken;
        }
    }

    private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
    {
        objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    source = this,
                    objectiveType = typeof(NoOpInteractableObjectiveTracker),
                    master = master
                });
    }

    private class NoOpInteractableObjectiveTracker: ObjectivePanelController.ObjectiveTracker
    {
        public override string GenerateString()
        {
            NoOpInteractableObjectiveController controller = (NoOpInteractableObjectiveController)sourceDescriptor.source;
            return Language.GetString(controller.objectiveToken);
        }

        public override bool IsDirty()
        {
            return false;
        }
    }

    private string objectiveToken;
}
