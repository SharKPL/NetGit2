using UnityEngine;
using UnityEngine.Events;

public static class GlobalEventManager 
{
    public static UnityEvent<bool> TurnSettings = new UnityEvent<bool>();
    public static UnityEvent<bool> TurnPlayerControl = new UnityEvent<bool>();
    public static UnityEvent<float> PlayerHealthChanged = new UnityEvent<float>();
    
    public static UnityEvent<string> TakeItemEvent = new UnityEvent<string>();
}
