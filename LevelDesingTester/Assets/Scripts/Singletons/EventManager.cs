using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> events;

    private static EventManager eventManager;
    private static EventManager instance
    {
        get 
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType<EventManager>();
                if(!eventManager)
                {
                    Debug.LogError("No EventManager in the scene!");
                }
                else
                {   
                    eventManager.Initialize();
                }
            }
            return eventManager;
        }
    }

    private void Initialize()
    {
        if(events == null)
        {
            events = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent e = null;
        if(instance.events.TryGetValue(eventName, out e)) 
        {
            e.AddListener(listener);
        }else
        {
            e = new UnityEvent();
            e.AddListener(listener);

            instance.events.Add(eventName, e);
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        UnityEvent e= null;

        if(eventManager != null)
        {
            return;
        }
        if (instance.events.TryGetValue(eventName, out e))
        {
            e.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName) 
    {
        UnityEvent e = null ;
        if (instance.events.TryGetValue(eventName, out e))
        {
            e.Invoke();
        }
    }

    private void OnDestroy()
    {
        if(eventManager == null)
            return;
        foreach (var e in instance.events.Values)
        {
            e.RemoveAllListeners();
        }
    }
}
