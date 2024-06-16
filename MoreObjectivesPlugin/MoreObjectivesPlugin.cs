using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using RoR2.UI;
using System.Collections.Generic;

namespace MoreObjectivesPlugin;

public class LockboxObjectiveTracker : ObjectivePanelController.ObjectiveTracker
{

    public override string GenerateString()
    {
        return "Open Rusty Lockbox";
    }

    public override bool IsDirty()
    {
        return true;
    }
}

[BepInDependency(LanguageAPI.PluginGUID)]
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class MoreObjectivesPlugin : BaseUnityPlugin
{
    // The Plugin GUID should be a unique ID for this plugin,
    // which is human readable (as it is used in places like the config).
    // If we see this PluginGUID as it is on thunderstore,
    // we will deprecate this mod.
    // Change the PluginAuthor and the PluginName !
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "ViperZer0";
    public const string PluginName = "MoreObjectives";
    public const string PluginVersion = "0.0.0";

    private InteractableManager interactableManager;

    // The Awake() method is run at the very start when the game is initialized.
    public void Awake()
    {
        Log.Init(Logger);
        Log.Info("MoreObjectives loaded!");
        interactableManager = gameObject.AddComponent(typeof(InteractableManager)) as InteractableManager;
        interactableManager.RegisterInteractable("iscLockbox", "LOCKBOX_OBJECTIVE");
        interactableManager.RegisterInteractable("iscLockboxVoid","LOCKBOX_VOID_OBJECTIVE");
        interactableManager.RegisterInteractable("iscFreeChest", "FREE_CHEST_OBJECTIVE");
        //interactableManager.RegisterInteractable("iscChest1", "CHEST_OBJECTIVE");
    }
}
