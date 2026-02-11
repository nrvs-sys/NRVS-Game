using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onGameReady;
    public UnityEvent onGameBegin;
    public UnityEvent onGameEnd;
    public UnityEvent onGameComplete;
    public UnityEvent onGameReset;

    bool gameManagerListenersAdded = false;

    void OnEnable()
    {
        Ref.Instance.OnRegistered += Ref_OnRegistered;
        Ref.Instance.OnUnregistered += Ref_OnUnregistered;

        if (Ref.TryGet<GameManager>(out var gameManager))
            Ref_OnRegistered(typeof(GameManager), gameManager);
    }

    void OnDisable()
    {
        if (Ref.Instance != null)
        {
            Ref.Instance.OnRegistered -= Ref_OnRegistered;
            Ref.Instance.OnUnregistered -= Ref_OnUnregistered;

            if (Ref.TryGet<GameManager>(out var gameManager))
                Ref_OnUnregistered(typeof(GameManager), gameManager);
        }
    }

    void OnDestroy()
    {
        if (gameManagerListenersAdded)
        {
            if (Ref.TryGet<GameManager>(out var gameManager))
            {
                GameManager_OnUnregistered(gameManager);
            }
            gameManagerListenersAdded = false;
        }
    }

    void Ref_OnRegistered(System.Type type, object instance)
    {
        if (instance is GameManager gameManager)
        {
            GameManager_OnRegistered(gameManager);
        }
    }

    void Ref_OnUnregistered(System.Type type, object instance)
    {
        if (instance is GameManager gameManager)
        {
            GameManager_OnUnregistered(gameManager);
        }
    }

    void GameManager_OnRegistered(GameManager gameManager)
    {
        if (!gameManagerListenersAdded)
        {
            gameManager.onGameReady.AddListener(GameManager_onGameReady);
            gameManager.onGameBegin.AddListener(GameManager_onGameBegin);
            gameManager.onGameEnd.AddListener(GameManager_onGameEnd);
            gameManager.onGameComplete.AddListener(GameManager_onGameComplete);
            gameManager.onGameReset.AddListener(GameManager_onGameReset);

            gameManagerListenersAdded = true;

            // Handle the current game state
            switch (gameManager.game.gameState)
            {
                case Game.GameState.Ready:
                    GameManager_onGameReady(gameManager.game);
                    break;
                case Game.GameState.Active:
                    GameManager_onGameBegin(gameManager.game);
                    break;
                case Game.GameState.Stopped:
                    if (gameManager.game.completed)
                        GameManager_onGameComplete(gameManager.game);
                    else
                        GameManager_onGameEnd(gameManager.game);
                    break;
            }
        }
    }

    void GameManager_OnUnregistered(GameManager gameManager)
    {
        if (gameManagerListenersAdded)
        {
            gameManager.onGameReady.RemoveListener(GameManager_onGameReady);
            gameManager.onGameBegin.RemoveListener(GameManager_onGameBegin);
            gameManager.onGameEnd.RemoveListener(GameManager_onGameEnd);
            gameManager.onGameComplete.RemoveListener(GameManager_onGameComplete);
            gameManager.onGameReset.RemoveListener(GameManager_onGameReset);

            gameManagerListenersAdded = false;
        }
    }

    void GameManager_onGameReady(Game game)
    {
        if (gameObject.activeInHierarchy)
            onGameReady?.Invoke();
    }

    void GameManager_onGameBegin(Game game)
    {
        if (gameObject.activeInHierarchy)
            onGameBegin?.Invoke();
    }

    void GameManager_onGameEnd(Game game)
    {
        if (gameObject.activeInHierarchy)
            onGameEnd?.Invoke();
    }

    void GameManager_onGameComplete(Game game)
    {
        if (gameObject.activeInHierarchy)
            onGameComplete?.Invoke();
    }

    void GameManager_onGameReset(Game game)
    {
        if (gameObject.activeInHierarchy)
            onGameReset?.Invoke();
    }
}
