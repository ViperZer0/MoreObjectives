using BepInEx.Configuration;

/// <summary>
/// Exposes configuration variables for what items should show up as objectives
/// </summary>
public static class Configuration
{
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

    public static void Init(ConfigFile config)
    {
        LockboxObjective = config.Bind<bool>("Objectives", "Lockbox", true, "Whether or not to show an objective for the rusty lockbox");
        LockboxVoidObjective = config.Bind<bool>("Objectives", "LockboxVoid", true, "Whether or not to show an objective for the encrusted cache");
        GoldChestObjective = config.Bind<bool>("Objectives", "GoldChest", true, "Whether or not to show an objective for the legendary chest");
        FreeChestObjective = config.Bind<bool>("Objectives", "FreeChest", true, "Whether or not to show an objective for the crashed multishop terminal");

    }

}
