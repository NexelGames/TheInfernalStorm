/// <summary>
/// Contains all custom events data
/// </summary>
public abstract class EventData { }

public class GameOverEvent : EventData {
    public bool Win;
}

public class TimeEvent : EventData {
    public bool IsFreezed;
}
