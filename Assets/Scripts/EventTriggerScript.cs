using UnityEngine;
using UnityEngine.Events;

public class EventTriggerScript : MonoBehaviour
{
    [SerializeField]UnityEvent OnStart;
    [SerializeField]UnityEvent OnEnableEvent;

    private void Start()
    {
        OnStart?.Invoke();
    }

    private void OnEnable()
    {
        OnEnableEvent?.Invoke();
    }
}


