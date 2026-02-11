using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameModeDependency : MonoBehaviour
{
    public UnityEvent<GameModeDependency> onDependencyLoaded = new();
    public UnityEvent<GameModeDependency> onDependencyUnloaded = new();

    public GameMode gameMode { get; set; }

    bool isLoaded = false;

    private void OnDestroy()
    {
        Unload();
    }

    public void Load()
    {
        if (isLoaded)
            return;

        isLoaded = true;
        onDependencyLoaded?.Invoke(this);
    }
    public void Unload()
    {
        if (!isLoaded)
            return;

        isLoaded = false;
        onDependencyUnloaded?.Invoke(this);
    }
}
