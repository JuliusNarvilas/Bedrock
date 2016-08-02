using System.Diagnostics;
using Utilities.Events;

public class GameEventManager
{
    enum EGameEventsChannels
    {
        Selecting,

        Noof
    }

    static private readonly EventManager s_Instance = null;

    static GameEventManager()
    {
        Debug.Assert(s_Instance == null, "GameEventManager already initialised.");
        s_Instance = new EventManager((int)EGameEventsChannels.Noof);
    }

    static public EventManager Get()
    {
        return s_Instance;
    }
}
