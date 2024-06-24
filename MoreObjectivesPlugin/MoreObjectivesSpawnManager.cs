using UnityEngine;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin.Networking;

public class MoreObjectivesSpawnManager: MonoBehaviour
{
    public delegate GameObject SpawnDelegate(Vector3 position, NetworkHash128 assetId);
    public delegate void UnSpawnDelegate(GameObject spawned);

    public NetworkHash128 PluginAssetId { get => pluginAssetId; }

    public void Start()
    {
        pluginAssetId = NetworkHash128.Parse("MoreObjectivesPlugin");
        ClientScene.RegisterSpawnHandler(pluginAssetId, SpawnPlugin, UnSpawnPlugin);
    }

    public GameObject SpawnPlugin(Vector3 position, NetworkHash128 assetId)
    {
        if(spawnedPluginInstance != null)
        {
            Log.Debug("Plugin instance already exists, resetting plug instance.");
            Destroy(spawnedPluginInstance);
        }
        Log.Debug("Spawning MoreObjectives plugin object");
        GameObject pluginObject = new GameObject("MoreObjectives");
        DontDestroyOnLoad(pluginObject);
        pluginObject.AddComponent<NetworkIdentity>();
        // Network spawn happened, so server exists. Hacky, but having trouble
        // syncing network status otherwise.
        pluginObject.AddComponent<NetworkCheck>();
        NetworkCheck.SetServerExistence(true);
        pluginObject.AddComponent<MoreObjectivesBootstrap>();
        spawnedPluginInstance = pluginObject;
        return pluginObject;
    }

    public GameObject SpawnPluginLocal()
    {
        Log.Debug("Spawning MoreObjectives plugin object locally");
        GameObject pluginObject = new GameObject("MoreObjectives");
        DontDestroyOnLoad(pluginObject);
        pluginObject.AddComponent<NetworkIdentity>();
        pluginObject.AddComponent<NetworkCheck>();
        NetworkCheck.SetServerExistence(false);
        pluginObject.AddComponent<MoreObjectivesBootstrap>();
        spawnedPluginInstance = pluginObject;
        return pluginObject;
    }

    public void UnSpawnPlugin(GameObject spawned)
    {
        Destroy(spawned);
    }

    private NetworkHash128 pluginAssetId;
    private GameObject spawnedPluginInstance;
}
