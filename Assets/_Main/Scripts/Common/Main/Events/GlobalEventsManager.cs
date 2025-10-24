using UnityEngine.Events;

/// <summary>
/// Contains all global events
/// </summary>
public static class GlobalEventsManager
{
    //!-!-!
    //UnityEvent subscription will be automaticaly deleted if obj destroyed (obj subscribed to event before)
    //so we use them to prevent possible issues
    //!-!-!

    public static UnityEvent<GameOverEvent> OnGameOver = new UnityEvent<GameOverEvent>();
    public static UnityEvent<TimeEvent> OnTimeFreezeChanged = new UnityEvent<TimeEvent>();
    public static UnityEvent OnPlayerDeath = new UnityEvent();
    public static UnityEvent OnPlayerRevive = new UnityEvent();
}
