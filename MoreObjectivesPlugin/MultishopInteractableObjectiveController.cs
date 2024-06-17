using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace MoreObjectivesPlugin;

public class MultishopInteractableObjectiveController: ScriptableObject, IInteractableObjectiveController
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
        GlobalEventManager.OnInteractionsGlobal += OnGlobalInteraction;
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
    }

    public void Destroy()
    {
        Log.Info("Destroying MultishopInteractableObjectiveController");
        GlobalEventManager.OnInteractionsGlobal -= OnGlobalInteraction;
        ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
        Destroy(this);
    }

    public void AddInteractable(GameObject multishop)
    {
        MultiShopController multiShopController = multishop.GetComponent<MultiShopController>();
        if(multiShopController == null)
        {
            Log.Warning("Attempted to add an interactable that didn't have a MultiShopController component to the MultishopInteractableObjectiveController.");
            return;
        }
        totalInteractables++;
        Log.Debug(multiShopController._terminalGameObjects);
        Log.Debug(multiShopController.terminalGameObjects.Length);
        foreach(GameObject terminal in multiShopController._terminalGameObjects)
        {
            Log.Debug($"Terminal: {terminal}");
            terminalToMultishopMap.Add(terminal, multishop);
            interactableTerminals.Add(terminal);
        }
    }

    public void SetObjectiveToken(string objectiveToken)
    {
        if(this.objectiveToken == null)
        {
            this.objectiveToken = objectiveToken;
        }
    }

    private void OnGlobalInteraction(Interactor interactor, IInteractable interactable, GameObject terminal)
    {
        // If the registered interaction is one with a terminal we are tracking,
        // handle it.
        if(interactableTerminals.Contains(terminal))
        {
            interactablesActivated++;
            // Stop tracking the terminal.
            interactableTerminals.Remove(terminal);
            // Remove all other terminals that belong to the multishop we are
            // tracking. (Even if those terminals don't get disabled, we
            // consider the objective complete)
            GameObject multishop = terminalToMultishopMap[terminal];
            Dictionary<GameObject, GameObject> newMap = new();
            foreach (KeyValuePair<GameObject, GameObject> pair in terminalToMultishopMap)
            {
                // Filter out the bad terminals
                if(pair.Value != multishop)
                {
                    newMap.Add(pair.Key, pair.Value);
                }
                else 
                {
                    interactableTerminals.Remove(pair.Key);
                }
            }

            // Swap out old map with new map
            terminalToMultishopMap = newMap;
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
            return ((InteractableObjectiveController)sourceDescriptor.source).InteractablesActivated != interactablesActivated;
        }
    }

    /*****STATE*******/
    private int interactablesActivated = 0;
    private int totalInteractables = 0;
    private string objectiveToken;
    private Dictionary<GameObject, GameObject> terminalToMultishopMap = new();
    private List<GameObject> interactableTerminals = new();
}
