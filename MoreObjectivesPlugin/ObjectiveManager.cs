using System;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace MoreObjectivesPlugin;

public interface IObjectiveCollectable
{
    IEnumerable<ObjectiveManager.InteractableObjectiveSource> GetObjectives();
}

public class ObjectiveManager: MonoBehaviour
{
    public class InteractableObjectiveSource: UnityEngine.Object, IEquatable<InteractableObjectiveSource>
    {
        public string sourceId;
        public int interactablesActivated = 0;
        public int totalInteractables = 0;
        public string objectiveToken;

        public bool Equals(InteractableObjectiveSource other)
        {
            Log.Debug("Comparing STRINGS");
            return sourceId == other.sourceId;
        }

        public override bool Equals(object obj)
        {
            Log.Debug("Comparing!");
            if(obj is InteractableObjectiveSource other)
            {
                Log.Debug(Equals(other));
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator == (InteractableObjectiveSource lhs, InteractableObjectiveSource rhs)
        {
            Log.Debug("Comparing!");
            Log.Debug(lhs.Equals(rhs));
            return lhs.Equals(rhs);
        }

        public static bool operator != (InteractableObjectiveSource lhs, InteractableObjectiveSource rhs)
        {
            return !(lhs == rhs);
        }
    }

    private List<IObjectiveCollectable> objectiveCollectors = new();

    public void Awake()
    {
        Log.Info("ObjectiveManager loaded");
    }

    public void OnEnable()
    {
        ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
    }

    public void RegisterObjectiveCollector(IObjectiveCollectable objectiveCollector)
    {
        objectiveCollectors.Add(objectiveCollector);
    }

    /// <summary>
    /// Class used to generate objective text. It uses the base class's fields
    /// to get context, namely the source field which should be set to 
    /// <see cref="InteractableObjectiveSource"/> 
    /// </summary>
    private class InteractableObjectiveTracker: ObjectivePanelController.ObjectiveTracker
    {
        public override string GenerateString()
        {
            InteractableObjectiveSource context = (InteractableObjectiveSource)sourceDescriptor.source;
            // If more than one interactable, pluralize objective and pass in
            // the number of activated interactables and the total needed.
            if(context.totalInteractables > 1)
            {
                string objectiveToken = $"{context.objectiveToken}{Global.PLURAL_SUFFIX}";
                return Language.GetStringFormatted(objectiveToken, context.interactablesActivated, context.totalInteractables);
            }
            // Otherwise just use the singular objective string.
            else
            {
                return Language.GetString(context.objectiveToken);
            }
        }

        public override bool IsDirty()
        {
            return true;
        }
    }
            
    private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
    {
        foreach(IObjectiveCollectable objectiveCollector in objectiveCollectors)
        {
            foreach(InteractableObjectiveSource objectiveSource in objectiveCollector.GetObjectives())
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                        {
                            source = objectiveSource,
                            master = master,
                            objectiveType = typeof(InteractableObjectiveTracker),
                        });
            }
        }
    }
}
