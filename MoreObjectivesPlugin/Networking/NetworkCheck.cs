using UnityEngine.Networking;

namespace MoreObjectivesPlugin.Networking;

/// <summary>
/// This component only checks the network conditions. If this component/plugin
/// exists on the server, then we can run with some additional server-side
/// functionality. If the component is only running client-side,
/// we have to run with reduced functionality.
/// </summary>
public class NetworkCheck: NetworkBehaviour
{
    [SyncVar]
    private static bool? serverExists = null;

    /// <summary>
    /// Returns true in single-player or if the host player has the mod.
    /// Returns false if only the client player has the mod, so that we can run
    /// in client-only mode. 
    /// Returs null if this component hasn't been initialized yet!!!
    /// </summary>
    public static bool? ServerExists {
        get {
            if(serverExists.HasValue)
            {
                return serverExists.Value;
            }
            else
            {
                Log.Error("Attempted to get status of server before NetworkCheck was initialized!");
                return null;
            }
        }
    }

    public void Awake()
    {
        if(isServer)
        {
            Log.Info("Running on server detected");
            serverExists = true;
        }
        // Only set to false if it hasn't been set before (i.e by server)
        if(!serverExists.HasValue)
        {
            serverExists = false;
        }
    }
}

