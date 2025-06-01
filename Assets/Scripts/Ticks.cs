using UnityEngine;
using System;

public class TickSystem : MonoBehaviour
{
    // This is a ticking system I implemented to handle periodic updates in the game.
    // It allows for a consistent tick rate, which can be useful for game mechanics that need to update at regular intervals.
    public static TickSystem Instance { get; private set; }

    public float tickInterval = 1.0f; // 1 tick per second
    private float timer;

    public static event Action OnTick;
    public int TickCount { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer -= tickInterval;
            TickCount++;
            OnTick?.Invoke();
        }
    }
}
