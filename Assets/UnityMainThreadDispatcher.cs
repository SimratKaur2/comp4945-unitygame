using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        return FindObjectOfType<UnityMainThreadDispatcher>();
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    // Make sure the dispatcher exists in the scene from start.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (FindObjectOfType<UnityMainThreadDispatcher>() != null) return;

        var dispatcher = new GameObject("UnityMainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
        DontDestroyOnLoad(dispatcher.gameObject);
    }
}
