using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using RoR2.UI;
using System.Collections.Generic;

namespace CustomLevelObjectivesPlugin;

public class TestObjectiveTracker : ObjectivePanelController.ObjectiveTracker
{
    public override string GenerateString()
    {
        return "Hewoo :3";
    }

    public override bool IsDirty()
    {
        return true;
    }
}

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class CustomLevelObjectivesPlugin : BaseUnityPlugin
{
    // The Plugin GUID should be a unique ID for this plugin,
    // which is human readable (as it is used in places like the config).
    // If we see this PluginGUID as it is on thunderstore,
    // we will deprecate this mod.
    // Change the PluginAuthor and the PluginName !
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "ViperZer0";
    public const string PluginName = "CustomLevelObjectives";
    public const string PluginVersion = "0.0.0";


    // The Awake() method is run at the very start when the game is initialized.
    public void Awake()
    {
        Log.Init(Logger);

        Log.Debug("CustomLevelObjectives awake!");
        SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
    }

    private void OnEnable()
    {
        ObjectivePanelController.collectObjectiveSources += onCollectObjectiveSources;
    }

    private void SceneDirector_onPostPopulateSceneServer(SceneDirector director)
    {
        Log.Debug("Stage populated!");
    }

    private void onCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
    {
        output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    source = this,
                    master = master,
                    objectiveType = typeof(TestObjectiveTracker)
                });
    }

}
