using System;
using RoR2;
using UnityEngine.Networking;

namespace MoreObjectivesPlugin;

public class NetworkSanityCheck: NetworkBehaviour
{
    [SyncEvent]
    public event System.Action EventTestEventAction;

    [SyncEvent]
    public event EventHandler EventTestEventHandler;

    public delegate void TestDelegate();

    [SyncEvent]
    public event TestDelegate EventTestDelegate;

    public void OnEnable()
    {
        Log.Debug("NetworkSanityCheck enabled");
        EventTestEventAction += OnTestEventAction;
        EventTestEventHandler += OnTestEventHandler;
        EventTestDelegate += OnTestEventDelegate;
        Stage.onStageStartGlobal += (Stage stage) => FireEvents();
    }

    public void FireEvents()
    {
        Log.Debug("NetworkSanityCheck FireEvents called");
        if(NetworkServer.active)
        {
            Log.Debug("Invoking NetworkSanityCheck events");
            EventTestEventAction?.Invoke();
            EventTestEventHandler?.Invoke(this, EventArgs.Empty);
            EventTestDelegate?.Invoke();
        }
        else
        {
            Log.Debug("NetworkSanity check NetworkServer not active");
        }
    }

    void OnTestEventAction()
    {
        Log.Debug("OnTestEventAction fired");
    }

    void OnTestEventHandler(object sender, EventArgs e)
    {
        Log.Debug("OnTestEventHandler fired");
    }

    void OnTestEventDelegate()
    {
        Log.Debug("OnTestEventDelegate fired");
    }
}
