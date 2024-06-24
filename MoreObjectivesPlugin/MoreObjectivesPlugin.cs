using BepInEx;
using R2API;
using R2API.Networking;
using MoreObjectivesPlugin.Configuration;
using R2API.Utils;
using MoreObjectivesPlugin.Networking;
using MoreObjectivesPlugin.InteractableListeners;
using MoreObjectivesPlugin.InteractableEventWrappers;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace MoreObjectivesPlugin;

/// <summary>
/// Holds global variable for the plural suffix added to objective tokens.
/// </summary>
public static class Global
{
    // Suffix to append to an objective token if it is plural.
    // I.e "LOCKBOX_OBJECTIVE" is "Open Rusty Lockbox"
    // but "LOCKBOX_OBJECTIVE_PLURAL" is "Open (0/3) Rusty Lockboxes".
    // Plural localization should use {0} for the number of interactables
    // completed and {1} for the total number.
    public const string PLURAL_SUFFIX = "_PLURAL";
}

[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(NetworkingAPI.PluginGUID)]
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
// Not every player should need this mod to play together.
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
public class MoreObjectivesPlugin : BaseUnityPlugin
{
    // The Plugin GUID should be a unique ID for this plugin,
    // which is human readable (as it is used in places like the config).
    // If we see this PluginGUID as it is on thunderstore,
    // we will deprecate this mod.
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "ViperZer0";
    public const string PluginName = "MoreObjectives";
    public const string PluginVersion = "0.0.0";

    // The Awake() method is run at the very start when the game is initialized.
    public void Awake()
    {
        Log.Init(Logger);
        Log.Info("MoreObjectives loaded!");

        // Register configuration.
        ConfigurationManager.Init(Config);
        RegisterOptionsMenu();
        
        // Delay most plugin initialization until the run starts, so we know the
        // full network conditions we are running in.
        Run.onRunStartGlobal += OnRunStart;
    }

    private void OnRunStart(Run run){
         // Spawn a new GameObject for plugin stuff.
        GameObject pluginObject = new GameObject("MoreObjectives");
        DontDestroyOnLoad(pluginObject);
        pluginObject.AddComponent<NetworkIdentity>();
        if(NetworkServer.active)
        {
            Log.Debug("Spawning MoreObjectives plugin object");
            NetworkServer.Spawn(pluginObject);
        } 
        // We have to spawn the plugin object before we check network conditions
        // and instantiate everything else.
        pluginObject.AddComponent<NetworkCheck>();
        pluginObject.AddComponent<MoreObjectivesBootstrap>();
    }

    private void RegisterOptionsMenu()
    {
        if(RiskOfOptionsWrapper.Enabled)
        {
            RiskOfOptionsWrapper.AddBool(ConfigurationManager.LockboxObjective);
            RiskOfOptionsWrapper.AddBool(ConfigurationManager.LockboxVoidObjective);
            RiskOfOptionsWrapper.AddBool(ConfigurationManager.GoldChestObjective);
            RiskOfOptionsWrapper.AddBool(ConfigurationManager.FreeChestObjective);
        }
    }

    private GameObject pluginObject;
}
