using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();
    private static UnityMainThreadDispatcher instance;

    public void Enqueue(Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }

    public static bool IsInitialized => instance != null;

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            throw new Exception("UnityMainThreadDispatcher not initialized. Call Initialize() from main thread first.");
        }
        return instance;
    }

    public static void Initialize()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        lock (ExecutionQueue)
        {
            while (ExecutionQueue.Count > 0)
            {
                ExecutionQueue.Dequeue().Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
