using System;
using System.Collections.Generic;
using UnityEngine;

public class GameModeDependencyLoader : MonoBehaviour
{
    [Serializable]
    public class GameModeDependencies
    {
        /// <summary>
        /// The Game Mode that these dependencies are for
        /// </summary>
        public GameMode gameMode;
        /// <summary>
        /// The Prefabs or Scene Objects that this Game Mode depends on
        /// </summary>
        public List<GameObject> dependencies = new();

        [NonSerialized]
        public List<GameObject> instances = new();
    }

    public List<GameModeDependencies> gameModeDependencies = new();

    private GameManager gameManager;

    void Start()
    {
        Debug.Log("Game Mode Dependency Loader: Disabling any instantiated dependencies (Scene Objects)");

        foreach (var dependency in gameModeDependencies)
        {
            foreach (var obj in dependency.dependencies)
            {
                // Check if the object is a scene object
                if (obj.scene.IsValid())
                {
                    obj.SetActive(false);
                }
            }
        }

        InitializeGameManager();
    }


    void OnDestroy()
    {
        // Unsubscribe from GameManager events
        if (gameManager != null)
        {
            gameManager.onGameCreated.RemoveListener(GameManager_onGameCreated);
            gameManager.onGameDestroyed.RemoveListener(GameManager_onGameDestroyed);
        }
    }

    void InitializeGameManager()
    {
        if (Ref.TryGet(out gameManager))
        {
            gameManager.onGameCreated.AddListener(GameManager_onGameCreated);
            gameManager.onGameDestroyed.AddListener(GameManager_onGameDestroyed);

            if (gameManager.game != null)
                GameManager_onGameCreated(gameManager.game);
        }
        else
        {
            Debug.LogWarning("Game Mode Dependency Loader: GameManager not found.");
        }
    }

    void GameManager_onGameCreated(Game game)
    {
        foreach (var dependency in gameModeDependencies)
        {
            // TODO - improve this comparison from string name to something more robust
            if (game.gameMode.gameModeName == dependency.gameMode.gameModeName)
            {
                foreach (var obj in dependency.dependencies)
                {
                    GameObject instance = null;

                    // Check if the object is a prefab
                    if (!obj.scene.IsValid())
                    {
                        instance = Instantiate(obj);
                        instance.AddComponent<PrefabReference>().prefab = obj;
                    }
                    else
                    {
                        instance = obj;
                        instance.SetActive(true); // Activate the scene object
                    }

                    if (!instance.TryGetComponent(out GameModeDependency gameModeDependency))
                        gameModeDependency = instance.AddComponent<GameModeDependency>();

                    gameModeDependency.gameMode = game.gameMode;

                    // Remove existing listeners to prevent duplicates
                    gameModeDependency.onDependencyLoaded.RemoveListener(GameModeDependency_onDependencyLoaded);
                    gameModeDependency.onDependencyUnloaded.RemoveListener(GameModeDependency_onDependencyUnloaded);

                    gameModeDependency.onDependencyLoaded.AddListener(GameModeDependency_onDependencyLoaded);
                    gameModeDependency.onDependencyUnloaded.AddListener(GameModeDependency_onDependencyUnloaded);

                    dependency.instances.Add(instance);

                    Debug.Log($"Game Mode Dependency Loader: Loaded {instance.name} for {game.gameMode.gameModeName}");

                    gameModeDependency.Load();
                }
            }
        }
    }

    void GameManager_onGameDestroyed(Game game)
    {
        foreach (var dependency in gameModeDependencies)
        {
            if (game.gameMode.gameModeName == dependency.gameMode.gameModeName)
            {
                // Clean up all instances for this game mode
                foreach (var instance in dependency.instances)
                {
                    if (instance != null)
                    {
                        if (instance.TryGetComponent(out GameModeDependency gameModeDependency))
                        {
                            UnloadDependency(gameModeDependency);
                        }
                        else
                        {
                            // If no GameModeDependency component, deactivate or destroy based on prefab
                            UnloadInstance(instance);
                        }
                    }
                }
                dependency.instances.Clear();
            }
        }
    }

    void UnloadDependency(GameModeDependency gameModeDependency)
    {
        if (gameModeDependency == null)
            return;

        Debug.Log($"Game Mode Dependency Loader: Unloaded {gameModeDependency.name} for {gameModeDependency.gameMode.gameModeName}");

        gameModeDependency.onDependencyLoaded.RemoveListener(GameModeDependency_onDependencyLoaded);
        gameModeDependency.onDependencyUnloaded.RemoveListener(GameModeDependency_onDependencyUnloaded);

        gameModeDependency.Unload();

        UnloadInstance(gameModeDependency.gameObject);
    }

    void UnloadInstance(GameObject instance)
    {
        if (instance == null)
            return;

        // Check if the object is a prefab instance (has PrefabReference)
        if (instance.TryGetComponent(out PrefabReference prefabReference))
        {
            Destroy(instance);
        }
        else
        {
            instance.SetActive(false);
        }
    }

    void GameModeDependency_onDependencyLoaded(GameModeDependency gameModeDependency)
    {
        // Implement any logic needed when a dependency is loaded
    }

    void GameModeDependency_onDependencyUnloaded(GameModeDependency gameModeDependency)
    {
        UnloadDependency(gameModeDependency);
    }
}
