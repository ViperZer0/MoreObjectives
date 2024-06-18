using System.Runtime.CompilerServices;
using BepInEx.Configuration;

public static class RiskOfOptionsWrapper
{
    private static bool? enabled;

    public static bool Enabled {
        get {
            if (enabled == null) {
                enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
            }
            return (bool)enabled;
        }
    }

    // Apparently need these due to C# internals
    // https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/C%23-Programming/Mod-Compatibility%3A-Soft-Dependency/
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddBool(ConfigEntry<bool> config)
    {
        RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(config));
    }
}
