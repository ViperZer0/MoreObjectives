using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
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

    /// <summary>
    /// Whether or not the rusty lockbox should show up as an objective.
    /// </summary>
    public static ConfigEntry<bool> LockboxObjective { get; set;}

    /// <summary>
    /// Whether or not the encrusted cache should show up as an objective.
    /// </summary>
    public static ConfigEntry<bool> LockboxVoidObjective { get; set; }

    /// <summary>
    /// Whether or not the legendary chest should show up as an objective.
    /// </summary>
    public static ConfigEntry<bool> GoldChestObjective { get; set; }

    /// <summary>
    /// Whether or not the crashed multishop terminal should show up as an
    /// objective.
    /// </summary>
    public static ConfigEntry<bool> FreeChestObjective { get; set; }

    // The Awake() method is run at the very start when the game is initialized.
    public void Awake()
    {
        Log.Init(Logger);
        Log.Info("MoreObjectives loaded!");
        spawnInteractableManager = gameObject.AddComponent(typeof(SpawnInteractableManager)) as SpawnInteractableManager;
        stageInteractableManager = gameObject.AddComponent(typeof(StageInteractableManager)) as StageInteractableManager;

        LockboxObjective = Config.Bind<bool>("Objectives", "Lockbox", true, "Whether or not to show an objective for the rusty lockbox");
        LockboxVoidObjective = Config.Bind<bool>("Objectives", "LockboxVoid", true, "Whether or not to show an objective for the encrusted cache");
        GoldChestObjective = Config.Bind<bool>("Objectives", "GoldChest", true, "Whether or not to show an objective for the legendary chest");
        FreeChestObjective = Config.Bind<bool>("Objectives", "FreeChest", true, "Whether or not to show an objective for the crashed multishop terminal");

        if(RiskOfOptionsWrapper.Enabled)
        {
            RiskOfOptionsWrapper.AddBool(LockboxObjective);
            RiskOfOptionsWrapper.AddBool(LockboxVoidObjective);
            RiskOfOptionsWrapper.AddBool(GoldChestObjective);
            RiskOfOptionsWrapper.AddBool(FreeChestObjective);
        }

        RoR2.Run.onRunStartGlobal += OnRunStart;
    }

    /// <summary>
    /// Register interactables to watch for only once the run starts, in case
    /// the configuration is changed.
    /// </summary>
    /// <param name="run">The RoR2 run</param>
    public void OnRunStart(Run run)
    {
        Log.Info("Run started, registering objective trackers");
        if(LockboxObjective.Value)
        {
            spawnInteractableManager.RegisterInteractable("iscLockbox", "LOCKBOX_OBJECTIVE");
        }
        if(LockboxVoidObjective.Value)
        {
            spawnInteractableManager.RegisterInteractable("iscLockboxVoid","LOCKBOX_VOID_OBJECTIVE");
        }
        if(FreeChestObjective.Value)
        {
            spawnInteractableManager.RegisterInteractable("iscFreeChest", "FREE_CHEST_OBJECTIVE");
        }
        if(GoldChestObjective.Value)
        {
            stageInteractableManager.RegisterInteractable("GoldChest", "GOLD_CHEST_OBJECTIVE");
        }
    }

    private SpawnInteractableManager spawnInteractableManager;
    private StageInteractableManager stageInteractableManager;
}
